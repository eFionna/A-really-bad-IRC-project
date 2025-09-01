using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Server.Commands.General;

internal class JoinCommand : IServerCommand
{
    public string Name => "join";
    public string Description => "Join a channel";

    public async Task ExecuteAsync(string[] args, TcpClient client, AsyncTCPServer server)
    {
        if (args.Length < 1) return;

        string channel = args[0].Trim();

        if (!channel.StartsWith('#'))
            return;

        if (server.IsBanned(server.clients[client], channel))
        {
            server.EnqueueMessage(AsyncTCPServer.DefaultChannel, $"{server.clients[client]} tried to join {channel} but is banned.");
            return;
        }

        if (!server.channels.ContainsKey(channel) && server.channels.Count >= server.MaxChannels)
        {
            Console.WriteLine($"{server.clients[client]} tried to create a new channel, but the server limit is reached.");
            await AsyncTCPServer.SendMessageToClientAsync(client, "The maximum server limit is reached.");
            return;
        }

        var clientsInChannel = server.channels.GetOrAdd(channel, _ => new ConcurrentDictionary<TcpClient, bool>());
        var opsInChannel = server.channelOps.GetOrAdd(channel, _ => new ConcurrentDictionary<TcpClient, bool>());

        // Already in channel?
        if (clientsInChannel.ContainsKey(client))
        {
            server.EnqueueMessage(channel, $"{server.clients[client]} is already in {channel}");
            return;
        }

        // First user becomes OP?
        if (opsInChannel.IsEmpty)
            opsInChannel[client] = true;

        clientsInChannel[client] = true;
        server.EnqueueMessage(channel, $"{server.clients[client]} has joined {channel}");

        if (server.channelTopics.TryGetValue(channel, out string? topic))
        {
            await AsyncTCPServer.SendMessageToClientAsync(client, $"Topic for {channel}: {topic}");
        }
        else
        {
            await AsyncTCPServer.SendMessageToClientAsync(client, $"No topic is set for {channel}");
        }
    }
}
