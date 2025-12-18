using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // User configuration
        builder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.Property(e => e.PasswordHash).IsRequired();
        });
        
        // Role configuration
        builder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            
            entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Description).HasMaxLength(500);
        });
        
        // Permission configuration
        builder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
        });
        
        // UserRole composite key
        builder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId });
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Role)
                .WithMany()
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // RolePermission composite key
        builder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => new { e.RoleId, e.PermissionId });
            
            entity.HasOne(e => e.Role)
                .WithMany()
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Permission)
                .WithMany()
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // RefreshToken configuration
        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token);
            entity.HasIndex(e => e.UserId);
            
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}