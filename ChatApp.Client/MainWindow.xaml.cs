using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.AspNetCore.SignalR.Client;

namespace ChatApp.Client;

public partial class MainWindow : Window
{
    private const string HubUrl = "http://localhost:5000/chathub";

    private readonly ObservableCollection<ChatMessage> _messages = new();
    private HubConnection? _connection;
    private string _username = "";

    public MainWindow()
    {
        InitializeComponent();
        MessagesList.ItemsSource = _messages;
        Loaded += (_, _) => UsernameInput.Focus();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void Close_Click(object sender, RoutedEventArgs e) => Close();
    private async void SendButton_Click(object sender, RoutedEventArgs e) => await SendAsync();
    private async void JoinButton_Click(object sender, RoutedEventArgs e) => await JoinAsync();

    private void UsernameInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) { e.Handled = true; _ = JoinAsync(); }
    }

    private async Task JoinAsync()
    {
        var name = UsernameInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            ShowLoginError("Please choose a username.");
            return;
        }

        _username = name;
        JoinButton.IsEnabled = false;
        LoginError.Visibility = Visibility.Collapsed;

        _connection = new HubConnectionBuilder()
            .WithUrl(HubUrl)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<string, string>("ReceiveMessage", (user, message) =>
            Dispatcher.Invoke(() => AddMessage(user, message)));

        _connection.Reconnecting += _ => { Dispatcher.Invoke(() => SetStatus("reconnecting…", "#FBBF24")); return Task.CompletedTask; };
        _connection.Reconnected += _ => { Dispatcher.Invoke(() => SetStatus("connected", "#34D399")); return Task.CompletedTask; };
        _connection.Closed += _ => { Dispatcher.Invoke(() => SetStatus("disconnected", "#F87171")); return Task.CompletedTask; };

        try
        {
            await _connection.StartAsync();
        }
        catch (Exception ex)
        {
            JoinButton.IsEnabled = true;
            ShowLoginError($"Couldn't connect to the server. Is it running?\n({ex.Message})");
            return;
        }

        WhoAmI.Text = _username;
        SetStatus("connected", "#34D399");
        LoginView.Visibility = Visibility.Collapsed;
        ChatView.Visibility = Visibility.Visible;
        MessageInput.Focus();
    }

    private void MessageInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) { e.Handled = true; _ = SendAsync(); }
    }

    private async Task SendAsync()
    {
        var text = MessageInput.Text.Trim();
        if (string.IsNullOrEmpty(text) || _connection is null) return;

        MessageInput.Clear();
        try
        {
            await _connection.SendAsync("SendMessage", _username, text);
        }
        catch
        {
            // Offline; automatic reconnect will resume the connection.
        }
    }

    private void AddMessage(string user, string message)
    {
        _messages.Add(new ChatMessage { User = user, Text = message, IsMine = user == _username });
        MessagesScroller.ScrollToEnd();
    }

    private void SetStatus(string text, string colorHex)
    {
        StatusText.Text = text;
        StatusDot.Fill = (Brush)new BrushConverter().ConvertFromString(colorHex)!;
    }

    private void ShowLoginError(string message)
    {
        LoginError.Text = message;
        LoginError.Visibility = Visibility.Visible;
    }

    protected override async void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        if (_connection is not null)
        {
            try { await _connection.DisposeAsync(); } catch { }
        }
    }
}
