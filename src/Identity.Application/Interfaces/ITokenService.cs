using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Identity.Domain.Entities;

namespace Identity.Application.Interfaces;

public class TokenResult
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public interface ITokenService
{
    Task<TokenResult> GenerateTokenAsync(User user);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    string GenerateRefreshToken();
}