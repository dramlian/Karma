using ChatApp.Server;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();
builder.WebHost.UseUrls("http://localhost:5000");

var app = builder.Build();
app.MapHub<ChatHub>("/chathub");
app.Run();
