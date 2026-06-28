namespace Guildwise.Application.Common;

public sealed class NotFoundException : InvalidOperationException
{
    public NotFoundException(string entityName, Guid id)
        : base($"{entityName} '{id}' was not found.")
    {
    }
}
