using System.Reflection;
using NetArchTest.Rules;
using Xunit;

using DomainAssembly = Guildwise.Domain.AssemblyReference;
using ApplicationAssembly = Guildwise.Application.AssemblyReference;
using InfrastructureAssembly = Guildwise.Infrastructure.AssemblyReference;
using WebAssembly = Guildwise.Web.AssemblyReference;

namespace Guildwise.ArchitectureTests;

public sealed class LayerDependencyTests
{
    private static readonly Assembly Domain = typeof(DomainAssembly).Assembly;
    private static readonly Assembly Application = typeof(ApplicationAssembly).Assembly;
    private static readonly Assembly Infrastructure = typeof(InfrastructureAssembly).Assembly;
    private static readonly Assembly Web = typeof(WebAssembly).Assembly;

    [Fact]
    public void Domain_Should_Not_Reference_Other_Guildwise_Projects()
    {
        AssertAssemblyDoesNotReference(
            Domain,
            Application,
            Infrastructure,
            Web);
    }

    [Fact]
    public void Application_Should_Not_Reference_Infrastructure_Or_Web()
    {
        AssertAssemblyDoesNotReference(
            Application,
            Infrastructure,
            Web);
    }

    [Fact]
    public void Domain_Types_Should_Not_Depend_On_Application_Infrastructure_Or_Web()
    {
        var result = Types.InAssembly(Domain)
            .ShouldNot()
            .HaveDependencyOnAny(
                Application.GetName().Name!,
                Infrastructure.GetName().Name!,
                Web.GetName().Name!)
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Application_Types_Should_Not_Depend_On_Infrastructure_Or_Web()
    {
        var result = Types.InAssembly(Application)
            .ShouldNot()
            .HaveDependencyOnAny(
                Infrastructure.GetName().Name!,
                Web.GetName().Name!)
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Domain_Should_Not_Use_EfCore_Or_AspNetCore()
    {
        var result = Types.InAssembly(Domain)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore")
            .GetResult();

        AssertSuccessful(result);
    }

    [Fact]
    public void Application_Should_Not_Use_Infrastructure_Namespaces()
    {
        var result = Types.InAssembly(Application)
            .ShouldNot()
            .HaveDependencyOn("Guildwise.Infrastructure")
            .GetResult();

        AssertSuccessful(result);
    }

    private static void AssertAssemblyDoesNotReference(
        Assembly sourceAssembly,
        params Assembly[] forbiddenAssemblies)
    {
        var referencedAssemblyNames = sourceAssembly
            .GetReferencedAssemblies()
            .Select(assemblyName => assemblyName.Name)
            .ToHashSet(StringComparer.Ordinal);

        var forbiddenReferences = forbiddenAssemblies
            .Select(assembly => assembly.GetName().Name)
            .Where(name => name is not null)
            .Where(referencedAssemblyNames.Contains)
            .ToArray();

        Assert.True(
            forbiddenReferences.Length == 0,
            $"{sourceAssembly.GetName().Name} must not reference: {string.Join(", ", forbiddenReferences)}");
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
