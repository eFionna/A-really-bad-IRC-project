using System.Net.Sockets;
using System.Text;

namespace Client;

internal class AsyncTCPClient(string host, int port)
{
    public const int BufferSize = 2 * 1024;

    private readonly string host = host;
    private readonly int port = port;
    private readonly TcpClient client = new();
    private NetworkStream? stream;

    private string? currentChannel; // Keep track of the last joined channel

    private readonly byte[] receiveBuffer = new byte[BufferSize];


    public async Task RunAsync()
    {
        await client.ConnectAsync(host, port);
        stream = client.GetStream();

        _ = Task.Run(ReceiveMessagesAsync);

        await SendName();
        Thread.Sleep(10);

        while (client.Connected)
        {
            string? input = Console.ReadLine();
            ClearPreviousConsoleLine();

            if (string.IsNullOrEmpty(input)) continue;

            input = input.Trim();

            if (input.Equals("/quit", StringComparison.CurrentCultureIgnoreCase))
            {
                await SendAsync("/quit");
                break;
            }

            // If input starts with /, send it as a command
            if (input.StartsWith('/'))
            {
                // Handle /join to update currentChannel automatically
                if (input.StartsWith("/join ", StringComparison.OrdinalIgnoreCase))
                {
                    string[] parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                        currentChannel = parts[1].Trim();
                }

                await SendAsync(input);
            }
            else
            {
                // If user types a plain message, send it as /say to currentChannel
                if (string.IsNullOrEmpty(currentChannel))
                {
                    Console.WriteLine("You are not in any channel. Use /join #channel first.");
                    continue;
                }

                await SendAsync($"/say {currentChannel} {input}");
            }
        }

        await DisconnectAsync();
    }

    private async Task SendName()
    {
        Console.Write("Enter your username: ");

        string username = string.Empty;

        var input = Console.ReadLine();

        if (!string.IsNullOrEmpty(input))
        {
            username = input.Trim().Replace(' ', '_');
        }

        if (string.IsNullOrEmpty(username))
        {
            username = "Anonymous_" + DateTime.Now.Millisecond;
        }

        await SendAsync($"name:{username}");
    }

    private async Task SendAsync(string message)
    {
        if (stream == null) return;
        byte[] msgBytes = Encoding.UTF8.GetBytes(message + "\n");
        await stream.WriteAsync(msgBytes);
    }

    private async Task ReceiveMessagesAsync()
    {
        while (client.Connected)
        {
            try
            {
                int bytesRead = await stream!.ReadAsync(receiveBuffer);
                if (bytesRead == 0) break;

                string msg = Encoding.UTF8.GetString(receiveBuffer, 0, bytesRead).Trim();
                if (!string.IsNullOrEmpty(msg))
                {
                    if (msg.Contains("[SERVER]"))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;

                        if (msg.Contains("name", StringComparison.CurrentCultureIgnoreCase))
                        {
                            await DisconnectAsync();
                        }
                    }
                    else if (msg.Contains("[PM]"))
                    {
                        Console.ForegroundColor = ConsoleColor.Blue;
                    }

                    Console.WriteLine(msg);
                    Console.ResetColor();
                }
            }
            catch
            {
                break;
            }
        }
        Console.WriteLine("Disconnected from server.");
    }

    private static void ClearPreviousConsoleLine()
    {
        int currentLineCursor = Console.CursorTop;
        if (currentLineCursor > 0)
        {
            Console.SetCursorPosition(0, currentLineCursor - 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor - 1);
        }
    }

    private async Task DisconnectAsync()
    {
        if (stream != null) await stream.DisposeAsync();
        client.Close();
    }
}
