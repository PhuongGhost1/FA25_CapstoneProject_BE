using CusomMapOSM_Domain.Entities.Organizations;
using CusomMapOSM_Domain.Entities.Organizations.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Organization;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Organization;

public class OrganizationRepository : IOrganizationRepository
{
    private readonly CustomMapOSMDbContext _context;

    public OrganizationRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }
    
    public async Task<bool> CreateOrganization(CusomMapOSM_Domain.Entities.Organizations.Organization req)
    {
        _context.Organizations.Add(req);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<List<CusomMapOSM_Domain.Entities.Organizations.Organization>> GetAllOrganizations()
    {
        return await _context.Organizations
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<CusomMapOSM_Domain.Entities.Organizations.Organization?> GetOrganizationById(Guid id)
    {
        return await _context.Organizations
            .FirstOrDefaultAsync(x => x.OrgId == id && x.IsActive);
    }
    
    public async Task<bool> UpdateOrganization(CusomMapOSM_Domain.Entities.Organizations.Organization organization)
    {
        _context.Organizations.Update(organization);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> DeleteOrganization(Guid id)
    {
        var organization = await _context.Organizations.FirstOrDefaultAsync(x => x.OrgId == id);
        if (organization == null)
            return false;
            
        organization.IsActive = false;
        _context.Organizations.Update(organization);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> InviteMemberToOrganization(OrganizationInvitation req)
    {
        _context.OrganizationInvitations.Add(req);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> AddMemberToOrganization(OrganizationMember req)
    {
        _context.OrganizationMembers.Add(req);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<OrganizationInvitation?> GetInvitationById(Guid invitationId)
    {
        return await _context.OrganizationInvitations
            .Include(x => x.Organization)
            .Include(x => x.Inviter)
            .FirstOrDefaultAsync(x => x.InvitationId == invitationId);
    }
    
    public async Task<OrganizationInvitation?> GetInvitationByEmailAndOrg(string email, Guid orgId)
    {
        return await _context.OrganizationInvitations
            .FirstOrDefaultAsync(x => x.Email == email && x.OrgId == orgId);
    }
    
    public async Task<bool> UpdateInvitation(OrganizationInvitation invitation)
    {
        _context.OrganizationInvitations.Update(invitation);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<List<OrganizationInvitation>> GetInvitationsByEmail(string email)
    {
        return await _context.OrganizationInvitations
            .Include(x => x.Organization)
            .Include(x => x.Inviter)
            .Where(x => x.Email == email)
            .OrderByDescending(x => x.InvitedAt)
            .ToListAsync();
    }

    public async Task<List<OrganizationMember>> GetOrganizationMembers(Guid orgId)
    {
        return await _context.OrganizationMembers
            .Include(x => x.User)
            .Where(x => x.OrgId == orgId && x.Status == MemberStatus.Active)
            .OrderBy(x => x.JoinedAt)
            .ToListAsync();
    }

    public async Task<OrganizationMember?> GetOrganizationMemberById(Guid memberId)
    {
        return await _context.OrganizationMembers
            .Include(x => x.User)
            .Include(x => x.Organization)
            .FirstOrDefaultAsync(x => x.MemberId == memberId);
    }

    public async Task<OrganizationMember?> GetOrganizationMemberByUserAndOrg(Guid userId, Guid orgId)
    {
        return await _context.OrganizationMembers
            .Include(x => x.User)
            .Include(x => x.Organization)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.OrgId == orgId && x.Status == MemberStatus.Active);
    }

    public async Task<bool> UpdateOrganizationMember(OrganizationMember member)
    {
        _context.OrganizationMembers.Update(member);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> RemoveOrganizationMember(Guid memberId)
    {
        var member = await _context.OrganizationMembers
            .FirstOrDefaultAsync(x => x.MemberId == memberId);
        
        if (member == null)
            return false;
            
        // Soft delete by setting status to Removed
        member.Status = MemberStatus.Removed;
        _context.OrganizationMembers.Update(member);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteInvitation(Guid invitationId)
    {
        var invitation = await _context.OrganizationInvitations
            .FirstOrDefaultAsync(x => x.InvitationId == invitationId);
        
        if (invitation == null)
            return false;
            
        _context.OrganizationInvitations.Remove(invitation);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<List<OrganizationMember>> GetUserOrganizations(Guid userId)
    {
        return await _context.OrganizationMembers
            .Include(x => x.Organization)
            .Where(x => x.UserId == userId && x.Status == MemberStatus.Active && (x.Organization != null && x.Organization.IsActive))
            .OrderBy(x => x.JoinedAt)
            .ToListAsync();
    }
    public async Task<int> GetTotalOrganizationCount()
    {
        return await _context.Organizations.CountAsync(x => x.IsActive);
    }

    public async Task<bool> IsOrganizationNameExists(string orgName, Guid? excludeOrgId = null)
    {
        var query = _context.Organizations
            .Where(x => x.IsActive && x.OrgName.ToLower() == orgName.ToLower());
        
        if (excludeOrgId.HasValue)
        {
            query = query.Where(x => x.OrgId != excludeOrgId.Value);
        }
        
        return await query.AnyAsync();
    }
}