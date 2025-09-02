using Client.Commands;
using Newtonsoft.Json;

namespace Client;

internal static class LocalClient
{
    internal static readonly string USER_DATA_PATH = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "User");
    internal static readonly string SERVERS_PATH = Path.Combine(USER_DATA_PATH, "servers.json");
    internal static Dictionary<string, (string host, int port)> knownServers = [];
    internal static string? currentServerKey;

    internal static readonly Dictionary<string, IClientCommand> commands = [];


    internal static void LoadServers()
    {
        if (!File.Exists(SERVERS_PATH)) return;

        string json = File.ReadAllText(SERVERS_PATH);
        var d = JsonConvert.DeserializeObject<Dictionary<string, (string host, int port)>>(json);
        knownServers = d ?? [];
    }

    internal static void SaveServers()
    {
        string json = JsonConvert.SerializeObject(knownServers, Formatting.Indented);
        File.WriteAllText(SERVERS_PATH, json);
    }
    internal static void RegisterCommands()
    {
        RegisterCommand(new AddserverCommand());
        RegisterCommand(new ConnectCommand());
        RegisterCommand(new HelpCommand());
        RegisterCommand(new ListServersCommand());
        RegisterCommand(new RemoveServerCommand());
        RegisterCommand(new UpdateServerCommand());
    }

    static void RegisterCommand(IClientCommand command)
    {
        commands.Add(command.Name, command);
    }

}
