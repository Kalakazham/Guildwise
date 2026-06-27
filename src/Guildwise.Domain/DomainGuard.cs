namespace Guildwise.Domain;

internal static class DomainGuard
{
    public static string Required(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        }

        return value.Trim();
    }

    public static void RequiredEnum<TEnum>(TEnum value, string parameterName)
        where TEnum : struct, Enum
    {
        if (EqualityComparer<TEnum>.Default.Equals(value, default))
        {
            throw new ArgumentOutOfRangeException(parameterName, $"{parameterName} is required.");
        }
    }
}
