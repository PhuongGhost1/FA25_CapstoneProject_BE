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

namespace CusomMapOSM_Infrastructure.Databases.Repositories.Interfaces.Type;

public interface ITypeRepository
{
    // AnnotationType removed; no longer exposed via repository
    Task<MembershipPlanTypeEnum?> GetMembershipPlanTypeById(MembershipPlanTypeEnum name);
    Task<PaymentGatewayEnum?> GetPaymentGatewayById(PaymentGatewayEnum name);
}
