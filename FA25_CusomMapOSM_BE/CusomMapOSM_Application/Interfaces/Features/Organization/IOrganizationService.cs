using CusomMapOSM_Application.Common.Errors;
using CusomMapOSM_Application.Models.DTOs.Features.Authentication.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Organization.Request;
using CusomMapOSM_Application.Models.DTOs.Features.Organization.Response;
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
}