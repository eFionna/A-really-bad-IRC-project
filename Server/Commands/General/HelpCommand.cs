using System.Net.Sockets;

namespace Server.Commands.General;

internal class HelpCommand : IServerCommand
{
    public string Name => "help";
    public string Description => "Lists all available commands";

    public async Task ExecuteAsync(string[] args, TcpClient client, AsyncTCPServer server)
    {
        if (!client.Connected) return;

        await AsyncTCPServer.SendMessageToClientAsync(client, "Available commands:");

        foreach (var cmd in server.GetCommands())
        {
            await AsyncTCPServer.SendMessageToClientAsync(client, $"  /{cmd.Name} - {cmd.Description}");
        }
    }
}
