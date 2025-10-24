using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace CaixaSeguradora.Infrastructure.Services;

/// <summary>
/// T137 [Security] - JWT Token Service
/// Generates and validates JWT tokens for authentication
/// </summary>
public interface IJwtTokenService
{
    string GenerateToken(string userId, string[] roles, int expirationMinutes);
    ClaimsPrincipal? ValidateToken(string token);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtTokenService> _logger;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtTokenService(
        IConfiguration configuration,
        ILogger<JwtTokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        _secretKey = _configuration["JWT:SecretKey"]
            ?? throw new InvalidOperationException("JWT:SecretKey not configured");
        _issuer = _configuration["JWT:Issuer"] ?? "CaixaSeguradoraAPI";
        _audience = _configuration["JWT:Audience"] ?? "CaixaSeguradoraClient";

        // Ensure secret key is strong enough (min 32 characters)
        if (_secretKey.Length < 32)
        {
            throw new InvalidOperationException("JWT:SecretKey must be at least 32 characters");
        }
    }

    public string GenerateToken(string userId, string[] roles, int expirationMinutes)
    {
        _logger.LogInformation("Generating JWT token for user: {UserId}", userId);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, userId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        // Add roles
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogInformation("JWT token generated for user {UserId}, expires in {Minutes} minutes",
            userId, expirationMinutes);

        return tokenString;
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // No clock skew tolerance
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            // Additional validation: ensure token is JWT and uses HMAC SHA256
            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogWarning("Invalid token algorithm");
                return null;
            }

            _logger.LogDebug("Token validated successfully for user: {UserId}",
                principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            return principal;
        }
        catch (SecurityTokenExpiredException)
        {
            _logger.LogWarning("Token validation failed: Token expired");
            return null;
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Token validation failed: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token validation");
            return null;
        }
    }
}
