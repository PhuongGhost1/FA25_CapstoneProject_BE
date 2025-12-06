using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.Authentication.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Organization.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Organization.Response;
using Microsoft.AspNetCore.Http;
using Optional;

namespace CusomMapOSM_Application.Interfaces.Features.Organization;

public interface IOrganizationService
{
    Task<Option<OrganizationResDto, Error>> Create(OrganizationReqDto req);
    Task<Option<GetAllOrganizationsResDto, Error>> GetAll();
    Task<Option<GetOrganizationByIdResDto, Error>> GetById(Guid id);
    Task<Option<UpdateOrganizationResDto, Error>> Update(Guid id, OrganizationReqDto req);
    Task<Option<DeleteOrganizationResDto, Error>> Delete(Guid id);
    Task<Option<InviteMemberOrganizationResDto, Error>> InviteMember(InviteMemberOrganizationReqDto req);
    Task<Option<AcceptInviteOrganizationResDto, Error>> AcceptInvite(AcceptInviteOrganizationReqDto req);
    Task<Option<GetInvitationsResDto, Error>> GetMyInvitations();
    Task<Option<GetOrganizationMembersResDto, Error>> GetMembers(Guid orgId);
    Task<Option<UpdateMemberRoleResDto, Error>> UpdateMemberRole(UpdateMemberRoleReqDto req);
    Task<Option<RemoveMemberResDto, Error>> RemoveMember(RemoveMemberReqDto req);
    Task<Option<RejectInviteOrganizationResDto, Error>> RejectInvite(RejectInviteOrganizationReqDto req);
    Task<Option<CancelInviteOrganizationResDto, Error>> CancelInvite(CancelInviteOrganizationReqDto req);
    Task<Option<GetMyOrganizationsResDto, Error>> GetMyOrganizations();
    Task<Option<TransferOwnershipResDto, Error>> TransferOwnership(Guid orgId, TransferOwnershipReqDto req);
    Task<Option<BulkCreateStudentsResponse, Error>> BulkCreateStudents(IFormFile excelFile, BulkCreateStudentsRequest request);
    Task<Option<ValidateOrganizationNameResDto, Error>> ValidateOrganizationName(string orgName, Guid? excludeOrgId = null);
}