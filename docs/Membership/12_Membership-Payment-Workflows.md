# Membership Payment Workflows

# Custom Map OSM Backend System

## Overview

This document provides detailed step-by-step workflows for membership purchase and add-on purchase processes in the Custom Map OSM Backend System. These workflows cover the complete process from payment initiation to service activation.

## Table of Contents

1. [Membership Purchase Workflow](#membership-purchase-workflow)
2. [Add-on Purchase Workflow](#add-on-purchase-workflow)
3. [Database Tables Involved](#database-tables-involved)
4. [Error Handling](#error-handling)
5. [Business Rules](#business-rules)

---

## Membership Purchase Workflow

### Phase 1: Payment Initiation

#### Step 1: User Initiates Payment

- **Action**: User selects a membership plan and clicks "Buy Membership"
- **Frontend**: Sends `POST /transaction/process-payment` with `ProcessPaymentReq`
- **Request Body**:

```json
{
  "Total": 29.99,
  "PaymentGateway": "payOS",
  "Purpose": "membership",
  "UserId": "user-guid",
  "OrgId": "org-guid",
  "PlanId": 1,
  "AutoRenew": true
}
```

#### Step 2: Transaction Service Processes Payment Request

- **Service**: `TransactionService.ProcessPaymentAsync()` is called
- **Database Action 1**: Gets Payment Gateway ID from `PaymentGateway` table
- **Database Action 2**: Creates pending transaction record in `Transactions` table:

```sql
INSERT INTO Transactions (
  TransactionId, PaymentGatewayId, Amount, Status,
  Purpose, TransactionDate, CreatedAt, MembershipId, ExportId
) VALUES (
  'new-guid', 'gateway-guid', 29.99, 'pending',
  'membership', NOW(), NOW(), NULL, NULL
)
```

- **Database Action 3**: Stores business context in transaction metadata (UserId, OrgId, PlanId, AutoRenew)
- **External Call**: Creates checkout session with payment gateway (PayOS/PayPal/Stripe)
- **Database Action 4**: Updates transaction with gateway session ID
- **Response**: Returns approval URL to redirect user to payment gateway

### Phase 2: Payment Processing

#### Step 3: User Completes Payment

- **Action**: User is redirected to payment gateway (PayOS/PayPal/Stripe)
- **Action**: User enters payment details and completes transaction
- **Action**: Payment gateway redirects back to `POST /transaction/confirm-payment-with-context`

#### Step 4: Payment Confirmation

- **Service**: `TransactionService.ConfirmPaymentWithContextAsync()` is called
- **External Call**: Confirms payment with payment gateway
- **Database Action**: Updates transaction status from "pending" to "success"
- **Database Action**: Retrieves stored business context from transaction metadata

### Phase 3: Membership Creation & Access Granting

#### Step 5: Membership Creation

- **Service**: `MembershipService.CreateOrRenewMembershipAsync()` is called
- **Database Action**: Checks if membership exists for UserId + OrgId
- **If NEW membership**:

```sql
INSERT INTO Membership (
  MembershipId, UserId, OrgId, PlanId, StartDate,
  EndDate, StatusId, AutoRenew, CurrentUsage,
  LastResetDate, CreatedAt, UpdatedAt
) VALUES (
  'new-guid', 'user-guid', 'org-guid', 1, NOW(),
  NULL, 'status-guid', true, NULL, NOW(), NOW(), NOW()
)
```

- **Database Action**: Creates initial usage record in `MembershipUsage` table:

```sql
INSERT INTO MembershipUsage (
  UsageId, MembershipId, OrgId, MapsCreatedThisCycle,
  ExportsThisCycle, ActiveUsersInOrg, FeatureFlags,
  CycleStartDate, CycleEndDate, CreatedAt, UpdatedAt
) VALUES (
  'new-guid', 'membership-guid', 'org-guid', 0, 0, 0,
  NULL, NOW(), DATE_ADD(NOW(), INTERVAL 1 MONTH), NOW(), NOW()
)
```

- **If EXISTING membership**: Updates plan and auto-renewal settings

#### Step 6: Access Tools Granting

- **Service**: `UserAccessToolService.UpdateAccessToolsForMembershipAsync()` is called
- **Database Action**: Gets membership plan details to determine access tools
- **Database Action**: Revokes all existing access tools for the user
- **Database Action**: Grants new access tools based on plan:

```sql
INSERT INTO UserAccessTool (
  UserId, AccessToolId, GrantedAt, ExpiredAt
) VALUES (
  'user-guid', 1, NOW(), 'membership-expiry-date'
)
```

### Phase 4: Transaction Completion

#### Step 7: Final Response

- **Response**: Returns success response with:

```json
{
  "MembershipId": "membership-guid",
  "TransactionId": "transaction-guid",
  "AccessToolsGranted": true
}
```

---

## Add-on Purchase Workflow

### Phase 1: Payment Initiation

#### Step 1: User Initiates Add-on Purchase

- **Action**: User selects an add-on and clicks "Purchase Add-on"
- **Frontend**: Sends `POST /transaction/process-payment` with `ProcessPaymentReq`
- **Request Body**:

```json
{
  "Total": 9.99,
  "PaymentGateway": "payOS",
  "Purpose": "addon",
  "MembershipId": "membership-guid",
  "AddonKey": "extra_exports",
  "Quantity": 50
}
```

#### Step 2: Transaction Service Processes Payment Request

- **Service**: `TransactionService.ProcessPaymentAsync()` is called
- **Database Action 1**: Gets Payment Gateway ID from `PaymentGateway` table
- **Database Action 2**: Creates pending transaction record in `Transactions` table:

```sql
INSERT INTO Transactions (
  TransactionId, PaymentGatewayId, Amount, Status,
  Purpose, TransactionDate, CreatedAt, MembershipId, ExportId
) VALUES (
  'new-guid', 'gateway-guid', 9.99, 'pending',
  'addon', NOW(), NOW(), 'membership-guid', NULL
)
```

- **Database Action 3**: Stores business context in transaction metadata (MembershipId, AddonKey, Quantity)
- **External Call**: Creates checkout session with payment gateway
- **Database Action 4**: Updates transaction with gateway session ID
- **Response**: Returns approval URL to redirect user to payment gateway

### Phase 2: Payment Processing

#### Step 3: User Completes Payment

- **Action**: User is redirected to payment gateway
- **Action**: User enters payment details and completes transaction
- **Action**: Payment gateway redirects back to `POST /transaction/confirm-payment-with-context`

#### Step 4: Payment Confirmation

- **Service**: `TransactionService.ConfirmPaymentWithContextAsync()` is called
- **External Call**: Confirms payment with payment gateway
- **Database Action**: Updates transaction status from "pending" to "success"
- **Database Action**: Retrieves stored business context from transaction metadata

### Phase 3: Add-on Creation

#### Step 5: Add-on Creation

- **Service**: `MembershipService.AddAddonAsync()` is called
- **Database Action**: Creates add-on record in `MembershipAddon` table:

```sql
INSERT INTO MembershipAddon (
  AddonId, MembershipId, OrgId, AddonKey, Quantity,
  FeaturePayload, PurchasedAt, EffectiveFrom,
  EffectiveUntil, CreatedAt, UpdatedAt
) VALUES (
  'new-guid', 'membership-guid', 'org-guid', 'extra_exports', 50,
  NULL, NOW(), NOW(), NULL, NOW(), NOW()
)
```

### Phase 4: Transaction Completion

#### Step 6: Final Response

- **Response**: Returns success response with:

```json
{
  "AddonId": "addon-guid",
  "TransactionId": "transaction-guid"
}
```

---

## Database Tables Involved

### Primary Tables

1. **`Transactions`** - Payment transaction records

   - Stores payment information, status, and business context
   - Links to membership or export records

2. **`Membership`** - User membership records

   - Contains membership details, plan information, and status
   - One record per UserId + OrgId combination

3. **`MembershipUsage`** - Usage tracking records

   - Tracks resource consumption per organization
   - Initialized for new memberships

4. **`MembershipAddon`** - Add-on purchase records

   - Stores purchased add-ons and their quantities
   - Links to specific memberships

5. **`UserAccessTool`** - User access permissions
   - Grants access to specific tools based on membership
   - Automatically managed during membership changes

### Supporting Tables

6. **`PaymentGateway`** - Payment gateway configuration

   - Contains gateway settings and credentials
   - Read-only during payment processing

7. **`Plan`** - Membership plan definitions

   - Contains plan features, limits, and pricing
   - Referenced during membership creation

8. **`AccessTool`** - Available access tools
   - Defines tools that can be granted to users
   - Referenced during access tool granting

---

## Error Handling

### Payment Gateway Errors

- **Gateway Unavailable**: Returns error, no database changes
- **Invalid Gateway**: Returns validation error
- **Gateway Configuration Error**: Returns configuration error

### Payment Processing Errors

- **Payment Confirmation Fails**: Transaction status updated to "failed"
- **Payment Cancelled**: Transaction status updated to "cancelled"
- **Payment Timeout**: Transaction status updated to "expired"

### Business Logic Errors

- **Membership Creation Fails**: Payment successful but membership not created
- **Access Tool Granting Fails**: Membership created but access tools not granted
- **Add-on Creation Fails**: Payment successful but add-on not created

### Recovery Procedures

- **Failed Payments**: Manual intervention required for refunds
- **Partial Failures**: Logged for manual resolution
- **Data Inconsistencies**: Background jobs to reconcile state

---

## Business Rules

### Membership Purchase Rules

- **One Membership Per User/Org**: Only one active membership per UserId + OrgId combination
- **Immediate Activation**: Membership becomes active immediately upon successful payment
- **Access Tool Management**: Access tools automatically granted based on plan
- **Usage Tracking**: Usage counters initialized to 0 for new memberships

### Add-on Purchase Rules

- **Active Membership Required**: Add-ons can only be purchased for active memberships
- **Immediate Effect**: Add-ons take effect immediately unless specified otherwise
- **Quantity Tracking**: Add-on quantities are tracked separately from plan limits
- **Feature Flags**: Feature-based add-ons can include JSON payload for configuration

### Payment Rules

- **Transaction Context**: Business context stored in transaction metadata for post-payment fulfillment
- **Idempotency**: Payment processing is idempotent to prevent duplicate charges
- **Audit Trail**: Complete audit trail from payment initiation to service activation
- **Error Recovery**: Failed payments can be retried with proper error handling

### Data Integrity Rules

- **Foreign Key Constraints**: All relationships enforced at database level
- **Transaction Isolation**: Payment processing uses database transactions for consistency
- **Status Tracking**: All entities have proper status tracking for monitoring
- **Timestamp Management**: All entities track creation and update timestamps

---

## JSON Format Requirements

### Payment Gateway Enum Values

The system uses camel case naming for JSON serialization. When sending requests, use these exact values for the `PaymentGateway` field:

- `"vnPay"` - VNPay payment gateway
- `"payPal"` - PayPal payment service
- `"stripe"` - Stripe payment processing
- `"bankTransfer"` - Bank transfer
- `"payOS"` - PayOS payment gateway

**Note**: The `JsonStringEnumConverter` with `JsonNamingPolicy.CamelCase` automatically converts enum values to camel case. So `VNPay` becomes `"vnPay"`, `PayPal` becomes `"payPal"`, etc.

### Example Request Format

```json
{
  "Total": 29.99,
  "PaymentGateway": "payOS",
  "Purpose": "membership",
  "UserId": "user-guid",
  "OrgId": "org-guid",
  "PlanId": 1,
  "AutoRenew": true
}
```

## API Endpoints Summary

### Transaction Endpoints

- `POST /transaction/process-payment` - Initiate payment processing
- `POST /transaction/confirm-payment-with-context` - Confirm payment and fulfill business logic
- `POST /transaction/cancel-payment` - Cancel payment transaction
- `GET /transaction/{transactionId}` - Get transaction details

### Membership Endpoints

- `POST /membership/create-or-renew` - Create or renew membership
- `POST /membership/change-plan` - Change subscription plan
- `POST /membership/purchase-addon` - Purchase add-on
- `POST /membership/track-usage` - Track resource usage
- `GET /membership/{membershipId}/org/{orgId}/feature/{featureKey}` - Check feature access

### Access Tool Endpoints

- `GET /user-access-tool/get-all` - Get all user access tools
- `GET /user-access-tool/get-active` - Get active user access tools
- `POST /user-access-tool/grant-access` - Grant access to specific tool
- `POST /user-access-tool/revoke-access` - Revoke access to specific tool
- `POST /user-access-tool/update-access-tools-for-membership` - Update access tools for membership

---

## Monitoring and Logging

### Key Metrics to Monitor

- Payment success/failure rates
- Membership creation success rates
- Access tool granting success rates
- Transaction processing times
- Error rates by payment gateway

### Logging Requirements

- All payment processing steps
- Database operations with transaction IDs
- Error conditions with full context
- Business logic decisions and outcomes
- External API calls and responses

### Alerting

- Payment gateway failures
- High error rates in payment processing
- Failed membership creations after successful payments
- Data consistency issues
- Performance degradation in payment flows
