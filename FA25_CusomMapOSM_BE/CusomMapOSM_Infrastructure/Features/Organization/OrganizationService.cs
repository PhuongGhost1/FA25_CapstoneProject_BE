using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Interfaces.Features.Organization;
using CusomMapOSM_Application.Interfaces.Services.Mail;
using CusomMapOSM_Application.Interfaces.Services.User;
using CusomMapOSM_Application.Models.DTOs.Features.Organization.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Organization.Response;
using CusomMapOSM_Application.Models.DTOs.Services;
using CusomMapOSM_Application.Models.Templates.Email;
using CusomMapOSM_Domain.Entities.Organizations;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Authentication;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Organization;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Type;
using Optional;

namespace CusomMapOSM_Infrastructure.Features.Organization;

public class OrganizationService : IOrganizationService
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IAuthenticationRepository _authenticationRepository;
    private readonly ITypeRepository _typeRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IRabbitMQService _rabbitMqService;

    public OrganizationService(IOrganizationRepository organizationRepository,
        IAuthenticationRepository authenticationRepository, ITypeRepository typeRepository,
        ICurrentUserService currentUserService, IRabbitMQService rabbitMqService)
    {
        _organizationRepository = organizationRepository;
        _authenticationRepository = authenticationRepository;
        _typeRepository = typeRepository;
        _currentUserService = currentUserService;
        _rabbitMqService = rabbitMqService;
    }

    public async Task<Option<OrganizationResDto, Error>> Create(OrganizationReqDto req)
    {
        var newOrg = new CusomMapOSM_Domain.Entities.Organizations.Organization()
        {
            OrgName = req.OrgName,
            Abbreviation = req.Abbreviation,
            Description = req.Description,
            LogoUrl = req.LogoUrl,
            ContactEmail = req.ContactEmail,
            ContactPhone = req.ContactPhone,
            Address = req.Address,
            OwnerUserId = _currentUserService.GetUserId()!.Value,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
        };
        await _organizationRepository.CreateOrganization(newOrg);
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

        var memberType = await _typeRepository.GetOrganizationMemberTypeByName(req.MemberType);
        if (memberType is null)
        {
            return Option.None<InviteMemberOrganizationResDto, Error>(
                Error.NotFound("Organization.MemberTypeNotFound", "Member type not found"));
        }

        var existingInvitation = await _organizationRepository.GetInvitationByEmailAndOrg(req.MemberEmail, req.OrgId);
        if (existingInvitation != null && !existingInvitation.IsAccepted)
        {
            return Option.None<InviteMemberOrganizationResDto, Error>(
                Error.Conflict("Organization.InvitationAlreadyExists",
                    "An invitation has already been sent to this email for this organization"));
        }

        var newInvitation = new OrganizationInvitation()
        {
            OrgId = req.OrgId,
            Email = req.MemberEmail,
            InvitedBy = currentUserId.Value,
            MembersRoleId = memberType.TypeId,
            InvitedAt = DateTime.UtcNow,
            IsAccepted = false
        };

        var invitationResult = await _organizationRepository.InviteMemberToOrganization(newInvitation);
        if (!invitationResult)
        {
            return Option.None<InviteMemberOrganizationResDto, Error>(
                Error.Failure("Organization.InvitationFailed", "Failed to create invitation"));
        }

        // Send email notification asynchronously
        var organization = await _organizationRepository.GetOrganizationById(req.OrgId);
        var inviter = await _authenticationRepository.GetUserById(currentUserId.Value);

        var mail = new MailRequest
        {
            ToEmail = req.MemberEmail,
            Subject = $"Invitation to join {organization?.OrgName ?? "Organization"}",
            Body = EmailTemplates.Organization.GetInvitationTemplate(
                inviter?.FullName ?? "Unknown User",
                organization?.OrgName ?? "an organization", 
                req.MemberType)
        };

        await _rabbitMqService.EnqueueEmailAsync(mail);

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

        if (invitation.IsAccepted)
        {
            return Option.None<AcceptInviteOrganizationResDto, Error>(
                Error.Conflict("Organization.InvitationAlreadyAccepted", "Invitation has already been accepted"));
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

        var newMember = new OrganizationMember()
        {
            OrgId = invitation.OrgId,
            UserId = user.UserId,
            MembersRoleId = invitation.MembersRoleId,
            InvitedBy = invitation.InvitedBy,
            JoinedAt = DateTime.UtcNow,
            IsActive = true
        };

        var memberResult = await _organizationRepository.AddMemberToOrganization(newMember);
        if (!memberResult)
        {
            return Option.None<AcceptInviteOrganizationResDto, Error>(
                Error.Failure("Organization.MemberAddFailed", "Failed to add member to organization"));
        }

        invitation.IsAccepted = true;
        invitation.AcceptedAt = DateTime.UtcNow;

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
            MemberType = invitation.Role?.Name ?? "Unknown Role",
            InvitedAt = invitation.InvitedAt,
            IsAccepted = invitation.IsAccepted,
            AcceptedAt = invitation.AcceptedAt
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
            Role = member.Role?.Name ?? "Unknown Role",
            JoinedAt = member.JoinedAt ?? DateTime.UtcNow,
            IsActive = member.IsActive
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

        var newRole = await _typeRepository.GetOrganizationMemberTypeByName(req.NewRole);
        if (newRole is null)
        {
            return Option.None<UpdateMemberRoleResDto, Error>(
                Error.NotFound("Organization.RoleNotFound", "Role not found"));
        }

        member.MembersRoleId = newRole.TypeId;
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

        if (invitation.IsAccepted)
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

        if (invitation.IsAccepted)
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
            MyRole = member.Role?.Name ?? "Unknown Role",
            JoinedAt = member.JoinedAt ?? DateTime.UtcNow,
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

        var ownerRole = await _typeRepository.GetOrganizationMemberTypeByName("Owner");
        if (ownerRole is null)
        {
            return Option.None<TransferOwnershipResDto, Error>(
                Error.NotFound("Organization.OwnerRoleNotFound", "Owner role not found"));
        }

        var currentOwnerMember = await _organizationRepository.GetOrganizationMemberByUserAndOrg(currentUserId.Value, req.OrgId);
        if (currentOwnerMember is null || currentOwnerMember.Role?.Name != "Owner")
        {
            return Option.None<TransferOwnershipResDto, Error>(                Error.Forbidden("Organization.NotOwner", "Only the current owner can transfer ownership"));
        }

        // Update new owner's role
        newOwnerMember.MembersRoleId = ownerRole.TypeId;
        var updateNewOwnerResult = await _organizationRepository.UpdateOrganizationMember(newOwnerMember);
        
        if (!updateNewOwnerResult)
        {
            return Option.None<TransferOwnershipResDto, Error>(
                Error.Failure("Organization.TransferOwnershipFailed", "Failed to transfer ownership"));
        }

        // Optionally, demote current owner to a different role (e.g., Admin)
        var adminRole = await _typeRepository.GetOrganizationMemberTypeByName("Admin");
        if (adminRole != null)
        {
            currentOwnerMember.MembersRoleId = adminRole.TypeId;
            await _organizationRepository.UpdateOrganizationMember(currentOwnerMember);
        }

        return Option.Some<TransferOwnershipResDto, Error>(
            new TransferOwnershipResDto { Result = "Ownership transferred successfully" });

    }
}