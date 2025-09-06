using Shared.Commands;

namespace Client.Commands.TCPClient;

internal class SetNameCommand(AsyncTCPClient client) : IAsyncCommand
{
    public string Name => "setname";

    public string Description => "Sets your name on the server your currently connected to";

    public string Usage => "setname <username>";

    public async Task ExecuteAsync(AsyncCommandBaseArgs args)
    {
        if (args.Arguments.Length != 1)
            return;

        string username = args.Arguments[0].Trim().Replace(' ', '_') ?? "";

        if (string.IsNullOrEmpty(username))
        {
            username = "Anonymous_" + DateTime.Now.Millisecond;
        }
        client.NameIsSet = true;
        await client.SendAsync($"name:{username}");   
    }
}
