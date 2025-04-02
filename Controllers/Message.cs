using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SillyChatBackend.DTOs;
using SillyChatBackend.Models;
using SillyChatBackend.Services;
using SillyChatBackend.Utils;

namespace SillyChatBackend.Controllers;

[ApiController]
[Route("/messages")]
public class MessageController(IUserContext userContext, IMessagesService messagesService, ILogger<MessageController> logger) : ControllerBase
{
    [HttpGet]
    [Route("unread")]
    [Authorize]
    public IActionResult GetUnreadMessages()
    {
        try
        {
            var userId = userContext.ExtractUserId();
            if (userId == null)
            {
                return Unauthorized();
            }
            var messages = messagesService.GetUnreadMessages(userId.Value);
            return Ok(new {messages});
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost]
    [Route("")]
    [Authorize]
    public IActionResult SendMessage([FromBody] MessageRequest messageRequest)
    {
        try
        {
            var userId = userContext.ExtractUserId();
            if (userId == null)
            {
                return Unauthorized();
            }
            messagesService.SendMessage(userId.Value, messageRequest.RecipientId, messageRequest.Content);
            return Created();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}