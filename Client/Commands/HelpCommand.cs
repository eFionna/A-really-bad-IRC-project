namespace Client.Commands;

internal class HelpCommand : IClientCommand
{
    public string Name => "help";

    public string Description => "Lists all availabel commands";

    public void Execute(string[] args)
    {
        foreach (var cmd in LocalClient.commands.Values) 
        {
            Console.WriteLine($"{cmd.Name} - {cmd.Description}");
        }
    }
}
