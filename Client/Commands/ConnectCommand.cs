namespace Client.Commands;

internal class ConnectCommand : IClientCommand
{
    public string Name => "connect";

    public string Description => "Connect to a specific known server";

    public void Execute(string[] args)
    {
        if (args.Length != 1) return;

        string alias = args[0];

        if (!LocalClient.knownServers.TryGetValue(alias, out var server))
        {
            Console.WriteLine($"Server '{alias}' not found");
            return;
        }
        LocalClient.currentServerKey = alias;
        Console.WriteLine($"Connecting to {alias} ({server.host}:{server.port})....");
    }
}
