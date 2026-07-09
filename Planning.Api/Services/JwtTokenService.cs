using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Planning.Application.Common;
using Planning.Domain.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Planning.Api.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(TokenUserContext context)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT signing key is not configured.");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, (context.UserId ?? context.AccountId).ToString()),
            new("accountId", context.AccountId.ToString()),
            new(JwtRegisteredClaimNames.Email, context.Email)
        };

        if (context.OrganizationId is Guid organizationId)
        {
            claims.Add(new Claim("organizationId", organizationId.ToString()));
        }

        if (context.UserId is Guid userId)
        {
            claims.Add(new Claim("userId", userId.ToString()));
        }

        if (context.Role is UserRole role)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
        }

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var expirationMinutes = int.TryParse(jwtSettings["ExpirationMinutes"], out var minutes)
            ? minutes
            : 60;

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
