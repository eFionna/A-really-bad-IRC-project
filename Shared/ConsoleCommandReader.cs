using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Shared;

public class ConsoleCommandReader : IDisposable
{
    private readonly Channel<string> channel;
    private readonly CancellationTokenSource cts = new();
    private readonly Task readerTask;

    public ConsoleCommandReader()
    {
        channel = Channel.CreateUnbounded<string>();
        readerTask = Task.Run(ReadLoop);
    }

    private async Task ReadLoop()
    {
        while (!cts.IsCancellationRequested)
        {
            string? line = Console.ReadLine();

            if (line == null)
                break;

            ClearPreviousConsoleLine();
            await channel.Writer.WriteAsync(line, cts.Token);
        }

        channel.Writer.Complete();
    }

    public async IAsyncEnumerable<string> ReadAllAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (var line in channel.Reader.ReadAllAsync(token))
        {
            yield return line;
        }
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

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        cts.Cancel();
        try { readerTask.Wait(500); } catch { }
        cts.Dispose();
    }
}