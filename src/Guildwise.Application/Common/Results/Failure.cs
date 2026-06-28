namespace Guildwise.Application.Common.Results;

public sealed record Failure(FailureType Type, string Message);
