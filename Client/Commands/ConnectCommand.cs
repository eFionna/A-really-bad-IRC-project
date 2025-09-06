using Shared.Commands;

namespace Client.Commands;

internal class ConnectCommand : IAsyncCommand
{
    public string Name => "connect";

    public string Description => "Connect to a specific known server";

    public string Usage => "connect <alias>";


    public Task ExecuteAsync(AsyncCommandBaseArgs args)
    {
        if (args.Arguments.Length != 1) 
            return Task.CompletedTask;

        string alias = args.Arguments[0];

        if (!LocalClient.knownServers.TryGetValue(alias, out var server))
        {
            Console.WriteLine($"Server '{alias}' not found");
            return Task.CompletedTask;
        }
         LocalClient.StartChatClient(alias);

        return Task.CompletedTask;
    }
}
