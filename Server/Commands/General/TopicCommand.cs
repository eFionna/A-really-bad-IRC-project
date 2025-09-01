
using System.Net.Sockets;


namespace Server.Commands.General;

internal class TopicCommand : IServerCommand
{
    public string Name => "topic";
    public string Description => "View or set the topic of a channel";

    public async Task ExecuteAsync(string[] args, TcpClient client, AsyncTCPServer server)
    {
        if (args.Length < 1) return;

        string channel = args[0];
        if (!channel.StartsWith('#')) return;

        if (args.Length == 1)
        {
            if (server.channelTopics.TryGetValue(channel, out string? topic))
            {
                await AsyncTCPServer.SendMessageToClientAsync(client, $"Topic for {channel}: {topic}");
            }
            else
            {
                await AsyncTCPServer.SendMessageToClientAsync(client, $"No topic is set for {channel}");
            }
            return ;
        }


        if (!server.IsOp(client, channel)) return;

        string newTopic = string.Join(' ', args.Skip(1));
        server.channelTopics[channel] = newTopic;
        server.EnqueueMessage(channel, $"{server.clients[client]} set the topic: {newTopic}");
    }
}
