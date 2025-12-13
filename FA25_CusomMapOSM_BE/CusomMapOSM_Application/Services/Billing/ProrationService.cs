namespace CusomMapOSM_Application.Services.Billing;

public class ProrationResult
{
    public decimal UnusedCredit { get; set; }           // Credit from old plan
    public decimal ProratedNewPlanCost { get; set; }    // Cost for remaining days in new plan
    public decimal AmountDue { get; set; }              // What user pays today
    public int DaysRemaining { get; set; }              // Days left in billing cycle
    public string? Message { get; set; }                // Optional message (e.g., "Upgrade is free!")
}

public interface IProrationService
{
    /// <summary>
    /// Calculate proration for plan upgrade using Option C (Instant Upgrade with Proration).
    /// 
    /// Formula:
    /// 1. Calculate unused credit from current plan: (currentPrice / totalDays) × daysRemaining
    /// 2. Calculate prorated new plan cost: (newPrice / totalDays) × daysRemaining
    /// 3. Amount due = proratedNewPlanCost - unusedCredit
    /// </summary>
    ProrationResult CalculateUpgradeProration(
        decimal currentPlanPrice,
        decimal newPlanPrice,
        DateTime billingCycleStartDate,
        DateTime billingCycleEndDate,
        DateTime upgradeDate);
}

public class ProrationService : IProrationService
{
    // PayOS minimum transaction: 3,000 VND (~$0.50 USD)
    // If amount due is below this, we make the upgrade free (better UX)
    private const decimal MINIMUM_CHARGE = 0.50m;
    
    public ProrationResult CalculateUpgradeProration(
        decimal currentPlanPrice,
        decimal newPlanPrice,
        DateTime billingCycleStartDate,
        DateTime billingCycleEndDate,
        DateTime upgradeDate)
    {
        // Calculate total days in billing cycle (usually 30)
        var totalDaysInCycle = (int)Math.Ceiling(
            (billingCycleEndDate - billingCycleStartDate).TotalDays
        );
        
        // Ensure minimum 1 day
        if (totalDaysInCycle <= 0)
            totalDaysInCycle = 30; // Default to 30 days
        
        // Calculate days remaining from upgrade date to end of cycle
        var daysRemaining = (int)Math.Ceiling(
            (billingCycleEndDate - upgradeDate).TotalDays
        );
        
        // Edge case: upgrading on or after last day
        if (daysRemaining <= 0)
        {
            return new ProrationResult
            {
                UnusedCredit = 0,
                ProratedNewPlanCost = newPlanPrice, // Full month for new plan
                AmountDue = newPlanPrice,
                DaysRemaining = 0,
                Message = "Billing cycle ended. Charging full price for new plan."
            };
        }
        
        // Step 1: Calculate unused credit from current plan
        var dailyRateCurrentPlan = currentPlanPrice / totalDaysInCycle;
        var unusedCredit = dailyRateCurrentPlan * daysRemaining;
        
        // Step 2: Calculate prorated cost for new plan (remaining days)
        var dailyRateNewPlan = newPlanPrice / totalDaysInCycle;
        var proratedNewPlanCost = dailyRateNewPlan * daysRemaining;
        
        // Step 3: Amount due = prorated new plan - unused credit
        var amountDue = Math.Max(0, proratedNewPlanCost - unusedCredit);
        
        // Handle extremely small amounts (make it free if below $0.50)
        // PayOS minimum is 3k VND (~$0.50), so we make upgrades free if below this threshold
        if (amountDue > 0 && amountDue < MINIMUM_CHARGE)
        {
            return new ProrationResult
            {
                UnusedCredit = Math.Round(unusedCredit, 2),
                ProratedNewPlanCost = Math.Round(proratedNewPlanCost, 2),
                AmountDue = 0,
                DaysRemaining = daysRemaining,
                Message = "Upgrade is free due to remaining credit!"
            };
        }
        
        return new ProrationResult
        {
            UnusedCredit = Math.Round(unusedCredit, 2),
            ProratedNewPlanCost = Math.Round(proratedNewPlanCost, 2),
            AmountDue = Math.Round(amountDue, 2),
            DaysRemaining = daysRemaining
        };
    }
}

