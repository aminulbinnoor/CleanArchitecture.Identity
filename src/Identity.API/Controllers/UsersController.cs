using AutoMapper;
using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Application.Features.Users.Commands.UpdateUserRoles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MediatR;
using Identity.Domain.Entities;
namespace Identity.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;
    
    public UsersController(
        IMediator mediator,
        IIdentityService identityService,
        IMapper mapper)
    {
        _identityService = identityService;
        _mapper = mapper;
        _mediator = mediator;
    }
    
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            Console.WriteLine("=== GetCurrentUser Debug ===");
            
            // METHOD 1: Get ALL claims first for debugging
            var allClaims = User.Claims.ToList();
            Console.WriteLine($"Total claims: {allClaims.Count}");
            
            foreach (var claim in allClaims)
            {
                Console.WriteLine($"Claim Type: '{claim.Type}', Value: '{claim.Value}'");
            }
            
            // METHOD 2: Try multiple ways to get user ID
            var userId = User.FindFirstValue("sub") ??  // Try "sub" directly
                        User.FindFirstValue(ClaimTypes.NameIdentifier) ??  // Try NameIdentifier
                        User.FindFirstValue("userId") ??  // Try custom claim
                        User.FindFirstValue("userGuid");  // Try another custom claim
            
            Console.WriteLine($"User ID found: {userId ?? "NULL"}");
            
            if (string.IsNullOrEmpty(userId))
            {
                // If no user ID found, try email as last resort
                var email = User.FindFirstValue(ClaimTypes.Email) ?? 
                        User.FindFirstValue("email");
                
                Console.WriteLine($"Trying email fallback: {email}");
                
                if (!string.IsNullOrEmpty(email))
                {
                    var userByEmail = await _identityService.GetUserByEmailAsync(email);
                    if (userByEmail != null)
                    {
                        return await ReturnUserData(userByEmail);
                    }
                }
                
                return Unauthorized(new 
                { 
                    Message = "User ID not found in token",
                    AvailableClaims = allClaims.Select(c => new { c.Type, c.Value })
                });
            }
            
            // Try to parse as Guid
            if (Guid.TryParse(userId, out var userGuid))
            {
                Console.WriteLine($"Parsed as GUID: {userGuid}");
                
                var user = await _identityService.GetUserByIdAsync(userGuid);
                if (user == null)
                {
                    Console.WriteLine($"User not found in database: {userGuid}");
                    return NotFound(new { message = "User not found" });
                }
                
                Console.WriteLine($"User found: {user.Email}");
                return await ReturnUserData(user);
            }
            else
            {
                Console.WriteLine($"User ID is not a valid GUID: {userId}");
                return Unauthorized(new { message = "Invalid user ID format" });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetCurrentUser: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred", error = ex.Message });
        }
    }

    private async Task<IActionResult> ReturnUserData(User user)
    {
        var userDto = _mapper.Map<UserDto>(user);
        userDto.Roles = await _identityService.GetUserRolesAsync(user.Id);
        userDto.Permissions = await _identityService.GetUserPermissionsAsync(user.Id);
        
        Console.WriteLine($"Returning user data for: {user.Email}");
        return Ok(userDto);
    }
    
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _identityService.GetAllUsersAsync();
        var userDtos = new List<UserDto>();
        
        foreach (var user in users)
        {
            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = await _identityService.GetUserRolesAsync(user.Id);
            userDto.Permissions = await _identityService.GetUserPermissionsAsync(user.Id);
            userDtos.Add(userDto);
        }
        
        return Ok(userDtos);
    }
    
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _identityService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound(new { message = "User not found" });
        
        var userDto = _mapper.Map<UserDto>(user);
        userDto.Roles = await _identityService.GetUserRolesAsync(user.Id);
        userDto.Permissions = await _identityService.GetUserPermissionsAsync(user.Id);
        
        return Ok(userDto);
    }
    
    [HttpPut("{id}/roles")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateUserRoles(Guid id, [FromBody] UpdateUserRolesRequestDto request)
    {
        var command = new UpdateUserRolesCommand
        {
            UserId = id,
            Roles = request.Roles
        };
        
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            
            return Ok(new { message = "User roles updated successfully" });
        }
        
        return BadRequest(new { message = result.Error });
    }
    
    [HttpGet("check-permission/{permission}")]
    [Authorize(Policy = "UserManagement")]
    public IActionResult CheckPermission(string permission)
    {
        var hasPermission = User.HasClaim("permission", permission);
        
        return Ok(new
        {
            hasPermission,
            permission,
            user = User.FindFirstValue("sub"),
            allPermissions = User.FindAll("permission").Select(c => c.Value).ToList()
        });
    }
}

public class UpdateRolesRequest
{
    public List<string> Roles { get; set; } = new();
}