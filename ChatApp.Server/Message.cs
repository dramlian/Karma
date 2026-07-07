namespace ChatApp.Server;

public class Message
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTime SentAt { get; set; }
}
