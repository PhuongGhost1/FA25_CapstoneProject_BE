# Use Case Document

# Custom Map OSM Backend System

## Table of Contents

1. [Introduction](#introduction)
2. [Actors](#actors)
3. [Use Cases](#use-cases)
4. [Use Case Descriptions](#use-case-descriptions)
5. [Use Case Diagrams](#use-case-diagrams)

---

## Introduction

This document describes the use cases for the Custom Map OSM Backend System, a comprehensive mapping platform that allows users to create, manage, and collaborate on custom maps with OpenStreetMap integration. The system supports user management, organization management, subscription-based memberships, payment processing, and advanced mapping features.

---

## Actors

### Primary Actors

- **Customer**: End users who register, create maps, and use the platform
- **Organization Owner**: Users who create and manage organizations
- **Organization Member**: Users who are members of organizations
- **System Administrator**: Platform administrators who manage the system

### Secondary Actors

- **Payment Gateway**: External payment processing systems (PayOS, Stripe, VNPay, PayPal)
- **Email Service**: External email notification service
- **OpenStreetMap**: External mapping data provider

---

## Use Cases

| No. | Use Case ID | Use Case Name                       | Actor                    | Description                                    | Preconditions                            | Main Flow                                                                                              | Alternate Flow                       | Postconditions                           | Related Req. | Priority | Status |
| --- | ----------- | ----------------------------------- | ------------------------ | ---------------------------------------------- | ---------------------------------------- | ------------------------------------------------------------------------------------------------------ | ------------------------------------ | ---------------------------------------- | ------------ | -------- | ------ |
| 1   | UC-01       | User Registration                   | Customer                 | User creates an account with email & password  | User has valid email                     | 1. User opens Register → 2. Fills info → 3. System validates → 4. Success                              | Invalid email → show error           | Account created, confirmation sent       | FR-01        | High     | Draft  |
| 2   | UC-02       | User Authentication                 | Customer                 | User logs into the system                      | User has registered account              | 1. User enters credentials → 2. System validates → 3. Login successful                                 | Invalid credentials → show error     | User logged in, session created          | FR-02        | High     | Draft  |
| 3   | UC-03       | Password Reset                      | Customer                 | User resets forgotten password                 | User has registered account              | 1. User requests reset → 2. System sends email → 3. User clicks link → 4. Sets new password            | Invalid token → show error           | Password updated, user notified          | FR-03        | Medium   | Draft  |
| 4   | UC-04       | Profile Management                  | Customer                 | User updates profile information               | User is logged in                        | 1. User opens profile → 2. Edits information → 3. Saves changes                                        | Invalid data → show validation error | Profile updated successfully             | FR-05        | Medium   | Draft  |
| 5   | UC-05       | Create Organization                 | Organization Owner       | User creates a new organization                | User is logged in                        | 1. User clicks create org → 2. Fills org details → 3. System validates → 4. Org created                | Invalid data → show error            | Organization created, user becomes owner | FR-06        | High     | Draft  |
| 6   | UC-06       | Invite Organization Members         | Organization Owner/Admin | Owner/Admin invites users to organization      | User is org owner/admin                  | 1. User clicks invite → 2. Enters email & role → 3. System sends invitation                            | Email invalid → show error           | Invitation sent, expires in 7 days       | FR-07        | High     | Draft  |
| 7   | UC-07       | Accept Organization Invitation      | Customer                 | User accepts organization invitation           | User received invitation                 | 1. User clicks invitation link → 2. Reviews details → 3. Accepts invitation                            | Invitation expired → show error      | User becomes org member                  | FR-08        | Medium   | Draft  |
| 8   | UC-08       | Create Custom Map                   | Customer                 | User creates a new custom map                  | User is logged in, has active membership | 1. User clicks create map → 2. Selects template → 3. Configures map → 4. Saves map                     | Quota exceeded → show error          | Map created, usage tracked               | FR-10        | High     | Draft  |
| 9   | UC-09       | Add Layers to Map                   | Customer                 | User adds layers to existing map               | User owns map, has write access          | 1. User opens map → 2. Clicks add layer → 3. Selects layer type → 4. Configures layer                  | Layer limit exceeded → show error    | Layer added to map                       | FR-11        | High     | Draft  |
| 10  | UC-10       | Configure Map Settings              | Customer                 | User configures map view and display settings  | User owns map                            | 1. User opens map settings → 2. Adjusts view → 3. Sets base layer → 4. Saves settings                  | Invalid settings → show error        | Map settings updated                     | FR-12        | Medium   | Draft  |
| 11  | UC-11       | Save Map Template                   | Customer                 | User saves current map as reusable template    | User owns map                            | 1. User clicks save template → 2. Enters template details → 3. System saves template                   | Template name exists → show error    | Template saved, available for reuse      | FR-13        | Medium   | Draft  |
| 12  | UC-12       | Share Map                           | Customer                 | User shares map with other users               | User owns map                            | 1. User clicks share → 2. Sets visibility → 3. Invites collaborators → 4. Sets permissions             | Invalid permissions → show error     | Map shared, collaborators notified       | FR-14        | High     | Draft  |
| 13  | UC-13       | Upload Custom Layer                 | Customer                 | User uploads custom layer files                | User is logged in, has active membership | 1. User clicks upload → 2. Selects file → 3. System validates → 4. Layer processed                     | File format invalid → show error     | Layer uploaded, available for use        | FR-16        | High     | Draft  |
| 14  | UC-14       | Add External Layer Source           | Customer                 | User adds external layer sources (WMS, WFS)    | User is logged in                        | 1. User clicks add external → 2. Enters service URL → 3. System validates → 4. Layer added             | Service unavailable → show error     | External layer added to map              | FR-17        | Medium   | Draft  |
| 15  | UC-15       | Style Layer                         | Customer                 | User customizes layer appearance               | User owns layer                          | 1. User opens layer style → 2. Adjusts colors/symbols → 3. Sets opacity → 4. Saves style               | Invalid style → show error           | Layer style updated                      | FR-18        | Medium   | Draft  |
| 16  | UC-16       | Manage Layer Visibility             | Customer                 | User controls layer visibility and order       | User owns map                            | 1. User opens layer panel → 2. Toggles visibility → 3. Reorders layers → 4. Saves changes              | Invalid order → show error           | Layer visibility updated                 | FR-19        | Medium   | Draft  |
| 17  | UC-17       | Subscribe to Membership Plan        | Customer                 | User subscribes to a membership plan           | User is logged in, has organization      | 1. User selects plan → 2. Enters payment info → 3. Processes payment → 4. Membership activated         | Payment failed → show error          | Membership active, features unlocked     | FR-20, FR-25 | High     | Draft  |
| 18  | UC-18       | Track Usage Quotas                  | System                   | System monitors and enforces usage limits      | User has active membership               | 1. System tracks usage → 2. Checks against limits → 3. Updates counters → 4. Enforces restrictions     | Quota exceeded → block action        | Usage tracked, limits enforced           | FR-21        | High     | Draft  |
| 19  | UC-19       | Upgrade/Downgrade Plan              | Customer                 | User changes subscription plan                 | User has active membership               | 1. User selects new plan → 2. Confirms change → 3. System processes → 4. Plan updated                  | Invalid plan → show error            | Plan changed, features adjusted          | FR-22        | Medium   | Draft  |
| 20  | UC-20       | Process Payment                     | Customer                 | User processes payment for membership/features | User is logged in                        | 1. User initiates payment → 2. Selects gateway → 3. Completes payment → 4. System confirms             | Payment failed → retry flow          | Payment processed, membership activated  | FR-25        | High     | Draft  |
| 21  | UC-21       | Generate Payment Invoice            | System                   | System creates and sends payment invoices      | Payment successful                       | 1. System generates invoice → 2. Sends to user → 3. Records transaction                                | Email failed → retry sending         | Invoice generated and sent               | FR-26        | Medium   | Draft  |
| 22  | UC-22       | View Transaction History            | Customer                 | User views payment and transaction history     | User is logged in                        | 1. User opens history → 2. System displays transactions → 3. User can filter/search                    | No transactions → show empty state   | Transaction history displayed            | FR-27        | Medium   | Draft  |
| 23  | UC-23       | Process Refund                      | System Administrator     | Admin processes refund for user request        | Admin has refund permissions             | 1. Admin reviews request → 2. Validates refund → 3. Processes refund → 4. Notifies user                | Invalid request → reject refund      | Refund processed, user notified          | FR-28        | Low      | Draft  |
| 24  | UC-24       | Export Map                          | Customer                 | User exports map in various formats            | User owns map, has export quota          | 1. User selects export → 2. Chooses format → 3. System processes → 4. File generated                   | Quota exceeded → show error          | Map exported, file available             | FR-29        | High     | Draft  |
| 25  | UC-25       | Track Export Usage                  | System                   | System tracks export usage against quotas      | User has active membership               | 1. System tracks export → 2. Updates usage → 3. Checks limits → 4. Enforces restrictions               | Quota exceeded → block export        | Export usage tracked                     | FR-30        | High     | Draft  |
| 26  | UC-26       | View Export History                 | Customer                 | User views past exports and downloads          | User is logged in                        | 1. User opens export history → 2. System displays exports → 3. User can download                       | No exports → show empty state        | Export history displayed                 | FR-31        | Medium   | Draft  |
| 27  | UC-27       | Collaborate on Map                  | Customer                 | User collaborates with others on maps          | User has map access                      | 1. User opens map → 2. Makes changes → 3. System syncs → 4. Others notified                            | No write access → show error         | Map updated, collaborators notified      | FR-32        | High     | Draft  |
| 28  | UC-28       | Add Map Comments                    | Customer                 | User adds comments to maps and layers          | User has map access                      | 1. User clicks comment → 2. Enters comment → 3. Positions on map → 4. Saves comment                    | Invalid position → show error        | Comment added, others notified           | FR-33        | Medium   | Draft  |
| 29  | UC-29       | Receive Collaboration Notifications | Customer                 | User receives real-time collaboration updates  | User is logged in                        | 1. System detects change → 2. Generates notification → 3. Sends to user → 4. User receives update      | Notification failed → retry sending  | User notified of collaboration activity  | FR-34        | Medium   | Draft  |
| 30  | UC-30       | Create Support Ticket               | Customer                 | User creates support request                   | User is logged in                        | 1. User opens support → 2. Fills ticket form → 3. Submits request → 4. System creates ticket           | Invalid data → show error            | Support ticket created                   | FR-35        | Medium   | Draft  |
| 31  | UC-31       | Send System Notifications           | System                   | System sends notifications to users            | System event occurs                      | 1. System detects event → 2. Generates notification → 3. Sends to user → 4. User receives notification | Notification failed → retry sending  | User notified of system event            | FR-36        | Medium   | Draft  |
| 32  | UC-32       | Access FAQ Knowledge Base           | Customer                 | User accesses frequently asked questions       | User is logged in                        | 1. User opens FAQ → 2. Browses categories → 3. Views questions → 4. Gets answers                       | No FAQs → show empty state           | FAQ content displayed                    | FR-37        | Low      | Draft  |

---

## Use Case Descriptions

### UC-01: User Registration

**Actor**: Customer  
**Description**: A new user creates an account in the system with email and password.  
**Preconditions**: User has a valid email address.  
**Main Flow**:

1. User navigates to the registration page
2. User enters email, password, and full name
3. System validates email format and password strength
4. System checks if email is already registered
5. System creates user account with "Pending" status
6. System sends confirmation email
7. User receives welcome message

**Alternate Flows**:

- 3a. Invalid email format: System shows validation error
- 3b. Weak password: System shows password requirements
- 4a. Email already exists: System shows error message
- 6a. Email sending fails: System retries sending

**Postconditions**: User account created, confirmation email sent, account status is "Pending"

**Related Requirements**: FR-01, BR-01, BR-02

---

### UC-02: User Authentication

**Actor**: Customer  
**Description**: Registered user logs into the system using email and password.  
**Preconditions**: User has a registered account with "Active" status.  
**Main Flow**:

1. User navigates to login page
2. User enters email and password
3. System validates credentials
4. System checks account status
5. System creates user session
6. User is redirected to dashboard

**Alternate Flows**:

- 3a. Invalid credentials: System shows error message
- 4a. Account locked: System shows lockout message
- 4b. Account inactive: System shows activation required message

**Postconditions**: User is logged in, session is active, last login time updated

**Related Requirements**: FR-02, BR-03

---

### UC-17: Subscribe to Membership Plan

**Actor**: Customer  
**Description**: User subscribes to a membership plan for their organization.  
**Preconditions**: User is logged in and has an organization.  
**Main Flow**:

1. User navigates to membership plans
2. User selects desired plan
3. User enters payment information
4. System validates payment details
5. System processes payment through gateway
6. Payment is confirmed
7. System activates membership
8. System sends confirmation email
9. User gains access to plan features

**Alternate Flows**:

- 4a. Invalid payment info: System shows validation error
- 5a. Payment failed: System shows retry options
- 5b. Payment gateway unavailable: System shows alternative gateways
- 7a. Membership activation fails: System shows error, processes refund

**Postconditions**: Membership is active, user has access to plan features, payment recorded

**Related Requirements**: FR-20, FR-25, FR-26, BR-16, BR-17

---

### UC-08: Create Custom Map

**Actor**: Customer  
**Description**: User creates a new custom map with layers and configurations.  
**Preconditions**: User is logged in and has an active membership.  
**Main Flow**:

1. User navigates to map creation page
2. User selects map template or starts blank
3. User enters map name and description
4. System validates map details
5. System checks user's map quota
6. System creates map with initial configuration
7. User is redirected to map editor
8. System tracks map creation in usage

**Alternate Flows**:

- 4a. Invalid map name: System shows validation error
- 5a. Quota exceeded: System shows upgrade options
- 6a. Map creation fails: System shows error message

**Postconditions**: Map is created, user can edit map, usage quota updated

**Related Requirements**: FR-10, FR-21, BR-08, BR-12

---

### UC-24: Export Map

**Actor**: Customer  
**Description**: User exports their map in various formats (PDF, PNG, GeoJSON, KML).  
**Preconditions**: User owns the map and has export quota remaining.  
**Main Flow**:

1. User opens map for export
2. User selects export format
3. User configures export settings
4. System validates export request
5. System checks export quota
6. System processes export
7. System generates export file
8. System provides download link
9. System tracks export usage

**Alternate Flows**:

- 4a. Invalid export settings: System shows validation error
- 5a. Quota exceeded: System shows upgrade options
- 6a. Export processing fails: System shows error message
- 7a. File generation fails: System retries processing

**Postconditions**: Map is exported, file is available for download, export usage tracked

**Related Requirements**: FR-29, FR-30, FR-31, BR-20, BR-21

---

### UC-27: Collaborate on Map

**Actor**: Customer  
**Description**: User collaborates with other users on shared maps.  
**Preconditions**: User has access to the map with appropriate permissions.  
**Main Flow**:

1. User opens shared map
2. User makes changes to map
3. System validates user permissions
4. System saves changes
5. System notifies other collaborators
6. System updates map history
7. Other users see changes in real-time

**Alternate Flows**:

- 3a. No write permission: System shows read-only message
- 4a. Save fails: System shows error message
- 5a. Notification fails: System retries sending

**Postconditions**: Map is updated, collaborators are notified, changes are tracked

**Related Requirements**: FR-32, FR-33, FR-34, BR-23, BR-24

---

### UC-06: Invite Organization Members

**Actor**: Organization Owner/Admin  
**Description**: Organization owner or admin invites users to join the organization.  
**Preconditions**: User is organization owner or admin.  
**Main Flow**:

1. User navigates to organization members page
2. User clicks "Invite Member"
3. User enters email and selects role
4. System validates email and role
5. System checks if user is already a member
6. System creates invitation
7. System sends invitation email
8. Invitation expires in 7 days

**Alternate Flows**:

- 4a. Invalid email: System shows validation error
- 4b. Invalid role: System shows role selection error
- 5a. User already member: System shows existing member message
- 7a. Email sending fails: System retries sending

**Postconditions**: Invitation is sent, expires in 7 days, user can accept/decline

**Related Requirements**: FR-07, BR-06, BR-07

---

### UC-18: Track Usage Quotas

**Actor**: System  
**Description**: System automatically tracks and enforces usage quotas for memberships.  
**Preconditions**: User has an active membership.  
**Main Flow**:

1. System detects user action (create map, export, etc.)
2. System checks current usage against plan limits
3. System updates usage counters
4. System enforces quota restrictions
5. System logs usage for reporting
6. System resets quotas on anniversary date

**Alternate Flows**:

- 2a. Quota exceeded: System blocks action and shows upgrade options
- 4a. Enforcement fails: System logs error and retries

**Postconditions**: Usage is tracked, quotas are enforced, limits are respected

**Related Requirements**: FR-21, BR-13, BR-21

---

### UC-20: Process Payment

**Actor**: Customer  
**Description**: User processes payment for membership or additional features.  
**Preconditions**: User is logged in and has payment information.  
**Main Flow**:

1. User initiates payment process
2. User selects payment gateway (PayOS, Stripe, VNPay, PayPal)
3. User enters payment details
4. System validates payment information
5. System processes payment through gateway
6. Payment gateway confirms transaction
7. System updates transaction status
8. System activates purchased features
9. System sends confirmation email

**Alternate Flows**:

- 4a. Invalid payment info: System shows validation error
- 5a. Payment gateway unavailable: System shows alternative options
- 6a. Payment failed: System shows retry options
- 7a. Transaction update fails: System logs error and retries

**Postconditions**: Payment is processed, features are activated, transaction is recorded

**Related Requirements**: FR-25, FR-26, BR-16, BR-17, BR-19

---

### UC-30: Create Support Ticket

**Actor**: Customer  
**Description**: User creates a support ticket for assistance.  
**Preconditions**: User is logged in.  
**Main Flow**:

1. User navigates to support page
2. User clicks "Create Ticket"
3. User fills ticket form (subject, message, priority)
4. System validates ticket information
5. System creates support ticket
6. System assigns ticket ID
7. System notifies support team
8. User receives confirmation

**Alternate Flows**:

- 4a. Invalid ticket data: System shows validation error
- 5a. Ticket creation fails: System shows error message
- 7a. Notification fails: System retries sending

**Postconditions**: Support ticket is created, support team is notified, user can track ticket

**Related Requirements**: FR-35

---

## Use Case Diagrams

### Primary Use Case Diagram

```
[Customer] --> (User Registration)
[Customer] --> (User Authentication)
[Customer] --> (Create Custom Map)
[Customer] --> (Export Map)
[Customer] --> (Collaborate on Map)
[Organization Owner] --> (Create Organization)
[Organization Owner] --> (Invite Organization Members)
[System Administrator] --> (Process Refund)
[Payment Gateway] --> (Process Payment)
[Email Service] --> (Send Notifications)
```

### Membership and Payment Use Cases

```
[Customer] --> (Subscribe to Membership Plan)
[Customer] --> (Process Payment)
[System] --> (Track Usage Quotas)
[System] --> (Generate Payment Invoice)
[System] --> (Send Membership Notifications)
```

### Map Management Use Cases

```
[Customer] --> (Create Custom Map)
[Customer] --> (Add Layers to Map)
[Customer] --> (Configure Map Settings)
[Customer] --> (Share Map)
[Customer] --> (Upload Custom Layer)
[Customer] --> (Style Layer)
```

### Collaboration Use Cases

```
[Customer] --> (Collaborate on Map)
[Customer] --> (Add Map Comments)
[System] --> (Receive Collaboration Notifications)
[Organization Owner] --> (Invite Organization Members)
[Customer] --> (Accept Organization Invitation)
```

---

## Document Information

**Document Version**: 1.0  
**Last Updated**: [Current Date]  
**Author**: System Analyst  
**Reviewers**: Stakeholder, Business Analyst, Technical Lead  
**Approval Status**: Draft

**Change Log**:

- v1.0: Initial use case document creation
- Added 32 primary use cases covering all functional requirements
- Included detailed use case descriptions with main and alternate flows
- Added use case diagrams for different system areas
- Mapped use cases to functional requirements and business rules

**Next Steps**:

1. Stakeholder review and approval
2. Use case prioritization and sequencing
3. Detailed use case specifications
4. Test case development based on use cases
5. Implementation planning and resource allocation
