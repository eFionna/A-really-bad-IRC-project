using System.Net.Sockets;

namespace Server.Commands.General;

internal class PartCommand : IServerCommand
{
    public string Name => "part";

    public string Description => "Leave a channel";

    public Task ExecuteAsync(string[] args, TcpClient client, AsyncTCPServer server)
    {
        if (args.Length < 1) return Task.CompletedTask;

        string channel = args[0].Trim();

        if (!channel.StartsWith('#'))
            return Task.CompletedTask;


        if (server.channels.TryGetValue(channel, out var clientsInChannel))
        {
            if (clientsInChannel.TryRemove(client, out _))
            {
                server.EnqueueMessage(channel, $"{server.clients[client]} has left {channel}");
            }
        }

        return Task.CompletedTask;
    }
}
