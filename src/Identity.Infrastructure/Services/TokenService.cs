using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Identity.Application.Interfaces;
using Identity.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IIdentityService _identityService;
    
    public TokenService(
        IOptions<JwtSettings> jwtSettings,
        IIdentityService identityService)
    {
        _jwtSettings = jwtSettings.Value;
        _identityService = identityService;
    }
    
    public async Task<TokenResult> GenerateTokenAsync(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
        
        // Create claims - ADD BOTH "sub" AND NameIdentifier
        var claims = new List<Claim>
        {
            // Standard JWT claim
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            
            // ASP.NET Core expects this
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            
            // Email claims
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Email, user.Email),
            
            // Unique token ID
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            
            // Custom claims for easy access
            new Claim("userId", user.Id.ToString()),
            new Claim("userGuid", user.Id.ToString()),
            
            // User info
            new Claim("firstName", user.FirstName ?? ""),
            new Claim("lastName", user.LastName ?? ""),
            //new Claim("username", user.UserName ?? "")
        };
        
        // Add roles
        var roles = await _identityService.GetUserRolesAsync(user.Id);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
            claims.Add(new Claim("role", role)); // Custom role claim
        }
        
        // Add permissions
        var permissions = await _identityService.GetUserPermissionsAsync(user.Id);
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }
        
        // Log claims for debugging
        Console.WriteLine("\n=== GENERATING TOKEN ===");
        foreach (var claim in claims)
        {
            Console.WriteLine($"Adding claim: {claim.Type} = {claim.Value}");
        }
        Console.WriteLine("=== END TOKEN CLAIMS ===\n");
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.TokenExpirationInMinutes),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience
        };
        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var accessToken = tokenHandler.WriteToken(token);
        
        // DEBUG: Decode and show the generated token
        Console.WriteLine("\n=== DECODED GENERATED TOKEN ===");
        var decodedToken = tokenHandler.ReadJwtToken(accessToken);
        foreach (var claim in decodedToken.Claims)
        {
            Console.WriteLine($"Token contains: {claim.Type} = {claim.Value}");
        }
        Console.WriteLine("=== END DECODED TOKEN ===\n");
        
        var refreshToken = GenerateRefreshToken();
        
        await _identityService.AddRefreshTokenAsync(
            user.Id, 
            refreshToken, 
            DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays));
        
        return new TokenResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = tokenDescriptor.Expires.Value
        };
    }
    
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
            ValidateLifetime = false
        };
        
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            
            if (securityToken is not JwtSecurityToken jwtSecurityToken || 
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                return null;
            
            return principal;
        }
        catch
        {
            return null;
        }
    }
    
    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public int TokenExpirationInMinutes { get; set; } = 60;
    public int RefreshTokenExpirationInDays { get; set; } = 7;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}