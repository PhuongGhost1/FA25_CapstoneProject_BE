using DomainUser = CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Authentication;

public interface IAuthenticationRepository
{
    Task<bool> IsEmailExists(string email);
    Task<DomainUser.User?> Login(string email, string pwd);
    Task<bool> Register(DomainUser.User user);
    Task<DomainUser.User?> GetUserById(Guid? userId);
    Task<DomainUser.User?> GetUserByEmail(string email);
    Task<bool> UpdateUser(DomainUser.User user);
}