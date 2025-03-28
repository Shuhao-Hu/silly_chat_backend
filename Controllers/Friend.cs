using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SillyChatBackend.DTOs;
using SillyChatBackend.Services;
using SillyChatBackend.Utils;

namespace SillyChatBackend.Controllers;

[ApiController]
[Route("friends")]
public class FriendController(IAuthService authService, IFriendService friendService, IUserContext userContext) : ControllerBase
{
    [HttpGet]
    [Route("")]
    [Authorize]
    public IActionResult GetAllFriends()
    {
        var userId = userContext.ExtractUserId();
        if (userId == null)
        {
            return Unauthorized("failed to extract user id");
        }
        var friends = friendService.GetAllFriends(userId.Value);
        return Ok(new { friends });
    }

    [HttpGet]
    [Route("requests")]
    [Authorize]
    public IActionResult GetAllFriendRequests()
    {
        var userId = userContext.ExtractUserId();
        if (userId == null)
        {
            return Unauthorized("failed to extract user id");
        }
        var friendRequests = friendService.GetAllFriendRequests(userId.Value);
        return Ok(new { friend_requests = friendRequests });
    }

    [HttpPut]
    [Route("requests/{id}")]
    [Authorize]
    public IActionResult RespondFriendRequest([FromBody] FriendRequestResponse friendRequestResponse, uint id)
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

    [HttpGet]
    [Route("search")]
    [Authorize]
    public IActionResult SearchUser([FromQuery] string email)
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

    [HttpPost]
    [Route("requests")]
    [Authorize]
    public IActionResult SendFriendRequest([FromBody] FriendRequestCreation request)
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
}