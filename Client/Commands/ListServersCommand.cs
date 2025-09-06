using Shared.Commands;

namespace Client.Commands;

internal class ListServersCommand : IAsyncCommand
{
    public string Name => "listservers";

    public string Description => "Lists all servers";

    public string Usage => "listservers";


    public Task ExecuteAsync(AsyncCommandBaseArgs args)
    {
        if (LocalClient.knownServers.Count == 0)
        {
            Console.WriteLine("No known servers.");
            return Task.CompletedTask;
        }

        Console.WriteLine("Known servers:");
        foreach (var kvp in LocalClient.knownServers)
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value.Host}:{kvp.Value.Port}");
        }
        return Task.CompletedTask;
    }
}
