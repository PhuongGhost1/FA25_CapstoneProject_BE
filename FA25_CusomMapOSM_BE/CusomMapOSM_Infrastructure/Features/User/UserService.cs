using CusomMapOSM_Application.Interfaces.Features.User;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Authentication;
using Optional;
using ErrorCustom = CusomMapOSM_Application.Common.Errors;
using DomainUser = CusomMapOSM_Domain.Entities.Users;

namespace CusomMapOSM_Infrastructure.Features.User;

public class UserService : IUserService
{
    private readonly IAuthenticationRepository _authenticationRepository;

    public UserService(IAuthenticationRepository authenticationRepository)
    {
        _authenticationRepository = authenticationRepository;
    }

    public async Task<Option<DomainUser.User, ErrorCustom.Error>> GetUserByIdAsync(Guid userId, CancellationToken ct)
    {
        try
        {
            var user = await _authenticationRepository.GetUserById(userId);
            return user != null
                ? Option.Some<DomainUser.User, ErrorCustom.Error>(user)
                : Option.None<DomainUser.User, ErrorCustom.Error>(new ErrorCustom.Error("User.NotFound", "User not found", ErrorCustom.ErrorType.NotFound));
        }
        catch (Exception ex)
        {
            return Option.None<DomainUser.User, ErrorCustom.Error>(
                new ErrorCustom.Error("User.GetFailed", $"Failed to get user: {ex.Message}", ErrorCustom.ErrorType.Failure));
        }
    }

    public async Task<Option<DomainUser.User, ErrorCustom.Error>> GetUserByEmailAsync(string email, CancellationToken ct)
    {
        try
        {
            var user = await _authenticationRepository.GetUserByEmail(email);
            return user != null
                ? Option.Some<DomainUser.User, ErrorCustom.Error>(user)
                : Option.None<DomainUser.User, ErrorCustom.Error>(new ErrorCustom.Error("User.NotFound", "User not found", ErrorCustom.ErrorType.NotFound));
        }
        catch (Exception ex)
        {
            return Option.None<DomainUser.User, ErrorCustom.Error>(
                new ErrorCustom.Error("User.GetFailed", $"Failed to get user: {ex.Message}", ErrorCustom.ErrorType.Failure));
        }
    }
}
