using CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Authentication;

public interface IAuthenticationRepository
{
    Task<bool> IsEmailExists(string email);
    Task<User?> Login(string email, string pwd);
    Task<bool> Register(User user);
    Task<User?> GetUserById(Guid? userId);
    Task<User?> GetUserByEmail(string email);
    Task<bool> UpdateUser(User user);
}