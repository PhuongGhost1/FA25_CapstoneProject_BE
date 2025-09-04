using CusomMapOSM_Application.Interfaces.Services.Jwt;
using CusomMapOSM_Application.Common.ServiceConstants;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CusomMapOSM_Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly byte[] _key;
    private readonly JsonWebTokenHandler _handler;

    public JwtService()
    {
        var SecretKey = JwtConstant.JWT_SECRET_KEY;
        _key = Encoding.UTF8.GetBytes(SecretKey);
        _handler = new JsonWebTokenHandler();
    }

    // public string GenerateRefreshToken()
    // {
    //     return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    // }

    public string GenerateToken(Guid userId, string email, int exp)
    {
        var key = new SymmetricSecurityKey(_key);

        var claims = new Dictionary<string, object>
        {
            [ClaimTypes.Email] = email,
            [ClaimTypes.NameIdentifier] = userId.ToString(),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Claims = claims,
            IssuedAt = null,
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddSeconds(exp),
            Issuer = null,
            Audience = null,
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
        };

        _handler.SetDefaultTimesOnTokenCreation = false;

        var tokenString = _handler.CreateToken(tokenDescriptor);
        return tokenString;
    }

    public string HashObject<T>(T obj)
    {
        string json = JsonConvert.SerializeObject(obj);

        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            byte[] hashBytes = sha256.ComputeHash(bytes);

            StringBuilder hashString = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                hashString.Append(b.ToString("x2"));
            }

            return hashString.ToString();
        }
    }
}
