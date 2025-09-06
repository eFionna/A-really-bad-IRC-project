
using Shared.Commands;

namespace Client.Commands;

internal class RemoveServerCommand : IAsyncCommand
{
    public string Name => "removeserver";

    public string Description => "Remove a server from known servers";

    public string Usage => "removeserver <alias>";

    public Task ExecuteAsync(AsyncCommandBaseArgs args)
    {
        if (args.Arguments.Length != 1) return Task.CompletedTask;

        string alias = args.Arguments[0];

        if (LocalClient.knownServers.Remove(alias))
        {
            Console.WriteLine($"Removed server '{alias}'");
        }
        else
        {
            Console.WriteLine($"Server '{alias}' not found");
        }
        return Task.CompletedTask;
    }
}
