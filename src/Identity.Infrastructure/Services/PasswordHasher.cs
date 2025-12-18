using System;
using System.Security.Cryptography;
using System.Text;
using Identity.Domain.Entities;

namespace Identity.Infrastructure.Services;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string hashedPassword, string providedPassword);
}

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
    
    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        var providedHash = HashPassword(providedPassword);
        return hashedPassword == providedHash;
    }
}