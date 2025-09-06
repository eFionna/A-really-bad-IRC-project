
namespace Shared.Commands;

public interface IAsyncCommand
{
    string Name { get; }
    string Description { get; }
    string Usage { get; }

    Task ExecuteAsync(AsyncCommandBaseArgs args);
}
