BUSINESS RULES DOCUMENT
Custom Map OSM Backend System
=====================================

1. # USER MANAGEMENT BUSINESS RULES

1.1 User Entity Rules:

- Email must be unique and required (max 100 characters)
- Password hash is required for authentication
- Full name is optional (max 100 characters)
- Phone number is optional (max 20 characters)
- User must be associated with a role (UserRole)
- User must have an account status (AccountStatus)
- Created timestamp is automatically set to current time
- User can belong to multiple organizations through memberships

  1.2 User Role Rules:

- Role name is required (max 50 characters)
- Roles define user permissions and access levels
- Users must have exactly one role assigned

  1.3 Account Status Rules:

- Account status determines if user can access the system
- Status changes affect user authentication and authorization

  1.4 User Preferences Rules:

- Users can customize their application settings
- Preferences are user-specific and persist across sessions

  1.5 User Favorite Templates Rules:

- Users can save map templates as favorites
- Favorites are user-specific and help with quick access

  1.6 User Access Tools Rules:

- Users can be granted access to specific tools
- Tool access is controlled by membership levels

2. # ORGANIZATION MANAGEMENT BUSINESS RULES

2.1 Organization Entity Rules:

- Organization name is required (max 255 characters)
- Abbreviation is optional (max 50 characters)
- Contact email is optional (max 255 characters)
- Contact phone is optional (max 50 characters)
- Organization must have an owner (User)
- Owner cannot be deleted while organization exists (Restrict delete)
- Organization can be active or inactive
- Created and updated timestamps are tracked

  2.2 Organization Member Rules:

- Users can be members of multiple organizations
- Member type defines their role within the organization
- Membership can be active or inactive
- Members have specific permissions within the organization

  2.3 Organization Location Rules:

- Organizations can have multiple locations
- Location status tracks if location is active
- Geographic coordinates are stored for mapping purposes

  2.4 Organization Member Type Rules:

- Defines different types of membership within organizations
- Examples: Owner, Admin, Member, Viewer

3. # MAP MANAGEMENT BUSINESS RULES

3.1 Map Entity Rules:

- Map must belong to a user (creator)
- Map must belong to an organization
- Map name is optional (max 255 characters)
- Geographic bounds define the map's coverage area
- Map configuration stores view settings and layers
- Base layer defaults to "osm" if not specified
- Maps can be public or private
- Maps can be active or inactive
- Maps can be based on templates
- Created and updated timestamps are tracked
- Preview images can be stored for map thumbnails

  3.2 Map Layer Rules:

- Maps can have multiple layers
- Layer order determines display priority
- Layer visibility can be controlled
- Layer opacity can be adjusted
- Layer-specific settings are stored

  3.3 Map History Rules:

- All map changes are tracked in history
- History includes who made changes and when
- Previous versions can be restored
- History helps with collaboration and auditing

  3.4 Map Template Rules:

- Templates provide predefined map configurations
- Templates can be public or private
- Templates help users create maps quickly
- Template usage is tracked for analytics

4. # LAYER MANAGEMENT BUSINESS RULES

4.1 Layer Entity Rules:

- Layer must belong to a user (creator)
- Layer name is optional (max 255 characters)
- Layer must have a type (LayerType)
- Layer must have a source (LayerSource)
- Layer data can be stored as file path or direct data
- Layer style defines visual appearance
- Layers can be public or private
- Created and updated timestamps are tracked
- Layer data can be large (stored as text)
- Layer style is stored as JSON/text

  4.2 Layer Type Rules:

- Defines the type of layer (e.g., Vector, Raster, WMS)
- Type determines how layer is processed and displayed

  4.3 Layer Source Rules:

- Defines where layer data comes from
- Sources can be files, databases, web services, etc.

5. # MEMBERSHIP AND BILLING BUSINESS RULES

5.1 Membership Entity Rules:

- User must belong to an organization
- Membership must have a plan (MembershipPlan)
- Start date is required
- End date is optional (for ongoing memberships)
- Status tracks membership state (active, expired, etc.)
- Auto-renewal can be enabled/disabled
- Current usage is tracked as JSON data
- Usage resets are tracked with last reset date
- Created and updated timestamps are tracked
- Foreign key relationships prevent orphaned records

  5.2 Membership Plan Rules:

- Plans define pricing and feature limits
- Plans have specific quotas and restrictions
- Plan changes affect user access and billing

  5.3 Membership Status Rules:

- Tracks whether membership is active, expired, suspended, etc.
- Status affects user access to features

  5.4 Subscription Plan Change Rules:

- Users can change their subscription plan at any time during their active membership
- Plan changes take effect immediately upon successful processing
- New plan features and quotas become available immediately
- Auto-renewal settings can be updated during plan changes
- Plan changes are not allowed if the new plan is inactive or doesn't exist
- Users cannot change to the same plan they currently have
- Upgrades (higher-priced plans) reset usage cycles to provide immediate access to higher quotas
- Downgrades (lower-priced plans) cap current usage to new plan limits
- Pro-rated billing should be handled by the billing system (not in core membership logic)
- Plan changes maintain the original membership start date for billing continuity
- Usage tracking is adjusted based on plan change type (upgrade/downgrade)
- Failed plan changes do not affect existing membership status

6. # TRANSACTION AND PAYMENT BUSINESS RULES

6.1 Transaction Entity Rules:

- Transaction must have a payment gateway
- Transaction reference is required (max 100 characters)
- Amount is required with precision (18,2)
- Status defaults to "pending"
- Transaction date is required
- Purpose is required (max 100 characters)
- Can be linked to membership or export
- Created timestamp is required
- Foreign key relationships maintain data integrity

  6.2 Payment Gateway Rules:

- Defines available payment methods
- Gateway configuration affects transaction processing

7. # COLLABORATION BUSINESS RULES

7.1 Collaboration Entity Rules:

- Collaboration targets can be maps, layers, or other entities
- Target type defines what can be shared
- User must be specified (who is being given access)
- Permission level must be specified
- Invited by field tracks who initiated the collaboration
- Created and updated timestamps are tracked
- Cascade delete removes collaborations when user is deleted
- Restrict delete prevents deletion of referenced entities

  7.2 Collaboration Permission Rules:

- Defines different permission levels (read, write, admin)
- Permissions control what collaborators can do

  7.3 Collaboration Target Type Rules:

- Defines what types of entities can be shared
- Examples: Map, Layer, Organization

8. # EXPORT BUSINESS RULES

8.1 Export Entity Rules:

- File path is required (max 255 characters)
- File size is required
- Quota type is required (max 50 characters)
- Must be linked to a user
- Must be linked to a membership
- Must be linked to a map
- Must have an export type
- Created timestamp is automatically set
- Foreign key relationships prevent orphaned records

  8.2 Export Type Rules:

- Defines available export formats (PDF, PNG, GeoJSON, etc.)
- Type determines file format and processing

9. # ANNOTATION BUSINESS RULES

9.1 Annotation Entity Rules:

- Annotation must have a type
- Annotation must belong to a map
- Geometry is optional (stored as JSON)
- Properties are optional (stored as JSON)
- Created timestamp is required
- Cascade delete removes annotations when map is deleted
- Restrict delete prevents deletion of annotation types

  9.2 Annotation Type Rules:

- Defines different types of annotations (marker, polygon, line, etc.)
- Type determines how annotation is displayed and processed

10. # BOOKMARK BUSINESS RULES

10.1 Bookmark Entity Rules:

- Bookmark must belong to a map
- Bookmark must belong to a user
- Name is optional (max 100 characters)
- View state is stored as JSON
- Created timestamp is required
- Cascade delete removes bookmarks when map or user is deleted

  10.2 Data Source Bookmark Rules:

- Similar to regular bookmarks but for data sources
- Helps users quickly access frequently used data sources

11. # COMMENT BUSINESS RULES

11.1 Comment Entity Rules:

- Comment can be on a map or layer
- User is required
- Content is required (max 1000 characters)
- Position is optional (max 255 characters)
- Created and updated timestamps are tracked
- Restrict delete prevents deletion of referenced maps/layers
- Set null delete behavior for user (comment remains if user deleted)

12. # SUPPORT TICKET BUSINESS RULES

12.1 Support Ticket Entity Rules:

- User is required
- Subject is optional (max 255 characters)
- Message is stored as text (unlimited length)
- Status is required
- Priority defaults to "low"
- Created timestamp is automatically set
- Resolved timestamp tracks when ticket was closed
- Cascade delete removes tickets when user is deleted
- Restrict delete prevents deletion of ticket statuses

  12.2 Ticket Status Rules:

- Defines ticket states (open, in progress, resolved, closed)
- Status affects ticket processing workflow

13. # NOTIFICATION BUSINESS RULES

13.1 Notification Entity Rules:

- User is required
- Type is optional (max 100 characters)
- Message is optional (max 1000 characters)
- Status tracks notification state
- Created timestamp is automatically set
- Sent timestamp tracks when notification was delivered
- Cascade delete removes notifications when user is deleted

14. # ADVERTISEMENT BUSINESS RULES

14.1 Advertisement Entity Rules:

- Title is required (max 200 characters)
- Content is required (max 1000 characters)
- Image URL is required (max 500 characters)
- Start date is required
- End date is required
- Active status controls visibility
- Advertisements are system-wide (not user-specific)

15. # ACCESS TOOL BUSINESS RULES

15.1 Access Tool Entity Rules:

- Tool name is required (max 100 characters)
- Description is required (max 500 characters)
- Icon URL is required (max 255 characters)
- Required membership level controls access
- Tools are system-wide features that can be restricted

16. # FAQ BUSINESS RULES

16.1 FAQ Entity Rules:

- Question is required (max 500 characters)
- Answer is required (unlimited text)
- Category is required (max 100 characters)
- Created timestamp is automatically set
- FAQs are system-wide knowledge base entries

17. # GENERAL BUSINESS RULES

17.1 Data Integrity Rules:

- All foreign key relationships maintain referential integrity
- Cascade delete behaviors are carefully designed to prevent orphaned data
- Restrict delete behaviors prevent accidental deletion of referenced data
- Set null behaviors allow graceful handling of deleted references

  17.2 Timestamp Rules:

- Created timestamps are automatically set for most entities
- Updated timestamps track modification times
- Timestamps use database-specific functions for consistency

  17.3 String Length Rules:

- All string fields have appropriate maximum lengths
- Text fields are used for unlimited content
- JSON fields store complex structured data

  17.4 Required Field Rules:

- Critical fields are marked as required
- Optional fields allow flexibility in data entry
- Default values are provided where appropriate

  17.5 Relationship Rules:

- One-to-many relationships are properly configured
- Many-to-many relationships use junction tables where needed
- Self-referencing relationships are supported (e.g., user invitations)

  17.6 Security Rules:

- User authentication and authorization are enforced
- Role-based access control is implemented
- Organization-based data isolation is maintained
- Public/private visibility controls are in place

  17.7 Performance Rules:

- Indexes are created on frequently queried fields
- Large data fields (JSON, text) are optimized for storage
- Cascade operations are minimized to prevent performance issues

  17.8 Audit Rules:

- Creation and modification timestamps are tracked
- User actions are logged where appropriate
- Data changes can be traced through history tables

This document provides a comprehensive overview of all business rules governing the Custom Map OSM Backend System, ensuring data integrity, security, and proper system operation.
