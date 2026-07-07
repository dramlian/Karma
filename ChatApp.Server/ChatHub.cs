using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Server;

public class ChatHub : Hub
{
    private readonly ChatDbContext _db;

    public ChatHub(ChatDbContext db) => _db = db;

    public async Task SendMessage(string user, string message)
    {
        _db.Messages.Add(new Message
        {
            Username = user,
            Content = message,
            SentAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public override async Task OnConnectedAsync()
    {
        var history = await _db.Messages
            .OrderByDescending(m => m.SentAt)
            .Take(50)
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        foreach (var m in history)
            await Clients.Caller.SendAsync("ReceiveMessage", m.Username, m.Content);

        await base.OnConnectedAsync();
    }
}
