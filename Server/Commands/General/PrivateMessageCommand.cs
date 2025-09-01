using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server.Commands.General;

internal class PrivateMessageCommand : IServerCommand
{
    public string Name => "msg";
    public string Description => "Send a private message to a specific user";

    public async Task ExecuteAsync(string[] args, TcpClient client, AsyncTCPServer server)
    {
        if (args.Length < 2) return;

        string targetName = args[0].Trim();
        string message = string.Join(' ', args.Skip(1));

        var targetClient = server.clients.FirstOrDefault(kv => kv.Value.Equals(targetName, StringComparison.OrdinalIgnoreCase)).Key;

        if (targetClient != null && targetClient.Connected)
        {
            await AsyncTCPServer.SendMessageToClientAsync(targetClient, $"[PM] {server.clients[client]}: {message}");
            await AsyncTCPServer.SendMessageToClientAsync(client, $"[PM to {targetName}]: {message}");
        }
        else
        {
            await AsyncTCPServer.SendMessageToClientAsync(client, $"User '{targetName}' not found or disconnected.");
        }
    }
}
