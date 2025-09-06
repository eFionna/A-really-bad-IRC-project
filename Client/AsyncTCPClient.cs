using Client.Commands.TCPClient;
using Shared.Commands;
using System;
using System.Net.Sockets;
using System.Text;

namespace Client;

internal sealed class AsyncTCPClient(string host, int port)
{
    public const int BufferSize = 2 * 1024;
    private readonly TcpClient client = new();
    private NetworkStream? stream;

    private string? currentChannel;
    private readonly byte[] receiveBuffer = new byte[BufferSize];

    private readonly CommandParser commandParser = new();
    private bool stopped;

    private bool nameIsSet;
    internal bool NameIsSet
    {
        get { return nameIsSet; }
        set
        {
            if (value && !nameIsSet)
                nameIsSet = true;
        }
    }

    internal string? CurrentChannel { get => currentChannel; set => currentChannel = value; }

    public async Task StartAsync()
    {
        commandParser.RegisterCommand(new SetNameCommand(this));


        Console.WriteLine($"Connecting to {host}:{port})....");
        await client.ConnectAsync(host, port);
        stream = client.GetStream();
        _ = Task.Run(ReceiveMessagesAsync);


        //if (input.Equals("/quit", StringComparison.CurrentCultureIgnoreCase))
        //{
        //    await SendAsync("/quit");
        //    break;
        //}
    }

    private bool NameSetCheck(string input)
    {
        if (nameIsSet) return true;

        if (input.StartsWith("/setname", StringComparison.CurrentCultureIgnoreCase)) return true;

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("You need to set you name with /setname <username>");
        Console.ResetColor();

        return false;
    }

    internal async Task ReciveInput(string input)
    {
        if (!NameSetCheck(input))
            return;

        if (commandParser.TryGetCommandFromInput(input, out IAsyncCommand cmd))
        {
            var args = new AsyncCommandBaseArgs(CommandParser.GetArgumentsFromCommand(input));
            _ = Task.Run(async () =>
            {
                await cmd.ExecuteAsync(args);
            });
            return;
        }

        if (input[0] == '/')
        {
            await SendAsync(input);
            return;
        }

        if (string.IsNullOrEmpty(currentChannel))
        {
            Console.WriteLine("You are not in any channel. Use /join #channel first.");
            return;
        }

        await SendAsync($"/say {currentChannel} {input}");
    }

    internal async Task SendAsync(string message)
    {
        if (stream == null) return;
        byte[] msgBytes = Encoding.UTF8.GetBytes(message + "\n");
        await stream.WriteAsync(msgBytes);
    }



    private async Task ReceiveMessagesAsync()
    {
        while (client.Connected && !stopped)
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
                            await LocalClient.StopChatClient();
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


    internal async Task StopAsync()
    {
        if (stopped) return;
        stopped = true;

        if (stream != null)
        {
            await stream.DisposeAsync();
            stream = null;
        }

        client.Close();
        client.Dispose();
    }
}
