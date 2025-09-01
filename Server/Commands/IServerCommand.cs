using System.Net.Sockets;


namespace Server.Commands;

internal interface IServerCommand
{
    string Name { get; }
    string Description { get; }

    Task ExecuteAsync(string[] args, TcpClient client, AsyncTCPServer server);
}
