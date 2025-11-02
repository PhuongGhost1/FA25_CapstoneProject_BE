using CusomMapOSM_Application.Models.DTOs.Features.Organization.Request;
using CusomMapOSM_Domain.Entities.Organizations;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Organization;

public interface IOrganizationRepository
{
    Task<bool> CreateOrganization(CusomMapOSM_Domain.Entities.Organizations.Organization req);
    
    Task<List<CusomMapOSM_Domain.Entities.Organizations.Organization>> GetAllOrganizations();
    
    Task<CusomMapOSM_Domain.Entities.Organizations.Organization?> GetOrganizationById(Guid id);
    
    Task<bool> UpdateOrganization(CusomMapOSM_Domain.Entities.Organizations.Organization organization);
    
    Task<bool> DeleteOrganization(Guid id);
    
    Task<bool> InviteMemberToOrganization(OrganizationInvitation req);
    
    Task<bool> AddMemberToOrganization(OrganizationMember req);
    
    Task<OrganizationInvitation?> GetInvitationById(Guid invitationId);
    
    Task<OrganizationInvitation?> GetInvitationByEmailAndOrg(string email, Guid orgId);
    
    Task<bool> UpdateInvitation(OrganizationInvitation invitation);
    
    Task<List<OrganizationInvitation>> GetInvitationsByEmail(string email);
    
    Task<List<OrganizationMember>> GetOrganizationMembers(Guid orgId);
    
    Task<OrganizationMember?> GetOrganizationMemberById(Guid memberId);
    
    Task<OrganizationMember?> GetOrganizationMemberByUserAndOrg(Guid userId, Guid orgId);
    
    Task<bool> UpdateOrganizationMember(OrganizationMember member);
    
    Task<bool> RemoveOrganizationMember(Guid memberId);
    
    Task<bool> DeleteInvitation(Guid invitationId);
    
    Task<List<OrganizationMember>> GetUserOrganizations(Guid userId);
    Task<int> GetTotalOrganizationCount();
}