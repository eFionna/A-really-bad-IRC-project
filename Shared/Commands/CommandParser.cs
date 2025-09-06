namespace Shared.Commands;

public class CommandParser()
{
    private readonly Dictionary<string, IAsyncCommand> commands = [];
    public const char CommandPrefix = '/';
    public IReadOnlyDictionary<string, IAsyncCommand> Commands => commands;



    public static string[] GetArgumentsFromCommand(string command) => [.. command.Split(' ').Skip(1)];

    public bool TryGetCommandFromInput(string input, out IAsyncCommand command)
    {
        command = default;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        var key = input.Trim().ToLower().Split(' ').First();

        if(key.Length <= 1) 
            return false;

        if (!key.StartsWith(CommandPrefix))
            return false;

        var clean = key[1..];

        return commands.TryGetValue(clean, out command);
    }


    public bool RegisterCommand(IAsyncCommand command)
    {
        return commands.TryAdd(command.Name.ToLower(), command);
    }
}
