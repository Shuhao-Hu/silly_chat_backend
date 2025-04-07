using Microsoft.AspNetCore.Mvc;
using SillyChatBackend.Services;
using SillyChatBackend.Utils;

namespace SillyChatBackend.Controllers;

[ApiController]
[Route("/ws")]
public class WebSocketController(WebsocketConnectionManager manager, IUserContext userContext, ILogger<WebSocketController> logger) : ControllerBase
{
    [HttpGet]
    public async Task Get()
    {
        try
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
                var client = new Client(userId.Value, webSocket, manager, logger);
                while (!manager.AddClient(client))
                {
                    manager.RemoveClient(client.userId);
                }
                Task[] tasks = [client.ReadFromUser(), client.WriteToUser()];
                Task.WaitAny(tasks);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
        catch (Exception ex)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
}