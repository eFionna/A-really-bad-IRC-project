using Server.Commands;
using Server.Commands.General;
using Server.Commands.Operator;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server;

internal class AsyncTCPServer
{
    private readonly TcpListener listener;

    internal readonly ConcurrentDictionary<TcpClient, string> clients = new();

    internal readonly ConcurrentDictionary<string, ConcurrentDictionary<TcpClient, bool>> channels = new();

    internal readonly ConcurrentDictionary<string, ConcurrentDictionary<TcpClient, bool>> channelOps = new();

    internal readonly ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> channelBans = new();

    internal readonly ConcurrentDictionary<string, string> channelTopics = new();

    private readonly ConcurrentQueue<(string channel, string message)> messageQueue = new();

    private readonly Dictionary<string, IServerCommand> commands = new(StringComparer.OrdinalIgnoreCase);
    internal IEnumerable<IServerCommand> GetCommands() => commands.Values;
    private readonly HashSet<string> globalOps = [];
    private readonly HashSet<string> bannedIps = [];

    internal int Port { get; }
    internal bool Running { get; private set; }
    internal int MaxChannels { get; private set; } = 999;
    internal const string DefaultChannel = "#general";

    private const int BufferSize = 2 * 1024;
  
    public AsyncTCPServer(int port)
    {
        Port = port;
        listener = new TcpListener(IPAddress.Any, Port);

        channels[DefaultChannel] = new ConcurrentDictionary<TcpClient, bool>();

        RegisterCommands();
    }

    private void RegisterCommands()
    {
        RegisterCommand(new HelpCommand());
        RegisterCommand(new JoinCommand());
        RegisterCommand(new ListCommand());
        RegisterCommand(new PartCommand());
        RegisterCommand(new PrivateMessageCommand());
        RegisterCommand(new QuitCommand());
        RegisterCommand(new SayCommand());
        RegisterCommand(new TopicCommand());
        RegisterCommand(new WhoCommand());

        RegisterCommand(new BanCommand());
        RegisterCommand(new DeopCommand());
        RegisterCommand(new KickCommand());
        RegisterCommand(new OpCommand());
        RegisterCommand(new RemoveChannelCommand());
    }

    public async Task RunAsync()
    {
        Console.WriteLine($"Starting server on port {Port}");
        listener.Start();
        Running = true;

        _ = Task.Run(ProcessMessageQueueAsync);

        while (Running)
        {
            var tcpClient = await listener.AcceptTcpClientAsync();
            _ = Task.Run(() => HandleClientAsync(tcpClient));
        }
    }

    public void Shutdown() => Running = false;

    internal void EnqueueMessage(string channel, string v) => messageQueue.Enqueue((channel, v));

    internal bool IsOp(TcpClient client, string channel)
    {
        return channelOps.TryGetValue(channel, out var ops) && ops.ContainsKey(client);
    }

    internal bool IsBanned(string user, string channel)
    {
        return channelBans.TryGetValue(channel, out var bans) && bans.ContainsKey(user);
    }

    internal void DisconnectClient(TcpClient client, string reason = "has left the server")
    {
        if (clients.TryRemove(client, out var name))
        {
            RemoveClientFromAllChannels(client);

            BroadcastSystemMessage($"{name} {reason}.");

            Console.WriteLine($"Client disconnected: {name} ({client.Client.RemoteEndPoint})");
        }

        CleanupClient(client);
    }

    internal static async Task SendMessageToClientAsync(TcpClient client,string from, string message)
    {
        if (client == null || !client.Connected) return;

        try
        {
            string time = DateTime.Now.ToString("HH:mm");
            string formatted = $"[{time}] [{from}] {message}";
            byte[] data = Encoding.UTF8.GetBytes(formatted + "\n");
            await client.GetStream().WriteAsync(data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to send message to {client.Client.RemoteEndPoint}: {ex.Message}");
        }
    }

    private void RegisterCommand(IServerCommand cmd) => commands[cmd.Name] = cmd;

    private async Task HandleClientAsync(TcpClient tcpClient)
    {
        var endPoint = (IPEndPoint)tcpClient.Client.RemoteEndPoint!;
        string remoteIp = endPoint.Address.ToString();

        // Check global IP ban list
        if (IsIPBanned(remoteIp))
        {
            await SendMessageToClientAsync(tcpClient,"SERVER", "You are banned");
            Console.WriteLine($"Rejected connection from banned IP: {remoteIp}");
            CleanupClient(tcpClient);
            return;
        }

        Console.WriteLine($"New connection from {endPoint}");
        tcpClient.ReceiveBufferSize = BufferSize;
        tcpClient.SendBufferSize = BufferSize;

        var stream = tcpClient.GetStream();
        var buffer = new byte[BufferSize];

        try
        {
            //Receive username
            int bytesRead = await stream.ReadAsync(buffer);
            if (bytesRead == 0) { CleanupClient(tcpClient); return; }

            string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
            if (!msg.StartsWith("name:"))
            {
                CleanupClient(tcpClient);
                return;
            }

            string name = msg[(msg.IndexOf(':') + 1)..].Trim();
            if (string.IsNullOrEmpty(name) || clients.Values.Contains(name))
            {
                await SendMessageToClientAsync(tcpClient, "SERVER", "Name is allredy Taken");
                CleanupClient(tcpClient);
                return;
            }

            clients[tcpClient] = name;

            //Automatically join DefaultChannel
            //channels.GetOrAdd("#general", _ => new ConcurrentDictionary<TcpClient, bool>())[tcpClient] = true;
            //EnqueueMessage("#general", $"{name} has joined #general");
            await SendMessageToClientAsync(tcpClient, "SERVER", $"Welcome {name}");

            //Message loop
            var sb = new StringBuilder();
            while (tcpClient.Connected)
            {
                bytesRead = await stream.ReadAsync(buffer);
                if (bytesRead == 0) break;

                string incoming = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                await ProcessIncomingDataAsync(sb, incoming, tcpClient);
            }
        }
        finally
        {
            DisconnectClient(tcpClient);
        }
    }

    private async Task ProcessIncomingDataAsync(StringBuilder sb, string newData, TcpClient client)
    {
        sb.Append(newData);
        string content = sb.ToString();

        int newlineIndex;
        while ((newlineIndex = content.IndexOf('\n')) >= 0)
        {
            string fullMessage = content[..newlineIndex].Trim();
            if (!string.IsNullOrEmpty(fullMessage))
            {
                await ProcessCommandOrMessage(fullMessage, client);
            }

            content = content[(newlineIndex + 1)..];
        }

        sb.Clear();
        sb.Append(content);
    }

    private async Task ProcessCommandOrMessage(string input, TcpClient client)
    {
 
        if (input.StartsWith('/'))
        {
            var parts = input[1..].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;
            string cmdName = parts[0];
            string[] args = [.. parts.Skip(1)];

            if (commands.TryGetValue(cmdName, out var cmd))
            {
                await cmd.ExecuteAsync(args, client, this);
            }
        }
        //else
        //{
        //    foreach (var kvp in channels)
        //    {
        //        if (kvp.Value.ContainsKey(client))
        //            EnqueueMessage(kvp.Key, $"{clients[client]}: {input}");
        //    }
        //}
    }

    private async Task ProcessMessageQueueAsync()
    {
        while (Running)
        {
            while (messageQueue.TryDequeue(out var msg))
            {
                if (channels.TryGetValue(msg.channel, out var clientsInChannel))
                {
                    string time = DateTime.Now.ToString("HH:mm");
                    string formatted = $"[{time}] [{msg.channel}] {msg.message}";
                    byte[] data = Encoding.UTF8.GetBytes(formatted + "\n");
                    foreach (var c in clientsInChannel.Keys)
                    {
                        try
                        {
                            await c.GetStream().WriteAsync(data);
                        }
                        catch { }
                    }
                }
            }
            await Task.Delay(10);
        }
    }

    private void RemoveClientFromAllChannels(TcpClient client)
    {
        foreach (var kvp in channels)
        {
            kvp.Value.TryRemove(client, out _);
        }
    }

    private void BroadcastSystemMessage(string message)
    {
        foreach (var kvp in channels)
        {
            byte[] data = Encoding.UTF8.GetBytes(message + "\n");
            foreach (var c in kvp.Value.Keys)
            {
                try
                {
                    c.GetStream().Write(data, 0, data.Length);
                }
                catch { }
            }
        }
    }

    private static void CleanupClient(TcpClient client)
    {
        try { client.GetStream().Close(); } catch { }
        try { client.Close(); } catch { }
    }



    public void AddGlobalOp(string username) => globalOps.Add(username);
    public bool IsGlobalOp(string username) => globalOps.Contains(username);
    public IReadOnlyCollection<string> GetGlobalOps() => globalOps;


    public void RemoveChannel(string channel, string byUser)
    {
        if (channel == DefaultChannel)
        {
            Console.WriteLine("Cannot remove the default channel.");
            return;
        }

        if (channels.TryRemove(channel, out var clientsInChannel))
        {
            channelOps.TryRemove(channel, out _);
            channelBans.TryRemove(channel, out _);
            channelTopics.TryRemove(channel, out _);

            foreach (var client in clientsInChannel.Keys)
            {
                SendMessageToClientAsync(client, channel.ToUpper(), $"Channel {channel} was removed by {byUser}").Wait();
                channels[DefaultChannel][client] = true;
            }

            Console.WriteLine($"Channel {channel} removed by {byUser}");
        }
        else
        {
            Console.WriteLine($"Channel {channel} does not exist.");
        }
    }

    public bool KickUser(string username)
    {
        var client = clients.FirstOrDefault(x => x.Value.Equals(username, StringComparison.OrdinalIgnoreCase)).Key;
        if (client == null)
            return false;

        DisconnectClient(client, "was kicked by server");
        return true;
    }

    public bool BanIp(string ip)
    {
        if (!IPAddress.TryParse(ip, out var address))
            return false;

        if (!bannedIps.Add(ip))
            return false;

        
        foreach (var kvp in clients.Keys)
        {
            var remoteIp = ((IPEndPoint)kvp.Client.RemoteEndPoint!).Address.ToString();
            if (remoteIp == ip)
            {
                DisconnectClient(kvp, "was banned by server");
            }
        }

        Console.WriteLine($"IP {ip} banned.");
        return true;
    }

    public bool UnbanIp(string ip)
    {
        if (bannedIps.Remove(ip))
        {
            Console.WriteLine($"IP {ip} unbanned.");
            return true;
        }
        return false;
    }

    public bool IsIPBanned(string ip) => bannedIps.Contains(ip);
}
