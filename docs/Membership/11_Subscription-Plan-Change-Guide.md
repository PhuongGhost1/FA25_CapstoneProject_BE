# Subscription Plan Change Guide

# Custom Map OSM Backend System

## Overview

This document outlines the business rules, implementation, and usage of the subscription plan change functionality in the Custom Map OSM Backend System.

## Business Rules

### 1. Plan Change Policy

**✅ Allowed:**

- Users can change their subscription plan at any time during their active membership
- Plan changes take effect immediately
- New plan features and quotas become available immediately
- Auto-renewal settings can be updated during plan changes

**❌ Not Allowed:**

- Changing to an inactive or non-existent plan
- Changing to the same plan currently active
- Plan changes for expired or suspended memberships

### 2. Upgrade vs Downgrade Behavior

#### Upgrades (Higher-priced plans)

- **Usage Reset**: Usage cycles are reset to provide immediate access to higher quotas
- **Immediate Access**: New features and higher limits become available immediately
- **Billing**: Pro-rated billing should be handled by the billing system

#### Downgrades (Lower-priced plans)

- **Usage Capping**: Current usage is capped to new plan limits
- **Feature Access**: Access to features not included in new plan is restricted
- **Billing**: Pro-rated billing should be handled by the billing system

### 3. Data Integrity

- Original membership start date is preserved for billing continuity
- Usage tracking is adjusted based on plan change type
- Failed plan changes do not affect existing membership status
- All changes are logged with timestamps

## API Endpoints

### Change Subscription Plan

**Endpoint:** `POST /membership/change-plan`

**Request Body:**

```json
{
  "userId": "guid",
  "orgId": "guid",
  "newPlanId": 3,
  "autoRenew": true
}
```

**Response:**

```json
{
  "membershipId": "guid",
  "status": "Plan changed successfully",
  "proRatedAmount": null,
  "effectiveDate": "2025-01-15T10:30:00Z"
}
```

**Validation Rules:**

- `userId`: Required, valid GUID
- `orgId`: Required, valid GUID
- `newPlanId`: Required, must be greater than 0
- `autoRenew`: Required, boolean value

## Implementation Details

### Service Layer

The `ChangeSubscriptionPlanAsync` method in `MembershipService` handles:

1. **Validation**

   - Verify new plan exists and is active
   - Check current membership exists
   - Ensure not changing to same plan

2. **Plan Comparison**

   - Determine if upgrade or downgrade
   - Compare pricing to classify change type

3. **Usage Management**

   - **Upgrades**: Reset usage cycles and quotas
   - **Downgrades**: Cap usage to new plan limits

4. **Membership Update**
   - Update plan ID and auto-renewal setting
   - Preserve original start date
   - Update timestamps

### Error Handling

**Common Error Scenarios:**

- `Membership.PlanNotFound`: New plan doesn't exist or is inactive
- `Membership.NotFound`: No active membership found
- `Membership.SamePlan`: Attempting to change to same plan
- `Membership.CurrentPlanNotFound`: Current plan not found
- `Membership.ChangePlanFailed`: General failure during plan change

## Usage Examples

### Scenario 1: Basic to Pro Upgrade

**Current State:**

- Plan: Basic ($9.99/month)
- Usage: 15 maps created, 20 exports
- Quotas: 25 maps/month, 50 exports

**After Upgrade to Pro:**

- Plan: Pro ($29.99/month)
- Usage: Reset to 0 maps, 0 exports
- Quotas: 100 maps/month, 200 exports
- New features: Analytics, version history

### Scenario 2: Pro to Basic Downgrade

**Current State:**

- Plan: Pro ($29.99/month)
- Usage: 80 maps created, 150 exports
- Quotas: 100 maps/month, 200 exports

**After Downgrade to Basic:**

- Plan: Basic ($9.99/month)
- Usage: Capped to 25 maps, 50 exports
- Quotas: 25 maps/month, 50 exports
- Features: Analytics and version history removed

## Integration Points

### Billing System Integration

The core membership system handles plan changes, but billing calculations should be handled by a separate billing service:

```csharp
// Example billing integration
public async Task<decimal> CalculateProRatedAmount(
    Guid membershipId,
    int newPlanId,
    DateTime changeDate)
{
    // Calculate pro-rated amount based on:
    // - Current plan price
    // - New plan price
    // - Days remaining in billing cycle
    // - Usage patterns
}
```

### Access Tool Integration

When plans change, access tools are automatically updated via the `UserAccessToolService`:

```csharp
// Automatically called after plan change
await userAccessToolService.UpdateAccessToolsForMembershipAsync(
    userId,
    newPlanId,
    membershipExpiryDate,
    ct);
```

## Monitoring and Logging

### Audit Trail

All plan changes are logged with:

- User ID and organization ID
- Old plan ID and new plan ID
- Change timestamp
- Change type (upgrade/downgrade)
- Success/failure status

### Metrics to Track

- Plan change frequency by plan type
- Upgrade vs downgrade ratios
- Time between plan changes
- Failed plan change attempts
- Usage patterns after plan changes

## Testing Scenarios

### Unit Tests

1. **Valid Plan Change**

   - Upgrade from Basic to Pro
   - Downgrade from Pro to Basic
   - Same plan change (should fail)

2. **Invalid Scenarios**

   - Non-existent plan
   - Inactive plan
   - No membership found
   - Invalid user/organization

3. **Usage Management**
   - Usage reset on upgrade
   - Usage capping on downgrade
   - Quota enforcement

### Integration Tests

1. **End-to-End Flow**

   - Complete plan change process
   - Billing integration
   - Access tool updates

2. **Error Scenarios**
   - Database failures
   - Concurrent plan changes
   - Network timeouts

## Security Considerations

1. **Authorization**

   - Only organization owners/admins can change plans
   - Validate user permissions before allowing changes

2. **Data Protection**

   - Encrypt sensitive billing information
   - Log plan changes for audit purposes

3. **Rate Limiting**
   - Prevent rapid plan changes
   - Implement cooldown periods if needed

## Future Enhancements

### Planned Features

1. **Scheduled Plan Changes**

   - Allow users to schedule plan changes for future dates
   - Automatic execution at scheduled time

2. **Plan Change Notifications**

   - Email notifications for plan changes
   - Usage warnings before downgrades

3. **Advanced Billing**

   - Pro-rated credit calculations
   - Refund processing for downgrades
   - Usage-based billing adjustments

4. **Analytics Dashboard**
   - Plan change analytics
   - Usage pattern analysis
   - Revenue impact tracking

## Conclusion

The subscription plan change functionality provides a flexible and user-friendly way for customers to adjust their subscription levels. The implementation ensures data integrity, proper usage management, and seamless integration with the billing and access control systems.

By following the business rules outlined in this document, the system maintains consistency and provides a positive user experience while protecting the business interests and ensuring proper resource allocation.

