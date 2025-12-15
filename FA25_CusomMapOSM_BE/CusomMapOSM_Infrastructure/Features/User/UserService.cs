using CusomMapOSM_Application.Interfaces.Features.User;
using CusomMapOSM_Application.Models.DTOs.Features.User;
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

    public async Task<Option<DomainUser.User, ErrorCustom.Error>> GetUserByIdAsync(Guid userId)
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

    public async Task<Option<UpdateUserPersonalInfoResponse, ErrorCustom.Error>> UpdateUserPersonalInfoAsync(Guid userId, UpdateUserPersonalInfoRequest request, CancellationToken ct)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.FullName) && string.IsNullOrWhiteSpace(request.Phone))
            {
                return Option.None<UpdateUserPersonalInfoResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("User.UpdateValidation", "At least one field (FullName or Phone) must be provided", ErrorCustom.ErrorType.Validation));
            }

            // Get existing user
            var existingUser = await _authenticationRepository.GetUserById(userId);
            if (existingUser == null)
            {
                return Option.None<UpdateUserPersonalInfoResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("User.NotFound", "User not found", ErrorCustom.ErrorType.NotFound));
            }

            // Update only provided fields
            if (!string.IsNullOrWhiteSpace(request.FullName))
            {
                existingUser.FullName = request.FullName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(request.Phone))
            {
                existingUser.Phone = request.Phone.Trim();
            }

            // Save changes
            var updateResult = await _authenticationRepository.UpdateUser(existingUser);
            if (!updateResult)
            {
                return Option.None<UpdateUserPersonalInfoResponse, ErrorCustom.Error>(
                    new ErrorCustom.Error("User.UpdateFailed", "Failed to update user information", ErrorCustom.ErrorType.Failure));
            }

            // Return updated user info
            var userInfoDto = new UserInfoDto
            {
                UserId = existingUser.UserId,
                Email = existingUser.Email,
                FullName = existingUser.FullName,
                Phone = existingUser.Phone,
                Role = existingUser.Role.ToString(),
                AccountStatus = existingUser.AccountStatus.ToString(),
                CreatedAt = existingUser.CreatedAt,
                LastLogin = existingUser.LastLogin
            };

            return Option.Some<UpdateUserPersonalInfoResponse, ErrorCustom.Error>(new UpdateUserPersonalInfoResponse
            {
                Result = "User personal information updated successfully",
                User = userInfoDto
            });
        }
        catch (Exception ex)
        {
            return Option.None<UpdateUserPersonalInfoResponse, ErrorCustom.Error>(
                new ErrorCustom.Error("User.UpdateFailed", $"Failed to update user information: {ex.Message}", ErrorCustom.ErrorType.Failure));
        }
    }
}
