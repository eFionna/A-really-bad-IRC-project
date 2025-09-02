using Client;

LocalClient.RegisterCommands();

if (!Path.Exists(LocalClient.USER_DATA_PATH))
{
    Directory.CreateDirectory(LocalClient.USER_DATA_PATH);
}

LocalClient.LoadServers();

bool running = true;

while (running)
{
    var cmd = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(cmd))
        continue;

    var parts = cmd.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
    var command = parts[0].ToLower();


    if (parts[0].Equals("/exit", StringComparison.CurrentCultureIgnoreCase))
    {
        running = false;
    }


    if (LocalClient.commands.TryGetValue(command, out var result))
    {
        var arguments = parts.Length>1 ? parts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries) : [];
        result.Execute(arguments);
    }

    if (!string.IsNullOrWhiteSpace(LocalClient.currentServerKey)
        && LocalClient.knownServers.TryGetValue(LocalClient.currentServerKey, out var server))
    {
        LocalClient.currentServerKey = null; 
        AsyncTCPClient client = new(server.host, server.port);
        await client.RunAsync();
    }

}

LocalClient.SaveServers();


