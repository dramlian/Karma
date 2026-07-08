using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Server;

public class ChatHub : Hub
{
    private readonly ChatDbContext _db;
    private readonly PresenceTracker _presence;

    public ChatHub(ChatDbContext db, PresenceTracker presence)
    {
        _db = db;
        _presence = presence;
    }

    public async Task JoinRoom(string user, string room)
    {
        var previousRoom = _presence.GetRoom(Context.ConnectionId);
        if (previousRoom is not null && previousRoom != room)
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, previousRoom);

        await Groups.AddToGroupAsync(Context.ConnectionId, room);
        _presence.Add(Context.ConnectionId, user, room);

        var history = await _db.Messages
            .Where(m => m.Room == room)
            .OrderByDescending(m => m.SentAt)
            .Take(50)
            .OrderBy(m => m.SentAt)
            .Select(m => new MessageDto(m.Username, m.Content))
            .ToListAsync();

        await Clients.Caller.SendAsync("ReceiveHistory", history);
        await Clients.All.SendAsync("ReceiveOnlineUsers", _presence.GetOnlineUsers());
    }

    public async Task SendMessage(string user, string room, string message)
    {
        _db.Messages.Add(new Message
        {
            Username = user,
            Content = message,
            Room = room,
            SentAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        await Clients.Group(room).SendAsync("ReceiveMessage", user, message);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _presence.Remove(Context.ConnectionId);
        await Clients.All.SendAsync("ReceiveOnlineUsers", _presence.GetOnlineUsers());
        await base.OnDisconnectedAsync(exception);
    }
}
