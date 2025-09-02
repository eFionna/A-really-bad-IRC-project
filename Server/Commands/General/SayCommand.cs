using System.Net.Sockets;


namespace Server.Commands.General;

internal class SayCommand : IServerCommand
{
    public string Name => "say";
    public string Description => "Sends a message in a channel";

    public Task ExecuteAsync(string[] args, TcpClient client, AsyncTCPServer server)
    {
        if (args.Length < 2) return Task.CompletedTask; 

        string channel = args[0].Trim().ToLower();

        if (!channel.StartsWith('#')) return Task.CompletedTask;

        if (!server.channels.TryGetValue(channel, out var clientsInChannel) || !clientsInChannel.ContainsKey(client))
        {
            return Task.CompletedTask;
        }

        string message = string.Join(' ', args.Skip(1));

        server.EnqueueMessage(channel, $"{server.clients[client]}: {message}");
        return Task.CompletedTask;
    }
}

