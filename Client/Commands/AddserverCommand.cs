using Shared.Commands;

namespace Client.Commands;

internal class AddserverCommand : IAsyncCommand
{
    public string Name => "addserver";

    public string Description => "Adds a server to the know server list";

    public string Usage => "addserver <alias> <address> <port>";


    public Task ExecuteAsync(AsyncCommandBaseArgs args)
    {
        if (args.Arguments.Length != 3) 
            return Task.CompletedTask;

        string alias = args.Arguments[0];
        string host = args.Arguments[1];
        int port = int.Parse(args.Arguments[2]);

        LocalClient.knownServers[alias] = (host, port);
        Console.WriteLine($"Server '{alias}' set to {host}:{port}");

        return Task.CompletedTask;
    }
}
