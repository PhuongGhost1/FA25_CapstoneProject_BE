namespace CusomMapOSM_Application.Interfaces.Services.Jwt;

public interface IJwtService
{
    string GenerateToken(Guid userId, string email, int exp);
    string HashObject<T>(T obj);
    // string GenerateRefreshToken(); optional if using refresh token
}