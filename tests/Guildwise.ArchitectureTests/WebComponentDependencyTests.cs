using System.Reflection;
using NetArchTest.Rules;
using Xunit;

using WebAssembly = Guildwise.Web.AssemblyReference;

namespace Guildwise.ArchitectureTests;

public sealed class WebComponentDependencyTests
{
    private const string ComponentNamespacePattern = @"^Guildwise\.Web\.Components(\.|$)";
    private static readonly Assembly Web = typeof(WebAssembly).Assembly;

    [Fact]
    public void Web_Components_Should_Not_Depend_On_Infrastructure_Persistence()
    {
        AssertComponentSelectionIsNotEmpty();

        var result = Types.InAssembly(Web)
            .That()
            .ResideInNamespaceMatching(ComponentNamespacePattern)
            .ShouldNot()
            .HaveDependencyOn("Guildwise.Infrastructure.Persistence")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Web_Components_Should_Not_Use_EfCore()
    {
        AssertComponentSelectionIsNotEmpty();

        var result = Types.InAssembly(Web)
            .That()
            .ResideInNamespaceMatching(ComponentNamespacePattern)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        AssertSuccessful(result);
    }

    private static void AssertComponentSelectionIsNotEmpty()
    {
        var hasMatchingTypes = Web
            .GetTypes()
            .Any(type => type.Namespace is "Guildwise.Web.Components"
                         || type.Namespace?.StartsWith(
                             "Guildwise.Web.Components.",
                             StringComparison.Ordinal) == true);

        Assert.True(
            hasMatchingTypes,
            $"Architecture rule selected no types in namespace 'Guildwise.Web.Components' for assembly '{Web.GetName().Name}'.");
    }

    private static void AssertSuccessful(TestResult result)
    {
        var failingTypes = result.FailingTypes is null
            ? string.Empty
            : string.Join(
                Environment.NewLine,
                result.FailingTypes.Select(type => $" - {type.FullName}"));

        Assert.True(
            result.IsSuccessful,
            $"Architecture rule failed. Failing types:{Environment.NewLine}{failingTypes}");
    }
}
