using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using SillyChatBackend.Controllers;
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

    public bool ClientExists(uint userId)
    {
        return _clients.TryGetValue(userId, out _);
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
            if (_clients.TryGetValue(message.RecipientId, out var client))
            {
                _ = client.SendMessageAsync(JsonSerializer.Serialize(
                    new
                    {
                        type = "dm",
                        payload = message
                    }
                ));
            }
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

    private readonly ILogger<WebSocketController> _logger;

    public Client(uint userId, WebSocket websocket, WebsocketConnectionManager manager, ILogger<WebSocketController> logger)
    {
        _manager = manager;
        this.userId = userId;
        _websocket = websocket;
        _lastMessageReceived = DateTime.UtcNow;
        _logger = logger;
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
                var bytes = Encoding.UTF8.GetBytes(message);
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
            while (_websocket.State == WebSocketState.Open)
            {
                var receiveBuffer = new ArraySegment<byte>(buffer);
            
                WebSocketReceiveResult result;
                using (var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5)))
                {
                    try
                    {
                        result = await _websocket.ReceiveAsync(receiveBuffer, cts.Token);
                        // Update the last message timestamp whenever we receive any message
                        _lastMessageReceived = DateTime.UtcNow;
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogWarning($"WebSocket read timed out for {userId}");
                        break;
                    }
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    _logger.LogInformation($"Message received: {message}");
                    
                    // Process the message
                    if (message.ToLower() == "ping")
                    {
                        _logger.LogInformation($"Ping received from client {userId}, sending pong");
                        
                        // Send a pong response
                        var pongMessage = Encoding.UTF8.GetBytes("pong");
                        await _websocket.SendAsync(
                            new ArraySegment<byte>(pongMessage),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                    }
                    else
                    {
                        // Echo the message back
                        var responseMessage = Encoding.UTF8.GetBytes($"Server received: {message}");
                        await _websocket.SendAsync(
                            new ArraySegment<byte>(responseMessage),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation($"WebSocket close frame received for {userId}");
                    break;
                }
            }
        }
        catch (WebSocketException ex)
        {
            _logger.LogError($"WebSocket error for {userId}: {ex.Message}");
        }
        finally
        {
            _logger.LogInformation($"WebSocket connection closing: {userId}");
            
            
            _manager.RemoveClient(userId);
            // Close the socket gracefully if it's still open
            if (_websocket.State == WebSocketState.Open)
            {
                await _websocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Connection closed by the server",
                    CancellationToken.None);
            }
            
            _websocket.Dispose();
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