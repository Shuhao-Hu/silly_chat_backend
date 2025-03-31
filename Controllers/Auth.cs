using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SillyChatBackend.DTOs;
using SillyChatBackend.Services;
using SillyChatBackend.Utils;

namespace SillyChatBackend.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IAuthService authService, IUserContext userContext, ITokenService tokenService, ILogger<AuthController> logger) : ControllerBase
{

    [HttpPost("login")]
    public IActionResult Login([FromBody] AuthData authData)
    {
        try
        {
            if (string.IsNullOrEmpty(authData.Email) || string.IsNullOrEmpty(authData.Password))
            {
                return BadRequest();
            }
            var result = authService.Login(authData);
            if (!result.Success)
            {
                return NotFound(result);
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("signup")]
    public IActionResult Signup([FromBody] AuthData authData)
    {
        try
        {
            if (string.IsNullOrEmpty(authData.Username) || string.IsNullOrEmpty(authData.Email) || string.IsNullOrEmpty(authData.Password))
            {
                return BadRequest();
            }
            var result = authService.Signup(authData);
            if (!result.Success)
            {
                return Conflict(new { error = result.Message });
            }
            return Created();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] RefreshRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.RefreshToken))
            {
                return BadRequest();
            }
            var result = tokenService.RefreshTokens(request.RefreshToken);
            if (result == null)
            {
                return Unauthorized();
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPut("username")]
    [Authorize]
    public IActionResult UpdateUsername([FromBody] UsernameUpdate username)
    {
        try
        {
            var userId = userContext.ExtractUserId();
            if (userId == null)
            {
                return Unauthorized();
            }
            
            if (string.IsNullOrEmpty(username.Username))
            {
                return BadRequest();
            }

            return authService.UpdateUsername(userId.Value, username.Username) ? Ok() : Unauthorized();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}