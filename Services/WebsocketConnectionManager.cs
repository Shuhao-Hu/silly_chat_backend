using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Channels;
using SillyChatBackend.Models;

namespace SillyChatBackend.Services;

public class WebsocketConnectionManager
{
    private readonly ConcurrentDictionary<uint, Client> _clients = new();
    private readonly Channel<Message> _directMessageChannel = Channel.CreateUnbounded<Message>();
    private readonly Channel<uint> _friendRequestChannel = Channel.CreateUnbounded<uint>();

    public WebsocketConnectionManager()
    {
        Task.Run(ProcessMessages);
        Task.Run(ProcessFriendRequests);
    }

    public bool AddClient(Client client)
    {
        RemoveClient(client.userId);
        return _clients.TryAdd(client.userId, client);
    }

    public bool RemoveClient(uint userId)
    {
        if (!_clients.TryRemove(userId, out var client)) return false;
        _ = client.Cleanup();
        return true;
    }

    public async Task SendMessageToUser(Message message)
    {
        await _directMessageChannel.Writer.WriteAsync(message);
    }

    public async Task SendFriendRequestToUser(uint targetUserId)
    {
        await _friendRequestChannel.Writer.WriteAsync(targetUserId);
    }

    private async Task ProcessMessages()
    {
        await foreach (var message in _directMessageChannel.Reader.ReadAllAsync())
        {
            // TODO
        }
    }

    private async Task ProcessFriendRequests()
    {
        await foreach (var targetUserId in _friendRequestChannel.Reader.ReadAllAsync())
        {
            if (_clients.TryGetValue(targetUserId, out var client))
            {
                _ = client.SendMessageAsync(JsonSerializer.Serialize(new { type = "friend_request" }));
            }
        }
    }
}

public class Client
{
    private readonly WebsocketConnectionManager _manager;
    private readonly WebSocket _websocket;
    public uint userId { get; }

    private readonly Channel<string> _messageChannel = Channel.CreateUnbounded<string>();

    private DateTime _lastMessageReceived;

    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(120);

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public Client(uint userId, WebSocket websocket, WebsocketConnectionManager manager)
    {
        _manager = manager;
        this.userId = userId;
        _websocket = websocket;
        _lastMessageReceived = DateTime.UtcNow;
        _ = MonitorTimeout(_cancellationTokenSource.Token);
    }

    public async Task SendMessageAsync(string message)
    {
        await _messageChannel.Writer.WriteAsync(message);
    }

    public async Task WriteToUser()
    {
        try
        {
            await foreach (var message in _messageChannel.Reader.ReadAllAsync(_cancellationTokenSource.Token))
            {
                if (_websocket.State != WebSocketState.Open) continue;
                var bytes = System.Text.Encoding.UTF8.GetBytes(message);
                await _websocket.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
        catch (Exception)
        {
            _websocket.Abort();
        }
    }

    public async Task ReadFromUser()
    {
        var buffer = new byte[1024 * 4];
        try
        {
            while (_websocket.State == WebSocketState.Open && !_cancellationTokenSource.IsCancellationRequested)
            {
                var result = await _websocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                _lastMessageReceived = DateTime.UtcNow;

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    _manager.RemoveClient(userId);
                    break;
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                    if (message.Equals("ping", StringComparison.OrdinalIgnoreCase))
                    {
                        var pongMessage = "pong";
                        var pongBytes = System.Text.Encoding.UTF8.GetBytes(pongMessage);
                        await _websocket.SendAsync(new ArraySegment<byte>(pongBytes), WebSocketMessageType.Text, true,
                            CancellationToken.None);
                    }
                    else
                    {
                        // TODO
                    }
                }
            }
        }
        catch (Exception)
        {
            _manager.RemoveClient(userId);
        }
    }

    private async Task MonitorTimeout(CancellationToken cancellationToken)
    {
        while (_websocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
        {
            if (DateTime.UtcNow - _lastMessageReceived > _timeout)
            {
                await _websocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "Timeout",
                    CancellationToken.None);
                _manager.RemoveClient(userId);
                break;
            }

            await Task.Delay(5000, cancellationToken);
        }
    }

    public async Task Cleanup()
    {
        await _cancellationTokenSource.CancelAsync();
        await _websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client removed", CancellationToken.None);
    }
}