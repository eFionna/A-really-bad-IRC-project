using Shared.Commands;

namespace Client.Commands;

internal class HelpCommand : IAsyncCommand
{
    public string Name => "help";

    public string Description => "Lists all availabel commands";

    public string Usage => "help";


    public Task ExecuteAsync(AsyncCommandBaseArgs args)
    {
        foreach (var cmd in LocalClient.commandParser.Commands.Values)
        {
            Console.WriteLine($"{cmd.Name} - {cmd.Description} - {cmd.Usage}");
        }
        return Task.CompletedTask;
    }
}
