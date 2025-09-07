using DomainUser = CusomMapOSM_Domain.Entities.Users;
using Optional;
using ErrorCustom = CusomMapOSM_Application.Common.Errors;

namespace CusomMapOSM_Application.Interfaces.Features.User;

public interface IUserService
{
    Task<Option<DomainUser.User, ErrorCustom.Error>> GetUserByIdAsync(Guid userId, CancellationToken ct);
    Task<Option<DomainUser.User, ErrorCustom.Error>> GetUserByEmailAsync(string email, CancellationToken ct);
}
