namespace Client.Commands;

internal class ListServersCommand : IClientCommand
{
    public string Name => "listservers";

    public string Description => "Lists all servers";

    public void Execute(string[] args)
    {
        if (LocalClient.knownServers.Count == 0)
        {
            Console.WriteLine("No known servers.");
            return;
        }

        Console.WriteLine("Known servers:");
        foreach (var kvp in LocalClient.knownServers)
        {
            string marker = kvp.Key == LocalClient.currentServerKey ? "*" : " ";
            Console.WriteLine($"{marker} {kvp.Key}: {kvp.Value.host}:{kvp.Value.port}");
        }
    }
}
