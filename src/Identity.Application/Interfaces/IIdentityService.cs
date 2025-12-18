using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Identity.Domain.Common;
using Identity.Domain.Entities;

namespace Identity.Application.Interfaces;

public interface IIdentityService
{
    Task<Result> RegisterAsync(string firstName, string lastName, string email, string password);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(Guid id);
    Task<bool> CheckPasswordAsync(User user, string password); // Fixed: Takes User and password
    Task<List<string>> GetUserRolesAsync(Guid userId);
    Task<List<string>> GetUserPermissionsAsync(Guid userId);
    Task<Result> RevokeRefreshTokenAsync(Guid userId, string refreshToken);
    Task<Result> AddRefreshTokenAsync(Guid userId, string refreshToken, DateTime expires);
    Task<RefreshToken?> GetRefreshTokenAsync(string token);
    Task<Result> UpdateUserRolesAsync(Guid userId, List<string> roles);
    Task<List<User>> GetAllUsersAsync();
}