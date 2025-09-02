using System.Net.Sockets;

namespace Server.Commands.General;

internal class WhoCommand : IServerCommand
{
    public string Name => "who";
    public string Description => "Lists all users in a specific channel";

    public async Task ExecuteAsync(string[] args, TcpClient client, AsyncTCPServer server)
    {
        if (args.Length < 1) return;

        string channel = args[0].Trim().ToLower();

        if (!channel.StartsWith('#'))
        {
            await AsyncTCPServer.SendMessageToClientAsync(client,"SERVER", "Invalid channel name. Channels must start with '#'.");
            return;
        }

        if (!server.channels.TryGetValue(channel, out var clientsInChannel) || clientsInChannel.IsEmpty)
        {
            await AsyncTCPServer.SendMessageToClientAsync(client, "SERVER", $"Channel {channel} does not exist or has no users.");
            return;
        }

        await AsyncTCPServer.SendMessageToClientAsync(client, "SERVER", $"Users in {channel}:");

        foreach (var kvp in clientsInChannel)
        {
            string username = server.clients.TryGetValue(kvp.Key, out var name) ? name : "Unknown";
            await AsyncTCPServer.SendMessageToClientAsync(client, "SERVER", $"  {username}");
        }
    }
}
