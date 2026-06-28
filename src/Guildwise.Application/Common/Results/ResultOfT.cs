namespace Guildwise.Application.Common.Results;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1000:Do not declare static members on generic types",
    Justification = "Result<T> exposes typed factory methods as its public creation API.")]
public sealed record Result<T>
{
    private Result(bool isSuccess, T? value, Failure? failure)
    {
        IsSuccess = isSuccess;
        Value = value;
        Failure = failure;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public T? Value { get; }

    public Failure? Failure { get; }

    public static Result<T> Success(T value)
        => new(true, value, null);

    public static Result<T> NotFound(string message)
        => new(false, default, new Failure(FailureType.NotFound, message));

    public static Result<T> Validation(string message)
        => new(false, default, new Failure(FailureType.Validation, message));

    public static Result<T> Conflict(string message)
        => new(false, default, new Failure(FailureType.Conflict, message));

    public static Result<T> BusinessRule(string message)
        => new(false, default, new Failure(FailureType.BusinessRule, message));
}
