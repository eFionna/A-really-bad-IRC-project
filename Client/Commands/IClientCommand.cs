namespace Client.Commands;

internal interface IClientCommand
{
    string Name { get; }
    string Description { get; }

    void Execute(string[] args);
}
