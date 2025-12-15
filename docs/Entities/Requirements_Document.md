# Requirements Document

# Custom Map OSM Backend System

## Table of Contents

1. [Functional Requirements (FR)](#functional-requirements-fr)
2. [Non-Functional Requirements (NFR)](#non-functional-requirements-nfr)
3. [Business Rules (BR)](#business-rules-br)
4. [Requirements Traceability Matrix](#requirements-traceability-matrix)

---

## Functional Requirements (FR)

### User Management Requirements

| No. | Req ID | Type | Requirement Description                                                      | Category  | Priority | Source        | Acceptance Criteria                                                  | Status |
| --- | ------ | ---- | ---------------------------------------------------------------------------- | --------- | -------- | ------------- | -------------------------------------------------------------------- | ------ |
| 1   | FR-01  | FR   | The system shall allow users to register an account with email and password. | User Mgmt | High     | Stakeholder   | User can sign up, confirmation email sent successfully.              | Draft  |
| 2   | FR-02  | FR   | The system shall allow users to authenticate using email and password.       | User Mgmt | High     | Stakeholder   | User can login with valid credentials, invalid attempts are blocked. | Draft  |
| 3   | FR-03  | FR   | The system shall allow users to reset their password via email.              | User Mgmt | Medium   | Stakeholder   | User receives reset link, can set new password successfully.         | Draft  |
| 4   | FR-04  | FR   | The system shall support role-based access control (Admin, User, Guest).     | User Mgmt | High     | Business Rule | Users can only access features based on their assigned role.         | Draft  |
| 5   | FR-05  | FR   | The system shall allow users to update their profile information.            | User Mgmt | Medium   | Stakeholder   | User can modify name, phone, and other profile details.              | Draft  |

### Organization Management Requirements

| No. | Req ID | Type | Requirement Description                                                                      | Category          | Priority | Source        | Acceptance Criteria                                                    | Status |
| --- | ------ | ---- | -------------------------------------------------------------------------------------------- | ----------------- | -------- | ------------- | ---------------------------------------------------------------------- | ------ |
| 6   | FR-06  | FR   | The system shall allow users to create organizations.                                        | Organization Mgmt | High     | Stakeholder   | User can create organization with name, description, and contact info. | Draft  |
| 7   | FR-07  | FR   | The system shall allow organization owners to invite members.                                | Organization Mgmt | High     | Stakeholder   | Owner can send invitations via email with specific roles.              | Draft  |
| 8   | FR-08  | FR   | The system shall allow organization members to accept/decline invitations.                   | Organization Mgmt | Medium   | Stakeholder   | Invited users can accept or decline organization membership.           | Draft  |
| 9   | FR-09  | FR   | The system shall support different organization member roles (Owner, Admin, Member, Viewer). | Organization Mgmt | High     | Business Rule | Each role has specific permissions within the organization.            | Draft  |

### Map Management Requirements

| No. | Req ID | Type | Requirement Description                                      | Category | Priority | Source      | Acceptance Criteria                                                       | Status |
| --- | ------ | ---- | ------------------------------------------------------------ | -------- | -------- | ----------- | ------------------------------------------------------------------------- | ------ |
| 10  | FR-10  | FR   | The system shall allow users to create custom maps.          | Map Mgmt | High     | Stakeholder | User can create map with name, description, and initial configuration.    | Draft  |
| 11  | FR-11  | FR   | The system shall allow users to add layers to maps.          | Map Mgmt | High     | Stakeholder | User can add various layer types (raster, vector, WMS) to maps.           | Draft  |
| 12  | FR-12  | FR   | The system shall allow users to configure map view settings. | Map Mgmt | Medium   | Stakeholder | User can set zoom level, center coordinates, and base layer.              | Draft  |
| 13  | FR-13  | FR   | The system shall allow users to save map templates.          | Map Mgmt | Medium   | Stakeholder | User can save current map configuration as reusable template.             | Draft  |
| 14  | FR-14  | FR   | The system shall allow users to share maps with other users. | Map Mgmt | High     | Stakeholder | User can set map visibility (public/private) and collaborate with others. | Draft  |
| 15  | FR-15  | FR   | The system shall track map usage and history.                | Map Mgmt | Medium   | Stakeholder | System logs map modifications with user and timestamp information.        | Draft  |

### Layer Management Requirements

| No. | Req ID | Type | Requirement Description                                                | Category   | Priority | Source      | Acceptance Criteria                                               | Status |
| --- | ------ | ---- | ---------------------------------------------------------------------- | ---------- | -------- | ----------- | ----------------------------------------------------------------- | ------ |
| 16  | FR-16  | FR   | The system shall allow users to upload custom layer files.             | Layer Mgmt | High     | Stakeholder | User can upload supported file formats (GeoJSON, Shapefile, KML). | Draft  |
| 17  | FR-17  | FR   | The system shall allow users to add external layer sources (WMS, WFS). | Layer Mgmt | Medium   | Stakeholder | User can add layers from external web services.                   | Draft  |
| 18  | FR-18  | FR   | The system shall allow users to style layers.                          | Layer Mgmt | Medium   | Stakeholder | User can customize layer appearance (colors, symbols, opacity).   | Draft  |
| 19  | FR-19  | FR   | The system shall allow users to manage layer visibility and order.     | Layer Mgmt | Medium   | Stakeholder | User can show/hide layers and reorder them in the map.            | Draft  |

### Membership & Subscription Requirements

| No. | Req ID | Type | Requirement Description                                    | Category   | Priority | Source        | Acceptance Criteria                                                  | Status |
| --- | ------ | ---- | ---------------------------------------------------------- | ---------- | -------- | ------------- | -------------------------------------------------------------------- | ------ |
| 20  | FR-20  | FR   | The system shall support multiple subscription plans.      | Membership | High     | Business Rule | System offers different plans with varying features and quotas.      | Draft  |
| 21  | FR-21  | FR   | The system shall track membership usage quotas.            | Membership | High     | Business Rule | System monitors and enforces plan limits (maps, exports, users).     | Draft  |
| 22  | FR-22  | FR   | The system shall allow users to upgrade/downgrade plans.   | Membership | Medium   | Stakeholder   | User can change subscription plan with immediate effect.             | Draft  |
| 23  | FR-23  | FR   | The system shall support automatic membership renewal.     | Membership | Medium   | Business Rule | System can automatically renew memberships based on user preference. | Draft  |
| 24  | FR-24  | FR   | The system shall send membership expiration notifications. | Membership | Medium   | Stakeholder   | Users receive notifications before membership expires.               | Draft  |

### Payment & Transaction Requirements

| No. | Req ID | Type | Requirement Description                                           | Category | Priority | Source        | Acceptance Criteria                                           | Status |
| --- | ------ | ---- | ----------------------------------------------------------------- | -------- | -------- | ------------- | ------------------------------------------------------------- | ------ |
| 25  | FR-25  | FR   | The system shall integrate with payment gateways (PayOS, Stripe). | Payment  | High     | Business Rule | System can process payments through multiple gateways.        | Draft  |
| 26  | FR-26  | FR   | The system shall generate payment invoices.                       | Payment  | Medium   | Stakeholder   | System creates and sends invoices for successful payments.    | Draft  |
| 27  | FR-27  | FR   | The system shall track payment transaction history.               | Payment  | Medium   | Stakeholder   | Users can view their payment history and transaction details. | Draft  |
| 28  | FR-28  | FR   | The system shall support refund processing.                       | Payment  | Low      | Stakeholder   | Administrators can process refunds for valid requests.        | Draft  |

### Export & Download Requirements

| No. | Req ID | Type | Requirement Description                                          | Category | Priority | Source        | Acceptance Criteria                                 | Status |
| --- | ------ | ---- | ---------------------------------------------------------------- | -------- | -------- | ------------- | --------------------------------------------------- | ------ |
| 29  | FR-29  | FR   | The system shall allow users to export maps in multiple formats. | Export   | High     | Stakeholder   | User can export maps as PDF, PNG, GeoJSON, KML.     | Draft  |
| 30  | FR-30  | FR   | The system shall track export usage against membership quotas.   | Export   | High     | Business Rule | System enforces export limits based on user's plan. | Draft  |
| 31  | FR-31  | FR   | The system shall provide export history and download links.      | Export   | Medium   | Stakeholder   | User can view past exports and re-download files.   | Draft  |

### Collaboration Requirements

| No. | Req ID | Type | Requirement Description                                          | Category      | Priority | Source      | Acceptance Criteria                                                  | Status |
| --- | ------ | ---- | ---------------------------------------------------------------- | ------------- | -------- | ----------- | -------------------------------------------------------------------- | ------ |
| 32  | FR-32  | FR   | The system shall allow users to collaborate on maps.             | Collaboration | High     | Stakeholder | Users can share maps with specific permissions (read, write, admin). | Draft  |
| 33  | FR-33  | FR   | The system shall allow users to add comments to maps and layers. | Collaboration | Medium   | Stakeholder | Users can add contextual comments with position information.         | Draft  |
| 34  | FR-34  | FR   | The system shall support real-time collaboration notifications.  | Collaboration | Medium   | Stakeholder | Users receive notifications for collaboration activities.            | Draft  |

### Support & Communication Requirements

| No. | Req ID | Type | Requirement Description                              | Category | Priority | Source      | Acceptance Criteria                                        | Status |
| --- | ------ | ---- | ---------------------------------------------------- | -------- | -------- | ----------- | ---------------------------------------------------------- | ------ |
| 35  | FR-35  | FR   | The system shall provide a support ticket system.    | Support  | Medium   | Stakeholder | Users can create and track support requests.               | Draft  |
| 36  | FR-36  | FR   | The system shall send system notifications to users. | Support  | Medium   | Stakeholder | Users receive notifications for system events and updates. | Draft  |
| 37  | FR-37  | FR   | The system shall provide a FAQ knowledge base.       | Support  | Low      | Stakeholder | Users can access frequently asked questions and answers.   | Draft  |

---

## Non-Functional Requirements (NFR)

### Performance Requirements

| No. | Req ID | Type | Requirement Description                                                          | Category    | Priority | Source        | Acceptance Criteria                                          | Status |
| --- | ------ | ---- | -------------------------------------------------------------------------------- | ----------- | -------- | ------------- | ------------------------------------------------------------ | ------ |
| 1   | NFR-01 | NFR  | The system shall respond to user actions within 2 seconds for 95% of requests.   | Performance | High     | Supervisor    | Response time ≤ 2s measured under normal load (≤ 500 users). | Draft  |
| 2   | NFR-02 | NFR  | The system shall support concurrent access by up to 1000 users.                  | Performance | High     | Business Rule | System maintains performance with 1000 concurrent users.     | Draft  |
| 3   | NFR-03 | NFR  | The system shall process map exports within 30 seconds for files up to 100MB.    | Performance | Medium   | Stakeholder   | Export completion time ≤ 30s for standard file sizes.        | Draft  |
| 4   | NFR-04 | NFR  | The system shall load map data within 5 seconds for standard map configurations. | Performance | Medium   | Stakeholder   | Map rendering time ≤ 5s for typical map setups.              | Draft  |

### Reliability Requirements

| No. | Req ID | Type | Requirement Description                                         | Category    | Priority | Source        | Acceptance Criteria                                    | Status |
| --- | ------ | ---- | --------------------------------------------------------------- | ----------- | -------- | ------------- | ------------------------------------------------------ | ------ |
| 5   | NFR-05 | NFR  | The system shall be available 99.5% during business hours.      | Reliability | High     | Policy        | Uptime log shows ≥ 99.5% per month.                    | Draft  |
| 6   | NFR-06 | NFR  | The system shall have automated backup and recovery procedures. | Reliability | High     | Business Rule | Daily backups with 4-hour recovery time objective.     | Draft  |
| 7   | NFR-07 | NFR  | The system shall handle graceful degradation during high load.  | Reliability | Medium   | Supervisor    | System maintains core functionality during peak usage. | Draft  |

### Security Requirements

| No. | Req ID | Type | Requirement Description                                                          | Category | Priority | Source        | Acceptance Criteria                                              | Status   |
| --- | ------ | ---- | -------------------------------------------------------------------------------- | -------- | -------- | ------------- | ---------------------------------------------------------------- | -------- |
| 8   | NFR-08 | NFR  | The system shall encrypt all sensitive data (passwords, payments) using AES-256. | Security | High     | Mentor        | DB inspection confirms hashed/encrypted fields.                  | Approved |
| 9   | NFR-09 | NFR  | The system shall implement HTTPS for all communications.                         | Security | High     | Policy        | All API endpoints and web interfaces use HTTPS.                  | Draft    |
| 10  | NFR-10 | NFR  | The system shall implement rate limiting to prevent abuse.                       | Security | Medium   | Business Rule | API endpoints have rate limits (100 requests/minute per user).   | Draft    |
| 11  | NFR-11 | NFR  | The system shall log all security-relevant events.                               | Security | Medium   | Policy        | Security events logged with user, timestamp, and action details. | Draft    |
| 12  | NFR-12 | NFR  | The system shall support multi-factor authentication.                            | Security | Low      | Stakeholder   | Users can enable 2FA for enhanced account security.              | Draft    |

### Usability Requirements

| No. | Req ID | Type | Requirement Description                                                                  | Category  | Priority | Source      | Acceptance Criteria                                                | Status |
| --- | ------ | ---- | ---------------------------------------------------------------------------------------- | --------- | -------- | ----------- | ------------------------------------------------------------------ | ------ |
| 13  | NFR-13 | NFR  | The user interface shall be mobile responsive and support major browsers (Chrome, Edge). | Usability | Medium   | Supervisor  | UI tested on mobile + desktop passes layout checks.                | Draft  |
| 14  | NFR-14 | NFR  | The system shall provide intuitive navigation and user experience.                       | Usability | Medium   | Stakeholder | User testing shows 90% task completion rate for common operations. | Draft  |
| 15  | NFR-15 | NFR  | The system shall support internationalization (i18n) for multiple languages.             | Usability | Low      | Stakeholder | System supports at least English and Vietnamese languages.         | Draft  |

### Scalability Requirements

| No. | Req ID | Type | Requirement Description                                         | Category    | Priority | Source        | Acceptance Criteria                                              | Status |
| --- | ------ | ---- | --------------------------------------------------------------- | ----------- | -------- | ------------- | ---------------------------------------------------------------- | ------ |
| 16  | NFR-16 | NFR  | The system shall support horizontal scaling for increased load. | Scalability | Medium   | Business Rule | System can scale to 10,000 users with additional infrastructure. | Draft  |
| 17  | NFR-17 | NFR  | The system shall handle large file uploads (up to 500MB).       | Scalability | Medium   | Stakeholder   | System processes large layer files without timeout errors.       | Draft  |

### Compatibility Requirements

| No. | Req ID | Type | Requirement Description                                                    | Category      | Priority | Source        | Acceptance Criteria                                        | Status |
| --- | ------ | ---- | -------------------------------------------------------------------------- | ------------- | -------- | ------------- | ---------------------------------------------------------- | ------ |
| 18  | NFR-18 | NFR  | The system shall support standard geospatial file formats.                 | Compatibility | High     | Business Rule | System supports GeoJSON, Shapefile, KML, GPX, and GeoTIFF. | Draft  |
| 19  | NFR-19 | NFR  | The system shall integrate with external mapping services (OpenStreetMap). | Compatibility | High     | Stakeholder   | System can use OSM tiles and data as base layers.          | Draft  |

---

## Business Rules (BR)

### User Management Business Rules

| No. | Rule ID | Business Rule Description                                                               | Category   | Rationale / Purpose                          | Source          | Impact      | Status   | Related Req.  | Notes                         |
| --- | ------- | --------------------------------------------------------------------------------------- | ---------- | -------------------------------------------- | --------------- | ----------- | -------- | ------------- | ----------------------------- |
| 1   | BR-01   | A user must register with a valid email before accessing the system.                    | Validation | Ensure unique identification & communication | Stakeholder     | Requirement | Draft    | FR-01, FR-02  | Email format check required.  |
| 2   | BR-02   | User passwords must meet minimum security requirements (8+ chars, mixed case, numbers). | Validation | Ensure account security                      | Security Policy | Requirement | Draft    | FR-01, NFR-08 | Password strength validation. |
| 3   | BR-03   | User accounts are locked after 5 failed login attempts.                                 | Security   | Prevent brute force attacks                  | Security Policy | Process     | Draft    | FR-02, NFR-10 | Account lockout mechanism.    |
| 4   | BR-04   | Each user can belong to multiple organizations with different roles.                    | Data       | Support multi-organization membership        | Business Rule   | Data Model  | Approved | FR-06, FR-09  | Many-to-many relationship.    |

### Organization Management Business Rules

| No. | Rule ID | Business Rule Description                                             | Category      | Rationale / Purpose             | Source        | Impact     | Status   | Related Req. | Notes                                     |
| --- | ------- | --------------------------------------------------------------------- | ------------- | ------------------------------- | ------------- | ---------- | -------- | ------------ | ----------------------------------------- |
| 5   | BR-05   | An organization must have exactly one owner who cannot be removed.    | Data          | Maintain organization ownership | Business Rule | Data Model | Approved | FR-06, FR-09 | Owner role is permanent.                  |
| 6   | BR-06   | Organization invitations expire after 7 days if not accepted.         | Process       | Prevent stale invitations       | Business Rule | Process    | Draft    | FR-07, FR-08 | Automatic cleanup of expired invitations. |
| 7   | BR-07   | Organization members can only invite users with equal or lower roles. | Authorization | Maintain role hierarchy         | Business Rule | Process    | Draft    | FR-07, FR-09 | Role-based invitation permissions.        |

### Map Management Business Rules

| No. | Rule ID | Business Rule Description                                                          | Category      | Rationale / Purpose                                 | Source        | Impact     | Status   | Related Req. | Notes                  |
| --- | ------- | ---------------------------------------------------------------------------------- | ------------- | --------------------------------------------------- | ------------- | ---------- | -------- | ------------ | ---------------------- |
| 8   | BR-08   | Each map must belong to exactly one user (creator) and one organization.           | Data          | Ensure ownership and access control                 | Business Rule | Data Model | Approved | FR-10, FR-14 | Map ownership model.   |
| 9   | BR-09   | Public maps are visible to all users, private maps only to organization members.   | Authorization | Control map visibility                              | Business Rule | Process    | Draft    | FR-14, FR-32 | Map visibility rules.  |
| 10  | BR-10   | Map templates can be used by any user but modified versions belong to the creator. | Process       | Support template sharing while protecting ownership | Business Rule | Process    | Draft    | FR-13, FR-14 | Template usage model.  |
| 11  | BR-11   | Map history is retained for 1 year for audit purposes.                             | Data          | Compliance and debugging                            | Business Rule | Data Model | Draft    | FR-15        | Data retention policy. |

### Membership & Subscription Business Rules

| No. | Rule ID | Business Rule Description                                                | Category      | Rationale / Purpose           | Source        | Impact     | Status   | Related Req. | Notes                        |
| --- | ------- | ------------------------------------------------------------------------ | ------------- | ----------------------------- | ------------- | ---------- | -------- | ------------ | ---------------------------- |
| 12  | BR-12   | Users must have an active membership to access premium features.         | Authorization | Enforce subscription model    | Business Rule | Process    | Approved | FR-20, FR-21 | Feature access control.      |
| 13  | BR-13   | Membership quotas reset monthly on the anniversary date.                 | Process       | Fair usage tracking           | Business Rule | Process    | Draft    | FR-21, FR-24 | Quota reset mechanism.       |
| 14  | BR-14   | Plan upgrades take effect immediately, downgrades at next billing cycle. | Process       | Customer-friendly billing     | Business Rule | Process    | Draft    | FR-22        | Plan change timing.          |
| 15  | BR-15   | Users can have only one active membership per organization.              | Data          | Prevent duplicate memberships | Business Rule | Data Model | Approved | FR-20        | One membership per org rule. |

### Payment & Transaction Business Rules

| No. | Rule ID | Business Rule Description                                     | Category   | Rationale / Purpose                | Source          | Impact          | Status   | Related Req.  | Notes                       |
| --- | ------- | ------------------------------------------------------------- | ---------- | ---------------------------------- | --------------- | --------------- | -------- | ------------- | --------------------------- |
| 16  | BR-16   | Payment must be completed before membership activation.       | Process    | Guarantee revenue and reduce fraud | Business Policy | Workflow/Design | Approved | FR-25, FR-26  | Payment-first model.        |
| 17  | BR-17   | Each transaction ID must be unique across the system.         | Data       | Prevent duplication in database    | System Analyst  | Database Schema | Approved | FR-27, NFR-06 | Use UUID for ID generation. |
| 18  | BR-18   | Refund requests must be processed within 7 working days.      | Constraint | Comply with customer service SLA   | Legal/Policy    | Process         | Draft    | FR-28         | SLA document reference.     |
| 19  | BR-19   | Failed payments result in membership suspension after 3 days. | Process    | Manage payment failures            | Business Rule   | Process         | Draft    | FR-25, FR-24  | Payment failure handling.   |

### Export & Download Business Rules

| No. | Rule ID | Business Rule Description                                        | Category      | Rationale / Purpose         | Source        | Impact     | Status   | Related Req. | Notes                    |
| --- | ------- | ---------------------------------------------------------------- | ------------- | --------------------------- | ------------- | ---------- | -------- | ------------ | ------------------------ |
| 20  | BR-20   | Export files are retained for 30 days before automatic deletion. | Data          | Storage management          | Business Rule | Data Model | Draft    | FR-29, FR-31 | File retention policy.   |
| 21  | BR-21   | Export quotas are enforced per membership plan.                  | Authorization | Enforce subscription limits | Business Rule | Process    | Approved | FR-30, BR-12 | Quota enforcement.       |
| 22  | BR-22   | Large exports (>100MB) require premium membership.               | Authorization | Resource management         | Business Rule | Process    | Draft    | FR-29, FR-30 | Size-based restrictions. |

### Collaboration Business Rules

| No. | Rule ID | Business Rule Description                                         | Category      | Rationale / Purpose                  | Source        | Impact  | Status | Related Req. | Notes                     |
| --- | ------- | ----------------------------------------------------------------- | ------------- | ------------------------------------ | ------------- | ------- | ------ | ------------ | ------------------------- |
| 23  | BR-23   | Map collaborators can only modify maps they have write access to. | Authorization | Protect map integrity                | Business Rule | Process | Draft  | FR-32, FR-33 | Permission-based editing. |
| 24  | BR-24   | Collaboration invitations expire after 14 days.                   | Process       | Prevent stale collaboration requests | Business Rule | Process | Draft  | FR-32        | Collaboration timeout.    |
| 25  | BR-25   | Comments are visible to all users with access to the map/layer.   | Authorization | Transparent collaboration            | Business Rule | Process | Draft  | FR-33        | Comment visibility rules. |

### System & Data Business Rules

| No. | Rule ID | Business Rule Description                                                | Category | Rationale / Purpose        | Source          | Impact     | Status   | Related Req.   | Notes                   |
| --- | ------- | ------------------------------------------------------------------------ | -------- | -------------------------- | --------------- | ---------- | -------- | -------------- | ----------------------- |
| 26  | BR-26   | All user data is encrypted at rest using AES-256.                        | Security | Data protection compliance | Security Policy | Data Model | Approved | NFR-08         | Encryption requirement. |
| 27  | BR-27   | System logs are retained for 1 year for audit purposes.                  | Data     | Compliance and security    | Business Rule   | Data Model | Draft    | NFR-11         | Log retention policy.   |
| 28  | BR-28   | User accounts are automatically deactivated after 2 years of inactivity. | Process  | Data lifecycle management  | Business Rule   | Process    | Draft    | FR-01, FR-02   | Account lifecycle.      |
| 29  | BR-29   | All API requests must include valid authentication tokens.               | Security | API security               | Security Policy | Process    | Approved | NFR-09, NFR-10 | API authentication.     |
| 30  | BR-30   | System backups are performed daily with 30-day retention.                | Data     | Disaster recovery          | Business Rule   | Data Model | Draft    | NFR-06         | Backup policy.          |

---

## Requirements Traceability Matrix

### Functional Requirements Traceability

| FR ID | Requirement         | Related BR          | Related NFR    | Dependencies | Test Cases     |
| ----- | ------------------- | ------------------- | -------------- | ------------ | -------------- |
| FR-01 | User Registration   | BR-01, BR-02        | NFR-08, NFR-09 | -            | TC-001, TC-002 |
| FR-02 | User Authentication | BR-01, BR-03        | NFR-08, NFR-10 | FR-01        | TC-003, TC-004 |
| FR-06 | Create Organization | BR-04, BR-05        | NFR-01         | FR-01, FR-02 | TC-005         |
| FR-10 | Create Maps         | BR-08, BR-09        | NFR-01, NFR-04 | FR-01, FR-06 | TC-006         |
| FR-20 | Subscription Plans  | BR-12, BR-15        | NFR-01         | FR-01        | TC-007         |
| FR-25 | Payment Integration | BR-16, BR-17        | NFR-08, NFR-09 | FR-20        | TC-008         |
| FR-29 | Export Maps         | BR-20, BR-21, BR-22 | NFR-03, NFR-17 | FR-10, FR-20 | TC-009         |
| FR-32 | Map Collaboration   | BR-23, BR-24        | NFR-01         | FR-10, FR-14 | TC-010         |

### Non-Functional Requirements Traceability

| NFR ID | Requirement         | Related BR   | Related FR          | Dependencies | Test Cases |
| ------ | ------------------- | ------------ | ------------------- | ------------ | ---------- |
| NFR-01 | Response Time       | -            | FR-01 to FR-37      | -            | TC-011     |
| NFR-08 | Data Encryption     | BR-26        | FR-01, FR-02, FR-25 | -            | TC-012     |
| NFR-09 | HTTPS Communication | BR-29        | All FR              | -            | TC-013     |
| NFR-10 | Rate Limiting       | BR-03, BR-29 | All API FR          | -            | TC-014     |
| NFR-18 | File Format Support | -            | FR-16, FR-29        | -            | TC-015     |

### Business Rules Traceability

| BR ID | Business Rule          | Related FR   | Related NFR | Impact Level | Implementation Priority |
| ----- | ---------------------- | ------------ | ----------- | ------------ | ----------------------- |
| BR-01 | Email Validation       | FR-01, FR-02 | NFR-08      | High         | P1                      |
| BR-05 | Organization Ownership | FR-06, FR-09 | -           | High         | P1                      |
| BR-08 | Map Ownership          | FR-10, FR-14 | -           | High         | P1                      |
| BR-12 | Membership Access      | FR-20, FR-21 | -           | High         | P1                      |
| BR-16 | Payment First          | FR-25, FR-26 | NFR-08      | High         | P1                      |
| BR-26 | Data Encryption        | -            | NFR-08      | High         | P1                      |

---

## Document Information

**Document Version:** 1.0  
**Last Updated:** [Current Date]  
**Author:** System Analyst  
**Reviewers:** Stakeholder, Business Analyst, Technical Lead  
**Approval Status:** Draft

**Change Log:**

- v1.0: Initial requirements document creation
- Added 37 Functional Requirements
- Added 19 Non-Functional Requirements
- Added 30 Business Rules
- Included traceability matrix

**Next Steps:**

1. Stakeholder review and approval
2. Technical feasibility analysis
3. Test case development
4. Implementation planning
