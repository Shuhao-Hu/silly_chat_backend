using Microsoft.AspNetCore.Mvc;
using SillyChatBackend.Services;
using SillyChatBackend.Utils;

namespace SillyChatBackend.Controllers;

[ApiController]
[Route("/ws")]
public class WebSocketController(WebsocketConnectionManager manager, IUserContext userContext) : ControllerBase
{
    [HttpGet]
    public async Task Get()
    {
        var token = HttpContext.Request.Query["token"].ToString();
        if (string.IsNullOrEmpty(token))
        {
            HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var userId = userContext.GetSubjectFromToken(token);
        if (userId == null)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var client = new Client(userId.Value, webSocket, manager);
            manager.AddClient(client);
            Task[] tasks = { client.ReadFromUser(), client.WriteToUser() };
            Task.WaitAny(tasks);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}