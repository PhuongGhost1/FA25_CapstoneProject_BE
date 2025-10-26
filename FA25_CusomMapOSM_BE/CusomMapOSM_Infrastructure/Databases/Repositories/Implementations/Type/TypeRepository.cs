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
    
    public async Task<MembershipPlanTypeEnum?> GetMembershipPlanTypeById(MembershipPlanTypeEnum name)
    {
        var entity = await _context.Plans.FirstOrDefaultAsync(x => x.PlanName == name.ToString());
        return entity != null ? Enum.TryParse<MembershipPlanTypeEnum>(entity.PlanName, out var result) ? result : (MembershipPlanTypeEnum?)null : null;
    }

    public async Task<PaymentGatewayEnum?> GetPaymentGatewayById(PaymentGatewayEnum name)
    {
        var entity = await _context.PaymentGateways.FirstOrDefaultAsync(x => x.Name == name.ToString());
        return entity != null ? Enum.TryParse<PaymentGatewayEnum>(entity.Name, out var result) ? result : (PaymentGatewayEnum?)null : null;
    }
}