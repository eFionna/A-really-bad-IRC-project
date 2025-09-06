using Shared.Commands;


namespace Client.Commands;

internal class QuitCommand : IAsyncCommand
{
    public string Name => "quit";

    public string Description => "Exits the program";

    public string Usage => "quit";

    public Task ExecuteAsync(AsyncCommandBaseArgs args)
    {
        _ = LocalClient.StopAsync();
        return Task.CompletedTask;
    }
}
