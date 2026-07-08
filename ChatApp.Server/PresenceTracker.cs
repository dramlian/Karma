using System.Collections.Concurrent;

namespace ChatApp.Server;

public class PresenceTracker
{
    private readonly ConcurrentDictionary<string, (string User, string Room)> _connections = new();

    public void Add(string connectionId, string user, string room) =>
        _connections[connectionId] = (user, room);

    public void Remove(string connectionId) => _connections.TryRemove(connectionId, out _);

    public string? GetRoom(string connectionId) =>
        _connections.TryGetValue(connectionId, out var entry) ? entry.Room : null;

    public List<string> GetOnlineUsers() =>
        _connections.Values.Select(v => v.User).Distinct().OrderBy(u => u).ToList();
}
