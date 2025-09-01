using System.Net.Sockets;


namespace Server.Commands.Operator;

internal class DeopCommand : IServerCommand
{
    public string Name => "deop";
    public string Description => "[OP only] Remove operator privileges from a user in a channel";

    public Task ExecuteAsync(string[] args, TcpClient client, AsyncTCPServer server)
    {
        if (args.Length < 2) return Task.CompletedTask;

        string channel = args[0];
        string targetName = args[1];

        if (!channel.StartsWith('#')) return Task.CompletedTask;
        if (!server.IsOp(client, channel)) return Task.CompletedTask;

        if (server.channelOps.TryGetValue(channel, out var ops))
        {
            var target = ops.Keys.FirstOrDefault(c => server.clients[c] == targetName);
            if (target != null && ops.TryRemove(target, out _))
            {
                server.EnqueueMessage(channel, $"{targetName} is no longer an operator in {channel}");
            }
        }
        return Task.CompletedTask;
    }
}
