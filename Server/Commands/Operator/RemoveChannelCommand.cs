using System.Net.Sockets;

namespace Server.Commands.Operator;

internal class RemoveChannelCommand : IServerCommand
{
    public string Name => "rmchan";
    public string Description => "[OP only] Removes a channel (ops only)";

    public async Task ExecuteAsync(string[] args, TcpClient client, AsyncTCPServer server)
    {
        if (args.Length < 1) return;

        string channel = args[0].Trim().ToLower();

        if (!channel.StartsWith('#'))
            return;

        if (channel.Equals(AsyncTCPServer.DefaultChannel, StringComparison.OrdinalIgnoreCase))
        {
            await AsyncTCPServer.SendMessageToClientAsync(client,"SERVER", "You cannot remove the default channel.");
            return;
        }

        if (!server.channels.ContainsKey(channel))
        {
            await AsyncTCPServer.SendMessageToClientAsync(client, "SERVER", $"Channel {channel} does not exist.");
            return;
        }

        if (!server.channelOps.TryGetValue(channel, out var ops) || !ops.ContainsKey(client))
        {
            await AsyncTCPServer.SendMessageToClientAsync(client, "SERVER", $"You are not an operator in {channel}.");
            return;
        }

        // Remove the channel and notify users
        if (server.channels.TryRemove(channel, out var clientsInChannel))
        {
            // Kick everyone in the channel back to #general
            foreach (var c in clientsInChannel.Keys)
            {
                if (c.Connected)
                {
                    // Ensure they are added back to #general
                    server.channels[AsyncTCPServer.DefaultChannel][c] = true;
                    await AsyncTCPServer.SendMessageToClientAsync(c, "SERVER", $"Channel {channel} was removed. You were moved to {AsyncTCPServer.DefaultChannel}.");
                }
            }

            // Clean up
            server.channelOps.TryRemove(channel, out _);
            server.channelTopics.TryRemove(channel, out _);

            server.EnqueueMessage(AsyncTCPServer.DefaultChannel, $"{server.clients[client]} has removed {channel}");
        }
    }
}