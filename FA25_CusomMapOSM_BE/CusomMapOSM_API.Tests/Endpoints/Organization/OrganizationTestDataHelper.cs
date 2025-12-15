using Bogus;
using CusomMapOSM_Application.Models.DTOs.Features.Organization.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Organization.Response;

namespace CusomMapOSM_API.Tests.Endpoints.Organization;

public static class OrganizationTestDataHelper
{
    private static readonly Faker _faker = new();

    public static OrganizationReqDto CreateValidOrganizationRequest()
    {
        return new Faker<OrganizationReqDto>()
            .RuleFor(r => r.OrgName, f => f.Company.CompanyName())
            .RuleFor(r => r.Abbreviation, f => f.Random.String2(3, 5).ToUpper())
            .RuleFor(r => r.Description, f => f.Lorem.Sentence())
            .RuleFor(r => r.LogoUrl, f => f.Internet.Url())
            .RuleFor(r => r.ContactEmail, f => f.Internet.Email())
            .RuleFor(r => r.ContactPhone, f => f.Phone.PhoneNumber())
            .RuleFor(r => r.Address, f => f.Address.FullAddress())
            .Generate();
    }

    public static OrganizationReqDto CreateInvalidOrganizationRequest()
    {
        return new OrganizationReqDto
        {
            OrgName = "", // Invalid: empty name
            Abbreviation = "",
            Description = null
        };
    }

    public static InviteMemberOrganizationReqDto CreateValidInviteMemberRequest(Guid? orgId = null)
    {
        return new Faker<InviteMemberOrganizationReqDto>()
            .RuleFor(r => r.OrgId, orgId ?? _faker.Random.Guid())
            .RuleFor(r => r.MemberEmail, f => f.Internet.Email())
            .RuleFor(r => r.MemberType, f => f.PickRandom("Admin", "Member", "Viewer"))
            .Generate();
    }

    public static InviteMemberOrganizationReqDto CreateInvalidInviteMemberRequest(Guid? orgId = null)
    {
        return new InviteMemberOrganizationReqDto
        {
            OrgId = orgId ?? Guid.NewGuid(),
            MemberEmail = "invalid-email", // Invalid email format
            MemberType = "Member"
        };
    }

    public static AcceptInviteOrganizationReqDto CreateAcceptInviteRequest(Guid? invitationId = null)
    {
        return new AcceptInviteOrganizationReqDto
        {
            InvitationId = invitationId ?? Guid.NewGuid()
        };
    }

    public static OrganizationDetailDto CreateOrganizationDetail(Guid? orgId = null)
    {
        return new Faker<OrganizationDetailDto>()
            .RuleFor(o => o.OrgId, orgId ?? _faker.Random.Guid())
            .RuleFor(o => o.OrgName, f => f.Company.CompanyName())
            .RuleFor(o => o.Abbreviation, f => f.Random.String2(3, 5).ToUpper())
            .RuleFor(o => o.Description, f => f.Lorem.Sentence())
            .RuleFor(o => o.LogoUrl, f => f.Internet.Url())
            .RuleFor(o => o.ContactEmail, f => f.Internet.Email())
            .RuleFor(o => o.ContactPhone, f => f.Phone.PhoneNumber())
            .RuleFor(o => o.Address, f => f.Address.FullAddress())
            .RuleFor(o => o.CreatedAt, f => f.Date.Past())
            .RuleFor(o => o.IsActive, f => f.Random.Bool())
            .Generate();
    }

    public static List<OrganizationDetailDto> CreateOrganizationDetailList(int count = 3)
    {
        return new Faker<OrganizationDetailDto>()
            .RuleFor(o => o.OrgId, f => f.Random.Guid())
            .RuleFor(o => o.OrgName, f => f.Company.CompanyName())
            .RuleFor(o => o.Abbreviation, f => f.Random.String2(3, 5).ToUpper())
            .RuleFor(o => o.Description, f => f.Lorem.Sentence())
            .RuleFor(o => o.LogoUrl, f => f.Internet.Url())
            .RuleFor(o => o.ContactEmail, f => f.Internet.Email())
            .RuleFor(o => o.ContactPhone, f => f.Phone.PhoneNumber())
            .RuleFor(o => o.Address, f => f.Address.FullAddress())
            .RuleFor(o => o.CreatedAt, f => f.Date.Past())
            .RuleFor(o => o.IsActive, f => f.Random.Bool())
            .Generate(count);
    }

    public static InvitationDto CreateInvitation(Guid? invitationId = null, Guid? orgId = null)
    {
        return new Faker<InvitationDto>()
            .RuleFor(i => i.InvitationId, invitationId ?? _faker.Random.Guid())
            .RuleFor(i => i.OrgId, orgId ?? _faker.Random.Guid())
            .RuleFor(i => i.OrgName, f => f.Company.CompanyName())
            .RuleFor(i => i.Email, f => f.Internet.Email())
            .RuleFor(i => i.InviterEmail, f => f.Internet.Email())
            .RuleFor(i => i.MemberType, f => f.PickRandom("Admin", "Member", "Viewer"))
            .RuleFor(i => i.InvitedAt, f => f.Date.Recent())
            .RuleFor(i => i.IsAccepted, false)
            .RuleFor(i => i.AcceptedAt, (DateTime?)null)
            .Generate();
    }

    public static List<InvitationDto> CreateInvitationList(int count = 2)
    {
        return new Faker<InvitationDto>()
            .RuleFor(i => i.InvitationId, f => f.Random.Guid())
            .RuleFor(i => i.OrgId, f => f.Random.Guid())
            .RuleFor(i => i.OrgName, f => f.Company.CompanyName())
            .RuleFor(i => i.Email, f => f.Internet.Email())
            .RuleFor(i => i.InviterEmail, f => f.Internet.Email())
            .RuleFor(i => i.MemberType, f => f.PickRandom("Admin", "Member", "Viewer"))
            .RuleFor(i => i.InvitedAt, f => f.Date.Recent())
            .RuleFor(i => i.IsAccepted, false)
            .RuleFor(i => i.AcceptedAt, (DateTime?)null)
            .Generate(count);
    }

    public static MemberDto CreateMember(Guid? memberId = null)
    {
        return new Faker<MemberDto>()
            .RuleFor(m => m.MemberId, memberId ?? _faker.Random.Guid())
            .RuleFor(m => m.Email, f => f.Internet.Email())
            .RuleFor(m => m.FullName, f => f.Name.FullName())
            .RuleFor(m => m.Role, f => f.PickRandom("Owner", "Admin", "Member", "Viewer"))
            .RuleFor(m => m.JoinedAt, f => f.Date.Past())
            .RuleFor(m => m.IsActive, true)
            .Generate();
    }

    public static List<MemberDto> CreateMemberList(int count = 3)
    {
        return new Faker<MemberDto>()
            .RuleFor(m => m.MemberId, f => f.Random.Guid())
            .RuleFor(m => m.Email, f => f.Internet.Email())
            .RuleFor(m => m.FullName, f => f.Name.FullName())
            .RuleFor(m => m.Role, f => f.PickRandom("Owner", "Admin", "Member", "Viewer"))
            .RuleFor(m => m.JoinedAt, f => f.Date.Past())
            .RuleFor(m => m.IsActive, true)
            .Generate(count);
    }

    public static MyOrganizationDto CreateMyOrganization(Guid? orgId = null)
    {
        return new Faker<MyOrganizationDto>()
            .RuleFor(o => o.OrgId, orgId ?? _faker.Random.Guid())
            .RuleFor(o => o.OrgName, f => f.Company.CompanyName())
            .RuleFor(o => o.Abbreviation, f => f.Random.String2(3, 5).ToUpper())
            .RuleFor(o => o.MyRole, f => f.PickRandom("Owner", "Admin", "Member"))
            .RuleFor(o => o.JoinedAt, f => f.Date.Past())
            .RuleFor(o => o.LogoUrl, f => f.Internet.Url())
            .Generate();
    }

    public static List<MyOrganizationDto> CreateMyOrganizationList(int count = 2)
    {
        return new Faker<MyOrganizationDto>()
            .RuleFor(o => o.OrgId, f => f.Random.Guid())
            .RuleFor(o => o.OrgName, f => f.Company.CompanyName())
            .RuleFor(o => o.Abbreviation, f => f.Random.String2(3, 5).ToUpper())
            .RuleFor(o => o.MyRole, f => f.PickRandom("Owner", "Admin", "Member"))
            .RuleFor(o => o.JoinedAt, f => f.Date.Past())
            .RuleFor(o => o.LogoUrl, f => f.Internet.Url())
            .Generate(count);
    }

    public static UpdateMemberRoleReqDto CreateUpdateMemberRoleRequest(Guid? orgId = null, Guid? memberId = null)
    {
        return new UpdateMemberRoleReqDto
        {
            OrgId = orgId ?? Guid.NewGuid(),
            MemberId = memberId ?? Guid.NewGuid(),
            NewRole = _faker.PickRandom("Admin", "Member", "Viewer")
        };
    }

    public static RemoveMemberReqDto CreateRemoveMemberRequest(Guid? orgId = null, Guid? memberId = null)
    {
        return new RemoveMemberReqDto
        {
            OrgId = orgId ?? Guid.NewGuid(),
            MemberId = memberId ?? Guid.NewGuid()
        };
    }

    public static TransferOwnershipReqDto CreateTransferOwnershipRequest(Guid? newOwnerId = null)
    {
        return new TransferOwnershipReqDto
        {
            NewOwnerId = newOwnerId ?? Guid.NewGuid()
        };
    }

    public static RejectInviteOrganizationReqDto CreateRejectInviteRequest(Guid? invitationId = null)
    {
        return new RejectInviteOrganizationReqDto
        {
            InvitationId = invitationId ?? Guid.NewGuid()
        };
    }

    public static CancelInviteOrganizationReqDto CreateCancelInviteRequest(Guid? invitationId = null)
    {
        return new CancelInviteOrganizationReqDto
        {
            InvitationId = invitationId ?? Guid.NewGuid()
        };
    }

    // Response DTOs
    public static OrganizationResDto CreateOrganizationResponse(string result = "Organization created successfully", Guid? orgId = null)
    {
        return new OrganizationResDto 
        { 
            Result = result,
            OrgId = orgId ?? Guid.NewGuid()
        };
    }

    public static InviteMemberOrganizationResDto CreateInviteMemberResponse(string result = "Invitation sent successfully")
    {
        return new InviteMemberOrganizationResDto { Result = result };
    }

    public static AcceptInviteOrganizationResDto CreateAcceptInviteResponse(string result = "Invitation accepted successfully")
    {
        return new AcceptInviteOrganizationResDto { Result = result };
    }

    public static UpdateOrganizationResDto CreateUpdateOrganizationResponse(string result = "Organization updated successfully")
    {
        return new UpdateOrganizationResDto { Result = result };
    }

    public static DeleteOrganizationResDto CreateDeleteOrganizationResponse(string result = "Organization deleted successfully")
    {
        return new DeleteOrganizationResDto { Result = result };
    }

    public static GetAllOrganizationsResDto CreateGetAllOrganizationsResponse(List<OrganizationDetailDto>? organizations = null)
    {
        return new GetAllOrganizationsResDto 
        { 
            Organizations = organizations ?? CreateOrganizationDetailList() 
        };
    }

    public static GetOrganizationByIdResDto CreateGetOrganizationByIdResponse(OrganizationDetailDto? organization = null)
    {
        return new GetOrganizationByIdResDto 
        { 
            Organization = organization ?? CreateOrganizationDetail() 
        };
    }

    public static GetInvitationsResDto CreateGetInvitationsResponse(List<InvitationDto>? invitations = null)
    {
        return new GetInvitationsResDto 
        { 
            Invitations = invitations ?? CreateInvitationList() 
        };
    }

    public static GetOrganizationMembersResDto CreateGetOrganizationMembersResponse(List<MemberDto>? members = null)
    {
        return new GetOrganizationMembersResDto 
        { 
            Members = members ?? CreateMemberList() 
        };
    }

    public static GetMyOrganizationsResDto CreateGetMyOrganizationsResponse(List<MyOrganizationDto>? organizations = null)
    {
        return new GetMyOrganizationsResDto 
        { 
            Organizations = organizations ?? CreateMyOrganizationList() 
        };
    }

    public static UpdateMemberRoleResDto CreateUpdateMemberRoleResponse(string result = "Member role updated successfully")
    {
        return new UpdateMemberRoleResDto { Result = result };
    }

    public static RemoveMemberResDto CreateRemoveMemberResponse(string result = "Member removed successfully")
    {
        return new RemoveMemberResDto { Result = result };
    }

    public static TransferOwnershipResDto CreateTransferOwnershipResponse(string result = "Ownership transferred successfully")
    {
        return new TransferOwnershipResDto { Result = result };
    }

    public static RejectInviteOrganizationResDto CreateRejectInviteResponse(string result = "Invitation rejected successfully")
    {
        return new RejectInviteOrganizationResDto { Result = result };
    }

    public static CancelInviteOrganizationResDto CreateCancelInviteResponse(string result = "Invitation cancelled successfully")
    {
        return new CancelInviteOrganizationResDto { Result = result };
    }
}