namespace Guildwise.E2ETests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class E2ETestCollection : ICollectionFixture<GuildwiseWebAppFixture>
{
    public const string Name = "Guildwise E2E tests";
}
