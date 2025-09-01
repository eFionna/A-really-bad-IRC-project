using System.Collections.Concurrent;
using System.Net.Sockets;


namespace Server.Commands.Operator;

internal class OpCommand : IServerCommand
{
    public string Name => "op";
    public string Description => "[OP only] Grant operator privileges to a user in a channel";

    public Task ExecuteAsync(string[] args, TcpClient client, AsyncTCPServer server)
    {
        if (args.Length < 2) return Task.CompletedTask;

        string channel = args[0].Trim();
        string targetName = args[1].Trim();

        if (!channel.StartsWith('#')) return Task.CompletedTask;

        // Only allow current ops to grant privileges
        if (!server.IsOp(client, channel)) return Task.CompletedTask;

        if (server.channels.TryGetValue(channel, out var clientsInChannel))
        {
            var targetClient = clientsInChannel.Keys.FirstOrDefault(c => server.clients[c] == targetName);
            if (targetClient != null)
            {
                var opsInChannel = server.channelOps.GetOrAdd(channel, _ => new ConcurrentDictionary<TcpClient, bool>());
                opsInChannel[targetClient] = true;
                server.EnqueueMessage(channel, $"{targetName} is now an operator in {channel}");
            }
        }

        return Task.CompletedTask;
    }
}
