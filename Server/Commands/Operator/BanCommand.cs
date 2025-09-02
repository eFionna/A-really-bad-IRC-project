using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Server.Commands.Operator;

internal class BanCommand : IServerCommand
{
    public string Name => "ban";
    public string Description => "[OP only] Ban a user from a channel";

    public Task ExecuteAsync(string[] args, TcpClient client, AsyncTCPServer server)
    {
        if (args.Length < 2) return Task.CompletedTask;

        string channel = args[0].ToLower();
        string targetName = args[1];

        if (!channel.StartsWith('#')) return Task.CompletedTask;
        if (!server.IsOp(client, channel)) return Task.CompletedTask;

        // Add to ban list
        var bans = server.channelBans.GetOrAdd(channel, _ => new ConcurrentDictionary<string, bool>());
        bans[targetName] = true;

        // Kick if inside
        if (server.channels.TryGetValue(channel, out var clientsInChannel))
        {
            var target = clientsInChannel.Keys.FirstOrDefault(c => server.clients[c] == targetName);
            if (target != null && clientsInChannel.TryRemove(target, out _))
            {
                server.EnqueueMessage(channel, $"{targetName} was banned from {channel}");
            }
        }

        return Task.CompletedTask;
    }
}
