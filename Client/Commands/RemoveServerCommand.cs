
namespace Client.Commands;

internal class RemoveServerCommand : IClientCommand
{
    public string Name => "removeserver";

    public string Description => "Remove a server from known servers";

    public void Execute(string[] args)
    {
        if (args.Length != 1) return;
        string alias = args[0];

        if (LocalClient.knownServers.Remove(alias))
        {
            Console.WriteLine($"Removed server '{alias}'");
        }
        else
        {
            Console.WriteLine($"Server '{alias}' not found");
        }
    }
}
