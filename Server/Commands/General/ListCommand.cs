using System.Net.Sockets;

namespace Server.Commands.General;

internal class ListCommand : IServerCommand
{
    public string Name => "list";

    public string Description => "Lists all channels with user counts and topics";

    public async Task ExecuteAsync(string[] args, TcpClient client, AsyncTCPServer server)
    {
        if (!client.Connected) return;

        if (server.channels.IsEmpty)
        {
            await AsyncTCPServer.SendMessageToClientAsync(client, "No channels are currently available.");
            return;
        }

        await AsyncTCPServer.SendMessageToClientAsync(client, "Available channels:");

        foreach (var kvp in server.channels)
        {
            string channelName = kvp.Key;
            int userCount = kvp.Value.Count;

            // Try to get the topic
            string topic = server.channelTopics.TryGetValue(channelName, out string t) ? t : "No topic";

            await AsyncTCPServer.SendMessageToClientAsync(
                client,
                $"  {channelName} ({userCount} users) - Topic: {topic}"
            );
        }
    }
}
