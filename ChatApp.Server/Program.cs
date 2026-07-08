using ChatApp.Server;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.Services.AddSingleton<PresenceTracker>();

var connectionString = builder.Configuration.GetConnectionString("ChatDb")
    ?? throw new InvalidOperationException("Connection string 'ChatDb' is not configured. Set ConnectionStrings:ChatDb in appsettings.json.");

builder.Services.AddDbContext<ChatDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 4, 0))));

builder.Services.AddScoped<DatabaseInitializer>();

builder.WebHost.UseUrls("http://localhost:5000");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync();
}

app.MapHub<ChatHub>("/chathub");
app.Run();
