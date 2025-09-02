

namespace Client.Commands;

internal class AddserverCommand : IClientCommand
{
    public string Name => "addserver";

    public string Description => "Adds a server to the know server list";

    public void Execute(string[] args)
    {
        if (args.Length != 3) return;

        string alias = args[0];
        string host = args[1];
        int port = int.Parse(args[2]);

        LocalClient.knownServers[alias] = (host, port);
        Console.WriteLine($"Server '{alias}' set to {host}:{port}");
    }
}
