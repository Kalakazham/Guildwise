using System.Reflection;
using NetArchTest.Rules;
using Xunit;

using ApplicationAssembly = Guildwise.Application.AssemblyReference;
using InfrastructureAssembly = Guildwise.Infrastructure.AssemblyReference;
using WebAssembly = Guildwise.Web.AssemblyReference;

namespace Guildwise.ArchitectureTests;

public sealed class FrameworkDependencyTests
{
    private static readonly Assembly Application = typeof(ApplicationAssembly).Assembly;
    private static readonly Assembly Infrastructure = typeof(InfrastructureAssembly).Assembly;
    private static readonly Assembly Web = typeof(WebAssembly).Assembly;

    [Fact]
    public void Application_Should_Not_Use_EfCore_Or_AspNetCore()
    {
        AssertNamespaceSelectionIsNotEmpty(Application, "Guildwise.Application");

        var result = Types.InAssembly(Application)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Web()
    {
        AssertNamespaceSelectionIsNotEmpty(Infrastructure, "Guildwise.Infrastructure");

        var referencedAssemblyNames = Infrastructure
            .GetReferencedAssemblies()
            .Select(assemblyName => assemblyName.Name)
            .ToHashSet(StringComparer.Ordinal);

        var forbiddenAssemblyName = Web.GetName().Name;

        Assert.True(
            forbiddenAssemblyName is not null
            && !referencedAssemblyNames.Contains(forbiddenAssemblyName),
            $"{Infrastructure.GetName().Name} must not reference {forbiddenAssemblyName}.");
    }

    [Fact]
    public void Infrastructure_Should_Not_Use_Blazor_Component_Types()
    {
        AssertNamespaceSelectionIsNotEmpty(Infrastructure, "Guildwise.Infrastructure");

        var result = Types.InAssembly(Infrastructure)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore.Components")
            .GetResult();

        AssertSuccessful(result);
    }

    private static void AssertNamespaceSelectionIsNotEmpty(
        Assembly assembly,
        string namespacePrefix)
    {
        var hasMatchingTypes = assembly
            .GetTypes()
            .Any(type => type.Namespace is not null
                         && type.Namespace.StartsWith(namespacePrefix, StringComparison.Ordinal));

        Assert.True(
            hasMatchingTypes,
            $"Architecture rule selected no types in namespace '{namespacePrefix}' for assembly '{assembly.GetName().Name}'.");
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
