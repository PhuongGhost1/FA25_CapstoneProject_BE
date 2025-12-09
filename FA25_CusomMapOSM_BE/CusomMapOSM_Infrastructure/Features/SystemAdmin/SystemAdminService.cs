using CusomMapOSM_Application.Interfaces.Features.SystemAdmin;
using CusomMapOSM_Application.Models.DTOs.Features.SystemAdmin;
using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.SystemAdmin;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.OrganizationAdmin;
using CusomMapOSM_Application.Interfaces.Features.Notifications;
using Optional;
using Optional.Unsafe;
using CusomMapOSM_Domain.Entities.Memberships;
using CusomMapOSM_Domain.Entities.Memberships.Enums;
using CusomMapOSM_Domain.Entities.Users;
using CusomMapOSM_Domain.Entities.Users.Enums;
using CusomMapOSM_Infrastructure.Databases;
using System.Text.Json;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Authentication;

namespace CusomMapOSM_Infrastructure.Features.SystemAdmin;

public class SystemAdminService : ISystemAdminService
{
    private readonly ISystemAdminRepository _systemAdminRepository;
    private readonly IAuthenticationRepository _authenticationRepository;
    private readonly IOrganizationAdminRepository _organizationAdminRepository;
    private readonly INotificationService _notificationService;

    public SystemAdminService(
        ISystemAdminRepository systemAdminRepository,
        IOrganizationAdminRepository organizationAdminRepository,
        INotificationService notificationService,
        IAuthenticationRepository authenticationRepository)
    {
        _systemAdminRepository = systemAdminRepository;
        _organizationAdminRepository = organizationAdminRepository;
        _notificationService = notificationService;
        _authenticationRepository = authenticationRepository;
    }

    // System User Management
    public async Task<Option<SystemUserListResponse, Error>> GetAllUsersAsync(int page = 1, int pageSize = 20, string? search = null, string? status = null, CancellationToken ct = default)
    {
        try
        {
            var users = await _systemAdminRepository.GetAllUsersAsync(page, pageSize, search, status, ct);
            var totalCount = await _systemAdminRepository.GetTotalUsersCountAsync(search, status, ct);

            var userDtos = new List<SystemUserDto>();
            foreach (var u in users)
            {
                var totalOrganizations = await _systemAdminRepository.GetUserTotalOrganizationsCountAsync(u.UserId, ct);
                var totalActiveMemberships = await _systemAdminRepository.GetUserActiveMembershipsCountAsync(u.UserId, ct);

                userDtos.Add(new SystemUserDto
                {
                    UserId = u.UserId,
                    UserName = u.FullName ?? "Unknown",
                    Email = u.Email,
                    FirstName = u.FullName?.Split(' ').FirstOrDefault() ?? "Unknown",
                    LastName = u.FullName?.Split(' ').Skip(1).FirstOrDefault() ?? "Unknown",
                    Phone = u.Phone ?? "N/A",
                    Status = u.AccountStatus.ToString(),
                    Role = "User", // Would need to determine from user roles
                    CreatedAt = u.CreatedAt,
                    LastLoginAt = u.LastLogin,
                    IsEmailVerified = u.AccountStatus == CusomMapOSM_Domain.Entities.Users.Enums.AccountStatusEnum.Active,
                    IsPhoneVerified = false, // Placeholder
                    TotalOrganizations = totalOrganizations,
                    TotalActiveMemberships = totalActiveMemberships
                });
            }

            return Option.Some<SystemUserListResponse, Error>(new SystemUserListResponse
            {
                Users = userDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
        catch (Exception ex)
        {
            return Option.None<SystemUserListResponse, Error>(Error.Failure("SystemAdmin.UsersFailed", $"Failed to get users: {ex.Message}"));
        }
    }

    public async Task<Option<SystemUserDto, Error>> GetUserDetailsAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            var user = await _authenticationRepository.GetUserById(userId);
            if (user == null)
            {
                return Option.None<SystemUserDto, Error>(Error.NotFound("User.NotFound", "User not found"));
            }

            var totalOrganizations = await _systemAdminRepository.GetUserTotalOrganizationsCountAsync(user.UserId, ct);
            var totalActiveMemberships = await _systemAdminRepository.GetUserActiveMembershipsCountAsync(user.UserId, ct);

            var userDto = new SystemUserDto
            {
                UserId = user.UserId,
                UserName = user.FullName ?? "Unknown",
                Email = user.Email,
                FirstName = user.FullName?.Split(' ').FirstOrDefault() ?? "Unknown",
                LastName = user.FullName?.Split(' ').Skip(1).FirstOrDefault() ?? "Unknown",
                Phone = user.Phone,
                Status = user.AccountStatus.ToString(),
                Role = "User",
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLogin,
                IsEmailVerified = user.AccountStatus == CusomMapOSM_Domain.Entities.Users.Enums.AccountStatusEnum.Active,
                IsPhoneVerified = false,
                TotalOrganizations = totalOrganizations,
                TotalActiveMemberships = totalActiveMemberships
            };

            return Option.Some<SystemUserDto, Error>(userDto);
        }
        catch (Exception ex)
        {
            return Option.None<SystemUserDto, Error>(Error.Failure("SystemAdmin.UserDetailsFailed", $"Failed to get user details: {ex.Message}"));
        }
    }

    public async Task<Option<UpdateUserStatusResponse, Error>> UpdateUserStatusAsync(UpdateUserStatusRequest request, CancellationToken ct = default)
    {
        try
        {
            var user = await _systemAdminRepository.GetUserByIdAsync(request.UserId, ct);
            if (user == null)
            {
                return Option.None<UpdateUserStatusResponse, Error>(Error.NotFound("User.NotFound", "User not found"));
            }

            var oldStatus = user.AccountStatus;
            var success = await _systemAdminRepository.UpdateUserStatusAsync(request.UserId, request.Status, ct);

            if (!success)
            {
                return Option.None<UpdateUserStatusResponse, Error>(Error.Failure("SystemAdmin.UserUpdateFailed", "Failed to update user status"));
            }

            return Option.Some<UpdateUserStatusResponse, Error>(new UpdateUserStatusResponse
            {
                UserId = request.UserId,
                OldStatus = oldStatus.ToString(),
                NewStatus = request.Status,
                Message = "User status updated successfully"
            });
        }
        catch (Exception ex)
        {
            return Option.None<UpdateUserStatusResponse, Error>(Error.Failure("SystemAdmin.UserUpdateFailed", $"Failed to update user status: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> DeleteUserAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            var success = await _systemAdminRepository.DeleteUserAsync(userId, ct);
            return Option.Some<bool, Error>(success);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("SystemAdmin.UserDeleteFailed", $"Failed to delete user: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> ImpersonateUserAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            // Would need to implement impersonation logic
            return Option.Some<bool, Error>(true);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("SystemAdmin.ImpersonationFailed", $"Failed to impersonate user: {ex.Message}"));
        }
    }

    // System Organization Management
    public async Task<Option<SystemOrganizationListResponse, Error>> GetAllOrganizationsAsync(int page = 1, int pageSize = 20, string? search = null, string? status = null, CancellationToken ct = default)
    {
        try
        {
            var organizations = await _systemAdminRepository.GetAllOrganizationsAsync(page, pageSize, search, status, ct);
            var totalCount = await _systemAdminRepository.GetTotalOrganizationsCountAsync(search, status, ct);

            var organizationDtos = new List<SystemOrganizationDto>();
            foreach (var o in organizations)
            {
                var totalMembers = await _systemAdminRepository.GetOrganizationMembersCountAsync(o.OrgId, ct);
                var totalActiveMemberships = await _systemAdminRepository.GetOrganizationActiveMembershipsCountAsync(o.OrgId, ct);
                var totalRevenue = await _systemAdminRepository.GetOrganizationTotalRevenueAsync(o.OrgId, ct);
                var primaryPlanName = await _systemAdminRepository.GetOrganizationPrimaryPlanNameAsync(o.OrgId, ct);

                organizationDtos.Add(new SystemOrganizationDto
                {
                    OrgId = o.OrgId,
                    Name = o.OrgName,
                    Description = o.Description,
                    Status = o.Status.ToString(),
                    OwnerUserId = o.OwnerUserId,
                    OwnerName = o.Owner?.FullName ?? "Unknown",
                    OwnerEmail = o.Owner?.Email ?? "Unknown",
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt,
                    TotalMembers = totalMembers,
                    TotalActiveMemberships = totalActiveMemberships,
                    TotalRevenue = totalRevenue,
                    PrimaryPlanName = primaryPlanName
                });
            }

            return Option.Some<SystemOrganizationListResponse, Error>(new SystemOrganizationListResponse
            {
                Organizations = organizationDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
        catch (Exception ex)
        {
            return Option.None<SystemOrganizationListResponse, Error>(Error.Failure("SystemAdmin.OrganizationsFailed", $"Failed to get organizations: {ex.Message}"));
        }
    }

    public async Task<Option<SystemOrganizationDto, Error>> GetOrganizationDetailsAsync(Guid orgId, CancellationToken ct = default)
    {
        try
        {
            var organization = await _systemAdminRepository.GetOrganizationByIdAsync(orgId, ct);
            if (organization == null)
            {
                return Option.None<SystemOrganizationDto, Error>(Error.NotFound("Organization.NotFound", "Organization not found"));
            }

            var totalMembers = await _systemAdminRepository.GetOrganizationMembersCountAsync(organization.OrgId, ct);
            var totalActiveMemberships = await _systemAdminRepository.GetOrganizationActiveMembershipsCountAsync(organization.OrgId, ct);
            var totalRevenue = await _systemAdminRepository.GetOrganizationTotalRevenueAsync(organization.OrgId, ct);
            var primaryPlanName = await _systemAdminRepository.GetOrganizationPrimaryPlanNameAsync(organization.OrgId, ct);

            var organizationDto = new SystemOrganizationDto
            {
                OrgId = organization.OrgId,
                Name = organization.OrgName,
                Description = organization.Description,
                Status = organization.Status.ToString(),
                OwnerUserId = organization.OwnerUserId,
                OwnerName = organization.Owner?.FullName ?? "Unknown",
                OwnerEmail = organization.Owner?.Email ?? "Unknown",
                CreatedAt = organization.CreatedAt,
                UpdatedAt = organization.UpdatedAt,
                TotalMembers = totalMembers,
                TotalActiveMemberships = totalActiveMemberships,
                TotalRevenue = totalRevenue,
                PrimaryPlanName = primaryPlanName
            };

            return Option.Some<SystemOrganizationDto, Error>(organizationDto);
        }
        catch (Exception ex)
        {
            return Option.None<SystemOrganizationDto, Error>(Error.Failure("SystemAdmin.OrganizationDetailsFailed", $"Failed to get organization details: {ex.Message}"));
        }
    }

    public async Task<Option<UpdateOrganizationStatusResponse, Error>> UpdateOrganizationStatusAsync(UpdateOrganizationStatusRequest request, CancellationToken ct = default)
    {
        try
        {
            var organization = await _systemAdminRepository.GetOrganizationByIdAsync(request.OrgId, ct);
            if (organization == null)
            {
                return Option.None<UpdateOrganizationStatusResponse, Error>(Error.NotFound("Organization.NotFound", "Organization not found"));
            }

            var oldStatus = organization.Status;
            var success = await _systemAdminRepository.UpdateOrganizationStatusAsync(request.OrgId, request.Status, ct);

            if (!success)
            {
                return Option.None<UpdateOrganizationStatusResponse, Error>(Error.Failure("SystemAdmin.OrganizationUpdateFailed", "Failed to update organization status"));
            }

            return Option.Some<UpdateOrganizationStatusResponse, Error>(new UpdateOrganizationStatusResponse
            {
                OrgId = request.OrgId,
                OldStatus = oldStatus.ToString(),
                NewStatus = request.Status,
                Message = "Organization status updated successfully"
            });
        }
        catch (Exception ex)
        {
            return Option.None<UpdateOrganizationStatusResponse, Error>(Error.Failure("SystemAdmin.OrganizationUpdateFailed", $"Failed to update organization status: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> DeleteOrganizationAsync(Guid orgId, CancellationToken ct = default)
    {
        try
        {
            var success = await _systemAdminRepository.DeleteOrganizationAsync(orgId, ct);
            return Option.Some<bool, Error>(success);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("SystemAdmin.OrganizationDeleteFailed", $"Failed to delete organization: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> TransferOrganizationOwnershipAsync(Guid orgId, Guid newOwnerId, CancellationToken ct = default)
    {
        try
        {
            var success = await _systemAdminRepository.TransferOrganizationOwnershipAsync(orgId, newOwnerId, ct);
            return Option.Some<bool, Error>(success);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("SystemAdmin.OrganizationTransferFailed", $"Failed to transfer organization ownership: {ex.Message}"));
        }
    }

    // System Subscription Plan Management
    public async Task<Option<List<SystemSubscriptionPlanDto>, Error>> GetAllSubscriptionPlansAsync(CancellationToken ct = default)
    {
        try
        {
            var plans = await _systemAdminRepository.GetAllSubscriptionPlansAsync(ct);

            var planDtos = new List<SystemSubscriptionPlanDto>();
            foreach (var plan in plans)
            {
                var subscribersCount = await _systemAdminRepository.GetSubscribersCountByPlanAsync(plan.PlanId, ct);
                var revenue = await _systemAdminRepository.GetRevenueByPlanAsync(plan.PlanId, ct);

                planDtos.Add(new SystemSubscriptionPlanDto
                {
                    PlanId = plan.PlanId,
                    Name = plan.PlanName,
                    Description = plan.Description,
                    Status = plan.IsActive ? "active" : "inactive",
                    PriceMonthly = plan.PriceMonthly ?? 0,
                    PriceYearly = plan.PriceMonthly ?? 0,
                    MapsLimit = plan.MaxMapsPerMonth,
                    ExportsLimit = plan.ExportQuota,
                    CustomLayersLimit = plan.MaxCustomLayers,
                    MonthlyTokenLimit = plan.MonthlyTokens,
                    IsPopular = plan.PrioritySupport,
                    IsActive = plan.IsActive,
                    CreatedAt = plan.CreatedAt,
                    UpdatedAt = plan.UpdatedAt,
                    TotalSubscribers = subscribersCount,
                    TotalRevenue = revenue
                });
            }

            return Option.Some<List<SystemSubscriptionPlanDto>, Error>(planDtos);
        }
        catch (Exception ex)
        {
            return Option.None<List<SystemSubscriptionPlanDto>, Error>(Error.Failure("SystemAdmin.PlansFailed", $"Failed to get subscription plans: {ex.Message}"));
        }
    }

    public async Task<Option<SystemSubscriptionPlanDto, Error>> GetSubscriptionPlanDetailsAsync(int planId, CancellationToken ct = default)
    {
        try
        {
            var plan = await _systemAdminRepository.GetSubscriptionPlanByIdAsync(planId, ct);
            if (plan == null)
            {
                return Option.None<SystemSubscriptionPlanDto, Error>(Error.NotFound("Plan.NotFound", "Subscription plan not found"));
            }

            var subscribersCount = await _systemAdminRepository.GetSubscribersCountByPlanAsync(plan.PlanId, ct);
            var revenue = await _systemAdminRepository.GetRevenueByPlanAsync(plan.PlanId, ct);

            var planDto = new SystemSubscriptionPlanDto
            {
                PlanId = plan.PlanId,
                Name = plan.PlanName,
                Description = plan.Description,
                Status = plan.IsActive ? "active" : "inactive",
                PriceMonthly = plan.PriceMonthly ?? 0,
                PriceYearly = plan.PriceMonthly ?? 0,
                MapsLimit = plan.MaxMapsPerMonth,
                ExportsLimit = plan.ExportQuota,
                CustomLayersLimit = plan.MaxCustomLayers,
                MonthlyTokenLimit = plan.MonthlyTokens,
                IsPopular = plan.PrioritySupport,
                IsActive = plan.IsActive,
                CreatedAt = plan.CreatedAt,
                UpdatedAt = plan.UpdatedAt,
                TotalSubscribers = subscribersCount,
                TotalRevenue = revenue
            };

            return Option.Some<SystemSubscriptionPlanDto, Error>(planDto);
        }
        catch (Exception ex)
        {
            return Option.None<SystemSubscriptionPlanDto, Error>(Error.Failure("SystemAdmin.PlanDetailsFailed", $"Failed to get subscription plan details: {ex.Message}"));
        }
    }

    public async Task<Option<SystemSubscriptionPlanDto, Error>> CreateSubscriptionPlanAsync(CreateSubscriptionPlanRequest request, CancellationToken ct = default)
    {
        try
        {
            var plan = new Plan
            {
                PlanName = request.Name,
                Description = request.Description,
                PriceMonthly = request.PriceMonthly,
                DurationMonths = 12,
                MaxOrganizations = 10,
                MaxLocationsPerOrg = 10,
                MaxMapsPerMonth = request.MapsLimit,
                MaxUsersPerOrg = 10,
                MapQuota = 10000,
                ExportQuota = request.ExportsLimit,
                MaxCustomLayers = request.CustomLayersLimit,
                MonthlyTokens = request.MonthlyTokenLimit,
                PrioritySupport = request.IsPopular,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var success = await _systemAdminRepository.CreateSubscriptionPlanAsync(plan, ct);
            if (!success)
            {
                return Option.None<SystemSubscriptionPlanDto, Error>(Error.Failure("SystemAdmin.PlanCreateFailed", "Failed to create subscription plan"));
            }

            var planDto = new SystemSubscriptionPlanDto
            {
                PlanId = plan.PlanId,
                Name = plan.PlanName,
                Description = plan.Description,
                Status = plan.IsActive ? "active" : "inactive",
                PriceMonthly = plan.PriceMonthly ?? 0,
                PriceYearly = plan.PriceMonthly ?? 0,
                MapsLimit = plan.MaxMapsPerMonth,
                ExportsLimit = plan.ExportQuota,
                CustomLayersLimit = plan.MaxCustomLayers,
                MonthlyTokenLimit = plan.MonthlyTokens,
                IsPopular = plan.PrioritySupport,
                IsActive = plan.IsActive,
                CreatedAt = plan.CreatedAt,
                UpdatedAt = plan.UpdatedAt,
                TotalSubscribers = 0,
                TotalRevenue = 0
            };

            return Option.Some<SystemSubscriptionPlanDto, Error>(planDto);
        }
        catch (Exception ex)
        {
            return Option.None<SystemSubscriptionPlanDto, Error>(Error.Failure("SystemAdmin.PlanCreateFailed", $"Failed to create subscription plan: {ex.Message}"));
        }
    }

    public async Task<Option<SystemSubscriptionPlanDto, Error>> UpdateSubscriptionPlanAsync(UpdateSubscriptionPlanRequest request, CancellationToken ct = default)
    {
        try
        {
            var plan = await _systemAdminRepository.GetSubscriptionPlanByIdAsync(request.PlanId, ct);
            if (plan == null)
            {
                return Option.None<SystemSubscriptionPlanDto, Error>(Error.NotFound("Plan.NotFound", "Subscription plan not found"));
            }

            // Update fields if provided
            if (!string.IsNullOrEmpty(request.Name)) plan.PlanName = request.Name;
            if (!string.IsNullOrEmpty(request.Description)) plan.Description = request.Description;
            if (request.PriceMonthly.HasValue) plan.PriceMonthly = request.PriceMonthly.Value;
            if (request.PriceYearly.HasValue) plan.PriceMonthly = request.PriceYearly.Value;
            if (request.MapsLimit.HasValue) plan.MaxMapsPerMonth = request.MapsLimit.Value;
            if (request.ExportsLimit.HasValue) plan.ExportQuota = request.ExportsLimit.Value;
            if (request.CustomLayersLimit.HasValue) plan.MaxCustomLayers = request.CustomLayersLimit.Value;
            if (request.MonthlyTokenLimit.HasValue) plan.MonthlyTokens = request.MonthlyTokenLimit.Value;
            if (request.IsPopular.HasValue) plan.PrioritySupport = request.IsPopular.Value;
            if (request.IsActive.HasValue) plan.IsActive = request.IsActive.Value;

            plan.UpdatedAt = DateTime.UtcNow;

            var success = await _systemAdminRepository.UpdateSubscriptionPlanAsync(plan, ct);
            if (!success)
            {
                return Option.None<SystemSubscriptionPlanDto, Error>(Error.Failure("SystemAdmin.PlanUpdateFailed", "Failed to update subscription plan"));
            }

            var subscribersCount = await _systemAdminRepository.GetSubscribersCountByPlanAsync(plan.PlanId, ct);
            var revenue = await _systemAdminRepository.GetRevenueByPlanAsync(plan.PlanId, ct);

            var planDto = new SystemSubscriptionPlanDto
            {
                PlanId = plan.PlanId,
                Name = plan.PlanName,
                Description = plan.Description,
                Status = plan.IsActive ? "active" : "inactive",
                PriceMonthly = plan.PriceMonthly ?? 0,
                PriceYearly = plan.PriceMonthly ?? 0,
                MapsLimit = plan.MaxMapsPerMonth,
                ExportsLimit = plan.ExportQuota,
                CustomLayersLimit = plan.MaxCustomLayers,
                MonthlyTokenLimit = plan.MonthlyTokens,
                IsPopular = plan.PrioritySupport,
                IsActive = plan.IsActive,
                CreatedAt = plan.CreatedAt,
                UpdatedAt = plan.UpdatedAt,
                TotalSubscribers = subscribersCount,
                TotalRevenue = revenue
            };

            return Option.Some<SystemSubscriptionPlanDto, Error>(planDto);
        }
        catch (Exception ex)
        {
            return Option.None<SystemSubscriptionPlanDto, Error>(Error.Failure("SystemAdmin.PlanUpdateFailed", $"Failed to update subscription plan: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> DeleteSubscriptionPlanAsync(int planId, CancellationToken ct = default)
    {
        try
        {
            var success = await _systemAdminRepository.DeleteSubscriptionPlanAsync(planId, ct);
            return Option.Some<bool, Error>(success);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("SystemAdmin.PlanDeleteFailed", $"Failed to delete subscription plan: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> ActivateSubscriptionPlanAsync(int planId, CancellationToken ct = default)
    {
        try
        {
            var success = await _systemAdminRepository.ActivateSubscriptionPlanAsync(planId, ct);
            return Option.Some<bool, Error>(success);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("SystemAdmin.PlanActivateFailed", $"Failed to activate subscription plan: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> DeactivateSubscriptionPlanAsync(int planId, CancellationToken ct = default)
    {
        try
        {
            var success = await _systemAdminRepository.DeactivateSubscriptionPlanAsync(planId, ct);
            return Option.Some<bool, Error>(success);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("SystemAdmin.PlanDeactivateFailed", $"Failed to deactivate subscription plan: {ex.Message}"));
        }
    }

    // System Support Ticket Management (placeholder implementations)
    public async Task<Option<SystemSupportTicketListResponse, Error>> GetAllSupportTicketsAsync(int page = 1, int pageSize = 20, string? status = null, string? priority = null, string? category = null, CancellationToken ct = default)
    {
        try
        {
            var tickets = await _systemAdminRepository.GetAllSupportTicketsAsync(page, pageSize, status, priority, category, ct);
            var totalCount = await _systemAdminRepository.GetTotalSupportTicketsCountAsync(status, priority, category, ct);

            // Convert objects to DTOs (placeholder implementation)
            var ticketDtos = tickets.Cast<SystemSupportTicketDto>().ToList();

            return Option.Some<SystemSupportTicketListResponse, Error>(new SystemSupportTicketListResponse
            {
                Tickets = ticketDtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
        catch (Exception ex)
        {
            return Option.None<SystemSupportTicketListResponse, Error>(Error.Failure("SystemAdmin.SupportTicketsFailed", $"Failed to get support tickets: {ex.Message}"));
        }
    }

    public async Task<Option<SystemSupportTicketDto, Error>> GetSupportTicketDetailsAsync(int ticketId, CancellationToken ct = default)
    {
        try
        {
            var ticket = await _systemAdminRepository.GetSupportTicketByIdAsync(ticketId, ct);
            if (ticket == null)
            {
                return Option.None<SystemSupportTicketDto, Error>(Error.NotFound("Ticket.NotFound", "Support ticket not found"));
            }
            return Option.Some<SystemSupportTicketDto, Error>((SystemSupportTicketDto)ticket);
        }
        catch (Exception ex)
        {
            return Option.None<SystemSupportTicketDto, Error>(Error.Failure("SystemAdmin.SupportTicketDetailsFailed", $"Failed to get support ticket details: {ex.Message}"));
        }
    }

    public async Task<Option<SystemAdminUpdateSupportTicketResponse, Error>> UpdateSupportTicketAsync(SystemAdminUpdateSupportTicketRequest request, CancellationToken ct = default)
    {
        try
        {
            // Get ticket details before updating
            var ticket = await _systemAdminRepository.GetSupportTicketByIdAsync(request.TicketId, ct);
            if (ticket == null)
            {
                return Option.None<SystemAdminUpdateSupportTicketResponse, Error>(Error.NotFound("Ticket.NotFound", "Support ticket not found"));
            }

            var updates = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(request.Status)) updates["Status"] = request.Status;
            if (!string.IsNullOrEmpty(request.Priority)) updates["Priority"] = request.Priority;
            if (request.AssignedToUserId.HasValue) updates["AssignedToUserId"] = request.AssignedToUserId.Value;
            if (!string.IsNullOrEmpty(request.Response)) updates["Response"] = request.Response;

            var success = await _systemAdminRepository.UpdateSupportTicketAsync(request.TicketId, updates, ct);

            if (success)
            {
                // Send notification to user about ticket update
                var notificationMessage = $"Your support ticket #{request.TicketId} has been updated. Please check the latest status.";
                await _notificationService.CreateNotificationAsync(
                    ((SystemSupportTicketDto)ticket).UserId,
                    "SupportTicketUpdated",
                    notificationMessage,
                    JsonSerializer.Serialize(new { ticketId = request.TicketId, updates = updates }),
                    ct);
            }

            return Option.Some<SystemAdminUpdateSupportTicketResponse, Error>(new SystemAdminUpdateSupportTicketResponse
            {
                TicketId = request.TicketId,
                Message = "Support ticket updated successfully",
                UpdatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return Option.None<SystemAdminUpdateSupportTicketResponse, Error>(Error.Failure("SystemAdmin.SupportTicketUpdateFailed", $"Failed to update support ticket: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> CloseSupportTicketAsync(int ticketId, string resolution, CancellationToken ct = default)
    {
        try
        {
            // Get ticket details before closing
            var ticket = await _systemAdminRepository.GetSupportTicketByIdAsync(ticketId, ct);
            if (ticket == null)
            {
                return Option.None<bool, Error>(Error.NotFound("Ticket.NotFound", "Support ticket not found"));
            }

            var success = await _systemAdminRepository.CloseSupportTicketAsync(ticketId, resolution, ct);

            if (success)
            {
                // Send notification to user about ticket closure
                var notificationMessage = $"Your support ticket #{ticketId} has been closed with resolution: {resolution}";
                await _notificationService.CreateNotificationAsync(
                    ((SystemSupportTicketDto)ticket).UserId,
                    "SupportTicketClosed",
                    notificationMessage,
                    JsonSerializer.Serialize(new { ticketId = ticketId, resolution = resolution }),
                    ct);
            }

            return Option.Some<bool, Error>(success);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("SystemAdmin.SupportTicketCloseFailed", $"Failed to close support ticket: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> AssignSupportTicketAsync(int ticketId, Guid assignedToUserId, CancellationToken ct = default)
    {
        try
        {
            // Get ticket details before assigning
            var ticket = await _systemAdminRepository.GetSupportTicketByIdAsync(ticketId, ct);
            if (ticket == null)
            {
                return Option.None<bool, Error>(Error.NotFound("Ticket.NotFound", "Support ticket not found"));
            }

            var success = await _systemAdminRepository.AssignSupportTicketAsync(ticketId, assignedToUserId, ct);

            if (success)
            {
                // Send notification to user about ticket assignment
                var notificationMessage = $"Your support ticket #{ticketId} has been assigned to a support agent and is being reviewed.";
                await _notificationService.CreateNotificationAsync(
                    ((SystemSupportTicketDto)ticket).UserId,
                    "SupportTicketAssigned",
                    notificationMessage,
                    JsonSerializer.Serialize(new { ticketId = ticketId, assignedToUserId = assignedToUserId }),
                    ct);
            }

            return Option.Some<bool, Error>(success);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("SystemAdmin.SupportTicketAssignFailed", $"Failed to assign support ticket: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> EscalateSupportTicketAsync(int ticketId, string reason, CancellationToken ct = default)
    {
        try
        {
            var success = await _systemAdminRepository.EscalateSupportTicketAsync(ticketId, reason, ct);
            return Option.Some<bool, Error>(true);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("SystemAdmin.SupportTicketEscalateFailed", $"Failed to escalate support ticket: {ex.Message}"));
        }
    }

    // System Usage Monitoring
    public async Task<Option<SystemUsageStatsDto, Error>> GetSystemUsageStatsAsync(CancellationToken ct = default)
    {
        try
        {
            var totalUsers = await _systemAdminRepository.GetTotalUsersCountAsync(ct: ct);
            var activeUsers = await _systemAdminRepository.GetActiveUsersCountAsync(ct);
            var verifiedUsers = await _systemAdminRepository.GetVerifiedUsersCountAsync(ct);
            var unverifiedUsers = totalUsers - verifiedUsers;

            var thisMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);
            var lastMonthEnd = thisMonthStart.AddDays(-1);

            var newUsersThisMonth = await _systemAdminRepository.GetUsersByDateRangeAsync(thisMonthStart, DateTime.UtcNow, ct);
            var newUsersLastMonth = await _systemAdminRepository.GetUsersByDateRangeAsync(lastMonthStart, lastMonthEnd, ct);

            var totalOrganizations = await _systemAdminRepository.GetTotalOrganizationsCountAsync(ct: ct);
            var activeOrganizations = await _systemAdminRepository.GetActiveOrganizationsCountAsync(ct);
            var organizationsWithSubscriptions = await _systemAdminRepository.GetOrganizationsWithActiveSubscriptionsCountAsync(ct);

            var newOrganizationsThisMonth = await _systemAdminRepository.GetOrganizationsByDateRangeAsync(thisMonthStart, DateTime.UtcNow, ct);
            var newOrganizationsLastMonth = await _systemAdminRepository.GetOrganizationsByDateRangeAsync(lastMonthStart, lastMonthEnd, ct);

            var activeMemberships = await _systemAdminRepository.GetActiveMembershipsAsync(ct);
            var expiredMemberships = await _systemAdminRepository.GetExpiredMembershipsAsync(ct);
            var cancelledMemberships = await _systemAdminRepository.GetCancelledMembershipsAsync(ct);

            var newMembershipsThisMonth = await _systemAdminRepository.GetNewMembershipsCountAsync(thisMonthStart, DateTime.UtcNow, ct);
            var newMembershipsLastMonth = await _systemAdminRepository.GetNewMembershipsCountAsync(lastMonthStart, lastMonthEnd, ct);

            var totalRevenue = await _systemAdminRepository.GetTotalRevenueAsync(ct);
            var revenueThisMonth = await _systemAdminRepository.GetRevenueByDateRangeAsync(thisMonthStart, DateTime.UtcNow, ct);
            var revenueLastMonth = await _systemAdminRepository.GetRevenueByDateRangeAsync(lastMonthStart, lastMonthEnd, ct);

            var usageStats = await _systemAdminRepository.GetSystemUsageStatsAsync(ct);

            var stats = new SystemUsageStatsDto
            {
                GeneratedAt = DateTime.UtcNow,
                UserStats = new SystemUserStatsDto
                {
                    TotalUsers = totalUsers,
                    ActiveUsers = activeUsers,
                    NewUsersThisMonth = newUsersThisMonth.Count,
                    NewUsersLastMonth = newUsersLastMonth.Count,
                    UserGrowthRate = newUsersLastMonth.Count > 0 ? (newUsersThisMonth.Count - newUsersLastMonth.Count) / newUsersLastMonth.Count * 100 : 0,
                    VerifiedUsers = verifiedUsers,
                    UnverifiedUsers = unverifiedUsers
                },
                OrganizationStats = new SystemOrganizationStatsDto
                {
                    TotalOrganizations = totalOrganizations,
                    ActiveOrganizations = activeOrganizations,
                    NewOrganizationsThisMonth = newOrganizationsThisMonth.Count,
                    NewOrganizationsLastMonth = newOrganizationsLastMonth.Count,
                    OrganizationGrowthRate = newOrganizationsLastMonth.Count > 0 ? (double)(newOrganizationsThisMonth.Count - newOrganizationsLastMonth.Count) / newOrganizationsLastMonth.Count * 100 : 0,
                    OrganizationsWithActiveSubscriptions = organizationsWithSubscriptions
                },
                SubscriptionStats = new SystemSubscriptionStatsDto
                {
                    TotalActiveSubscriptions = activeMemberships.Count,
                    TotalExpiredSubscriptions = expiredMemberships.Count,
                    TotalCancelledSubscriptions = cancelledMemberships.Count,
                    NewSubscriptionsThisMonth = newMembershipsThisMonth,
                    NewSubscriptionsLastMonth = newMembershipsLastMonth,
                    SubscriptionGrowthRate = newMembershipsLastMonth > 0 ? (double)(newMembershipsThisMonth - newMembershipsLastMonth) / newMembershipsLastMonth * 100 : 0,
                    SubscriptionsByPlan = await _systemAdminRepository.GetMembershipsByPlanAsync(ct) ?? new Dictionary<string, int>()
                },
                RevenueStats = new SystemRevenueStatsDto
                {
                    TotalRevenue = totalRevenue,
                    RevenueThisMonth = revenueThisMonth,
                    RevenueLastMonth = revenueLastMonth,
                    RevenueGrowthRate = (double)(revenueLastMonth > 0 ? (revenueThisMonth - revenueLastMonth) / revenueLastMonth * 100 : 0),
                    AverageRevenuePerUser = totalUsers > 0 ? totalRevenue / totalUsers : 0,
                    AverageRevenuePerOrganization = totalOrganizations > 0 ? totalRevenue / totalOrganizations : 0,
                    RevenueByPlan = await _systemAdminRepository.GetRevenueByPlanAsync(thisMonthStart, DateTime.UtcNow, ct) ?? new Dictionary<string, decimal>(),
                    RevenueByPaymentGateway = await _systemAdminRepository.GetRevenueByPaymentGatewayAsync(thisMonthStart, DateTime.UtcNow, ct) ?? new Dictionary<string, decimal>()
                },
                PerformanceStats = new SystemPerformanceStatsDto
                {
                    AverageResponseTime = 0.0, // Would need to implement performance monitoring
                    SystemUptime = 99.9, // Would need to implement uptime monitoring
                    TotalApiCalls = 0, // Would need to implement API call tracking
                    SuccessfulApiCalls = 0,
                    FailedApiCalls = 0,
                    ApiSuccessRate = 100.0,
                    ApiCallsByEndpoint = new Dictionary<string, int>(),
                    ErrorsByType = new Dictionary<string, int>()
                },
                TotalMaps = usageStats.GetValueOrDefault("total_maps", 0),
                TotalExports = usageStats.GetValueOrDefault("total_exports", 0),
                TotalCustomLayers = usageStats.GetValueOrDefault("total_custom_layers", 0),
                TotalTokens = usageStats.GetValueOrDefault("total_tokens", 0)
            };

            return Option.Some<SystemUsageStatsDto, Error>(stats);
        }
        catch (Exception ex)
        {
            return Option.None<SystemUsageStatsDto, Error>(Error.Failure("SystemAdmin.UsageStatsFailed", $"Failed to get system usage stats: {ex.Message}"));
        }
    }

    public async Task<Option<SystemDashboardDto, Error>> GetSystemDashboardAsync(CancellationToken ct = default)
    {
        try
        {
            var usageStats = await GetSystemUsageStatsAsync(ct);
            if (!usageStats.HasValue)
            {
                return Option.None<SystemDashboardDto, Error>(Error.Failure("SystemAdmin.DashboardFailed", $"Failed to get system dashboard"));
            }

            var topUsers = await GetTopUsersAsync(10, ct);
            var topOrganizations = await GetTopOrganizationsAsync(10, ct);

            var dashboard = new SystemDashboardDto
            {
                CurrentStats = usageStats.ValueOrDefault(),
                ActiveAlerts = new List<SystemAlertDto>(), // Would need to implement alert system
                RecentActivities = new List<SystemRecentActivityDto>(), // Would need to implement activity tracking
                TopUsers = topUsers.HasValue ? topUsers.ValueOrDefault() : new List<SystemTopUserDto>(),
                TopOrganizations = topOrganizations.HasValue ? topOrganizations.ValueOrDefault() : new List<SystemTopOrganizationDto>()
            };

            return Option.Some<SystemDashboardDto, Error>(dashboard);
        }
        catch (Exception ex)
        {
            return Option.None<SystemDashboardDto, Error>(Error.Failure("SystemAdmin.DashboardFailed", $"Failed to get system dashboard: {ex.Message}"));
        }
    }

    public async Task<Option<FlattenedSystemDashboardDto, Error>> GetFlattenedSystemDashboardAsync(CancellationToken ct = default)
    {
        try
        {
            var totalUsers = await _systemAdminRepository.GetTotalUsersCountAsync(ct: ct);
            var activeUsersToday = await _systemAdminRepository.GetActiveUsersTodayCountAsync(ct);
            
            var todayStart = DateTime.UtcNow.Date;
            var yesterdayStart = todayStart.AddDays(-1);
            var yesterdayEnd = todayStart;
            
            var yesterdayUsers = await _systemAdminRepository.GetUsersByDateRangeAsync(yesterdayStart, yesterdayEnd, ct);
            var activeUsersYesterday = yesterdayUsers.Count(u => u.LastLogin != null && u.LastLogin >= yesterdayStart && u.LastLogin < yesterdayEnd);
            
            var activeTodayChangePct = activeUsersYesterday > 0 
                ? ((double)(activeUsersToday - activeUsersYesterday) / activeUsersYesterday) * 100 
                : (activeUsersToday > 0 ? 100.0 : 0.0);

            var thisMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var lastMonthStart = thisMonthStart.AddMonths(-1);
            var lastMonthEnd = thisMonthStart.AddDays(-1);

            var newUsersThisMonth = await _systemAdminRepository.GetUsersByDateRangeAsync(thisMonthStart, DateTime.UtcNow, ct);
            var newUsersLastMonth = await _systemAdminRepository.GetUsersByDateRangeAsync(lastMonthStart, lastMonthEnd, ct);

            var newSignupsChangePct = newUsersLastMonth.Count > 0
                ? ((double)(newUsersThisMonth.Count - newUsersLastMonth.Count) / newUsersLastMonth.Count) * 100
                : (newUsersThisMonth.Count > 0 ? 100.0 : 0.0);

            var lastMonthEndDate = thisMonthStart.AddDays(-1);
            var usersAtLastMonthEnd = await _systemAdminRepository.GetTotalUsersCountAsync(ct: ct) - newUsersThisMonth.Count;
            var totalUsersChangePct = usersAtLastMonthEnd > 0
                ? ((double)(totalUsers - usersAtLastMonthEnd) / usersAtLastMonthEnd) * 100
                : (totalUsers > 0 ? 100.0 : 0.0);

            var errors24h = 0;
            var errorsPrevious24h = 0;
            var errors24hChangePct = 0.0;

            if (errorsPrevious24h > 0)
            {
                errors24hChangePct = ((double)(errors24h - errorsPrevious24h) / errorsPrevious24h) * 100;
            }

            var dashboard = new FlattenedSystemDashboardDto
            {
                TotalUsers = totalUsers,
                TotalUsersChangePct = totalUsersChangePct,
                ActiveToday = activeUsersToday,
                ActiveTodayChangePct = activeTodayChangePct,
                NewSignups = newUsersThisMonth.Count,
                NewSignupsChangePct = newSignupsChangePct,
                Errors24h = errors24h,
                Errors24hChangePct = errors24hChangePct
            };

            return Option.Some<FlattenedSystemDashboardDto, Error>(dashboard);
        }
        catch (Exception ex)
        {
            return Option.None<FlattenedSystemDashboardDto, Error>(Error.Failure("SystemAdmin.DashboardFailed", $"Failed to get system dashboard: {ex.Message}"));
        }
    }

    public async Task<Option<List<SystemAlertDto>, Error>> GetActiveAlertsAsync(CancellationToken ct = default)
    {
        try
        {
            // Would need to implement alert system
            return Option.Some<List<SystemAlertDto>, Error>(new List<SystemAlertDto>());
        }
        catch (Exception ex)
        {
            return Option.None<List<SystemAlertDto>, Error>(Error.Failure("SystemAdmin.AlertsFailed", $"Failed to get active alerts: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> ResolveAlertAsync(Guid alertId, string resolution, CancellationToken ct = default)
    {
        try
        {
            // Would need to implement alert system
            return Option.Some<bool, Error>(true);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("SystemAdmin.AlertResolveFailed", $"Failed to resolve alert: {ex.Message}"));
        }
    }

    public async Task<Option<List<SystemRecentActivityDto>, Error>> GetRecentActivitiesAsync(int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        try
        {
            // Would need to implement activity tracking system
            return Option.Some<List<SystemRecentActivityDto>, Error>(new List<SystemRecentActivityDto>());
        }
        catch (Exception ex)
        {
            return Option.None<List<SystemRecentActivityDto>, Error>(Error.Failure("SystemAdmin.ActivitiesFailed", $"Failed to get recent activities: {ex.Message}"));
        }
    }

    // System Analytics
    public async Task<Option<Dictionary<string, object>, Error>> GetSystemAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        try
        {
            var analytics = await _systemAdminRepository.GetSystemAnalyticsAsync(startDate, endDate, ct);
            return Option.Some<Dictionary<string, object>, Error>(analytics);
        }
        catch (Exception ex)
        {
            return Option.None<Dictionary<string, object>, Error>(Error.Failure("SystemAdmin.AnalyticsFailed", $"Failed to get system analytics: {ex.Message}"));
        }
    }

    public async Task<Option<List<SystemTopUserDto>, Error>> GetTopUsersAsync(int count = 10, CancellationToken ct = default)
    {
        try
        {
            var topUsers = await _systemAdminRepository.GetTopUsersByActivityAsync(count, ct);

            var userDtos = new List<SystemTopUserDto>();
            foreach (var u in topUsers)
            {
                var totalMaps = await _systemAdminRepository.GetUserTotalMapsAsync(u.UserId, ct);
                var totalExports = await _systemAdminRepository.GetUserTotalExportsAsync(u.UserId, ct);
                var totalSpent = await _systemAdminRepository.GetUserTotalSpentAsync(u.UserId, ct);

                userDtos.Add(new SystemTopUserDto
                {
                    UserId = u.UserId,
                    UserName = u.FullName ?? "Unknown",
                    Email = u.Email,
                    TotalMaps = totalMaps,
                    TotalExports = totalExports,
                    TotalSpent = totalSpent,
                    LastActive = u.LastLogin ?? u.CreatedAt
                });
            }

            return Option.Some<List<SystemTopUserDto>, Error>(userDtos);
        }
        catch (Exception ex)
        {
            return Option.None<List<SystemTopUserDto>, Error>(Error.Failure("SystemAdmin.TopUsersFailed", $"Failed to get top users: {ex.Message}"));
        }
    }

    public async Task<Option<List<SystemTopOrganizationDto>, Error>> GetTopOrganizationsAsync(int count = 10, CancellationToken ct = default)
    {
        try
        {
            var topOrganizations = await _systemAdminRepository.GetTopOrganizationsByActivityAsync(count, ct);

            var organizationDtos = new List<SystemTopOrganizationDto>();
            foreach (var o in topOrganizations)
            {
                var totalMembers = await _systemAdminRepository.GetOrganizationMembersCountAsync(o.OrgId, ct);
                var totalMaps = await _systemAdminRepository.GetOrganizationTotalMapsAsync(o.OrgId, ct);
                var totalExports = await _systemAdminRepository.GetOrganizationTotalExportsAsync(o.OrgId, ct);
                var totalSpent = await _systemAdminRepository.GetOrganizationTotalSpentAsync(o.OrgId, ct);

                var ownerName = "Unknown";
                if (o.Owner != null)
                {
                    ownerName = !string.IsNullOrEmpty(o.Owner.FullName) ? o.Owner.FullName : 
                               (!string.IsNullOrEmpty(o.Owner.Email) ? o.Owner.Email : "Unknown");
                }

                organizationDtos.Add(new SystemTopOrganizationDto
                {
                    OrgId = o.OrgId,
                    Name = o.OrgName,
                    OwnerName = ownerName,
                    TotalMembers = totalMembers,
                    TotalMaps = totalMaps,
                    TotalExports = totalExports,
                    TotalSpent = totalSpent,
                    CreatedAt = o.CreatedAt
                });
            }

            return Option.Some<List<SystemTopOrganizationDto>, Error>(organizationDtos);
        }
        catch (Exception ex)
        {
            return Option.None<List<SystemTopOrganizationDto>, Error>(Error.Failure("SystemAdmin.TopOrganizationsFailed", $"Failed to get top organizations: {ex.Message}"));
        }
    }

    public async Task<Option<Dictionary<string, decimal>, Error>> GetRevenueAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        try
        {
            var revenueByPlan = await _systemAdminRepository.GetRevenueByPlanAsync(startDate, endDate, ct);
            var revenueByGateway = await _systemAdminRepository.GetRevenueByPaymentGatewayAsync(startDate, endDate, ct);

            var analytics = new Dictionary<string, decimal>();
            foreach (var kvp in revenueByPlan)
            {
                analytics[$"revenue_plan_{kvp.Key}"] = kvp.Value;
            }
            foreach (var kvp in revenueByGateway)
            {
                analytics[$"revenue_gateway_{kvp.Key}"] = kvp.Value;
            }

            return Option.Some<Dictionary<string, decimal>, Error>(analytics);
        }
        catch (Exception ex)
        {
            return Option.None<Dictionary<string, decimal>, Error>(Error.Failure("SystemAdmin.RevenueAnalyticsFailed", $"Failed to get revenue analytics: {ex.Message}"));
        }
    }

    public async Task<Option<RevenueAnalyticsDto, Error>> GetDailyRevenueAnalyticsAsync(DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        try
        {
            var transactions = await _systemAdminRepository.GetTransactionsByDateRangeAsync(startDate, endDate, ct);
            var successfulTransactions = transactions.Where(t => t.Status == "completed" || t.Status == "paid" || t.Status == "success").ToList();

            var dailyRevenueMap = new Dictionary<string, DailyRevenueDto>();
            
            // Initialize all days in range
            var currentDate = startDate.Date;
            var endDateOnly = endDate.Date;
            while (currentDate <= endDateOnly)
            {
                var dateStr = currentDate.ToString("yyyy-MM-dd");
                dailyRevenueMap[dateStr] = new DailyRevenueDto
                {
                    Date = dateStr,
                    Value = 0,
                    TransactionCount = 0,
                    RevenueByPlan = new Dictionary<string, decimal>(),
                    RevenueByPaymentGateway = new Dictionary<string, decimal>()
                };
                currentDate = currentDate.AddDays(1);
            }

            // Group transactions by date
            foreach (var transaction in successfulTransactions)
            {
                var dateStr = transaction.TransactionDate.Date.ToString("yyyy-MM-dd");
                if (!dailyRevenueMap.ContainsKey(dateStr))
                {
                    dailyRevenueMap[dateStr] = new DailyRevenueDto
                    {
                        Date = dateStr,
                        Value = 0,
                        TransactionCount = 0,
                        RevenueByPlan = new Dictionary<string, decimal>(),
                        RevenueByPaymentGateway = new Dictionary<string, decimal>()
                    };
                }

                var daily = dailyRevenueMap[dateStr];
                daily.Value += transaction.Amount;
                daily.TransactionCount++;

                // Revenue by plan
                var planName = transaction.Membership?.Plan?.PlanName ?? "Unknown";
                if (!daily.RevenueByPlan.ContainsKey(planName))
                {
                    daily.RevenueByPlan[planName] = 0;
                }
                daily.RevenueByPlan[planName] += transaction.Amount;

                // Revenue by payment gateway
                var gatewayName = transaction.PaymentGateway?.Name ?? "Unknown";
                if (!daily.RevenueByPaymentGateway.ContainsKey(gatewayName))
                {
                    daily.RevenueByPaymentGateway[gatewayName] = 0;
                }
                daily.RevenueByPaymentGateway[gatewayName] += transaction.Amount;
            }

            var dailyRevenue = dailyRevenueMap.Values.OrderBy(d => d.Date).ToList();

            // Calculate summary statistics
            var totalRevenue = dailyRevenue.Sum(d => d.Value);
            var totalTransactions = dailyRevenue.Sum(d => d.TransactionCount);
            var averageTransactionValue = totalTransactions > 0 ? totalRevenue / totalTransactions : 0;

            // Aggregate revenue by plan and payment gateway
            var revenueByPlan = new Dictionary<string, decimal>();
            var revenueByPaymentGateway = new Dictionary<string, decimal>();

            foreach (var daily in dailyRevenue)
            {
                foreach (var kvp in daily.RevenueByPlan)
                {
                    if (!revenueByPlan.ContainsKey(kvp.Key))
                    {
                        revenueByPlan[kvp.Key] = 0;
                    }
                    revenueByPlan[kvp.Key] += kvp.Value;
                }

                foreach (var kvp in daily.RevenueByPaymentGateway)
                {
                    if (!revenueByPaymentGateway.ContainsKey(kvp.Key))
                    {
                        revenueByPaymentGateway[kvp.Key] = 0;
                    }
                    revenueByPaymentGateway[kvp.Key] += kvp.Value;
                }
            }

            var analytics = new RevenueAnalyticsDto
            {
                DailyRevenue = dailyRevenue,
                TotalRevenue = totalRevenue,
                TotalTransactions = totalTransactions,
                AverageTransactionValue = averageTransactionValue,
                RevenueByPlan = revenueByPlan,
                RevenueByPaymentGateway = revenueByPaymentGateway,
                StartDate = startDate,
                EndDate = endDate
            };

            return Option.Some<RevenueAnalyticsDto, Error>(analytics);
        }
        catch (Exception ex)
        {
            return Option.None<RevenueAnalyticsDto, Error>(Error.Failure("SystemAdmin.DailyRevenueAnalyticsFailed", $"Failed to get daily revenue analytics: {ex.Message}"));
        }
    }

    // System Maintenance
    public async Task<Option<bool, Error>> PerformSystemMaintenanceAsync(string maintenanceType, CancellationToken ct = default)
    {
        try
        {
            var success = await _systemAdminRepository.PerformSystemMaintenanceAsync(maintenanceType, ct);
            return Option.Some<bool, Error>(success);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("SystemAdmin.MaintenanceFailed", $"Failed to perform system maintenance: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> ClearSystemCacheAsync(CancellationToken ct = default)
    {
        try
        {
            var success = await _systemAdminRepository.ClearSystemCacheAsync(ct);
            return Option.Some<bool, Error>(success);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("SystemAdmin.CacheClearFailed", $"Failed to clear system cache: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> BackupSystemDataAsync(CancellationToken ct = default)
    {
        try
        {
            var success = await _systemAdminRepository.BackupSystemDataAsync(ct);
            return Option.Some<bool, Error>(success);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("SystemAdmin.BackupFailed", $"Failed to backup system data: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> RestoreSystemDataAsync(string backupId, CancellationToken ct = default)
    {
        try
        {
            var success = await _systemAdminRepository.RestoreSystemDataAsync(backupId, ct);
            return Option.Some<bool, Error>(success);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("SystemAdmin.RestoreFailed", $"Failed to restore system data: {ex.Message}"));
        }
    }

    // System Configuration
    public async Task<Option<Dictionary<string, object>, Error>> GetSystemConfigurationAsync(CancellationToken ct = default)
    {
        try
        {
            var configuration = await _systemAdminRepository.GetSystemConfigurationAsync(ct);
            return Option.Some<Dictionary<string, object>, Error>(configuration);
        }
        catch (Exception ex)
        {
            return Option.None<Dictionary<string, object>, Error>(Error.Failure("SystemAdmin.ConfigurationGetFailed", $"Failed to get system configuration: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> UpdateSystemConfigurationAsync(Dictionary<string, object> configuration, CancellationToken ct = default)
    {
        try
        {
            var success = await _systemAdminRepository.UpdateSystemConfigurationAsync(configuration, ct);
            return Option.Some<bool, Error>(success);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("SystemAdmin.ConfigurationUpdateFailed", $"Failed to update system configuration: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> ResetSystemConfigurationAsync(CancellationToken ct = default)
    {
        try
        {
            var success = await _systemAdminRepository.ResetSystemConfigurationAsync(ct);
            return Option.Some<bool, Error>(success);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("SystemAdmin.ConfigurationResetFailed", $"Failed to reset system configuration: {ex.Message}"));
        }
    }

    // Authorization Helpers
    public async Task<Option<bool, Error>> IsUserSystemAdminAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            var user = await _authenticationRepository.GetUserById(userId);
            return Option.Some<bool, Error>(user != null && user.Role == UserRoleEnum.Admin);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("SystemAdmin.AuthorizationFailed", $"Failed to check system admin status: {ex.Message}"));
        }
    }

    public async Task<Option<bool, Error>> IsUserSuperAdminAsync(Guid userId, CancellationToken ct = default)
    {
        try
        {
            var user = await _authenticationRepository.GetUserById(userId);
            return Option.Some<bool, Error>(user != null && user.Role == UserRoleEnum.Admin);
        }
        catch (Exception ex)
        {
            return Option.None<bool, Error>(Error.Failure("SystemAdmin.AuthorizationFailed", $"Failed to check super admin status: {ex.Message}"));
        }
    }
}
