using Guildwise.Application.Abstractions.Persistence;

namespace Guildwise.Infrastructure.Persistence;

public sealed class InMemoryTransactionRunner : ITransactionRunner
{
    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        await operation(cancellationToken);
    }

    public async Task<T> ExecuteAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        return await operation(cancellationToken);
    }
}
