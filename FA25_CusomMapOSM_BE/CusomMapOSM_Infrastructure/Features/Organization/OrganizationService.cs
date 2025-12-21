using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Organization;
using CusomMapOSM_Application.Interfaces.Features.Membership;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Organization.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Organization.Response;
using CusomMapOSM_Application.Models.DTOs.Services;
using CusomMapOSM_Application.Models.Templates.Email;
using CusomMapOSM_Domain.Entities.Organizations;
using CusomMapOSM_Domain.Entities.Organizations.Enums;
using CusomMapOSM_Domain.Entities.Users;
using CusomMapOSM_Domain.Entities.Users.Enums;
using CusomMapOSM_Domain.Entities.Workspaces.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Authentication;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Organization;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Type;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Membership;
using CusomMapOSM_Infrastructure.Services;
using CusomMapOSM_Application.Interfaces.Services.Jwt;
using Optional;
using Optional.Unsafe;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Workspaces;
using OfficeOpenXml;

namespace CusomMapOSM_Infrastructure.Features.Organization;

public class OrganizationService : IOrganizationService
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IAuthenticationRepository _authenticationRepository;
    private readonly ITypeRepository _typeRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly HangfireEmailService _hangfireEmailService;
    private readonly IMembershipService _membershipService;
    private readonly IJwtService _jwtService;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IMembershipRepository _membershipRepository;

    public OrganizationService(IOrganizationRepository organizationRepository,
        IAuthenticationRepository authenticationRepository, ITypeRepository typeRepository,
        ICurrentUserService currentUserService, HangfireEmailService hangfireEmailService,
        IMembershipService membershipService, IJwtService jwtService,
        IWorkspaceRepository workspaceRepository, IMembershipRepository membershipRepository)
    {
        _organizationRepository = organizationRepository;
        _authenticationRepository = authenticationRepository;
        _typeRepository = typeRepository;
        _currentUserService = currentUserService;
        _hangfireEmailService = hangfireEmailService;
        _membershipService = membershipService;
        _jwtService = jwtService;
        _workspaceRepository = workspaceRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<Option<OrganizationResDto, Error>> Create(OrganizationReqDto req)
    {
        var currentUserId = _currentUserService.GetUserId()!.Value;

        // Validate organization name uniqueness
        if (string.IsNullOrWhiteSpace(req.OrgName))
        {
            return Option.None<OrganizationResDto, Error>(
                Error.ValidationError("Organization.InvalidName", "Organization name cannot be empty"));
        }

        var nameExists = await _organizationRepository.IsOrganizationNameExists(req.OrgName, null);
        if (nameExists)
        {
            return Option.None<OrganizationResDto, Error>(
                Error.Conflict("Organization.NameAlreadyExists",
                    $"Organization name '{req.OrgName}' is already taken"));
        }

        var newOrg = new CusomMapOSM_Domain.Entities.Organizations.Organization()
        {
            OrgName = req.OrgName,
            Abbreviation = req.Abbreviation,
            Description = req.Description,
            LogoUrl = req.LogoUrl,
            ContactEmail = req.ContactEmail,
            ContactPhone = req.ContactPhone,
            Address = req.Address,
            OwnerUserId = currentUserId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
        };

        var createResult = await _organizationRepository.CreateOrganization(newOrg);
        if (!createResult)
        {
            return Option.None<OrganizationResDto, Error>(
                Error.Failure("Organization.CreateFailed", "Failed to create organization"));
        }

        // Get the created organization to get its ID
        var createdOrg = await _organizationRepository.GetOrganizationById(newOrg.OrgId);
        if (createdOrg == null)
        {
            return Option.None<OrganizationResDto, Error>(
                Error.Failure("Organization.NotFound", "Created organization not found"));
        }

        // Add creator as organization member with Owner role
        var creatorMember = new OrganizationMember()
        {
            OrgId = createdOrg.OrgId,
            UserId = currentUserId,
            Role = OrganizationMemberTypeEnum.Owner,
            InvitedBy = currentUserId, // Self-invited
            JoinedAt = DateTime.UtcNow,
            Status = MemberStatus.Active
        };

        var memberResult = await _organizationRepository.AddMemberToOrganization(creatorMember);
        if (!memberResult)
        {
            // Log warning but don't fail organization creation
            // The organization is created successfully, member addition is secondary
        }

        // Create free membership for the organization owner
        const int FREE_PLAN_ID = 1; // Free plan ID from database seed data
        var membershipResult = await _membershipService.CreateOrRenewMembershipAsync(
            currentUserId,
            createdOrg.OrgId,
            FREE_PLAN_ID,
            autoRenew: false, // Free plan doesn't auto-renew
            CancellationToken.None);

        if (!membershipResult.HasValue)
        {
            // Log warning but don't fail organization creation
            // The organization is created successfully, membership creation is secondary
            var error = membershipResult.Match(
                some: _ => (Error)null!,
                none: err => err
            );
        }
        else
        {
            // ✅ FIX: Track owner as first active user in organization
            // This increments ActiveUsersInOrg from 0 to 1
            var membership = membershipResult.Match(some: m => m, none: _ => null!);
            if (membership != null)
            {
                var quotaResult = await _membershipService.TryConsumeQuotaAsync(
                    membership.MembershipId,
                    createdOrg.OrgId,
                    "users",
                    1, // Owner counts as 1 active user
                    CancellationToken.None);

                if (!quotaResult.HasValue)
                {
                    // Log warning but don't fail organization creation
                    Console.WriteLine($"Failed to consume user quota for organization owner {createdOrg.OrgId}");
                }
            }
        }

        // Auto-create default workspace for the organization
        try
        {
            var currentUser = await _authenticationRepository.GetUserById(currentUserId);
            if (currentUser != null)
            {
                var defaultWorkspace = new CusomMapOSM_Domain.Entities.Workspaces.Workspace
                {
                    WorkspaceId = Guid.NewGuid(),
                    WorkspaceName = $"{req.OrgName} Workspace",
                    Description = "Default workspace created automatically",
                    OrgId = createdOrg.OrgId,
                    CreatedBy = currentUserId,
                    Creator = currentUser,
                    Access = WorkspaceAccessEnum.AllMembers,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _workspaceRepository.CreateAsync(defaultWorkspace);
            }
        }
        catch (Exception ex)
        {
            // Log warning but don't fail organization creation
            Console.WriteLine($"Failed to create default workspace for organization {createdOrg.OrgId}: {ex.Message}");
        }

        return Option.Some<OrganizationResDto, Error>(new OrganizationResDto
        {
            Result = "Create Organization Success",
            OrgId = createdOrg.OrgId
        });
    }

    public async Task<Option<GetAllOrganizationsResDto, Error>> GetAll()
    {
        var organizations = await _organizationRepository.GetAllOrganizations();

        var organizationDtos = organizations.Select(org => new OrganizationDetailDto
        {
            OrgId = org.OrgId,
            OrgName = org.OrgName,
            Abbreviation = org.Abbreviation,
            Description = org.Description,
            LogoUrl = org.LogoUrl,
            ContactEmail = org.ContactEmail,
            ContactPhone = org.ContactPhone,
            Address = org.Address,
            CreatedAt = org.CreatedAt,
            IsActive = org.IsActive
        }).ToList();

        return Option.Some<GetAllOrganizationsResDto, Error>(
            new GetAllOrganizationsResDto { Organizations = organizationDtos });
    }

    public async Task<Option<GetOrganizationByIdResDto, Error>> GetById(Guid id)
    {
        var organization = await _organizationRepository.GetOrganizationById(id);
        if (organization is null)
        {
            return Option.None<GetOrganizationByIdResDto, Error>(
                Error.NotFound("Organization.NotFound", "Organization not found"));
        }

        var organizationDto = new OrganizationDetailDto
        {
            OrgId = organization.OrgId,
            OrgName = organization.OrgName,
            Abbreviation = organization.Abbreviation,
            Description = organization.Description,
            LogoUrl = organization.LogoUrl,
            ContactEmail = organization.ContactEmail,
            ContactPhone = organization.ContactPhone,
            Address = organization.Address,
            CreatedAt = organization.CreatedAt,
            IsActive = organization.IsActive
        };

        return Option.Some<GetOrganizationByIdResDto, Error>(
            new GetOrganizationByIdResDto { Organization = organizationDto });
    }

    public async Task<Option<UpdateOrganizationResDto, Error>> Update(Guid id, OrganizationReqDto req)
    {
        // Check if current user is authenticated
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<UpdateOrganizationResDto, Error>(
                Error.Unauthorized("Organization.Unauthorized", "User not authenticated"));
        }

        var organization = await _organizationRepository.GetOrganizationById(id);
        if (organization is null)
        {
            return Option.None<UpdateOrganizationResDto, Error>(
                Error.NotFound("Organization.NotFound", "Organization not found"));
        }

        // Update organization properties
        organization.OrgName = req.OrgName;
        organization.Abbreviation = req.Abbreviation;
        organization.Description = req.Description;
        organization.LogoUrl = req.LogoUrl;
        organization.ContactEmail = req.ContactEmail;
        organization.ContactPhone = req.ContactPhone;
        organization.Address = req.Address;

        var updateResult = await _organizationRepository.UpdateOrganization(organization);
        if (!updateResult)
        {
            return Option.None<UpdateOrganizationResDto, Error>(
                Error.Failure("Organization.UpdateFailed", "Failed to update organization"));
        }

        return Option.Some<UpdateOrganizationResDto, Error>(
            new UpdateOrganizationResDto { Result = "Organization updated successfully" });
    }

    public async Task<Option<DeleteOrganizationResDto, Error>> Delete(Guid id)
    {
        // Check if current user is authenticated
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<DeleteOrganizationResDto, Error>(
                Error.Unauthorized("Organization.Unauthorized", "User not authenticated"));
        }

        var organization = await _organizationRepository.GetOrganizationById(id);
        if (organization is null)
        {
            return Option.None<DeleteOrganizationResDto, Error>(
                Error.NotFound("Organization.NotFound", "Organization not found"));
        }

        // Check if organization has workspaces
        var workspaces = await _workspaceRepository.GetByOrganizationIdAsync(id);
        var activeWorkspaces = workspaces.Where(w => w.IsActive).ToList();
        if (activeWorkspaces.Any())
        {
            return Option.None<DeleteOrganizationResDto, Error>(
                Error.ValidationError("Organization.HasWorkspaces",
                    "Cannot delete organization while it contains active workspaces. Please delete or move all workspaces first."));
        }

        var deleteResult = await _organizationRepository.DeleteOrganization(id);
        if (!deleteResult)
        {
            return Option.None<DeleteOrganizationResDto, Error>(
                Error.Failure("Organization.DeleteFailed", "Failed to delete organization"));
        }

        return Option.Some<DeleteOrganizationResDto, Error>(
            new DeleteOrganizationResDto { Result = "Organization deleted successfully" });
    }

    public async Task<Option<InviteMemberOrganizationResDto, Error>> InviteMember(InviteMemberOrganizationReqDto req)
    {
        var currentUserId = _currentUserService.GetUserId();

        if (currentUserId is null)
        {
            return Option.None<InviteMemberOrganizationResDto, Error>(
                Error.Unauthorized("Organization.Unauthorized", "User not authenticated"));
        }

        // Parse member type from request
        if (!Enum.TryParse<OrganizationMemberTypeEnum>(req.MemberType, out var memberTypeEnum))
        {
            return Option.None<InviteMemberOrganizationResDto, Error>(
                Error.NotFound("Organization.MemberTypeNotFound", "Invalid member type"));
        }

        var normalizedInviteEmail = req.MemberEmail.Trim().ToLowerInvariant();

        // Check if organization has active membership and quota available
        var organization = await _organizationRepository.GetOrganizationById(req.OrgId);
        if (organization is null)
        {
            return Option.None<InviteMemberOrganizationResDto, Error>(
                Error.NotFound("Organization.NotFound", "Organization not found"));
        }
                // Get organization owner's active membership and check quota
        var organizationOwner = await _organizationRepository.GetOrganizationMemberByUserAndOrg(organization.OwnerUserId, req.OrgId);
        if (organizationOwner == null)
        {
            return Option.None<InviteMemberOrganizationResDto, Error>(
                Error.NotFound("Organization.OwnerNotFound", "Organization owner not found"));
        }

        var activeMembership = await _membershipService.GetMembershipByUserOrgAsync(organization.OwnerUserId, req.OrgId, CancellationToken.None);
        if (!activeMembership.HasValue)
        {
            return Option.None<InviteMemberOrganizationResDto, Error>(
                Error.NotFound("Organization.NoActiveMembership", "Organization has no active membership"));
        }

        var membership = activeMembership.Match(some: m => m, none: _ => null!);
        var membershipWithPlan = await _membershipService.GetCurrentMembershipWithIncludesAsync(organization.OwnerUserId, req.OrgId, CancellationToken.None);
        if (!membershipWithPlan.HasValue)
        {
            return Option.None<InviteMemberOrganizationResDto, Error>(
                Error.NotFound("Organization.PlanNotFound", "Membership plan not found"));
        }

        var plan = membershipWithPlan.Match(some: m => m.Plan, none: _ => null!);
        if (plan == null)
        {
            return Option.None<InviteMemberOrganizationResDto, Error>(
                Error.NotFound("Organization.PlanNotFound", "Membership plan not found"));
        }

        // ✅ REMOVED: No quota check at invitation time
        // Owner can send unlimited invitations - quota is checked only when accepting
        // This allows first-come-first-served behavior when multiple people are invited

        // Check self-invitation and existing member in one place
        var currentUser = await _authenticationRepository.GetUserById(currentUserId.Value);
        var invitedUser = await _authenticationRepository.GetUserByEmail(req.MemberEmail);

        // Check 1: Prevent self-invitation (by email or by user ID)
        if (currentUser != null && !string.IsNullOrEmpty(currentUser.Email))
        {
            var normalizedCurrentEmail = currentUser.Email.Trim().ToLowerInvariant();
            if (normalizedInviteEmail == normalizedCurrentEmail)
            {
                return Option.None<InviteMemberOrganizationResDto, Error>(
                    Error.ValidationError("Organization.CannotInviteSelf",
                        "You cannot invite yourself to the organization"));
            }
        }

        if (invitedUser != null && invitedUser.UserId == currentUserId.Value)
        {
            return Option.None<InviteMemberOrganizationResDto, Error>(
                Error.ValidationError("Organization.CannotInviteSelf",
                    "You cannot invite yourself to the organization"));
        }

        // Check 2: Check if user is already a member of the organization
        if (invitedUser != null)
        {
            var existingMember =
                await _organizationRepository.GetOrganizationMemberByUserAndOrg(invitedUser.UserId, req.OrgId);
            if (existingMember != null && existingMember.Status == MemberStatus.Active)
            {
                return Option.None<InviteMemberOrganizationResDto, Error>(
                    Error.Conflict("Organization.AlreadyMember",
                        "This user is already a member of the organization"));
            }
        }


        // Check for any existing invitation (pending or otherwise) - prevent duplicate invitations
        var existingInvitation = await _organizationRepository.GetInvitationByEmailAndOrg(req.MemberEmail, req.OrgId);
        if (existingInvitation != null)
        {
            if (existingInvitation.Status == InvitationStatus.Pending)
            {
                return Option.None<InviteMemberOrganizationResDto, Error>(
                    Error.Conflict("Organization.InvitationAlreadyExists",
                        "An invitation has already been sent to this email for this organization"));
            }
        }

        var newInvitation = new OrganizationInvitation()
        {
            OrgId = req.OrgId,
            Email = req.MemberEmail,
            InvitedBy = currentUserId.Value,
            Role = memberTypeEnum,
            InvitedAt = DateTime.UtcNow,
            Status = InvitationStatus.Pending,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            InvitationToken = Guid.NewGuid().ToString()
        };

        var invitationResult = await _organizationRepository.InviteMemberToOrganization(newInvitation);
        if (!invitationResult)
        {
            return Option.None<InviteMemberOrganizationResDto, Error>(
                Error.Failure("Organization.InvitationFailed", "Failed to create invitation"));
        }

        // Send email notification asynchronously
        var inviter = await _authenticationRepository.GetUserById(currentUserId.Value);

        var mail = new MailRequest
        {
            ToEmail = req.MemberEmail,
            Subject = $"Invitation to join {organization?.OrgName ?? "Organization"}",
            Body = EmailTemplates.Organization.GetInvitationTemplate(
                inviter?.FullName ?? "Unknown User",
                organization.OrgName,
                req.MemberType)
        };

        _hangfireEmailService.EnqueueEmail(mail);

        return Option.Some<InviteMemberOrganizationResDto, Error>(
            new InviteMemberOrganizationResDto { Result = "Invitation sent successfully" });
    }

    public async Task<Option<AcceptInviteOrganizationResDto, Error>> AcceptInvite(AcceptInviteOrganizationReqDto req)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<AcceptInviteOrganizationResDto, Error>(
                Error.Unauthorized("Organization.Unauthorized", "User not authenticated"));
        }

        var invitation = await _organizationRepository.GetInvitationById(req.InvitationId);
        if (invitation is null)
        {
            return Option.None<AcceptInviteOrganizationResDto, Error>(
                Error.NotFound("Organization.InvitationNotFound", "Invitation not found"));
        }

        if (invitation.Status == InvitationStatus.Accepted)
        {
            return Option.None<AcceptInviteOrganizationResDto, Error>(
                Error.Conflict("Organization.InvitationAlreadyAccepted", "Invitation has already been accepted"));
        }

        if (invitation.Status == InvitationStatus.Expired || invitation.ExpiresAt < DateTime.UtcNow)
        {
            return Option.None<AcceptInviteOrganizationResDto, Error>(
                Error.Conflict("Organization.InvitationExpired", "Invitation has expired"));
        }

        var currentUserEmail = _currentUserService.GetEmail();
        if (currentUserEmail is null || currentUserEmail != invitation.Email)
        {
            return Option.None<AcceptInviteOrganizationResDto, Error>(
                Error.Forbidden("Organization.InvitationNotForUser", "This invitation is not for the current user"));
        }

        var user = await _authenticationRepository.GetUserByEmail(invitation.Email);
        if (user is null)
        {
            return Option.None<AcceptInviteOrganizationResDto, Error>(
                Error.NotFound("Organization.UserNotFound", "User not found"));
        }

        // Check if organization has active membership and quota available
        var organization = await _organizationRepository.GetOrganizationById(invitation.OrgId);
        if (organization is null)
        {
            return Option.None<AcceptInviteOrganizationResDto, Error>(
                Error.NotFound("Organization.NotFound", "Organization not found"));
        }

        // Get organization owner's active membership (since memberships are tied to users, not orgs directly)
        var organizationOwner =
            await _organizationRepository.GetOrganizationMemberByUserAndOrg(organization.OwnerUserId, invitation.OrgId);
        if (organizationOwner == null)
        {
            return Option.None<AcceptInviteOrganizationResDto, Error>(
                Error.NotFound("Organization.OwnerNotFound", "Organization owner not found"));
        }

        var activeMembership = await _membershipService.GetMembershipByUserOrgAsync(organization.OwnerUserId,
            invitation.OrgId, CancellationToken.None);
        if (!activeMembership.HasValue)
        {
            return Option.None<AcceptInviteOrganizationResDto, Error>(
                Error.NotFound("Organization.NoActiveMembership", "Organization has no active membership"));
        }

        var membership = activeMembership.Match(some: m => m, none: _ => null!);
        // Get plan details from the membership
        var membershipWithPlan =
            await _membershipService.GetCurrentMembershipWithIncludesAsync(organization.OwnerUserId, invitation.OrgId,
                CancellationToken.None);
        if (!membershipWithPlan.HasValue)
        {
            return Option.None<AcceptInviteOrganizationResDto, Error>(
                Error.NotFound("Organization.PlanNotFound", "Membership plan not found"));
        }

        var plan = membershipWithPlan.Match(some: m => m.Plan, none: _ => null!);
        if (plan == null)
        {
            return Option.None<AcceptInviteOrganizationResDto, Error>(
                Error.NotFound("Organization.PlanNotFound", "Membership plan not found"));
        }

        // Check current user count and quota limits
        var currentMembers = await _organizationRepository.GetOrganizationMembers(invitation.OrgId);
        var currentUserCount = currentMembers.Count(m => m.Status == MemberStatus.Active);

        // Check if adding this user would exceed quota (only if plan has user limits)
        // This implements first-come-first-served: quota is checked at acceptance time, not invitation time
        if (plan.MaxUsersPerOrg > 0 && currentUserCount >= plan.MaxUsersPerOrg)
        {
            return Option.None<AcceptInviteOrganizationResDto, Error>(
                Error.Conflict("Organization.UserQuotaExceeded",
                    $"Unable to accept invitation. Organization is now full - all {plan.MaxUsersPerOrg} member slots are occupied. Another member may have joined before you."));
        }

        // Check if member already exists (even if removed/left)
        var existingMember =
            await _organizationRepository.GetOrganizationMemberByUserAndOrgAnyStatus(user.UserId, invitation.OrgId);

        bool memberResult;
        bool isReactivation = existingMember != null;

        if (existingMember != null)
        {
            // Member exists but was removed/left - reactivate them
            // First check and consume quota (since they were removed, quota was released)
            var quotaResult = await _membershipService.TryConsumeQuotaAsync(
                membership.MembershipId,
                invitation.OrgId,
                "users",
                1,
                CancellationToken.None);

            if (!quotaResult.HasValue)
            {
                return Option.None<AcceptInviteOrganizationResDto, Error>(
                    Error.Failure("Organization.QuotaConsumptionFailed",
                        "Failed to consume user quota for reactivated member"));
            }

            // Update existing member record
            existingMember.Status = MemberStatus.Active;
            existingMember.Role = invitation.Role;
            existingMember.InvitationId = invitation.InvitationId;
            existingMember.InvitedBy = invitation.InvitedBy;
            existingMember.JoinedAt = DateTime.UtcNow; // Update join date to now
            existingMember.LeftAt = null; // Clear left date
            existingMember.LeaveReason = null; // Clear leave reason

            memberResult = await _organizationRepository.UpdateOrganizationMember(existingMember);

            if (!memberResult)
            {
                // Rollback quota if update fails
                await _membershipService.TryConsumeQuotaAsync(
                    membership.MembershipId,
                    invitation.OrgId,
                    "users",
                    -1, // Negative amount to release quota
                    CancellationToken.None);
            }
        }
        else
        {
            // New member - create new record
            // Consume user quota first
            var quotaResult = await _membershipService.TryConsumeQuotaAsync(
                membership.MembershipId,
                invitation.OrgId,
                "users",
                1,
                CancellationToken.None);

            if (!quotaResult.HasValue)
            {
                return Option.None<AcceptInviteOrganizationResDto, Error>(
                    Error.Failure("Organization.QuotaConsumptionFailed", "Failed to consume user quota"));
            }

            var newMember = new OrganizationMember()
            {
                OrgId = invitation.OrgId,
                UserId = user.UserId,
                Role = invitation.Role,
                InvitationId = invitation.InvitationId,
                InvitedBy = invitation.InvitedBy,
                JoinedAt = DateTime.UtcNow,
                Status = MemberStatus.Active
            };

            memberResult = await _organizationRepository.AddMemberToOrganization(newMember);

            if (!memberResult)
            {
                // Rollback quota consumption if member addition fails
                await _membershipService.TryConsumeQuotaAsync(
                    membership.MembershipId,
                    invitation.OrgId,
                    "users",
                    -1, // Negative amount to release quota
                    CancellationToken.None);
            }
        }

        if (!memberResult)
        {
            return Option.None<AcceptInviteOrganizationResDto, Error>(
                Error.Failure("Organization.MemberAddFailed",
                    isReactivation
                        ? "Failed to reactivate member in organization"
                        : "Failed to add member to organization"));
        }

        invitation.Status = InvitationStatus.Accepted;
        invitation.RespondedAt = DateTime.UtcNow;

        var updateResult = await _organizationRepository.UpdateInvitation(invitation);
        if (!updateResult)
        {
            return Option.None<AcceptInviteOrganizationResDto, Error>(
                Error.Failure("Organization.InvitationUpdateFailed", "Failed to update invitation status"));
        }

        return Option.Some<AcceptInviteOrganizationResDto, Error>(
            new AcceptInviteOrganizationResDto { Result = "Invitation accepted successfully" });
    }

    public async Task<Option<GetInvitationsResDto, Error>> GetMyInvitations()
    {
        var currentUserEmail = _currentUserService.GetEmail();
        if (currentUserEmail is null)
        {
            return Option.None<GetInvitationsResDto, Error>(
                Error.Unauthorized("Organization.Unauthorized", "User not authenticated"));
        }

        var invitations = await _organizationRepository.GetInvitationsByEmail(currentUserEmail);

        var invitationDtos = invitations.Select(invitation => new InvitationDto
        {
            InvitationId = invitation.InvitationId,
            OrgId = invitation.OrgId,
            OrgName = invitation.Organization?.OrgName ?? "Unknown Organization",
            Email = invitation.Email,
            InviterEmail = invitation.Inviter?.Email ?? "Unknown User",
            MemberType = invitation.Role.ToString(),
            InvitedAt = invitation.InvitedAt,
            IsAccepted = invitation.Status == InvitationStatus.Accepted,
            AcceptedAt = invitation.RespondedAt
        }).ToList();

        return Option.Some<GetInvitationsResDto, Error>(
            new GetInvitationsResDto { Invitations = invitationDtos });
    }

    public async Task<Option<GetOrganizationMembersResDto, Error>> GetMembers(Guid orgId)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<GetOrganizationMembersResDto, Error>(
                Error.Unauthorized("Organization.Unauthorized", "User not authenticated"));
        }

        var organization = await _organizationRepository.GetOrganizationById(orgId);
        if (organization is null)
        {
            return Option.None<GetOrganizationMembersResDto, Error>(
                Error.NotFound("Organization.NotFound", "Organization not found"));
        }

        var members = await _organizationRepository.GetOrganizationMembers(orgId);

        var memberDtos = members.Select(member => new MemberDto
        {
            UserId = member.UserId,
            MemberId = member.MemberId,
            Email = member.User?.Email ?? "Unknown Email",
            FullName = member.User?.FullName ?? "Unknown User",
            Role = member.Role.ToString(),
            JoinedAt = member.JoinedAt,
            IsActive = member.Status == MemberStatus.Active
        }).ToList();

        return Option.Some<GetOrganizationMembersResDto, Error>(
            new GetOrganizationMembersResDto { Members = memberDtos });
    }

    public async Task<Option<UpdateMemberRoleResDto, Error>> UpdateMemberRole(UpdateMemberRoleReqDto req)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<UpdateMemberRoleResDto, Error>(
                Error.Unauthorized("Organization.Unauthorized", "User not authenticated"));
        }

        var currentUserRole =
            await _organizationRepository.GetOrganizationMemberByUserAndOrg(currentUserId.Value, req.OrgId);
        if (currentUserRole is null || currentUserRole.Role.ToString() != "Owner")
        {
            return Option.None<UpdateMemberRoleResDto, Error>(
                Error.Forbidden("Organization.NotOwner", "Only the owner can update member roles"));
        }

        var member = await _organizationRepository.GetOrganizationMemberById(req.MemberId);
        if (member is null)
        {
            return Option.None<UpdateMemberRoleResDto, Error>(
                Error.NotFound("Organization.MemberNotFound", "Member not found"));
        }

        if (member.OrgId != req.OrgId)
        {
            return Option.None<UpdateMemberRoleResDto, Error>(
                Error.Failure("Organization.MemberNotInOrganization",
                    "Member does not belong to this organization"));
        }

        // Parse new role from request
        if (!Enum.TryParse<OrganizationMemberTypeEnum>(req.NewRole, out var newRoleEnum))
        {
            return Option.None<UpdateMemberRoleResDto, Error>(
                Error.NotFound("Organization.RoleNotFound", "Invalid role"));
        }

        member.Role = newRoleEnum;
        var updateResult = await _organizationRepository.UpdateOrganizationMember(member);

        if (!updateResult)
        {
            return Option.None<UpdateMemberRoleResDto, Error>(
                Error.Failure("Organization.UpdateMemberFailed", "Failed to update member role"));
        }

        return Option.Some<UpdateMemberRoleResDto, Error>(
            new UpdateMemberRoleResDto { Result = "Member role updated successfully" });
    }

    public async Task<Option<RemoveMemberResDto, Error>> RemoveMember(RemoveMemberReqDto req)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<RemoveMemberResDto, Error>(
                Error.Unauthorized("Organization.Unauthorized", "User not authenticated"));
        }

        var currentUserRole =
            await _organizationRepository.GetOrganizationMemberByUserAndOrg(currentUserId.Value, req.OrgId);
        if (currentUserRole is null || currentUserRole.Role.ToString() != "Owner")
        {
            return Option.None<RemoveMemberResDto, Error>(
                Error.Forbidden("Organization.NotOwner", "Only the owner can remove members"));
        }

        var member = await _organizationRepository.GetOrganizationMemberById(req.MemberId);
        if (member is null)
        {
            return Option.None<RemoveMemberResDto, Error>(
                Error.NotFound("Organization.MemberNotFound", "Member not found"));
        }

        if (member.OrgId != req.OrgId)
        {
            return Option.None<RemoveMemberResDto, Error>(
                Error.Failure("Organization.MemberNotInOrganization",
                    "Member does not belong to this organization"));
        }

        // Only release quota if member is currently Active (not already Removed/Left)
        bool shouldReleaseQuota = member.Status == MemberStatus.Active;

        // Get organization owner's active membership to release quota
        if (shouldReleaseQuota)
        {
            var organization = await _organizationRepository.GetOrganizationById(req.OrgId);
            if (organization != null)
            {
                var activeMembership =
                    await _membershipService.GetMembershipByUserOrgAsync(organization.OwnerUserId, req.OrgId,
                        CancellationToken.None);
                if (activeMembership.HasValue)
                {
                    var membership = activeMembership.Match(some: m => m, none: _ => null!);

                    // Release user quota when removing active member
                    var quotaResult = await _membershipService.TryConsumeQuotaAsync(
                        membership.MembershipId,
                        req.OrgId,
                        "users",
                        -1, // Negative amount to release quota
                        CancellationToken.None);

                    if (!quotaResult.HasValue)
                    {
                        // Log warning but don't fail the removal - quota release is secondary
                        Console.WriteLine(
                            $"Failed to release user quota for organization {req.OrgId} when removing member {req.MemberId}");
                    }
                }
            }
        }

        var removeResult = await _organizationRepository.RemoveOrganizationMember(req.MemberId);
        if (!removeResult)
        {
            // If removal fails and we released quota, rollback the quota
            if (shouldReleaseQuota)
            {
                var organization = await _organizationRepository.GetOrganizationById(req.OrgId);
                if (organization != null)
                {
                    var activeMembership =
                        await _membershipService.GetMembershipByUserOrgAsync(organization.OwnerUserId, req.OrgId,
                            CancellationToken.None);
                    if (activeMembership.HasValue)
                    {
                        var membership = activeMembership.Match(some: m => m, none: _ => null!);
                        await _membershipService.TryConsumeQuotaAsync(
                            membership.MembershipId,
                            req.OrgId,
                            "users",
                            1, // Rollback: add quota back
                            CancellationToken.None);
                    }
                }
            }

            return Option.None<RemoveMemberResDto, Error>(
                Error.Failure("Organization.RemoveMemberFailed", "Failed to remove member"));
        }

        return Option.Some<RemoveMemberResDto, Error>(
            new RemoveMemberResDto { Result = "Member removed successfully" });
    }

    public async Task<Option<RejectInviteOrganizationResDto, Error>> RejectInvite(RejectInviteOrganizationReqDto req)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<RejectInviteOrganizationResDto, Error>(
                Error.Unauthorized("Organization.Unauthorized", "User not authenticated"));
        }

        var invitation = await _organizationRepository.GetInvitationById(req.InvitationId);
        if (invitation is null)
        {
            return Option.None<RejectInviteOrganizationResDto, Error>(
                Error.NotFound("Organization.InvitationNotFound", "Invitation not found"));
        }

        if (invitation.Status == InvitationStatus.Accepted)
        {
            return Option.None<RejectInviteOrganizationResDto, Error>(
                Error.Conflict("Organization.InvitationAlreadyAccepted", "Invitation has already been accepted"));
        }

        var currentUserEmail = _currentUserService.GetEmail();
        if (currentUserEmail is null || currentUserEmail != invitation.Email)
        {
            return Option.None<RejectInviteOrganizationResDto, Error>(
                Error.Forbidden("Organization.InvitationNotForUser", "This invitation is not for the current user"));
        }

        var deleteResult = await _organizationRepository.DeleteInvitation(req.InvitationId);
        if (!deleteResult)
        {
            return Option.None<RejectInviteOrganizationResDto, Error>(
                Error.Failure("Organization.RejectInviteFailed", "Failed to reject invitation"));
        }

        return Option.Some<RejectInviteOrganizationResDto, Error>(
            new RejectInviteOrganizationResDto { Result = "Invitation rejected successfully" });
    }

    public async Task<Option<CancelInviteOrganizationResDto, Error>> CancelInvite(CancelInviteOrganizationReqDto req)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<CancelInviteOrganizationResDto, Error>(
                Error.Unauthorized("Organization.Unauthorized", "User not authenticated"));
        }

        var invitation = await _organizationRepository.GetInvitationById(req.InvitationId);
        if (invitation is null)
        {
            return Option.None<CancelInviteOrganizationResDto, Error>(
                Error.NotFound("Organization.InvitationNotFound", "Invitation not found"));
        }

        if (invitation.Status == InvitationStatus.Accepted)
        {
            return Option.None<CancelInviteOrganizationResDto, Error>(
                Error.Conflict("Organization.InvitationAlreadyAccepted", "Cannot cancel an accepted invitation"));
        }

        if (invitation.InvitedBy != currentUserId.Value)
        {
            return Option.None<CancelInviteOrganizationResDto, Error>(
                Error.Forbidden("Organization.CannotCancelInvitation", "You can only cancel invitations you sent"));
        }

        var deleteResult = await _organizationRepository.DeleteInvitation(req.InvitationId);
        if (!deleteResult)
        {
            return Option.None<CancelInviteOrganizationResDto, Error>(
                Error.Failure("Organization.CancelInviteFailed", "Failed to cancel invitation"));
        }

        return Option.Some<CancelInviteOrganizationResDto, Error>(
            new CancelInviteOrganizationResDto { Result = "Invitation cancelled successfully" });
    }

    public async Task<Option<GetMyOrganizationsResDto, Error>> GetMyOrganizations()
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<GetMyOrganizationsResDto, Error>(
                Error.Unauthorized("Organization.Unauthorized", "User not authenticated"));
        }

        var myOrganizations = await _organizationRepository.GetUserOrganizations(currentUserId.Value);

        var organizationDtos = myOrganizations.Select(member => new MyOrganizationDto
        {
            OrgId = member.Organization.OrgId,
            OrgName = member.Organization.OrgName,
            Abbreviation = member.Organization.Abbreviation,
            MyRole = member.Role.ToString(),
            JoinedAt = member.JoinedAt,
            LogoUrl = member.Organization.LogoUrl
        }).ToList();

        return Option.Some<GetMyOrganizationsResDto, Error>(
            new GetMyOrganizationsResDto { Organizations = organizationDtos });
    }

    public async Task<Option<TransferOwnershipResDto, Error>> TransferOwnership(Guid orgId, TransferOwnershipReqDto req)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<TransferOwnershipResDto, Error>(
                Error.Unauthorized("Organization.Unauthorized", "User not authenticated"));
        }

        // Prevent self-transfer
        if (req.NewOwnerId == currentUserId.Value)
        {
            return Option.None<TransferOwnershipResDto, Error>(
                Error.ValidationError("Organization.CannotTransferToSelf",
                    "You cannot transfer ownership to yourself"));
        }

        var organization = await _organizationRepository.GetOrganizationById(orgId);
        if (organization is null)
        {
            return Option.None<TransferOwnershipResDto, Error>(
                Error.NotFound("Organization.NotFound", "Organization not found"));
        }

        var newOwnerMember = await _organizationRepository.GetOrganizationMemberByUserAndOrg(req.NewOwnerId, orgId);
        if (newOwnerMember is null)
        {
            return Option.None<TransferOwnershipResDto, Error>(
                Error.NotFound("Organization.NewOwnerNotMember", "New owner is not a member of this organization"));
        }

        var currentOwnerMember =
            await _organizationRepository.GetOrganizationMemberByUserAndOrg(currentUserId.Value, orgId);
        if (currentOwnerMember is null || currentOwnerMember.Role.ToString() != "Owner")
        {
            return Option.None<TransferOwnershipResDto, Error>(Error.Forbidden("Organization.NotOwner",
                "Only the current owner can transfer ownership"));
        }

        // Store original roles for rollback
        var originalNewOwnerRole = newOwnerMember.Role;
        var originalCurrentOwnerRole = currentOwnerMember.Role;
        CusomMapOSM_Domain.Entities.Memberships.Membership? oldOwnerMembership = null;
        Guid? originalMembershipUserId = null;

        // Use transaction-like approach to ensure atomicity
        try
        {
            // Update new owner's role to Owner
            newOwnerMember.Role = OrganizationMemberTypeEnum.Owner;
            var updateNewOwnerResult = await _organizationRepository.UpdateOrganizationMember(newOwnerMember);

            if (!updateNewOwnerResult)
            {
                return Option.None<TransferOwnershipResDto, Error>(
                    Error.Failure("Organization.TransferOwnershipFailed", "Failed to update new owner role"));
            }

            // Demote current owner to Admin (keep them as admin for continued access)
            currentOwnerMember.Role = OrganizationMemberTypeEnum.Member;
            var updateCurrentOwnerResult = await _organizationRepository.UpdateOrganizationMember(currentOwnerMember);

            if (!updateCurrentOwnerResult)
            {
                // Rollback new owner role change
                newOwnerMember.Role = originalNewOwnerRole;
                await _organizationRepository.UpdateOrganizationMember(newOwnerMember);

                return Option.None<TransferOwnershipResDto, Error>(
                    Error.Failure("Organization.TransferOwnershipFailed", "Failed to update current owner role"));
            }

            // Update organization owner
            organization.OwnerUserId = req.NewOwnerId;
            var updateOrganizationResult = await _organizationRepository.UpdateOrganization(organization);

            if (!updateOrganizationResult)
            {
                // Rollback member role changes
                newOwnerMember.Role = originalNewOwnerRole;
                await _organizationRepository.UpdateOrganizationMember(newOwnerMember);
                currentOwnerMember.Role = originalCurrentOwnerRole;
                await _organizationRepository.UpdateOrganizationMember(currentOwnerMember);

                return Option.None<TransferOwnershipResDto, Error>(
                    Error.Failure("Organization.TransferOwnershipFailed", "Failed to update organization owner"));
            }

            // Transfer membership from old owner to new owner
            // Find membership owned by old owner for this organization
            oldOwnerMembership =
                await _membershipRepository.GetByUserOrgAsync(currentUserId.Value, orgId, CancellationToken.None);
            if (oldOwnerMembership != null)
            {
                // Store original UserId for rollback
                originalMembershipUserId = oldOwnerMembership.UserId;

                // Update membership to belong to new owner
                oldOwnerMembership.UserId = req.NewOwnerId;
                oldOwnerMembership.UpdatedAt = DateTime.UtcNow;
                await _membershipRepository.UpsertAsync(oldOwnerMembership, CancellationToken.None);
            }

            return Option.Some<TransferOwnershipResDto, Error>(
                new TransferOwnershipResDto { Result = "Ownership transferred successfully" });
        }
        catch (Exception ex)
        {
            // Attempt rollback on exception
            try
            {
                newOwnerMember.Role = originalNewOwnerRole;
                await _organizationRepository.UpdateOrganizationMember(newOwnerMember);
                currentOwnerMember.Role = originalCurrentOwnerRole;
                await _organizationRepository.UpdateOrganizationMember(currentOwnerMember);
                organization.OwnerUserId = currentUserId.Value; // Restore original owner
                await _organizationRepository.UpdateOrganization(organization);

                // Rollback membership transfer if it was changed
                if (oldOwnerMembership != null && originalMembershipUserId.HasValue &&
                    oldOwnerMembership.UserId != originalMembershipUserId.Value)
                {
                    oldOwnerMembership.UserId = originalMembershipUserId.Value;
                    oldOwnerMembership.UpdatedAt = DateTime.UtcNow;
                    await _membershipRepository.UpsertAsync(oldOwnerMembership, CancellationToken.None);
                }
            }
            catch
            {
                // Log that rollback failed but don't throw
                Console.WriteLine("Failed to rollback ownership transfer changes");
            }

            Console.WriteLine($"Error during ownership transfer: {ex.Message}");
            return Option.None<TransferOwnershipResDto, Error>(
                Error.Failure("Organization.TransferOwnershipFailed", "An error occurred during ownership transfer"));
        }
    }

    public async Task<Option<BulkCreateStudentsResponse, Error>> BulkCreateStudents(IFormFile excelFile,
        BulkCreateStudentsRequest request)
    {
        try
        {
            // Validate file
            if (excelFile == null || excelFile.Length == 0)
            {
                return Option.None<BulkCreateStudentsResponse, Error>(
                    Error.ValidationError("File.Empty", "Excel file is required"));
            }

            // Validate file size (max 10MB)
            const long maxFileSize = 10 * 1024 * 1024; // 10MB in bytes
            if (excelFile.Length > maxFileSize)
            {
                return Option.None<BulkCreateStudentsResponse, Error>(
                    Error.ValidationError("File.TooLarge", "File size exceeds 10MB limit"));
            }

            // Validate file extension
            var allowedExtensions = new[] { ".xlsx", ".xls" };
            var fileExtension = Path.GetExtension(excelFile.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return Option.None<BulkCreateStudentsResponse, Error>(
                    Error.ValidationError("File.InvalidFormat", "Only .xlsx and .xls files are supported"));
            }

            // Validate organization exists
            var organization = await _organizationRepository.GetOrganizationById(request.OrganizationId);
            if (organization == null)
            {
                return Option.None<BulkCreateStudentsResponse, Error>(
                    Error.NotFound("Organization.NotFound", "Organization not found"));
            }

            // Check if current user is owner/admin of organization
            var currentUserId = _currentUserService.GetUserId();
            if (!currentUserId.HasValue)
            {
                return Option.None<BulkCreateStudentsResponse, Error>(
                    Error.Unauthorized("User.NotAuthenticated", "User must be authenticated"));
            }

            var currentUserMember = await _organizationRepository.GetOrganizationMemberByUserAndOrg(
                currentUserId.Value, request.OrganizationId);
            if (currentUserMember == null ||
                (currentUserMember.Role != OrganizationMemberTypeEnum.Owner &&
                 currentUserMember.Role != OrganizationMemberTypeEnum.Member))
            {
                return Option.None<BulkCreateStudentsResponse, Error>(
                    Error.Forbidden("Organization.NotAuthorized",
                        "Only organization owners and admins can bulk create student accounts"));
            }

            // Check organization membership quota
            var activeMembership = await _membershipService.GetMembershipByUserOrgAsync(
                organization.OwnerUserId, request.OrganizationId, CancellationToken.None);
            if (!activeMembership.HasValue)
            {
                return Option.None<BulkCreateStudentsResponse, Error>(
                    Error.NotFound("Organization.NoActiveMembership", "Organization has no active membership"));
            }

            var membership = activeMembership.ValueOrDefault()!;
            var membershipWithPlan = await _membershipService.GetCurrentMembershipWithIncludesAsync(
                organization.OwnerUserId, request.OrganizationId, CancellationToken.None);
            if (!membershipWithPlan.HasValue || membershipWithPlan.ValueOrDefault()?.Plan == null)
            {
                return Option.None<BulkCreateStudentsResponse, Error>(
                    Error.NotFound("Organization.PlanNotFound", "Organization membership plan not found"));
            }

            var plan = membershipWithPlan.ValueOrDefault()!.Plan!;
            var currentMembers = await _organizationRepository.GetOrganizationMembers(request.OrganizationId);
            var currentUserCount = currentMembers.Count(m => m.Status == MemberStatus.Active);

            // Read Excel file and parse student data
            // EPPlus requires license context to be set
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            List<StudentExcelRow> students = new List<StudentExcelRow>();

            using (var stream = new MemoryStream())
            {
                await excelFile.CopyToAsync(stream);
                stream.Position = 0;

                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    if (worksheet == null || worksheet.Dimension == null)
                    {
                        return Option.None<BulkCreateStudentsResponse, Error>(
                            Error.ValidationError("File.NoWorksheet",
                                "Excel file must contain at least one worksheet"));
                    }

                    // Find header row (assume first row is headers)
                    int nameColumnIndex = -1;
                    int classColumnIndex = -1;

                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        var headerValue = worksheet.Cells[1, col].Text?.Trim().ToLower() ?? "";
                        if (headerValue == "name")
                        {
                            nameColumnIndex = col;
                        }
                        else if (headerValue == "class")
                        {
                            classColumnIndex = col;
                        }
                    }

                    if (nameColumnIndex == -1)
                    {
                        return Option.None<BulkCreateStudentsResponse, Error>(
                            Error.ValidationError("File.MissingColumn", "Excel file must contain a 'Name' column"));
                    }

                    if (classColumnIndex == -1)
                    {
                        return Option.None<BulkCreateStudentsResponse, Error>(
                            Error.ValidationError("File.MissingColumn", "Excel file must contain a 'Class' column"));
                    }

                    // Read data rows (skip header row)
                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        var name = worksheet.Cells[row, nameColumnIndex].Text?.Trim();
                        var studentClass = classColumnIndex > 0
                            ? worksheet.Cells[row, classColumnIndex].Text?.Trim()
                            : null;

                        if (string.IsNullOrWhiteSpace(name))
                        {
                            continue; // Skip empty rows
                        }

                        students.Add(new StudentExcelRow
                        {
                            Name = name,
                            Class = studentClass
                        });
                    }
                }
            }

            // Validate student count (max 80)
            const int maxStudents = 80;
            if (students.Count > maxStudents)
            {
                return Option.None<BulkCreateStudentsResponse, Error>(
                    Error.ValidationError("File.TooManyStudents",
                        $"Maximum {maxStudents} students allowed per upload. Found {students.Count}"));
            }

            if (students.Count == 0)
            {
                return Option.None<BulkCreateStudentsResponse, Error>(
                    Error.ValidationError("File.NoStudents", "No valid student data found in Excel file"));
            }

            // Check if adding students would exceed quota
            if (plan.MaxUsersPerOrg > 0 && (currentUserCount + students.Count) > plan.MaxUsersPerOrg)
            {
                return Option.None<BulkCreateStudentsResponse, Error>(
                    Error.Conflict("Organization.UserQuotaExceeded",
                        $"Cannot add {students.Count} students. Organization has {currentUserCount}/{plan.MaxUsersPerOrg} users. " +
                        $"Would exceed limit by {currentUserCount + students.Count - plan.MaxUsersPerOrg} users"));
            }

            // Process students: handle duplicates and create accounts
            var createdAccounts = new List<CreatedStudentAccount>();
            var skippedAccounts = new List<SkippedStudentAccount>();
            var nameCountInClass = new Dictionary<string, Dictionary<string, int>>(); // Class -> Name -> Count

            foreach (var student in students)
            {
                try
                {
                    // Generate email: studentName@domain
                    var cleanName = NormalizeNameForEmail(student.Name);
                    var baseEmail = $"{cleanName}@{request.Domain}";

                    // Handle duplicates within the same class
                    var studentClass = student.Class ?? "default";
                    if (!nameCountInClass.ContainsKey(studentClass))
                    {
                        nameCountInClass[studentClass] = new Dictionary<string, int>();
                    }

                    var nameCounts = nameCountInClass[studentClass];
                    if (!nameCounts.ContainsKey(cleanName))
                    {
                        nameCounts[cleanName] = 0;
                    }

                    string finalEmail = baseEmail;
                    if (nameCounts[cleanName] > 0)
                    {
                        // Second, third, etc. student with same name - add number suffix
                        finalEmail = $"{cleanName}{nameCounts[cleanName]}@{request.Domain}";
                    }

                    nameCounts[cleanName]++;

                    // Check if email already exists
                    var existingUser = await _authenticationRepository.GetUserByEmail(finalEmail);
                    if (existingUser != null)
                    {
                        skippedAccounts.Add(new SkippedStudentAccount
                        {
                            Name = student.Name,
                            Class = studentClass,
                            Reason = $"Email {finalEmail} already exists"
                        });
                        continue;
                    }

                    // Generate password: studentName + number (3-digit sequential number based on row index)
                    var passwordNumber = createdAccounts.Count + skippedAccounts.Count + 1;
                    var password = $"{cleanName}{passwordNumber:D3}"; // e.g., "johndoe001"

                    // Hash password
                    var passwordHash = _jwtService.HashObject<string>(password);

                    // Create user account
                    var newUser = new CusomMapOSM_Domain.Entities.Users.User
                    {
                        UserId = Guid.NewGuid(),
                        Email = finalEmail,
                        PasswordHash = passwordHash,
                        FullName = student.Name,
                        Phone = null,
                        Role = UserRoleEnum.RegisteredUser,
                        AccountStatus = AccountStatusEnum.Active, // Students accounts are auto-activated
                        CreatedAt = DateTime.UtcNow,
                        MonthlyTokenUsage = 0,
                        LastTokenReset = DateTime.UtcNow
                    };

                    var userCreated = await _authenticationRepository.Register(newUser);
                    if (!userCreated)
                    {
                        skippedAccounts.Add(new SkippedStudentAccount
                        {
                            Name = student.Name,
                            Class = studentClass,
                            Reason = "Failed to create user account"
                        });
                        continue;
                    }

                    // Add user as organization member with Viewer role
                    var newMember = new OrganizationMember
                    {
                        MemberId = Guid.NewGuid(),
                        OrgId = request.OrganizationId,
                        UserId = newUser.UserId,
                        Role = OrganizationMemberTypeEnum.Member,
                        InvitedBy = currentUserId.Value,
                        JoinedAt = DateTime.UtcNow,
                        Status = MemberStatus.Active
                    };

                    var memberAdded = await _organizationRepository.AddMemberToOrganization(newMember);
                    if (!memberAdded)
                    {
                        // Rollback: delete user if member addition fails
                        // Note: In production, you might want to handle this differently
                        skippedAccounts.Add(new SkippedStudentAccount
                        {
                            Name = student.Name,
                            Class = studentClass,
                            Reason = "Failed to add user to organization"
                        });
                        continue;
                    }

                    // Consume user quota
                    await _membershipService.TryConsumeQuotaAsync(
                        membership.MembershipId,
                        request.OrganizationId,
                        "users",
                        1,
                        CancellationToken.None);

                    createdAccounts.Add(new CreatedStudentAccount
                    {
                        UserId = newUser.UserId,
                        Email = finalEmail,
                        FullName = student.Name,
                        Password = password, // Return plain password so educator can share it
                        Class = studentClass
                    });
                }
                catch (Exception ex)
                {
                    skippedAccounts.Add(new SkippedStudentAccount
                    {
                        Name = student.Name,
                        Class = student.Class ?? "default",
                        Reason = $"Error: {ex.Message}"
                    });
                }
            }

            return Option.Some<BulkCreateStudentsResponse, Error>(new BulkCreateStudentsResponse
            {
                TotalCreated = createdAccounts.Count,
                TotalSkipped = skippedAccounts.Count,
                CreatedAccounts = createdAccounts,
                SkippedAccounts = skippedAccounts
            });
        }
        catch (Exception ex)
        {
            return Option.None<BulkCreateStudentsResponse, Error>(
                Error.Failure("BulkCreate.Failed", $"Failed to bulk create students: {ex.Message}"));
        }
    }

    public async Task<Option<ValidateOrganizationNameResDto, Error>> ValidateOrganizationName(string orgName,
        Guid? excludeOrgId = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(orgName))
            {
                return Option.None<ValidateOrganizationNameResDto, Error>(
                    Error.ValidationError("Organization.InvalidName", "Organization name cannot be empty"));
            }

            var exists = await _organizationRepository.IsOrganizationNameExists(orgName, excludeOrgId);

            return Option.Some<ValidateOrganizationNameResDto, Error>(new ValidateOrganizationNameResDto
            {
                IsAvailable = !exists,
                Message = exists
                    ? $"Organization name '{orgName}' is already taken"
                    : $"Organization name '{orgName}' is available"
            });
        }
        catch (Exception ex)
        {
            return Option.None<ValidateOrganizationNameResDto, Error>(
                Error.Failure("Organization.ValidationFailed", $"Failed to validate organization name: {ex.Message}"));
        }
    }

    private string NormalizeNameForEmail(string name)
    {
        // Convert to lowercase, remove spaces, and replace special characters
        var normalized = name.ToLower().Trim();
        normalized = Regex.Replace(normalized, @"[^a-z0-9]", ""); // Remove non-alphanumeric
        normalized = Regex.Replace(normalized, @"\s+", ""); // Remove all spaces

        // Replace common Vietnamese characters if needed (optional)
        // normalized = normalized.Replace("đ", "d").Replace("Đ", "d");

        return normalized;
    }

    private class StudentExcelRow
    {
        public string Name { get; set; } = string.Empty;
        public string? Class { get; set; }
    }
}