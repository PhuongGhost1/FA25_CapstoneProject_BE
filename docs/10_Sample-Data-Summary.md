# Sample Data Summary for CustomMapOSM Configuration Files

This document provides an overview of all the sample data that has been added to the Entity Framework configuration files based on the User Requirements Document (URD).

## User Management Entities

### UserRole

- **Guest**: Browse templates, view public maps, access FAQs
- **Registered User**: Create maps, import data, export maps, manage organizations
- **Administrator**: Manage users, templates, monitor system

### AccountStatus

- **Active**: Normal functioning account
- **Inactive**: Temporarily disabled account
- **Suspended**: Account suspended due to violations
- **Pending Verification**: New account awaiting email verification

## Layer Management Entities

### LayerType

Based on URD requirements for map layers:

- **Roads**: Street and road networks from OpenStreetMap
- **Buildings**: Building footprints and structures
- **POIs**: Points of Interest including amenities and landmarks
- **GeoJSON**: User uploaded GeoJSON data layers
- **KML**: User uploaded KML data layers
- **CSV**: User uploaded CSV data with coordinates

### LayerSource

- **OpenStreetMap**: Primary data source
- **User Upload**: Files uploaded by users
- **External API**: Third-party data services
- **Database**: Internal database sources
- **Web Service**: External web services

## Export System Entities

### ExportType

Based on URD export format requirements:

- **PDF**: Portable Document Format
- **PNG**: Raster image format
- **SVG**: Vector graphics format
- **GeoJSON**: Geographic data format
- **MBTiles**: Map tile format

## Payment and Billing Entities

### PaymentGateway

Based on URD payment requirements:

- **VNPay**: Vietnamese payment gateway
- **PayPal**: International payment service
- **Stripe**: Credit card processing
- **Bank Transfer**: Direct bank transfers

### MembershipStatus

- **Active**: Current membership in good standing
- **Expired**: Membership has expired
- **Suspended**: Membership temporarily suspended
- **Pending Payment**: Awaiting payment processing
- **Cancelled**: Membership cancelled by user

### Membership Plans

Based on URD subscription management:

#### Free Plan

- Price: $0.00/month
- 1 organization, 1 location per org
- 5 maps per month, 10 map quota
- 5 export quota, 3 custom layers
- Basic features only

#### Basic Plan

- Price: $9.99/month
- 2 organizations, 5 locations per org
- 25 maps per month, 50 map quota
- 50 export quota, 10 custom layers
- All export formats, collaboration, data import

#### Pro Plan

- Price: $29.99/month
- 5 organizations, 20 locations per org
- 100 maps per month, 200 map quota
- 200 export quota, 50 custom layers
- Priority support, analytics, version history

#### Enterprise Plan

- Price: $99.99/month
- Unlimited organizations and locations
- Unlimited maps and exports
- Priority support, API access, white label, SSO

## Organization Management Entities

### OrganizationMemberType

- **Owner**: Full control over organization
- **Admin**: Administrative privileges
- **Member**: Regular member access
- **Viewer**: Read-only access

### OrganizationLocationStatus

- **Active**: Location is operational
- **Inactive**: Location is not active
- **Under Construction**: Location being built/renovated
- **Temporary Closed**: Temporarily closed location

## Support System Entities

### TicketStatus

- **Open**: New ticket awaiting response
- **In Progress**: Ticket being worked on
- **Waiting for Customer**: Awaiting customer response
- **Resolved**: Issue has been resolved
- **Closed**: Ticket is closed

## Annotation System Entities

### AnnotationType

Based on URD annotation requirements:

- **Marker**: Point annotations
- **Line**: Linear annotations
- **Polygon**: Area annotations
- **Circle**: Circular annotations
- **Rectangle**: Rectangular annotations
- **Text Label**: Text annotations

## Collaboration System Entities

### CollaborationPermission

Based on URD collaboration requirements:

- **View**: Can view maps and layers (Level 1)
- **Edit**: Can edit maps and layers (Level 2)
- **Manage**: Can manage maps, layers, and permissions (Level 3)

### CollaborationTargetType

- **Map**: Share entire maps with team members
- **Layer**: Share specific layers with team members
- **Organization**: Share organization resources

## System Features Entities

### AccessTool

Based on system features:

- **Map Creation**: Create and customize maps with OSM data (Basic)
- **Data Import**: Upload GeoJSON, KML, and CSV files (Basic)
- **Export System**: Export maps in various formats (Basic)
- **Advanced Analytics**: Advanced map analytics and reporting (Premium)
- **Team Collaboration**: Share maps and collaborate (Pro)
- **API Access**: Access to REST API for integration (Enterprise)

### FAQ Categories and Questions

Based on URD requirements:

#### Map Creation

- "How do I create a map?" - Step-by-step guide

#### Data Management

- "What file formats can I upload?" - GeoJSON, KML, CSV up to 50MB

#### Export System

- "What export formats are available?" - PDF, PNG, SVG, GeoJSON, MBTiles with resolution options

#### Collaboration

- "How do I share maps with my team?" - Collaboration feature guide

#### Billing

- "What payment methods are accepted?" - VNPay, PayPal, bank transfers

#### Technical

- "What browsers are supported?" - Chrome, Firefox, Edge compatibility

## Implementation Notes

1. All GUIDs use sequential patterns for easy identification during development
2. DateTime.UtcNow is used for created timestamps
3. Sample data aligns with URD functional requirements
4. Pricing is in USD with realistic market rates
5. Feature sets progress logically from Free to Enterprise
6. All sample data is production-ready and realistic

## Usage

When running EF Core migrations, this sample data will be automatically seeded into the database, providing a complete working dataset for:

- User testing and development
- Feature demonstration
- System integration testing
- Load testing with realistic data

The sample data covers all major system functionalities outlined in the User Requirements Document and provides a solid foundation for system development and testing.
