6.1 Major Features
6.1.1 Guests use IMOS Web Application
FE-01: Authenticate web with email and password validation
FE-02: Browse public map templates and view public maps
FE-03: Access FAQ knowledge base for common questions
FE-04: View system information and educational features overview

6.1.2 Registered Users (Educators) use IMOS Web Application
As an educator who has logged in to IMOS, I want to be able to:
FE-01: Authenticate using email and password with account security
FE-02: Reset password via email when forgotten
FE-03: View, modify profile information (name, phone, contact details)
FE-04: Create, view, modify educational organizations
FE-05: Create, view, modify project
FE-06: Create, view, modify map
FE-07: Create, view, modify map style
FE-08: View and configure map view settings (zoom, center coordinates, base layers)
FE-09: Accept or decline organization invitations
FE-10: Create, view, modify educational maps with curriculum-aligned templates
FE-11: Create, view, modify various layer types (raster, vector, WMS) to educational maps
FE-12: Save and manage map configurations as reusable templates with categories for later use.
FE-13: Modify, view the permission map access
FE-14: Modify, view the base map style for the Project
FE-15: Execute the rendering or publishing process of the Map
FE-16: Create, view, modify necessary Layers within the Project
FE-17: Modify the appearance and order of Layers
FE-18: Create, view, modify fundamental map geometry objects are linked to specific Layers
FE-19: Map or link semantic entities (Location, Zone) to geometric Map Features (Pin, Polygon)
FE-20: Create, view, modify data annotation are linked to specific Layers
FE-21: Create, view, modify interaction behavior on map feature
FE-22: Create, view, modify Location or Point of Interest entities in the Project
FE-23: Create, view, modify Location Types for Location or Point of Interest
FE-24: Create, view, modify Location Tag for Location or Point of Interest
FE-25: Create, view, modify Zone in the Project
FE-26: Create, view, modify Zone Tag for Zone
FE-27: Create, view, modify Story Maps to narrate scenarios
FE-28: Create, view, modify events related to time/history within the Story Map
FE-29: Create, view, modify multiple Segments on Story Map
FE-30: Link Notes/Locations to Segments
FE-31: Add Media (video, images, information) to the Segments
FE-32: Create, view, modify Animations to describe transitions or movements between Notes
FE-33: Provide functionality to play the Story Map sequence
FE-34: Upload custom layer files (GeoJSON, Shapefile, KML) for lessons
FE-35: Subscribe to institutional membership plans
FE-36: View usage quotas against educational plan limits
FE-37: Upgrade/downgrade subscription plans as institutional needs change
FE-38: Process payments through educational payment gateways
FE-39: View payment transaction history for institutional accounting
FE-40: Export maps in multiple formats (PDF, PNG, GeoJSON, KML) for handouts, projections and lessons
FE-41: View export history educational materials
FE-42: Create support tickets for technical assistance
FE-43: Embed Created Maps into External Websites via Shareable Widget/Script
FE-44: Filter and Highlight Data by Zone or Zone Type

6.1.3 Organization Owners/Admins use IMOS Web Application
As an organization owner or admin who has logged in to IMOS, I want to be able to:
FE-01: Manage organization settings and contact information
FE-02: Invite educators with specific organizational roles (Owner, Admin, Member, Viewer)
FE-03: Manage organization member permissions and access levels
FE-04: Monitor organization-wide map usage and educational activities
FE-05: Manage institutional subscription and billing information
FE-06: Manage Shared Story Map Templates for Teachers in the Organization
6.1.4 System Administrators use IMOS Admin Console
As a system administrator who has logged in to the IMOS admin console, I want to be able to:
FE-01: Create, view, modify map template
FE-02: Manage all users, organizations, and system-wide settings
FE-03: Manage subscription plans and educational pricing tiers
FE-04: Handle support tickets and technical issues
FE-05: Manage map templates and educational content libraries
FE-06: Manage approve/decline export map

- User Management

* User Registration
* User Login
* Email Verification System
* Password Reset Functionality
* User Profile Management
* Role-Based Access Control (RBAC)
* View All Users
* Get User Details
* Update User Status
* Delete User

- Organization Management

* Create Organization
* Update Organization
* Delete Organization
* View Organization Details
* Get My Organizations
* View All Organizations
* Send invite to Organization
* Accept invite to Organization
* Reject invite to Organization
* Update Permission user in Organization
* View Organization Members
* Add Member to Organization
* Remove Member from Organization
* Update Member Role in Organization
* Transfer Organization Ownership
* Send Invite to Organization
* Accept Invite to Organization
* Reject Invite to Organization
* Cancel Invite to Organization
* View My Invitations
* View Organization Usage Statistics
* Monitor Organization Quotas
* Check Organization Resource Quota
* View Organization Subscription Details
* Monitor Active Memberships
* Track Pending/Expired Memberships
* View Organization Billing Information
* Monitor Recent Transactions
* Set Organization Admin Permissions

- Map Management

* Create Map
* Create Map from Template
* View/Get Map Details
* Update Map Information
* Delete Map
* List My Maps
* List Organization Maps
* Create Map Template
* View Map Templates
* Get Template Details
* Add Layer to Map
* Remove Layer from Map
* Update Map Layer Configuration
* Get Layer Data
* Share Map with Users
* Unshare Map
* Set Map Permissions

- Story Map Features

* Create Story Map Segment
* View Story Map Segments
* Update Story Map Segment
* Delete Story Map Segment
* Segment Timeline Management
* Create Segment Zone
* View Segment Zones
* Update Segment Zone
* Delete Segment Zone
* Zone Analytics Generation
* Create Segment Layer
* View Segment Layers
* Update Segment Layer
* Delete Segment Layer
* Create Timeline Step
* View Timeline Steps
* Update Timeline Step
* Delete Timeline Step
* Points of Interest (POI)
* Create Map POI
* View Map POIs
* Update Map POI
* Delete Map POI
* Search Map POIs
* Create Segment POI
* View Segment POIs
* Update Segment POI
* Delete Segment POI
* Filter POIs by Segment

- Payment System

* PayOS Integration
* Stripe Integration
* Process Payment
* Confirm Payment
* Cancel Payment
* View Payment History

- Subscription & Membership

* Create Membership Plan
* View Membership Plans
* Update Membership Plan
* Delete Membership Plan
* Plan Feature Management
* Subscribe to Plan
* Upgrade Subscription
* Downgrade Subscription
* Cancel Subscription
* Track Usage Quotas
* Reset Monthly Quotas

- Export System

* Generate High-Quality PDF
* Custom PDF Templates
* GeoJSON Export
* Publish map

- Notification System

* Send Email Notifications
* Create In-app Notifications
* View User Notifications
* Mark Notifications as Read

- Support & Help

* View All FAQs
* Get FAQs by Category
* Get FAQ Details
* Get FAQ Categories
* Create Support Ticket
* View Support Tickets
* Update Ticket Status
