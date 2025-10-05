using DomainUser = CusomMapOSM_Domain.Entities.Users;
using CusomMapOSM_Application.Models.DTOs.Features.User;
using Optional;
using ErrorCustom = CusomMapOSM_Application.Common.Errors;

namespace CusomMapOSM_Application.Interfaces.Features.User;

public interface IUserService
{
    Task<Option<DomainUser.User, ErrorCustom.Error>> GetUserByIdAsync(Guid userId, CancellationToken ct);
    Task<Option<DomainUser.User, ErrorCustom.Error>> GetUserByEmailAsync(string email, CancellationToken ct);
    Task<Option<UpdateUserPersonalInfoResponse, ErrorCustom.Error>> UpdateUserPersonalInfoAsync(Guid userId, UpdateUserPersonalInfoRequest request, CancellationToken ct);
}
