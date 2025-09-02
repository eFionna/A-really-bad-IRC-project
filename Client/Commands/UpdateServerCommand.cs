namespace Client.Commands;

internal class UpdateServerCommand : IClientCommand
{
    public string Name => "updateserver";

    public string Description => "Updates an existing server";

    public void Execute(string[] args)
    {
        if (args.Length < 3) return;

        string alias = args[0];
        string host = args[1];
        int port = int.Parse(args[2]);


        if (LocalClient.knownServers.ContainsKey(alias))
        {
            LocalClient.knownServers[alias] = (host, port);
            Console.WriteLine($"Server '{alias}' set to {host}:{port}");
        }
        else
        {
            Console.WriteLine($"No known servers with alias {alias}.");
        }
    }
}
