using Microsoft.AspNetCore.Mvc;
using SillyChatBackend.DTOs;
using SillyChatBackend.Services;

namespace SillyChatBackend.Controllers;

[ApiController]
[Route("auth")]
public class AuthController(IAuthService authService, ITokenService tokenService) : ControllerBase
{

    [HttpPost("login")]
    public IActionResult Login([FromBody] AuthData authData)
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

    [HttpPost("signup")]
    public IActionResult Signup([FromBody] AuthData authData)
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

    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] RefreshRequest request)
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
}