using Identity.Application.DTOs;
using Identity.Application.Interfaces;
using Identity.Domain.Common;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Identity.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RoleService> _logger;
    
    public RoleService(ApplicationDbContext context, ILogger<RoleService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<Result<RoleDto>> CreateRoleAsync(string name, string description, List<string> permissions)
    {
        // Check if role already exists
        var existingRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == name);
        
        if (existingRole != null)
            return Result.Failure<RoleDto>($"Role '{name}' already exists");
        
        // Create new role
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description
        };
        
        await _context.Roles.AddAsync(role);
        
        // Assign permissions
        var permissionEntities = await _context.Permissions
            .Where(p => permissions.Contains(p.Name))
            .ToListAsync();
        
        foreach (var permission in permissionEntities)
        {
            _context.RolePermissions.Add(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permission.Id
            });
        }
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Role created: {RoleName}", name);
        
        return Result.Success(await MapToRoleDto(role));
    }
    
    public async Task<Result<RoleDto>> UpdateRoleAsync(Guid roleId, string? description)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == roleId);
        
        if (role == null)
            return Result.Failure<RoleDto>("Role not found");
        
        if (!string.IsNullOrEmpty(description))
        {
            role.Description = description;
        }
        
        await _context.SaveChangesAsync();
        
        return Result.Success(await MapToRoleDto(role));
    }
    
    public async Task<Result<RoleDto>> AssignPermissionsAsync(Guid roleId, List<string> permissions)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == roleId);
        
        if (role == null)
            return Result.Failure<RoleDto>("Role not found");
        
        // Remove existing permissions
        var existingPermissions = _context.RolePermissions
            .Where(rp => rp.RoleId == roleId);
        
        _context.RolePermissions.RemoveRange(existingPermissions);
        
        // Add new permissions
        var permissionEntities = await _context.Permissions
            .Where(p => permissions.Contains(p.Name))
            .ToListAsync();
        
        foreach (var permission in permissionEntities)
        {
            _context.RolePermissions.Add(new RolePermission
            {
                RoleId = roleId,
                PermissionId = permission.Id
            });
        }
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Permissions updated for role: {RoleName}", role.Name);
        
        return Result.Success(await MapToRoleDto(role));
    }
    
    public async Task<Result> DeleteRoleAsync(Guid roleId)
    {
        var role = await _context.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == roleId);
        
        if (role == null)
            return Result.Failure("Role not found");
        
        // Check if any users have this role
        if (role.UserRoles.Any())
            return Result.Failure("Cannot delete role that is assigned to users");
        
        // Remove role permissions
        var rolePermissions = _context.RolePermissions
            .Where(rp => rp.RoleId == roleId);
        
        _context.RolePermissions.RemoveRange(rolePermissions);
        
        // Remove role
        _context.Roles.Remove(role);
        
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Role deleted: {RoleName}", role.Name);
        
        return Result.Success();
    }
    
    public async Task<RoleDto?> GetRoleByIdAsync(Guid roleId)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == roleId);
        
        if (role == null)
            return null;
        
        return await MapToRoleDto(role);
    }
    
    public async Task<RoleDto?> GetRoleByNameAsync(string name)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Name == name);
        
        if (role == null)
            return null;
        
        return await MapToRoleDto(role);
    }
    
    public async Task<List<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _context.Roles
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .ToListAsync();
        
        var roleDtos = new List<RoleDto>();
        
        foreach (var role in roles)
        {
            roleDtos.Add(await MapToRoleDto(role));
        }
        
        return roleDtos;
    }
    
    public async Task<List<PermissionDto>> GetAllPermissionsAsync()
    {
        return await _context.Permissions
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description
            })
            .ToListAsync();
    }
    
    public async Task<List<UserWithRolesDto>> GetUsersByRoleAsync(string roleName)
    {
        var users = await _context.UserRoles
            .Where(ur => ur.Role.Name == roleName)
            .Select(ur => ur.User)
            .ToListAsync();
        
        var userDtos = new List<UserWithRolesDto>();
        
        foreach (var user in users)
        {
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Select(ur => ur.Role.Name)
                .ToListAsync();
            
            var userPermissions = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToListAsync();
            
            userDtos.Add(new UserWithRolesDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Roles = userRoles,
                Permissions = userPermissions
            });
        }
        
        return userDtos;
    }
    
    private async Task<RoleDto> MapToRoleDto(Role role)
    {
        var permissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == role.Id)
            .Select(rp => rp.Permission.Name)
            .ToListAsync();
        
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            Permissions = permissions
        };
    }
}