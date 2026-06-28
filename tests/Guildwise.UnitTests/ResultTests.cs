using Guildwise.Application.Common.Results;

namespace Guildwise.UnitTests;

public sealed class ResultTests
{
    [Fact]
    public void Result_Success_Has_No_Failure()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Failure);
    }

    [Theory]
    [MemberData(nameof(ResultFailureFactories))]
    public void Result_Failure_Has_Failure(FailureType expectedType, Func<string, Result> createResult)
    {
        const string message = "Expected outcome.";

        var result = createResult(message);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.NotNull(result.Failure);
        Assert.Equal(expectedType, result.Failure.Type);
        Assert.Equal(message, result.Failure.Message);
    }

    [Fact]
    public void ResultOfT_Success_Has_Value_And_No_Failure()
    {
        const string value = "Created value.";

        var result = Result<string>.Success(value);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(value, result.Value);
        Assert.Null(result.Failure);
    }

    [Theory]
    [MemberData(nameof(ResultOfTFailureFactories))]
    public void ResultOfT_Failure_Has_No_Value_And_Has_Failure(
        FailureType expectedType,
        Func<string, Result<string>> createResult)
    {
        const string message = "Expected outcome.";

        var result = createResult(message);

        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Null(result.Value);
        Assert.NotNull(result.Failure);
        Assert.Equal(expectedType, result.Failure.Type);
        Assert.Equal(message, result.Failure.Message);
    }

    public static TheoryData<FailureType, Func<string, Result>> ResultFailureFactories()
        => new()
        {
            { FailureType.NotFound, Result.NotFound },
            { FailureType.Validation, Result.Validation },
            { FailureType.Conflict, Result.Conflict },
            { FailureType.BusinessRule, Result.BusinessRule }
        };

    public static TheoryData<FailureType, Func<string, Result<string>>> ResultOfTFailureFactories()
        => new()
        {
            { FailureType.NotFound, Result<string>.NotFound },
            { FailureType.Validation, Result<string>.Validation },
            { FailureType.Conflict, Result<string>.Conflict },
            { FailureType.BusinessRule, Result<string>.BusinessRule }
        };
}
