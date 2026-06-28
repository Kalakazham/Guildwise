using Guildwise.Application;
using Guildwise.Infrastructure;
using Guildwise.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGuildwiseApplicationUseCases();
AddConfiguredInfrastructure(builder);
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static void AddConfiguredInfrastructure(WebApplicationBuilder builder)
{
    var persistenceProvider = builder.Configuration["Guildwise:PersistenceProvider"];

    if (string.IsNullOrWhiteSpace(persistenceProvider)
        || string.Equals(persistenceProvider, "InMemory", StringComparison.OrdinalIgnoreCase))
    {
        builder.Services.AddInMemoryInfrastructure();
        return;
    }

    if (string.Equals(persistenceProvider, "Postgres", StringComparison.OrdinalIgnoreCase))
    {
        builder.Services.AddPostgresInfrastructure(builder.Configuration);
        return;
    }

    throw new InvalidOperationException(
        "Configuration value 'Guildwise:PersistenceProvider' must be either 'InMemory' or 'Postgres'.");
}
