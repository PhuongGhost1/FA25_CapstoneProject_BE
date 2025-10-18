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
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Authentication;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Organization;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Type;
using CusomMapOSM_Infrastructure.Services;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.Organization;

public class OrganizationService : IOrganizationService
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IAuthenticationRepository _authenticationRepository;
    private readonly ITypeRepository _typeRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly HangfireEmailService _hangfireEmailService;
    private readonly IMembershipService _membershipService;

    public OrganizationService(IOrganizationRepository organizationRepository,
        IAuthenticationRepository authenticationRepository, ITypeRepository typeRepository,
        ICurrentUserService currentUserService, HangfireEmailService hangfireEmailService,
        IMembershipService membershipService)
    {
        _organizationRepository = organizationRepository;
        _authenticationRepository = authenticationRepository;
        _typeRepository = typeRepository;
        _currentUserService = currentUserService;
        _hangfireEmailService = hangfireEmailService;
        _membershipService = membershipService;
    }

    public async Task<Option<OrganizationResDto, Error>> Create(OrganizationReqDto req)
    {
        var currentUserId = _currentUserService.GetUserId()!.Value;

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
            Console.WriteLine($"Failed to create free membership for user {currentUserId} in organization {createdOrg.OrgId}: {error?.Description}");
        }

        return Option.Some<OrganizationResDto, Error>(new OrganizationResDto
        { Result = "Create Organization Success" });
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

        var existingInvitation = await _organizationRepository.GetInvitationByEmailAndOrg(req.MemberEmail, req.OrgId);
        if (existingInvitation != null && existingInvitation.Status == InvitationStatus.Pending)
        {
            return Option.None<InviteMemberOrganizationResDto, Error>(
                Error.Conflict("Organization.InvitationAlreadyExists",
                    "An invitation has already been sent to this email for this organization"));
        }

        // Check if organization has active membership and quota available
        var organization = await _organizationRepository.GetOrganizationById(req.OrgId);
        if (organization is null)
        {
            return Option.None<InviteMemberOrganizationResDto, Error>(
                Error.NotFound("Organization.NotFound", "Organization not found"));
        }

        // Get organization owner's active membership (since memberships are tied to users, not orgs directly)
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
        // Get plan details from the membership
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

        // Check current user count and quota limits before sending invitation
        var currentMembers = await _organizationRepository.GetOrganizationMembers(req.OrgId);
        var currentUserCount = currentMembers.Count(m => m.Status == MemberStatus.Active);

        // Check if adding this user would exceed quota (only if plan has user limits)
        if (plan.MaxUsersPerOrg > 0 && currentUserCount >= plan.MaxUsersPerOrg)
        {
            return Option.None<InviteMemberOrganizationResDto, Error>(
                Error.Conflict("Organization.UserQuotaExceeded",
                    $"Cannot send invitation. Organization has reached the maximum user limit of {plan.MaxUsersPerOrg} users"));
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
        var organizationOwner = await _organizationRepository.GetOrganizationMemberByUserAndOrg(organization.OwnerUserId, invitation.OrgId);
        if (organizationOwner == null)
        {
            return Option.None<AcceptInviteOrganizationResDto, Error>(
                Error.NotFound("Organization.OwnerNotFound", "Organization owner not found"));
        }

        var activeMembership = await _membershipService.GetMembershipByUserOrgAsync(organization.OwnerUserId, invitation.OrgId, CancellationToken.None);
        if (!activeMembership.HasValue)
        {
            return Option.None<AcceptInviteOrganizationResDto, Error>(
                Error.NotFound("Organization.NoActiveMembership", "Organization has no active membership"));
        }

        var membership = activeMembership.Match(some: m => m, none: _ => null!);
        // Get plan details from the membership
        var membershipWithPlan = await _membershipService.GetCurrentMembershipWithIncludesAsync(organization.OwnerUserId, invitation.OrgId, CancellationToken.None);
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
        if (plan.MaxUsersPerOrg > 0 && currentUserCount >= plan.MaxUsersPerOrg)
        {
            return Option.None<AcceptInviteOrganizationResDto, Error>(
                Error.Conflict("Organization.UserQuotaExceeded",
                    $"Organization has reached the maximum user limit of {plan.MaxUsersPerOrg} users"));
        }

        // Consume user quota
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

        var memberResult = await _organizationRepository.AddMemberToOrganization(newMember);
        if (!memberResult)
        {
            // Rollback quota consumption if member addition fails
            await _membershipService.TryConsumeQuotaAsync(
                membership.MembershipId,
                invitation.OrgId,
                "users",
                -1, // Negative amount to release quota
                CancellationToken.None);

            return Option.None<AcceptInviteOrganizationResDto, Error>(
                Error.Failure("Organization.MemberAddFailed", "Failed to add member to organization"));
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

        var currentUserRole = await _organizationRepository.GetOrganizationMemberByUserAndOrg(currentUserId.Value, req.OrgId);
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
        var currentUserRole = await _organizationRepository.GetOrganizationMemberByUserAndOrg(currentUserId.Value, req.OrgId);
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

        // Get organization owner's active membership to release quota
        var organization = await _organizationRepository.GetOrganizationById(req.OrgId);
        if (organization != null)
        {
            var activeMembership = await _membershipService.GetMembershipByUserOrgAsync(organization.OwnerUserId, req.OrgId, CancellationToken.None);
            if (activeMembership.HasValue)
            {
                var membership = activeMembership.Match(some: m => m, none: _ => null!);

                // Release user quota when removing member
                var quotaResult = await _membershipService.TryConsumeQuotaAsync(
                    membership.MembershipId,
                    req.OrgId,
                    "users",
                    -1, // Negative amount to release quota
                    CancellationToken.None);

                if (!quotaResult.HasValue)
                {
                    // Log warning but don't fail the removal - quota release is secondary
                    Console.WriteLine($"Failed to release user quota for organization {req.OrgId} when removing member {req.MemberId}");
                }
            }
        }

        var removeResult = await _organizationRepository.RemoveOrganizationMember(req.MemberId);
        if (!removeResult)
        {
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

    public async Task<Option<TransferOwnershipResDto, Error>> TransferOwnership(TransferOwnershipReqDto req)
    {
        var currentUserId = _currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Option.None<TransferOwnershipResDto, Error>(
                Error.Unauthorized("Organization.Unauthorized", "User not authenticated"));
        }

        var organization = await _organizationRepository.GetOrganizationById(req.OrgId);
        if (organization is null)
        {
            return Option.None<TransferOwnershipResDto, Error>(
                Error.NotFound("Organization.NotFound", "Organization not found"));
        }

        var newOwnerMember = await _organizationRepository.GetOrganizationMemberByUserAndOrg(req.NewOwnerId, req.OrgId);
        if (newOwnerMember is null)
        {
            return Option.None<TransferOwnershipResDto, Error>(
                Error.NotFound("Organization.NewOwnerNotMember", "New owner is not a member of this organization"));
        }

        var currentOwnerMember = await _organizationRepository.GetOrganizationMemberByUserAndOrg(currentUserId.Value, req.OrgId);
        if (currentOwnerMember is null || currentOwnerMember.Role.ToString() != "Owner")
        {
            return Option.None<TransferOwnershipResDto, Error>(Error.Forbidden("Organization.NotOwner", "Only the current owner can transfer ownership"));
        }

        // Update new owner's role to Owner
        newOwnerMember.Role = OrganizationMemberTypeEnum.Owner;
        var updateNewOwnerResult = await _organizationRepository.UpdateOrganizationMember(newOwnerMember);

        if (!updateNewOwnerResult)
        {
            return Option.None<TransferOwnershipResDto, Error>(
                Error.Failure("Organization.TransferOwnershipFailed", "Failed to transfer ownership"));
        }

        // Demote current owner to Admin
        currentOwnerMember.Role = OrganizationMemberTypeEnum.Admin;
        await _organizationRepository.UpdateOrganizationMember(currentOwnerMember);

        return Option.Some<TransferOwnershipResDto, Error>(
            new TransferOwnershipResDto { Result = "Ownership transferred successfully" });

    }
}