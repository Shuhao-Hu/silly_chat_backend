using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SillyChatBackend.DTOs;
using SillyChatBackend.Services;
using SillyChatBackend.Utils;

namespace SillyChatBackend.Controllers;

[ApiController]
[Route("friends")]
public class FriendController(IAuthService authService, IFriendService friendService, IUserContext userContext, ILogger<FriendController> logger) : ControllerBase
{
    [HttpGet]
    [Route("")]
    [Authorize]
    public IActionResult GetAllFriends()
    {
        try
        {
            var userId = userContext.ExtractUserId();
            if (userId == null)
            {
                return Unauthorized("failed to extract user id");
            }

            var friends = friendService.GetAllFriends(userId.Value);
            return Ok(new { friends });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet]
    [Route("requests")]
    [Authorize]
    public IActionResult GetAllFriendRequests()
    {
        try
        {
            var userId = userContext.ExtractUserId();
            if (userId == null)
            {
                return Unauthorized("failed to extract user id");
            }
            var friendRequests = friendService.GetAllFriendRequests(userId.Value);
            return Ok(new { friend_requests = friendRequests });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPut]
    [Route("requests/{id}")]
    [Authorize]
    public IActionResult RespondFriendRequest([FromBody] FriendRequestResponse friendRequestResponse, uint id)
    {
        try
        {
            var userId = userContext.ExtractUserId();
            if (userId == null)
            {
                return Unauthorized("failed to extract user id");
            }

            var result = friendService.RespondFriendRequest(userId.Value, id, friendRequestResponse.SenderId, friendRequestResponse.Response);

            return result switch
            {
                FriendRequestStatus.Success => Ok(),
                FriendRequestStatus.NotFound => NotFound(),
                FriendRequestStatus.InvalidSender => Forbid(),
                FriendRequestStatus.InvalidResponse => BadRequest(),
                FriendRequestStatus.AlreadyResponded => Conflict(),
                _ => StatusCode(500, new { error = "Unexpected error." })
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet]
    [Route("search")]
    [Authorize]
    public IActionResult SearchUser([FromQuery] string email)
    {
        try
        {
            var userId = userContext.ExtractUserId();
            if (userId == null)
            {
                return Unauthorized("failed to extract user id");
            }

            var searchResult = authService.SearchUser(userId.Value, email);
            if (searchResult == null)
            {
                return NotFound();
            }
            return Ok(searchResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost]
    [Route("requests")]
    [Authorize]
    public IActionResult SendFriendRequest([FromBody] FriendRequestCreation request)
    {
        try
        {
            var userId = userContext.ExtractUserId();
            if (userId == null)
            {
                return Unauthorized("failed to extract user id");
            }

            var result = friendService.SendFriendRequest(userId.Value, request.FriendId);
            return result switch
            {
                FriendRequestStatus.Success => Created(),
                FriendRequestStatus.NotFound => NotFound(),
                FriendRequestStatus.AlreadyResponded => Conflict(),
                _ => StatusCode(500, new { error = "Unexpected error." })
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}