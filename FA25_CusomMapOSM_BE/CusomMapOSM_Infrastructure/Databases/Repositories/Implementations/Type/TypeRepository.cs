using CusomMapOSM_Domain.Entities.Collaborations;
using CusomMapOSM_Domain.Entities.Collaborations.Enums;
using CusomMapOSM_Domain.Entities.Exports;
using CusomMapOSM_Domain.Entities.Exports.Enums;
using CusomMapOSM_Domain.Entities.Layers.Enums;
using CusomMapOSM_Domain.Entities.Memberships.Enums;
using CusomMapOSM_Domain.Entities.Organizations;
using CusomMapOSM_Domain.Entities.Organizations.Enums;
using CusomMapOSM_Domain.Entities.Tickets.Enums;
using CusomMapOSM_Domain.Entities.Transactions.Enums;
using CusomMapOSM_Domain.Entities.Users;
using CusomMapOSM_Domain.Entities.Users.Enums;
using CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Type;
using Microsoft.EntityFrameworkCore;

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Implementations.Type;

public class TypeRepository : ITypeRepository
{
    private readonly CustomMapOSMDbContext _context;
    public TypeRepository(CustomMapOSMDbContext context)
    {
        _context = context;
    }
    // AnnotationType removed; repository no longer serves annotation types

    public async Task<CollaborationPermission?> GetCollaborationPermissionById(CollaborationPermissionEnum name)
    {
        return await _context.CollaborationPermissions.FirstOrDefaultAsync(x => x.PermissionName == name.ToString());
    }

    public async Task<CollaborationTargetType?> GetCollaborationTargetTypeById(CollaborationTargetTypeEnum name)
    {
        return await _context.CollaborationTargetTypes.FirstOrDefaultAsync(x => x.TypeName == name.ToString());
    }

    public async Task<ExportType?> GetExportTypeById(ExportTypeEnum name)
    {
        return await _context.ExportTypes.FirstOrDefaultAsync(x => x.Name == name.ToString());
    }
    
    public async Task<MembershipPlanTypeEnum?> GetMembershipPlanTypeById(MembershipPlanTypeEnum name)
    {
        var entity = await _context.Plans.FirstOrDefaultAsync(x => x.PlanName == name.ToString());
        return entity != null ? Enum.TryParse<MembershipPlanTypeEnum>(entity.PlanName, out var result) ? result : (MembershipPlanTypeEnum?)null : null;
    }

    public async Task<OrganizationMemberTypeEnum?> GetOrganizationMemberTypeById(OrganizationMemberTypeEnum name)
    {
        var entity = await _context.OrganizationMemberTypes.FirstOrDefaultAsync(x => x.Name == name.ToString());
        return entity != null ? Enum.TryParse<OrganizationMemberTypeEnum>(entity.Name, out var result) ? result : (OrganizationMemberTypeEnum?)null : null;
    }

    public async Task<OrganizationMemberType?> GetOrganizationMemberTypeByName(string name)
    {
        return await _context.OrganizationMemberTypes.FirstOrDefaultAsync(x => x.Name == name);
    }

    public async Task<PaymentGatewayEnum?> GetPaymentGatewayById(PaymentGatewayEnum name)
    {
        var entity = await _context.PaymentGateways.FirstOrDefaultAsync(x => x.Name == name.ToString());
        return entity != null ? Enum.TryParse<PaymentGatewayEnum>(entity.Name, out var result) ? result : (PaymentGatewayEnum?)null : null;
    }

    public async Task<UserRole?> GetUserRoleById(UserRoleEnum name)
    {
        return await _context.UserRoles.FirstOrDefaultAsync(x => x.Name == name.ToString());
    }
}