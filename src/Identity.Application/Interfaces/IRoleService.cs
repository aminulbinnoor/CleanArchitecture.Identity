using Identity.Application.DTOs;
using Identity.Domain.Common;

namespace Identity.Application.Interfaces;

public interface IRoleService
{
    Task<Result<RoleDto>> CreateRoleAsync(string name, string description, List<string> permissions);
    Task<Result<RoleDto>> UpdateRoleAsync(Guid roleId, string? description);
    Task<Result<RoleDto>> AssignPermissionsAsync(Guid roleId, List<string> permissions);
    Task<Result> DeleteRoleAsync(Guid roleId);
    Task<RoleDto?> GetRoleByIdAsync(Guid roleId);
    Task<RoleDto?> GetRoleByNameAsync(string name);
    Task<List<RoleDto>> GetAllRolesAsync();
    Task<List<PermissionDto>> GetAllPermissionsAsync();
    Task<List<UserWithRolesDto>> GetUsersByRoleAsync(string roleName);
}