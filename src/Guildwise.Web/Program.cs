using Guildwise.Application;
using Guildwise.Infrastructure;
using Guildwise.Web;
using Guildwise.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGuildwiseApplicationUseCases();
builder.Services.AddConfiguredGuildwiseInfrastructure(
    builder.Configuration,
    builder.Environment.IsDevelopment());
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (app.Environment.IsDevelopment()
    && app.Configuration.GetValue("Guildwise:Database:ApplyMigrationsOnStartup", true)
    && string.Equals(
        app.Configuration["Guildwise:PersistenceProvider"],
        "Postgres",
        StringComparison.OrdinalIgnoreCase))
{
    await app.Services.ApplyGuildwiseDatabaseMigrationsAsync();
}

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
