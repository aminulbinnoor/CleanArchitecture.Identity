using Identity.Application.DTOs;
using Identity.Application.Features.Auth.Commands.Login;
using Identity.Application.Features.Auth.Commands.Logout;
using Identity.Application.Features.Auth.Commands.RefreshToken;
using Identity.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Identity.Application.Interfaces;
using Identity.Infrastructure.Services;

namespace Identity.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;
    private readonly IIdentityService _identityService;
    
    public AuthController(IMediator mediator, ILogger<AuthController> logger, IIdentityService identityService)
    {
        _mediator = mediator;
        _logger = logger;
        _identityService = identityService;
    }
    
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var command = new RegisterCommand
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Password = request.Password
        };
        
        var result = await _mediator.Send(command);
        
        return result.IsSuccess 
            ? Ok(result.Value) 
            : BadRequest(new { message = result.Error });
    }
    
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password
        };
        
        var result = await _mediator.Send(command);
        
        return result.IsSuccess 
            ? Ok(result.Value) 
            : Unauthorized(new { message = result.Error });
    }
    
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            // Get user ID from claims (use NameIdentifier as it's mapped from "sub")
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
                return Unauthorized(new { message = "Invalid user token" });
            
            // Get refresh token from header
            var refreshToken = Request.Headers["Refresh-Token"].ToString();
            
            if (string.IsNullOrEmpty(refreshToken))
                return BadRequest(new { message = "Refresh token required" });
            
            // Remove the refresh token
            var refreshTokenResult = await _identityService.RevokeRefreshTokenAsync(userGuid, refreshToken);
            
            if (refreshTokenResult == null)
                return BadRequest(new { message = "Invalid refresh token" });
            
            return Ok(new 
            { 
                message = "Logged out successfully",
                logoutTime = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Logout failed");
            return StatusCode(500, new { message = "Logout failed", error = ex.Message });
        }
    }
    
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var command = new RefreshTokenCommand
        {
            AccessToken = request.AccessToken,
            RefreshToken = request.RefreshToken
        };
        
        var result = await _mediator.Send(command);
        
        return result.IsSuccess 
            ? Ok(result.Value) 
            : BadRequest(new { message = result.Error });
    }
    
    [HttpGet("test")]
    [Authorize]
    public IActionResult TestAuth()
    {
        var userId = User.FindFirstValue("sub");
        var email = User.FindFirstValue(ClaimTypes.Email);
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
        var permissions = User.FindAll("permission").Select(c => c.Value).ToList();
        
        return Ok(new
        {
            message = "You are authenticated!",
            userId,
            email,
            roles,
            permissions,
            claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }
    
    [HttpGet("test-admin")]
    [Authorize(Roles = "Admin")]
    public IActionResult TestAdmin()
    {
        return Ok(new { message = "You are an Admin!" });
    }
    
    [HttpGet("test-manager")]
    [Authorize(Roles = "Manager")]
    public IActionResult TestManager()
    {
        return Ok(new { message = "You are a Manager!" });
    }
    
    [HttpGet("test-user")]
    [Authorize(Roles = "User")]
    public IActionResult TestUser()
    {
        return Ok(new { message = "You are a User!" });
    }
}