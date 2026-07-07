using Microsoft.AspNetCore.SignalR.Client;

const string url = "http://localhost:5000/chathub";

Console.Write("Enter your username: ");
var username = Console.ReadLine();
if (string.IsNullOrWhiteSpace(username))
    username = "Anonymous";

var connection = new HubConnectionBuilder()
    .WithUrl(url)
    .WithAutomaticReconnect()
    .Build();

connection.On<string, string>("ReceiveMessage", (user, message) =>
{
    Console.WriteLine($"{user}: {message}");
});

try
{
    await connection.StartAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"Could not connect to {url}. Is ChatApp.Server running?");
    Console.WriteLine($"Details: {ex.Message}");
    return;
}

Console.WriteLine($"Connected as '{username}'. Type a message and press Enter. Empty line to quit.");

while (true)
{
    var message = Console.ReadLine();
    if (string.IsNullOrEmpty(message))
        break;

    // Erase the echoed input so the message only shows once, as "username: message".
    ClearPreviousConsoleLine();

    await connection.SendAsync("SendMessage", username, message);
}

await connection.StopAsync();

static void ClearPreviousConsoleLine()
{
    try
    {
        var top = Console.CursorTop - 1;
        if (top < 0) return;
        Console.SetCursorPosition(0, top);
        Console.Write(new string(' ', Console.WindowWidth - 1));
        Console.SetCursorPosition(0, top);
    }
    catch
    {
        // Terminal doesn't support cursor moves (e.g. redirected output) — leave the line as-is.
    }
}
