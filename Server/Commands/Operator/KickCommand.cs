using System.Net.Sockets;


namespace Server.Commands.Operator;

internal class KickCommand : IServerCommand
{
    public string Name => "kick";
    public string Description => "[OP only] Kick a user from a channel";

    public Task ExecuteAsync(string[] args, TcpClient client, AsyncTCPServer server)
    {
        if (args.Length < 2) return Task.CompletedTask;

        string channel = args[0];
        string targetName = args[1];
        string reason = args.Length > 2 ? string.Join(' ', args.Skip(2)) : "No reason given";

        if (!channel.StartsWith("#")) return Task.CompletedTask;
        if (!server.IsOp(client, channel)) return Task.CompletedTask;

        if (server.channels.TryGetValue(channel, out var clientsInChannel))
        {
            var target = clientsInChannel.Keys.FirstOrDefault(c => server.clients[c] == targetName);
            if (target != null && clientsInChannel.TryRemove(target, out _))
            {
                server.EnqueueMessage(channel, $"{targetName} was kicked from {channel} ({reason})");
            }
        }
        return Task.CompletedTask;
    }
}
