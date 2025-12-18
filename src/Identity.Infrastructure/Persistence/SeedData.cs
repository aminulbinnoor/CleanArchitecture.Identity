using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Identity.Domain.Entities;
using Identity.Infrastructure.Services;

namespace Identity.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = new PasswordHasher(); // Create instance directly
        
        await context.Database.MigrateAsync();
        
        if (!await context.Roles.AnyAsync())
        {
            var roles = new List<Role>
            {
                new Role { Id = Guid.NewGuid(), Name = "Admin", Description = "Administrator with full access" },
                new Role { Id = Guid.NewGuid(), Name = "Manager", Description = "Manager with limited admin access" },
                new Role { Id = Guid.NewGuid(), Name = "User", Description = "Regular user" }
            };
            
            await context.Roles.AddRangeAsync(roles);
        }
        
        if (!await context.Permissions.AnyAsync())
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
            
            await context.Permissions.AddRangeAsync(permissions);
        }
        
        await context.SaveChangesAsync();
        
        // Assign permissions to Admin role
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        var allPermissions = await context.Permissions.ToListAsync();
        
        if (adminRole != null && !await context.RolePermissions.AnyAsync())
        {
            foreach (var permission in allPermissions)
            {
                context.RolePermissions.Add(new RolePermission
                {
                    RoleId = adminRole.Id,
                    PermissionId = permission.Id
                });
            }
            
            await context.SaveChangesAsync();
        }
        
        // Create default admin user
        if (!await context.Users.AnyAsync(u => u.Email == "admin@example.com"))
        {
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@example.com",
                PasswordHash = passwordHasher.HashPassword("Admin@123"), // Fixed: Only one parameter
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            
            await context.Users.AddAsync(adminUser);
            await context.SaveChangesAsync();
            
            // Assign admin role
            if (adminRole != null)
            {
                context.UserRoles.Add(new UserRole
                {
                    UserId = adminUser.Id,
                    RoleId = adminRole.Id
                });
                
                await context.SaveChangesAsync();
            }
        }
    }
}