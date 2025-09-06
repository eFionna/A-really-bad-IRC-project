using Client.Commands;
using Client.Data;
using Newtonsoft.Json;
using Shared;
using Shared.Commands;
using System.Threading.Tasks;

namespace Client;

internal class LocalClient
{
    internal static string USER_DATA_PATH => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "User");
    internal static string SERVERS_PATH => Path.Combine(USER_DATA_PATH, "servers.json");

    internal static Dictionary<string, Server> knownServers = [];

    internal static CommandParser commandParser = new();

    private static readonly CancellationTokenSource MainCTS = new();
    private static bool Running = false;
    private static AsyncTCPClient? currentClient;
    internal static async Task RunAsync()
    {
        Running = true;
        using ConsoleCommandReader reader = new();

        await foreach (var input in reader.ReadAllAsync(MainCTS.Token))
        {
            if (!Running)
                break;


            if (string.IsNullOrEmpty(input) || string.IsNullOrWhiteSpace(input))
                continue;


            if (currentClient != null)
            {
                await currentClient.ReciveInput(input);
                continue;
            }


            if (!commandParser.TryGetCommandFromInput(input, out IAsyncCommand cmd))
                continue;

            var args = new AsyncCommandBaseArgs(CommandParser.GetArgumentsFromCommand(input));
            _ = Task.Run(async () =>
            {
                await cmd.ExecuteAsync(args);
            });
        }
    }

    internal static void StartChatClient(string serverKey)
    {
        var server = knownServers[serverKey];
        currentClient = new AsyncTCPClient(server.Host, server.Port);
        _ = Task.Run(currentClient.StartAsync);
    }
    internal static async Task StopChatClient()
    {
        if(currentClient == null)
            return;

        await currentClient.StopAsync();
        currentClient = null;
    }
    internal static async Task StopAsync()
    {
        await StopChatClient();
        Running = false;
        MainCTS.Cancel();
    }

    internal static void LoadServers()
    {
        if (!Path.Exists(USER_DATA_PATH))
        {
            Directory.CreateDirectory(USER_DATA_PATH);
            return;
        }

        if (!File.Exists(SERVERS_PATH))
            return;

        string json = File.ReadAllText(SERVERS_PATH);
        var d = JsonConvert.DeserializeObject<Dictionary<string, Server>>(json);
        knownServers = d ?? [];
    }

    internal static void SaveServers()
    {
        string json = JsonConvert.SerializeObject(knownServers, Formatting.Indented);
        File.WriteAllText(SERVERS_PATH, json);
    }
    internal static void RegisterCommands()
    {
        commandParser.RegisterCommand(new AddserverCommand());
        commandParser.RegisterCommand(new ConnectCommand());
        commandParser.RegisterCommand(new HelpCommand());
        commandParser.RegisterCommand(new ListServersCommand());
        commandParser.RegisterCommand(new RemoveServerCommand());
        commandParser.RegisterCommand(new UpdateServerCommand());

        commandParser.RegisterCommand(new QuitCommand());
    }
}
