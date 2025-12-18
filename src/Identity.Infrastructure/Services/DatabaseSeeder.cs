using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Services;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    
    public DatabaseSeeder(
        ApplicationDbContext context,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }
    
    public async Task SeedAsync()
    {
        await _context.Database.MigrateAsync();
        
        // Seed roles
        if (!await _context.Roles.AnyAsync())
        {
            var roles = new List<Role>
            {
                new Role { Id = Guid.NewGuid(), Name = "Admin", Description = "Administrator with full access" },
                new Role { Id = Guid.NewGuid(), Name = "Manager", Description = "Manager with limited admin access" },
                new Role { Id = Guid.NewGuid(), Name = "User", Description = "Regular user" }
            };
            
            await _context.Roles.AddRangeAsync(roles);
            await _context.SaveChangesAsync();
        }
        
        // Seed permissions
        if (!await _context.Permissions.AnyAsync())
        {
            var permissions = new List<Permission>
            {
                new() { Id = Guid.NewGuid(), Name = "users.read", Description = "Read users" },
                new() { Id = Guid.NewGuid(), Name = "users.create", Description = "Create users" },
                new() { Id = Guid.NewGuid(), Name = "users.update", Description = "Update users" },
                new() { Id = Guid.NewGuid(), Name = "users.delete", Description = "Delete users" },
                new() { Id = Guid.NewGuid(), Name = "roles.manage", Description = "Manage roles" },
                new() { Id = Guid.NewGuid(), Name = "settings.read", Description = "Read settings" },
                new() { Id = Guid.NewGuid(), Name = "settings.write", Description = "Write settings" }
            };
            
            await _context.Permissions.AddRangeAsync(permissions);
            await _context.SaveChangesAsync();
        }
        
        // Assign all permissions to Admin role
        var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        var allPermissions = await _context.Permissions.ToListAsync();
        
        if (adminRole != null && !await _context.RolePermissions.AnyAsync())
        {
            foreach (var permission in allPermissions)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = permission.Id
                });
            }
            
            await _context.SaveChangesAsync();
        }
        
        // Create default admin user
        if (!await _context.Users.AnyAsync(u => u.Email == "admin@example.com"))
        {
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@example.com",
                PasswordHash = _passwordHasher.HashPassword("Admin@123"),
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            await _context.Users.AddAsync(adminUser);
            await _context.SaveChangesAsync();
            
            // Assign admin role
            if (adminRole != null)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                });
                
                await _context.SaveChangesAsync();
            }
        }
        
        // Create test manager user
        if (!await _context.Users.AnyAsync(u => u.Email == "manager@example.com"))
        {
            var managerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Manager");
            var managerUser = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Manager",
                LastName = "User",
                Email = "manager@example.com",
                PasswordHash = _passwordHasher.HashPassword("Manager@123"),
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            await _context.Users.AddAsync(managerUser);
            await _context.SaveChangesAsync();
            
            // Assign manager role
            if (managerRole != null)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = managerUser.Id,
                    RoleId = managerRole.Id
                });
                
                await _context.SaveChangesAsync();
            }
        }
        
        // Create test regular user
        if (!await _context.Users.AnyAsync(u => u.Email == "user@example.com"))
        {
            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            var regularUser = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Regular",
                LastName = "User",
                Email = "user@example.com",
                PasswordHash = _passwordHasher.HashPassword("User@123"),
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            await _context.Users.AddAsync(regularUser);
            await _context.SaveChangesAsync();
            
            // Assign user role
            if (userRole != null)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = regularUser.Id,
                    RoleId = userRole.Id
                });
                
                await _context.SaveChangesAsync();
            }
        }
    }
}