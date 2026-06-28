namespace Guildwise.Application.Common.Results;

public sealed record Result
{
    private Result(bool isSuccess, Failure? failure)
    {
        IsSuccess = isSuccess;
        Failure = failure;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Failure? Failure { get; }

    public static Result Success()
        => new(true, null);

    public static Result NotFound(string message)
        => new(false, new Failure(FailureType.NotFound, message));

    public static Result Validation(string message)
        => new(false, new Failure(FailureType.Validation, message));

    public static Result Conflict(string message)
        => new(false, new Failure(FailureType.Conflict, message));

    public static Result BusinessRule(string message)
        => new(false, new Failure(FailureType.BusinessRule, message));
}
