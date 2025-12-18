using Identity.Application.DTOs;
using Identity.Application.Features.Roles.Commands.AssignPermissions;
using Identity.Application.Features.Roles.Commands.CreateRole;
using Identity.Application.Features.Roles.Commands.UpdateRole;
using Identity.Application.Features.Users.Commands.UpdateUserRoles;
using Identity.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IRoleService _roleService;
    private readonly ILogger<RolesController> _logger;
    
    public RolesController(
        IMediator mediator,
        IRoleService roleService,
        ILogger<RolesController> logger)
    {
        _mediator = mediator;
        _roleService = roleService;
        _logger = logger;
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(List<RoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRoles()
    {
        var roles = await _roleService.GetAllRolesAsync();
        return Ok(roles);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoleById(Guid id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        
        if (role == null)
            return NotFound(new { message = "Role not found" });
        
        return Ok(role);
    }
    
    [HttpGet("name/{name}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoleByName(string name)
    {
        var role = await _roleService.GetRoleByNameAsync(name);
        
        if (role == null)
            return NotFound(new { message = $"Role '{name}' not found" });
        
        return Ok(role);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequestDto request)
    {
        var command = new CreateRoleCommand
        {
            Name = request.Name,
            Description = request.Description
        };
        
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            _logger.LogInformation("Role created: {RoleName}", request.Name);
            return CreatedAtAction(nameof(GetRoleById), new { id = result.Value.Id }, result.Value);
        }
        
        return BadRequest(new { message = result.Error });
    }
    
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequestDto request)
    {
        var command = new UpdateRoleCommand
        {
            RoleId = id,
            Description = request.Description
        };
        
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
            return Ok(result.Value);
        
        return result.Error.Contains("not found") 
            ? NotFound(new { message = result.Error }) 
            : BadRequest(new { message = result.Error });
    }
    
    [HttpPost("{id}/permissions")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignPermissions(Guid id, [FromBody] AssignPermissionsRequestDto request)
    {
        var command = new AssignPermissionsCommand
        {
            RoleId = id,
            Permissions = request.Permissions
        };
        
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            _logger.LogInformation("Permissions assigned to role {RoleId}", id);
            return Ok(result.Value);
        }
        
        return BadRequest(new { message = result.Error });
    }
    
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        var result = await _roleService.DeleteRoleAsync(id);
        
        if (result.IsSuccess)
            return Ok(new { message = "Role deleted successfully" });
        
        return BadRequest(new { message = result.Error });
    }
    
    [HttpGet("permissions")]
    [ProducesResponseType(typeof(List<PermissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPermissions()
    {
        var permissions = await _roleService.GetAllPermissionsAsync();
        return Ok(permissions);
    }
    
    [HttpGet("{roleName}/users")]
    [ProducesResponseType(typeof(List<UserWithRolesDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsersByRole(string roleName)
    {
        var users = await _roleService.GetUsersByRoleAsync(roleName);
        return Ok(users);
    }
}