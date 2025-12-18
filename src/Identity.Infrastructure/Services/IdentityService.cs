using Identity.Application.Interfaces;
using Identity.Domain.Common;
using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    
    public IdentityService(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }
    
    public async Task<Result> RegisterAsync(
        string firstName, 
        string lastName, 
        string email, 
        string password)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
        
        if (existingUser != null)
            return Result.Failure("User with this email already exists");
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PasswordHash = _passwordHasher.HashPassword(password), // Fixed: Only one parameter
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            EmailConfirmed = true
        };
        
        await _context.Users.AddAsync(user);
        
        // Assign default User role
        var userRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == "User");
        
        if (userRole != null)
        {
            var userRoleEntity = new UserRole
            {
                UserId = user.Id,
                RoleId = userRole.Id
            };
            await _context.UserRoles.AddAsync(userRoleEntity);
        }
        
        await _context.SaveChangesAsync();
        
        return Result.Success();
    }
    
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }
    
    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }
    
    public async Task<bool> CheckPasswordAsync(User user, string password)
    {
        return _passwordHasher.VerifyPassword(user.PasswordHash, password); // Fixed: VerifyPassword takes two params
    }
    
    public async Task<List<string>> GetUserRolesAsync(Guid userId)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_context.Roles,
                ur => ur.RoleId,
                r => r.Id,
                (ur, r) => r.Name)
            .ToListAsync();
    }
    
    public async Task<List<string>> GetUserPermissionsAsync(Guid userId)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_context.RolePermissions,
                ur => ur.RoleId,
                rp => rp.RoleId,
                (ur, rp) => rp)
            .Join(_context.Permissions,
                rp => rp.PermissionId,
                p => p.Id,
                (rp, p) => p.Name)
            .Distinct()
            .ToListAsync();
    }
    
    public async Task<Result> RevokeRefreshTokenAsync(Guid userId, string refreshToken)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.UserId == userId && rt.Token == refreshToken);
        
        if (token == null)
            return Result.Failure("Refresh token not found");
        
        token.Revoked = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        return Result.Success();
    }
    
    public async Task<Result> AddRefreshTokenAsync(Guid userId, string refreshToken, DateTime expires)
    {
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshToken,
            UserId = userId,
            Expires = expires,
            Created = DateTime.UtcNow
        };
        
        await _context.RefreshTokens.AddAsync(token);
        await _context.SaveChangesAsync();
        
        return Result.Success();
    }
    
    public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }
    
    public async Task<Result> UpdateUserRolesAsync(Guid userId, List<string> roles)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null)
            return Result.Failure("User not found");
        
        var existingUserRoles = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .ToListAsync();
        
        _context.UserRoles.RemoveRange(existingUserRoles);
        
        var roleEntities = await _context.Roles
            .Where(r => roles.Contains(r.Name))
            .ToListAsync();
        
        foreach (var role in roleEntities)
        {
            _context.UserRoles.Add(new UserRole
            {
                UserId = userId,
                RoleId = role.Id
            });
        }
        
        await _context.SaveChangesAsync();
        
        return Result.Success();
    }
    
    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }
}