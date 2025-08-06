using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Domain.Entities.Memberships.ErrorMessages;

public class MembershipErrors
{
    public const string InvalidCredentials = "Invalid credentials provided.";
    public const string Unauthorized = "Unauthorized access.";
}
public class MembershipStatusErrors
{
    public const string InvalidStatus = "Invalid membership status.";
    public const string StatusAlreadyExists = "Membership status already exists.";
    public const string StatusNotFound = "Membership status not found.";
}

public class MembershipPlanErrors
{
    public const string InvalidPlan = "Invalid membership plan.";
    public const string PlanAlreadyExists = "Membership plan already exists.";
    public const string PlanNotFound = "Membership plan not found.";
}