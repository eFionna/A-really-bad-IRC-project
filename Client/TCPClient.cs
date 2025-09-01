using System.Net.Sockets;
using System.Text;

namespace Client;

internal class TCPClient(string host, int port)
{
    public const int BufferSize = 2 * 1024;

    private readonly string host = host;
    private readonly int port = port;
    private TcpClient? client;
    private NetworkStream? stream;
    private string? username;



    public async Task RunAsync()
    {
        client = new TcpClient();
        await client.ConnectAsync(host, port);
        stream = client.GetStream();

        Console.Write("Enter your username: ");
        username = Console.ReadLine()?.Trim() ?? "Anonymous";

        byte[] nameBytes = Encoding.UTF8.GetBytes($"name:{username}\n");
        await stream.WriteAsync(nameBytes);

        _ = Task.Run(ReceiveMessagesAsync);

        while (true)
        {
            string? input = Console.ReadLine();
            if (string.IsNullOrEmpty(input)) continue;

            if (input.Equals("/quit", StringComparison.CurrentCultureIgnoreCase))
            {
                byte[] quitMsg = Encoding.UTF8.GetBytes("/quit\n");
                await stream.WriteAsync(quitMsg);
                break;
            }

            byte[] msgBytes = Encoding.UTF8.GetBytes(input + "\n");
            await stream.WriteAsync(msgBytes);
        }

        stream.Close();
        client.Close();
    }

    private async Task ReceiveMessagesAsync()
    {
        var buffer = new byte[2048];

        while (client!.Connected)
        {
            try
            {
                int bytesRead = await stream!.ReadAsync(buffer);
                if (bytesRead == 0) break; // Server disconnected

                string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                if (!string.IsNullOrEmpty(msg))
                    Console.WriteLine(msg);
            }
            catch
            {
                break;
            }
        }

        Console.WriteLine("Disconnected from server.");
    }
}
