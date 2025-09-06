using Shared.Commands;

namespace Client.Commands;

internal class UpdateServerCommand : IAsyncCommand
{
    public string Name => "updateserver";

    public string Description => "Updates an existing server";

    public string Usage => "updateserver <alias> <address> <port>";


    public Task ExecuteAsync(AsyncCommandBaseArgs args)
    {
        if (args.Arguments.Length < 3) 
            return Task.CompletedTask;

        string alias = args.Arguments[0];
        string host = args.Arguments[1];
        int port = int.Parse(args.Arguments[2]);


        if (LocalClient.knownServers.ContainsKey(alias))
        {
            LocalClient.knownServers[alias] = (host, port);
            Console.WriteLine($"Server '{alias}' set to {host}:{port}");
        }
        else
        {
            Console.WriteLine($"No known servers with alias {alias}.");
        }

        return Task.CompletedTask;
    }
}
