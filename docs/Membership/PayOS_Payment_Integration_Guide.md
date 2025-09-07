# PayOS Payment Integration Guide

## Overview

This guide explains the complete PayOS payment integration flow, including the three main APIs and how data flows between them.

## Payment Flow Overview

```
1. Process Payment → 2. User Pays → 3. Confirm/Cancel Payment
     ↓                    ↓                    ↓
Create Payment Link   PayOS Gateway      Update Transaction
Generate QR Code     Process Payment     Grant Access Tools
```

## API Endpoints

### 1. Process Payment API

**Endpoint**: `POST /Transaction/process-payment`  
**Purpose**: Create a payment link and QR code for PayOS payment

### 2. Confirm Payment API

**Endpoint**: `POST /Transaction/confirm-payment-with-context`  
**Purpose**: Verify payment completion and grant access tools

### 3. Cancel Payment API

**Endpoint**: `POST /Transaction/cancel-payment`  
**Purpose**: Cancel a pending payment transaction

---

## 1. Process Payment API

### Request Payload

#### For Membership Purchase

```json
{
  "total": 10.0,
  "paymentGateway": "payOS",
  "purpose": "membership",
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "orgId": "123e4567-e89b-12d3-a456-426614174001",
  "planId": 1,
  "autoRenew": true
}
```

#### For Add-on Purchase (Single Quantity)

```json
{
  "total": 5.0,
  "paymentGateway": "payOS",
  "purpose": "addon",
  "membershipId": "123e4567-e89b-12d3-a456-426614174002",
  "addonKey": "advanced_analytics",
  "quantity": 1
}
```

#### For Add-on Purchase (Multiple Quantities)

```json
{
  "total": 15.0,
  "paymentGateway": "payOS",
  "purpose": "addon",
  "membershipId": "123e4567-e89b-12d3-a456-426614174002",
  "addonKey": "api_access",
  "quantity": 3
}
```

### Response Payload

```json
{
  "approvalUrl": "https://pay.payos.vn/web/db1b43524ae44985a85d80f85a8dd852",
  "paymentGateway": "PayOS",
  "sessionId": "db1b43524ae44985a85d80f85a8dd852",
  "qrCode": "00020101021238600010A000000727013000069704160116LOCCASS0003330260208QRIBFTTA530370454062447555802VN62400836CS62H9BJD45 Payment for CustomMapOSM6304019A",
  "orderCode": "8552886145241895"
}
```

### Key Response Fields for Next APIs

- **`sessionId`** → Use as `paymentId` in confirm/cancel APIs
- **`orderCode`** → Use as `orderCode` in confirm/cancel APIs
- **`approvalUrl`** → Direct user to this URL for payment
- **`qrCode`** → Display QR code for mobile payment

---

## 2. Confirm Payment API

### Request Payload

```json
{
  "paymentGateway": "payOS",
  "paymentId": "db1b43524ae44985a85d80f85a8dd852", // Required: PayOS payment link ID
  "orderCode": "8552886145241895", // Required: Unique order identifier
  "signature": "7659d87175c48409df88ecf3c8b893be86167e5b87e7b59191d0c63eb143f537", // Optional: Passed through to response only
  "purpose": "membership", // Required: For business context
  "transactionId": "97588bfa-606e-4098-8dec-f0b52d2e5235", // Required: Internal transaction tracking
  "userId": "123e4567-e89b-12d3-a456-426614174000", // Required: For membership creation
  "orgId": "123e4567-e89b-12d3-a456-426614174001", // Required: For membership creation
  "planId": 1, // Required: For membership creation
  "autoRenew": true // Required: For membership creation
}

*note:
{
  "paymentGateway": "payOS",
  "paymentId": "db1b43524ae44985a85d80f85a8dd852", // sessionId on processPayment
  "orderCode": "8552886145241895",
  "purpose": "membership",
  "transactionId": "97588bfa-606e-4098-8dec-f0b52d2e5235",
}
```

### Response Payload (Success)

```json
{
  "membershipId": "123e4567-e89b-12d3-a456-426614174002",
  "transactionId": "97588bfa-606e-4098-8dec-f0b52d2e5235",
  "accessToolsGranted": true
}
```

### Important Notes

1. **Payment Status Check**: The confirm payment API checks if the PayOS payment status is "PAID". If the status is "PENDING" (user hasn't completed payment yet), you'll get a "Payment is not completed" error.

2. **Transaction Context**: The system stores business context (userId, orgId, planId, etc.) in the transaction record during process-payment. This context is retrieved during confirm-payment to create memberships and grant access tools.

3. **Debugging**: Check the console logs for detailed information about:
   - PayOS payment status
   - Transaction context storage and retrieval
   - JSON deserialization process

### Response Payload (Failure)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Payment is not completed",
  "status": 400,
  "detail": "Payment.PayOS.NotPaid: Payment is not completed"
}
```

---

## 3. Cancel Payment API

### Request Payload

```json
{
  "paymentGateway": "payOS",
  "paymentId": "db1b43524ae44985a85d80f85a8dd852", // Required: PayOS payment link ID
  "orderCode": "8552886145241895", // Required: Unique order identifier
  "signature": "7659d87175c48409df88ecf3c8b893be86167e5b87e7b59191d0c63eb143f537", // Not used: PayOS doesn't validate signature
  "transactionId": "97588bfa-606e-4098-8dec-f0b52d2e5235" // Required: Internal transaction tracking
}
```

### Response Payload (Success)

```json
{
  "status": "cancelled",
  "gatewayName": "PayOS"
}
```

### Response Payload (Failure)

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Payment has already been completed",
  "status": 400,
  "detail": "Payment.PayOS.AlreadyPaid: Payment has already been completed and cannot be cancelled"
}
```

---

## Data Mapping Between APIs

### From Process Payment → Confirm/Cancel Payment

| Process Payment Response   | Confirm/Cancel Request | Description             |
| -------------------------- | ---------------------- | ----------------------- |
| `sessionId`                | `paymentId`            | PayOS payment link ID   |
| `orderCode`                | `orderCode`            | Unique order identifier |
| `transactionId` (from URL) | `transactionId`        | Internal transaction ID |

### Required Fields for Each API

#### Process Payment (Required)

- `total`: Payment amount
- `paymentGateway`: "payOS"
- `purpose`: "membership" or "addon"
- `userId`, `orgId`, `planId`: For membership creation

#### Confirm Payment (Required)

- `paymentGateway`: "payOS"
- `paymentId`: From process payment `sessionId`
- `orderCode`: From process payment `orderCode`
- `transactionId`: From process payment URL parameter
- `purpose`: Same as process payment
- Business context: `userId`, `orgId`, `planId` (for membership)

#### Confirm Payment (Optional)

- `signature`: Passed through to response only (not validated)

#### Cancel Payment (Required)

- `paymentGateway`: "payOS"
- `paymentId`: From process payment `sessionId`
- `orderCode`: From process payment `orderCode`
- `transactionId`: From process payment URL parameter

#### Cancel Payment (Not Used)

- `signature`: PayOS doesn't validate or use this field

---

## PayOS-Specific Implementation Details

### Signature Usage Clarification

**Important**: The `signature` field in confirm and cancel payment requests is **NOT validated** by the PayOS implementation:

- **Confirm Payment**: Signature is only passed through to the response (not validated)
- **Cancel Payment**: Signature is completely ignored (not used at all)

The actual payment verification is done by:

1. Calling PayOS API to get payment status
2. Checking if `status == "PAID"` for confirm payment
3. Checking if `status != "PAID"` for cancel payment

**Note**: You can omit the `signature` field entirely from confirm/cancel requests if desired.

### Order Code Generation

- **Format**: Numeric timestamp-based (e.g., `8552886145241895`)
- **Constraint**: Must be ≤ 9007199254740991 (JavaScript Number.MAX_SAFE_INTEGER)
- **Uniqueness**: Timestamp + random number ensures uniqueness

### Signature Generation

- **Algorithm**: HMAC-SHA256
- **Data**: Alphabetically sorted query string
- **Format**: `amount=$amount&cancelUrl=$cancelUrl&description=$description&orderCode=$orderCode&returnUrl=$returnUrl`
- **Secret**: PayOS checksum key

### Multi-Item Support

The payment system now supports both single-item (membership) and multi-item (add-on) purchases:

#### **Membership Purchases**

- **Purpose**: `"membership"`
- **Items**: Single membership plan
- **Quantity**: Always 1
- **Description**: "CustomMapOSM Membership"

#### **Add-on Purchases**

- **Purpose**: `"addon"`
- **Items**: Add-on with quantity support
- **Quantity**: 1 or more (specified in request)
- **Description**: "Addon: {addonKey}" or "Addon: {addonKey} x{quantity}"

#### **Price Calculation**

- **Membership**: `total` = membership plan price
- **Add-on**: `total` = (unit price × quantity)
- **Unit Price**: Automatically calculated as `total / quantity`

### PayOS Response Fields

```json
{
  "code": "00",
  "desc": "success",
  "data": {
    "bin": "970416",
    "accountNumber": "LOCCASS000333026",
    "accountName": "HOANG TRONG PHUONG",
    "amount": 244755,
    "description": "CS62H9BJD45 Payment for CustomMapOSM",
    "orderCode": 8552886145241895,
    "currency": "VND",
    "paymentLinkId": "db1b43524ae44985a85d80f85a8dd852",
    "status": "PENDING",
    "checkoutUrl": "https://pay.payos.vn/web/db1b43524ae44985a85d80f85a8dd852",
    "qrCode": "00020101021238600010A000000727013000069704160116LOCCASS0003330260208QRIBFTTA530370454062447555802VN62400836CS62H9BJD45 Payment for CustomMapOSM6304019A"
  },
  "signature": "7659d87175c48409df88ecf3c8b893be86167e5b87e7b59191d0c63eb143f537"
}
```

---

## Error Handling

### Common Error Scenarios

#### 1. Invalid Signature

```json
{
  "code": "201",
  "desc": "Mã kiểm tra(signature) không hợp lệ",
  "data": null
}
```

#### 2. Order Code Validation

```json
{
  "code": "20",
  "desc": "order_code: order_code must not be greater than 9007199254740991, order_code: order_code must be a positive number, order_code: orderCode must be a number conforming to the specified constraints, description: Mô tả tối đa 25 kí tự"
}
```

#### 3. Payment Not Completed

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Payment is not completed",
  "status": 400,
  "detail": "Payment.PayOS.NotPaid: Payment is not completed"
}
```

---

## Testing Workflow

### Step 1: Create Payment

```bash
curl -X POST "http://localhost:5233/Transaction/process-payment" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "total": 10.00,
    "paymentGateway": "payOS",
    "purpose": "membership",
    "userId": "123e4567-e89b-12d3-a456-426614174000",
    "orgId": "123e4567-e89b-12d3-a456-426614174001",
    "planId": 1,
    "autoRenew": true
  }'
```

### Step 2: Confirm Payment

```bash
curl -X POST "http://localhost:5233/Transaction/confirm-payment-with-context" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "paymentGateway": "payOS",
    "paymentId": "PAYMENT_LINK_ID_FROM_STEP_1",      // Required: sessionId from step 1
    "orderCode": "ORDER_CODE_FROM_STEP_1",           // Required: orderCode from step 1
    "signature": "SIGNATURE_FROM_PAYOS_WEBHOOK",     // Optional: Not validated, just passed through
    "purpose": "membership",                         // Required: Business context
    "transactionId": "TRANSACTION_ID_FROM_STEP_1",   // Required: From URL parameter
    "userId": "123e4567-e89b-12d3-a456-426614174000", // Required: For membership creation
    "orgId": "123e4567-e89b-12d3-a456-426614174001",  // Required: For membership creation
    "planId": 1,                                     // Required: For membership creation
    "autoRenew": true                                // Required: For membership creation
  }'
```

### Step 3: Cancel Payment (if needed)

```bash
curl -X POST "http://localhost:5233/Transaction/cancel-payment" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "paymentGateway": "payOS",
    "paymentId": "PAYMENT_LINK_ID_FROM_STEP_1",      // Required: sessionId from step 1
    "orderCode": "ORDER_CODE_FROM_STEP_1",           // Required: orderCode from step 1
    "signature": "SIGNATURE_FROM_PAYOS_WEBHOOK",     // Not used: PayOS doesn't validate this
    "transactionId": "TRANSACTION_ID_FROM_STEP_1"    // Required: From URL parameter
  }'
```

---

## Security Considerations

1. **Signature Verification**: Always verify PayOS signatures using HMAC-SHA256
2. **Order Code Validation**: Ensure order codes are numeric and within limits
3. **Transaction Tracking**: Use transaction IDs to prevent duplicate processing
4. **Status Checking**: Verify payment status before granting access tools

---

## Troubleshooting

### Issue: 500 Error with Successful PayOS Response

**Cause**: JSON deserialization mapping issues  
**Solution**: Ensure JsonPropertyName attributes are correctly mapped for camelCase fields

### Issue: Invalid Signature

**Cause**: Incorrect signature generation algorithm  
**Solution**: Use HMAC-SHA256 with checksum key as secret

### Issue: Order Code Validation Error

**Cause**: Order code format or size issues  
**Solution**: Use numeric order codes ≤ 9007199254740991

### Issue: Description Too Long

**Cause**: Description exceeds 25 character limit  
**Solution**: Shorten description to fit PayOS constraints

### Issue: Missing Membership Context

**Cause**: Transaction context not properly stored or retrieved  
**Symptoms**:

- "Missing membership context" error during confirm payment
- Context data (userId, orgId, planId) not found

**Solution**:

1. Ensure process-payment API is called first to store context
2. Check console logs for "Storing Transaction Context" and "Parsing Transaction Context" messages
3. Verify the transaction.Purpose field contains both purpose and context JSON separated by "|"
4. Make sure all required fields (userId, orgId, planId) are provided in process-payment request

**Debug Steps**:

1. Check console logs for transaction context storage
2. Verify transaction.Purpose field format: `"membership|{\"UserId\":\"...\",\"OrgId\":\"...\",\"PlanId\":1}`
3. Ensure confirm-payment uses the same transactionId from process-payment

### Issue: Multi-Item Support for Add-ons

**Cause**: Payment service not handling multiple quantities for add-on purchases  
**Symptoms**:

- Add-on purchases always show quantity = 1 in payment gateway
- Total amount doesn't reflect quantity × unit price
- Payment description doesn't show quantity information

**Solution**:

- Fixed: Enhanced payment service interface to support `ProcessPaymentReq` with quantity information
- All payment gateways now support multi-item purchases
- PayOS, Stripe, PayPal, and VNPay all handle quantity-based pricing

**Debug Steps**:

1. Check console logs for "Purpose: addon" and quantity information
2. Verify that `request.Quantity` is properly set in the payment request
3. Ensure `request.Total` = (unit price × quantity)
4. Check payment gateway response for correct item breakdown

**Example Multi-Item Request**:

```json
{
  "total": 15.0,
  "paymentGateway": "payOS",
  "purpose": "addon",
  "membershipId": "123e4567-e89b-12d3-a456-426614174002",
  "addonKey": "api_access",
  "quantity": 3
}
```

**Expected PayOS Items**:

```json
{
  "items": [
    {
      "name": "CustomMapOSM Addon: api_access",
      "quantity": 3,
      "price": 5000000 // 15.0 * 24500 / 3 = 122500 per unit
    }
  ]
}
```
