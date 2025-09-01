using System.Net.Sockets;

namespace Server.Commands.General;

internal class QuitCommand : IServerCommand
{
    public string Name => "quit";

    public string Description => "Leaves the server";

    public Task ExecuteAsync(string[] args, TcpClient client, AsyncTCPServer server)
    {
        server.DisconnectClient(client, "has quit");
        return Task.CompletedTask;
    }
}
