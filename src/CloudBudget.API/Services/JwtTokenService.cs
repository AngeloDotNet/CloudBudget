using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CloudBudget.API.Entities;
using CloudBudget.API.Services.Interfaces;
using CloudBudget.API.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CloudBudget.API.Services;

public class JwtTokenService(IOptions<JwtSettings> opts, UserManager<ApplicationUser> userManager) : IJwtTokenService
{
    private readonly JwtSettings settings = opts.Value;

    public async Task<(string Token, DateTime ExpiresAtUtc)> GenerateTokenAsync(ApplicationUser user)
    {
        var expires = DateTime.UtcNow.AddMinutes(settings.ExpireMinutes);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("uid", user.Id.ToString())
        };

        var roles = await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var userClaims = await userManager.GetClaimsAsync(user);
        if (userClaims != null && userClaims.Any())
        {
            claims.AddRange(userClaims);
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return (tokenString, expires);
    }
}