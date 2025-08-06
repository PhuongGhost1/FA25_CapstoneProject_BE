### Lookup Tables (Enumerations/Reference Data)
1. **user_roles**: Defines the roles that users can have in the system.
   - `role_id`: Primary key (UUID).
   - `name`: Unique role name (e.g., 'Admin', 'User', 'Guest').
2. **account_statuses**: Statuses for user accounts.
   - `status_id`: Primary key (UUID).
   - `name`: Unique status name (e.g., 'Active', 'Suspended', 'Pending').
3. **layer_sources**: Source types for layers (e.g., OSM, Uploaded, External).
   - `source_type_id`: Primary key (UUID).
   - `name`: Unique source name.
4. **payment_gateways**: Payment gateways integrated (e.g., 'VNPay', 'PayPal').
   - `gateway_id`: Primary key (UUID).
   - `name`: Unique gateway name.
5. **ticket_statuses**: Statuses for support tickets.
   - `status_id`: Primary key (UUID).
   - `name`: Unique status name (e.g., 'Open', 'In Progress', 'Closed').
6. **export_types**: Types of exports (e.g., 'PDF', 'PNG', 'GeoJSON').
   - `type_id`: Primary key (UUID).
   - `name`: Unique export type name.
7. **membership_statuses**: Statuses for memberships (e.g., 'Active', 'Expired', 'Canceled').
   - `status_id`: Primary key (UUID).
   - `name`: Unique status name.
8. **organization_members_types**: Types of roles for organization members (e.g., 'Owner', 'Admin', 'Member').
   - `type_id`: Primary key (UUID).
   - `name`: Unique role type name.
9. **organization_locations_status**: Statuses for organization locations (e.g., 'Active', 'Inactive', 'Pending').
   - `status_id`: Primary key (UUID).
   - `name`: Unique status name.
10. **layer_types**: Types of layers (e.g., 'Point', 'Line', 'Polygon', 'Raster').
    - `layer_type_id`: Auto-increment primary key.
    - `type_name`: Unique name for the layer type.
    - `description`: Description of the layer type.
    - `icon_url`: URL to an icon representing the layer type.
    - `is_active`: Whether the layer type is active.
    - `created_at`: Timestamp of creation.
11. **collaboration_target_types**: Types of targets for collaboration (e.g., 'Map', 'Layer', 'Organization').
    - `target_type_id`: Primary key.
    - `type_name`: Name of the target type.
    - `description`: Description of the target type.
    - `is_active`: Whether the type is active.
    - `created_at`: Timestamp of creation.
12. **collaboration_permissions**: Permissions for collaborations (e.g., 'View', 'Edit', 'Admin').
    - `permission_id`: Primary key.
    - `permission_name`: Name of the permission.
    - `description`: Description of the permission.
    - `level_order`: Order of the permission level (for hierarchy).
    - `is_active`: Whether the permission is active.
    - `created_at`: Timestamp of creation.
13. **annotation_types**: Types of annotations (e.g., 'Point', 'Line', 'Polygon', 'Text').
    - `type_id`: Primary key.
    - `type_name`: Name of the annotation type.
### Core Tables
14. **users**: Stores user accounts.
    - `user_id`: Primary key (UUID).
    - `email`: Unique email address.
    - `password_hash`: Hashed password.
    - `full_name`: User's full name.
    - `phone`: Phone number.
    - `role_id`: Foreign key to `user_roles` (role of the user).
    - `account_status_id`: Foreign key to `account_statuses` (status of the account).
    - `created_at`: Timestamp of account creation.
    - `last_login`: Timestamp of last login.
15. **plans**: Subscription plans.
    - `plan_id`: Auto-increment primary key.
    - `plan_name`: Unique plan name.
    - `description`: Description of the plan.
    - `price_monthly`: Monthly price.
    - `duration_months`: Duration of the plan in months (if applicable).
    - `max_organizations`: Maximum organizations a user can create.
    - `max_locations_per_org`: Maximum locations per organization.
    - `max_maps_per_month`: Maximum maps that can be created per month.
    - `max_users_per_org`: Maximum users per organization.
    - `map_quota`: Map quota (if any).
    - `export_quota`: Export quota (number of exports allowed).
    - `max_custom_layers`: Maximum custom layers allowed.
    - `priority_support`: Whether priority support is included.
    - `features`: JSON of additional features.
    - `is_active`: Whether the plan is active.
    - `created_at`: Timestamp of creation.
    - `updated_at`: Timestamp of last update.
16. **organizations**: Organizations that users belong to.
    - `org_id`: Primary key (UUID).
    - `org_name`: Organization name.
    - `abbreviation`: Abbreviation of the organization.
    - `description`: Description of the organization.
    - `logo_url`: URL to the organization's logo.
    - `contact_email`: Contact email for the organization.
    - `contact_phone`: Contact phone for the organization.
    - `address`: Physical address of the organization.
    - `owner_user_id`: Foreign key to `users` (the owner of the organization).
    - `created_at`: Timestamp of creation.
    - `updated_at`: Timestamp of last update.
    - `is_active`: Whether the organization is active.
17. **memberships**: Represents a user's membership in an organization under a plan.
    - `membership_id`: Primary key (UUID).
    - `user_id`: Foreign key to `users`.
    - `org_id`: Foreign key to `organizations`.
    - `plan_id`: Foreign key to `plans`.
    - `start_date`: Start date of the membership.
    - `end_date`: End date of the membership.
    - `status_id`: Foreign key to `membership_statuses`.
    - `auto_renew`: Whether the membership auto-renews.
    - `current_usage`: JSON field to track current usage (e.g., number of exports, maps created).
    - `last_reset_date`: Last date when usage was reset (for monthly quotas).
    - `created_at`: Timestamp of creation.
    - `updated_at`: Timestamp of last update.
18. **organization_members**: Members of an organization and their roles.
    - `member_id`: Primary key (UUID).
    - `org_id`: Foreign key to `organizations`.
    - `user_id`: Foreign key to `users`.
    - `members_role_id`: Foreign key to `organization_members_types` (role in the organization).
    - `invited_by`: Foreign key to `users` (who invited the member).
    - `joined_at`: Timestamp when the member joined.
    - `is_active`: Whether the membership is active.
19. **organization_locations**: Physical locations of an organization.
    - `location_id`: Auto-increment primary key.
    - `org_id`: Foreign key to `organizations`.
    - `location_name`: Name of the location.
    - `address`: Address of the location.
    - `latitude`: Latitude of the location.
    - `longitude`: Longitude of the location.
    - `phone`: Phone number of the location.
    - `email`: Email of the location.
    - `website`: Website of the location.
    - `operating_hours`: JSON containing operating hours.
    - `services`: JSON of services offered.
    - `categories`: JSON of categories.
    - `amenities`: JSON of amenities.
    - `photos`: JSON of photo URLs.
    - `social_media`: JSON of social media links.
    - `organization_locations_status_id`: Foreign key to `organization_locations_status` (status of the location).
    - `verified`: Whether the location is verified.
    - `last_verified_at`: Timestamp of last verification.
    - `created_at`: Timestamp of creation.
    - `updated_at`: Timestamp of last update.
20. **map_templates**: Predefined map templates.
    - `template_id`: Auto-increment primary key.
    - `template_name`: Name of the template.
    - `description`: Description of the template.
    - `preview_image`: URL to a preview image.
    - `default_bounds`: JSON of the default map bounds.
    - `template_config`: JSON configuration for the template.
    - `base_layer`: Base layer (e.g., 'osm').
    - `initial_layers`: JSON array of initial layers.
    - `view_state`: JSON of the initial view state.
    - `is_public`: Whether the template is public.
    - `is_active`: Whether the template is active.
    - `is_featured`: Whether the template is featured.
    - `usage_count`: Count of how many times the template has been used.
    - `created_by`: Foreign key to `users` (creator of the template).
    - `created_at`: Timestamp of creation.
    - `updated_at`: Timestamp of last update.
21. **maps**: Maps created by users.
    - `map_id`: Primary key (UUID).
    - `user_id`: Foreign key to `users` (creator of the map).
    - `org_id`: Foreign key to `organizations` (if the map belongs to an organization).
    - `map_name`: Name of the map.
    - `description`: Description of the map.
    - `geographic_bounds`: JSON of the geographic bounds of the map.
    - `map_config`: JSON configuration of the map.
    - `base_layer`: Base layer (e.g., 'osm').
    - `view_state`: JSON of the view state (center, zoom, etc.).
    - `preview_image`: URL to a preview image of the map.
    - `is_public`: Whether the map is public.
    - `is_active`: Whether the map is active.
    - `template_id`: Foreign key to `map_templates` (if created from a template).
    - `created_at`: Timestamp of creation.
    - `updated_at`: Timestamp of last update.
22. **layers**: Layers that can be added to maps.
    - `layer_id`: Primary key (UUID).
    - `user_id`: Foreign key to `users` (creator of the layer).
    - `layer_name`: Name of the layer.
    - `layer_type_id`: Foreign key to `layer_types`.
    - `source_id`: Foreign key to `layer_sources`.
    - `file_path`: Path to the file if the layer is from a file.
    - `layer_data`: JSON data of the layer (if vector data).
    - `layer_style`: JSON style configuration for the layer.
    - `is_public`: Whether the layer is public.
    - `created_at`: Timestamp of creation.
    - `updated_at`: Timestamp of last update.
23. **map_layers**: Association between maps and layers (with additional attributes).
    - `map_layer_id`: Auto-increment primary key.
    - `map_id`: Foreign key to `maps`.
    - `layer_id`: Foreign key to `layers`.
    - `is_visible`: Whether the layer is visible in the map.
    - `z_index`: Z-index for layer ordering.
    - `layer_order`: Order of the layer in the layer list.
    - `custom_style`: JSON of custom style overrides for this layer in the map.
    - `filter_config`: JSON of filter configurations for the layer.
    - `created_at`: Timestamp of creation.
    - `updated_at`: Timestamp of last update.
24. **annotations**: Annotations on maps.
    - `annotation_id`: Auto-increment primary key.
    - `type_id`: Foreign key to `annotation_types`.
    - `map_id`: Foreign key to `maps`.
    - `geometry`: Geometry of the annotation (point, line, polygon).
    - `properties`: JSON of properties (e.g., label, description).
    - `created_at`: Timestamp of creation.
25. **bookmarks**: Bookmarks for map views.
    - `bookmark_id`: Auto-increment primary key.
    - `map_id`: Foreign key to `maps`.
    - `user_id`: Foreign key to `users`.
    - `name`: Name of the bookmark.
    - `view_state`: JSON of the view state (center, zoom, etc.) for the bookmark.
    - `created_at`: Timestamp of creation.
26. **collaborations**: Collaborations (sharing) on targets (maps, layers, etc.).
    - `collaboration_id`: Auto-increment primary key.
    - `target_type_id`: Foreign key to `collaboration_target_types` (type of target: map, layer, etc.).
    - `target_id`: ID of the target (UUID, but stored as string).
    - `user_id`: Foreign key to `users` (the user being given access).
    - `permission_id`: Foreign key to `collaboration_permissions` (permission level).
    - `invited_by`: Foreign key to `users` (who invited the collaborator).
    - `created_at`: Timestamp of creation.
    - `updated_at`: Timestamp of last update.
27. **transactions**: Payment transactions.
    - `transaction_id`: Primary key (UUID).
    - `payment_gateway_id`: Foreign key to `payment_gateways`.
    - `transaction_reference`: Reference ID from the payment gateway.
    - `amount`: Amount of the transaction.
    - `currency`: Currency (default 'USD').
    - `status`: Enum of statuses ('success', 'failed', 'pending').
    - `transaction_date`: Timestamp of the transaction (default now).
    - `created_at`: Timestamp of record creation.
    - `membership_id`: Foreign key to `memberships` (if for a membership).
    - `export_id`: Foreign key to `exports` (if for an extra export).
    - `purpose`: Enum of purposes ('membership', 'extra_export').
28. **notifications**: Notifications for users.
    - `notification_id`: Auto-increment primary key.
    - `user_id`: Foreign key to `users`.
    - `type`: Type of notification (e.g., 'order', 'system').
    - `message`: Notification message.
    - `status`: Status of the notification (e.g., 'unread', 'read').
    - `created_at`: Timestamp of creation.
    - `sent_at`: Timestamp when the notification was sent.
29. **support_tickets**: Support tickets submitted by users.
    - `ticket_id`: Auto-increment primary key.
    - `user_id`: Foreign key to `users`.
    - `subject`: Subject of the ticket.
    - `message`: Detailed message.
    - `status_id`: Foreign key to `ticket_statuses`.
    - `priority`: Enum of priorities ('low', 'medium', 'high').
    - `created_at`: Timestamp of creation.
    - `resolved_at`: Timestamp when the ticket was resolved.
30. **advertisements**: Advertisements for the platform.
    - `ad_id`: Auto-increment primary key.
    - `title`: Title of the ad.
    - `content`: Content of the ad.
    - `image_url`: URL to the ad image.
    - `start_date`: Start date of the ad campaign.
    - `end_date`: End date of the ad campaign.
    - `is_active`: Whether the ad is active.
31. **faqs**: Frequently asked questions.
    - `faq_id`: Auto-increment primary key.
    - `question`: FAQ question.
    - `answer`: FAQ answer.
    - `category`: Category of the FAQ.
    - `created_at`: Timestamp of creation.
32. **map_history**: History of map changes (versioning).
    - `version_id`: Auto-increment primary key.
    - `map_id`: Foreign key to `maps`.
    - `user_id`: Foreign key to `users` (who made the change).
    - `snapshot_data`: JSON snapshot of the map at that version.
    - `created_at`: Timestamp of the version.
33. **access_tools**: Tools that can be accessed by users.
    - `tool_id`: Auto-increment primary key.
    - `name`: Name of the tool.
    - `description`: Description of the tool.
    - `icon_url`: URL to an icon for the tool.
    - `requires_membership`: Whether the tool requires a membership.
34. **user_access_tools**: Tools granted to users.
    - `user_access_tools_id`: Auto-increment primary key.
    - `user_id`: Foreign key to `users`.
    - `tool_id`: Foreign key to `access_tools`.
    - `granted_at`: Timestamp when access was granted.
    - `expires_at`: Timestamp when access expires.
35. **user_favorite_templates**: User's favorite map templates.
    - `user_favorite_templates_id`: Auto-increment primary key.
    - `user_id`: Foreign key to `users`.
    - `template_id`: Foreign key to `map_templates`.
    - `favorited_at`: Timestamp when favorited.
36. **comments**: Comments on maps or layers.
    - `comment_id`: Auto-increment primary key.
    - `map_id`: Foreign key to `maps` (if comment is on a map).
    - `layer_id`: Foreign key to `layers` (if comment is on a layer).
    - `user_id`: Foreign key to `users` (commenter).
    - `content`: Text content of the comment.
    - `position`: JSON of the position on the map (if applicable).
    - `created_at`: Timestamp of creation.
    - `updated_at`: Timestamp of last update.
37. **user_preferences**: User preferences.
    - `user_id`: Primary key (and foreign key to `users`).
    - `language`: Preferred language (default 'en').
    - `default_map_style`: Default map style for the user.
    - `measurement_units`: Preferred measurement units (default 'metric').
38. **data_source_bookmarks**: Bookmarks for data sources (e.g., OSM queries).
    - `bookmark_id`: Auto-increment primary key.
    - `user_id`: Foreign key to `users`.
    - `osm_query`: JSON of the OSM query.
    - `name`: Name of the bookmark.
    - `created_at`: Timestamp of creation.
39. **exports**: Records of map exports.
    - `export_id`: Auto-increment primary key.
    - `user_id`: Foreign key to `users` (who exported).
    - `membership_id`: Foreign key to `memberships` (membership used for export).
    - `map_id`: Foreign key to `maps` (the map that was exported).
    - `file_path`: Path to the exported file.
    - `file_size`: Size of the exported file in bytes.
    - `type_id`: Foreign key to `export_types` (type of export).
    - `quota_type`: Enum indicating if the export used 'included' quota or was an 'extra' (paid).
    - `generated_at`: Timestamp of export generation.

CREATE TABLE `user_roles` (
  `role_id` char(36) PRIMARY KEY,
  `name` varchar(50) UNIQUE NOT NULL
);

CREATE TABLE `account_statuses` (
  `status_id` char(36) PRIMARY KEY,
  `name` varchar(50) UNIQUE NOT NULL
);

CREATE TABLE `layer_sources` (
  `source_type_id` char(36) PRIMARY KEY,
  `name` varchar(50) UNIQUE NOT NULL
);

CREATE TABLE `payment_gateways` (
  `gateway_id` char(36) PRIMARY KEY,
  `name` varchar(50) UNIQUE NOT NULL
);

CREATE TABLE `ticket_statuses` (
  `status_id` char(36) PRIMARY KEY,
  `name` varchar(50) UNIQUE NOT NULL
);

CREATE TABLE `export_types` (
  `type_id` char(36) PRIMARY KEY,
  `name` varchar(50) UNIQUE NOT NULL
);

CREATE TABLE `membership_statuses` (
  `status_id` char(36) PRIMARY KEY,
  `name` varchar(50) UNIQUE NOT NULL
);

CREATE TABLE `organization_members_types` (
  `type_id` char(36) PRIMARY KEY,
  `name` varchar(50) UNIQUE NOT NULL
);

CREATE TABLE `organization_locations_status` (
  `status_id` char(36) PRIMARY KEY,
  `name` varchar(50) UNIQUE NOT NULL
);

CREATE TABLE `layer_types` (
  `layer_type_id` int PRIMARY KEY AUTO_INCREMENT,
  `type_name` varchar(100) UNIQUE NOT NULL,
  `description` text,
  `icon_url` varchar(255),
  `is_active` boolean,
  `created_at` datetime DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE `collaboration_target_types` (
  `target_type_id` char(36) PRIMARY KEY,
  `type_name` varchar(100),
  `description` TEXT,
  `is_active` bool,
  `created_at` datetime DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE `collaboration_permissions` (
  `permission_id` char(36) PRIMARY KEY,
  `permission_name` VARCHAR(50),
  `description` TEXT,
  `level_order` int,
  `is_active` bool,
  `created_at` datetime DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE `annotation_types` (
  `type_id` char(36) PRIMARY KEY,
  `type_name` VARCHAR(50)
);

CREATE TABLE `users` (
  `user_id` char(36) PRIMARY KEY,
  `email` varchar(255) UNIQUE NOT NULL,
  `password_hash` varchar(255) NOT NULL,
  `full_name` varchar(255),
  `phone` varchar(20),
  `role_id` char(36),
  `account_status_id` char(36),
  `created_at` datetime DEFAULT (CURRENT_TIMESTAMP),
  `last_login` datetime
);

CREATE TABLE `plans` (
  `plan_id` int PRIMARY KEY AUTO_INCREMENT,
  `plan_name` varchar(100) UNIQUE NOT NULL,
  `description` text,
  `price_monthly` decimal(10,2),
  `duration_months` int,
  `max_organizations` int,
  `max_locations_per_org` int,
  `max_maps_per_month` int,
  `max_users_per_org` int,
  `map_quota` int,
  `export_quota` int,
  `max_custom_layers` int,
  `priority_support` boolean,
  `features` json,
  `is_active` boolean,
  `created_at` datetime DEFAULT (CURRENT_TIMESTAMP),
  `updated_at` timestamp
);

CREATE TABLE `organizations` (
  `org_id` char(36) PRIMARY KEY,
  `org_name` varchar(255) NOT NULL,
  `abbreviation` varchar(50),
  `description` text,
  `logo_url` varchar(500),
  `contact_email` varchar(255),
  `contact_phone` varchar(20),
  `address` text,
  `owner_user_id` char(36),
  `created_at` datetime DEFAULT (CURRENT_TIMESTAMP),
  `updated_at` timestamp,
  `is_active` boolean
);

CREATE TABLE `memberships` (
  `membership_id` char(36) PRIMARY KEY,
  `user_id` char(36),
  `org_id` char(36),
  `plan_id` int,
  `start_date` timestamp,
  `end_date` timestamp,
  `status_id` char(36),
  `auto_renew` boolean,
  `current_usage` json,
  `last_reset_date` date,
  `created_at` datetime DEFAULT (CURRENT_TIMESTAMP),
  `updated_at` timestamp
);

CREATE TABLE `organization_members` (
  `member_id` char(36) PRIMARY KEY,
  `org_id` char(36),
  `user_id` char(36),
  `members_role_id` char(36),
  `invited_by` char(36),
  `joined_at` timestamp,
  `is_active` boolean
);

CREATE TABLE `organization_locations` (
  `location_id` int PRIMARY KEY AUTO_INCREMENT,
  `org_id` char(36),
  `location_name` varchar(255) NOT NULL,
  `address` text,
  `latitude` decimal(10,8),
  `longitude` decimal(11,8),
  `phone` varchar(20),
  `email` varchar(255),
  `website` varchar(255),
  `operating_hours` json,
  `services` json,
  `categories` json,
  `amenities` json,
  `photos` json,
  `social_media` json,
  `organization_locations_status_id` char(36),
  `verified` boolean DEFAULT false,
  `last_verified_at` timestamp,
  `created_at` datetime DEFAULT (CURRENT_TIMESTAMP),
  `updated_at` timestamp
);

CREATE TABLE `map_templates` (
  `template_id` int PRIMARY KEY AUTO_INCREMENT,
  `template_name` varchar(255) NOT NULL,
  `description` text,
  `preview_image` varchar(255),
  `default_bounds` json,
  `template_config` json,
  `base_layer` varchar(100) NOT NULL DEFAULT 'osm',
  `initial_layers` json,
  `view_state` json,
  `is_public` boolean DEFAULT false,
  `is_active` boolean DEFAULT true,
  `is_featured` boolean DEFAULT false,
  `usage_count` int DEFAULT 0,
  `created_by` char(36),
  `created_at` datetime DEFAULT (CURRENT_TIMESTAMP),
  `updated_at` timestamp
);

CREATE TABLE `maps` (
  `map_id` char(36) PRIMARY KEY,
  `user_id` char(36) NOT NULL,
  `org_id` char(36),
  `map_name` varchar(255) NOT NULL,
  `description` text,
  `geographic_bounds` json,
  `map_config` json,
  `base_layer` varchar(100) NOT NULL DEFAULT 'osm',
  `view_state` json,
  `preview_image` varchar(255),
  `is_public` boolean DEFAULT false,
  `is_active` boolean DEFAULT true,
  `template_id` int,
  `created_at` datetime DEFAULT (CURRENT_TIMESTAMP),
  `updated_at` timestamp
);

CREATE TABLE `layers` (
  `layer_id` char(36) PRIMARY KEY,
  `user_id` char(36),
  `layer_name` varchar(255),
  `layer_type_id` int,
  `source_id` char(36),
  `file_path` varchar(500),
  `layer_data` json,
  `layer_style` json,
  `is_public` boolean,
  `created_at` datetime DEFAULT (CURRENT_TIMESTAMP),
  `updated_at` timestamp
);

CREATE TABLE `map_layers` (
  `map_layer_id` int PRIMARY KEY AUTO_INCREMENT,
  `map_id` char(36),
  `layer_id` char(36),
  `is_visible` boolean,
  `z_index` int,
  `layer_order` int,
  `custom_style` json,
  `filter_config` json,
  `created_at` datetime DEFAULT (CURRENT_TIMESTAMP),
  `updated_at` timestamp
);

CREATE TABLE `annotations` (
  `annotation_id` int PRIMARY KEY AUTO_INCREMENT,
  `type_id` char(36) NOT NULL,
  `map_id` char(36),
  `geometry` geometry,
  `properties` json,
  `created_at` datetime DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE `bookmarks` (
  `bookmark_id` int PRIMARY KEY AUTO_INCREMENT,
  `map_id` char(36),
  `user_id` char(36),
  `name` varchar(255),
  `view_state` json,
  `created_at` datetime DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE `collaborations` (
  `collaboration_id` int PRIMARY KEY AUTO_INCREMENT,
  `target_type_id` char(36) NOT NULL,
  `target_id` varchar(36) NOT NULL,
  `user_id` char(36) NOT NULL,
  `permission_id` char(36) NOT NULL,
  `invited_by` char(36),
  `created_at` datetime DEFAULT (CURRENT_TIMESTAMP),
  `updated_at` timestamp
);

CREATE TABLE `transactions` (
  `transaction_id` char(36) PRIMARY KEY,
  `payment_gateway_id` char(36) NOT NULL,
  `transaction_reference` varchar(255) UNIQUE,
  `amount` decimal(10,2) NOT NULL,
  `currency` varchar(10) NOT NULL DEFAULT 'USD',
  `status` enum(success,failed,pending) NOT NULL DEFAULT 'pending',
  `transaction_date` timestamp NOT NULL DEFAULT (now()),
  `created_at` datetime DEFAULT (CURRENT_TIMESTAMP),
  `membership_id` char(36),
  `export_id` int,
  `purpose` enum(membership,extra_export) NOT NULL
);

CREATE TABLE `notifications` (
  `notification_id` int PRIMARY KEY AUTO_INCREMENT,
  `user_id` char(36),
  `type` varchar(50),
  `message` text,
  `status` varchar(50),
  `created_at` datetime DEFAULT (CURRENT_TIMESTAMP),
  `sent_at` datetime
);

CREATE TABLE `support_tickets` (
  `ticket_id` int PRIMARY KEY AUTO_INCREMENT,
  `user_id` char(36),
  `subject` varchar(255),
  `message` text,
  `status_id` varchar(36),
  `priority` enum(low,medium,high),
  `created_at` datetime DEFAULT (CURRENT_TIMESTAMP),
  `resolved_at` datetime
);

CREATE TABLE `advertisements` (
  `ad_id` int PRIMARY KEY AUTO_INCREMENT,
  `title` varchar(255),
  `content` text,
  `image_url` varchar(255),
  `start_date` datetime,
  `end_date` datetime,
  `is_active` boolean DEFAULT true
);

CREATE TABLE `faqs` (
  `faq_id` int PRIMARY KEY AUTO_INCREMENT,
  `question` varchar(255),
  `answer` text,
  `category` varchar(100),
  `created_at` datetime DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE `map_history` (
  `version_id` int PRIMARY KEY AUTO_INCREMENT,
  `map_id` char(36) NOT NULL,
  `user_id` char(36) NOT NULL,
  `snapshot_data` json NOT NULL,
  `created_at` datetime DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE `access_tools` (
  `tool_id` int PRIMARY KEY AUTO_INCREMENT,
  `name` varchar(100) NOT NULL,
  `description` text,
  `icon_url` varchar(255),
  `requires_membership` boolean DEFAULT true
);

CREATE TABLE `user_access_tools` (
  `user_access_tools_id` int PRIMARY KEY AUTO_INCREMENT,
  `user_id` char(36) NOT NULL,
  `tool_id` int NOT NULL,
  `granted_at` datetime DEFAULT (CURRENT_TIMESTAMP),
  `expires_at` datetime
);

CREATE TABLE `user_favorite_templates` (
  `user_favorite_templates_id` int PRIMARY KEY AUTO_INCREMENT,
  `user_id` char(36) NOT NULL,
  `template_id` int,
  `favorited_at` datetime DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE `comments` (
  `comment_id` int PRIMARY KEY AUTO_INCREMENT,
  `map_id` char(36),
  `layer_id` char(36),
  `user_id` char(36) NOT NULL,
  `content` text NOT NULL,
  `position` json,
  `created_at` datetime DEFAULT (CURRENT_TIMESTAMP),
  `updated_at` timestamp
);

CREATE TABLE `user_preferences` (
  `user_id` char(36) PRIMARY KEY,
  `language` varchar(10) DEFAULT 'en',
  `default_map_style` varchar(50),
  `measurement_units` varchar(20) DEFAULT 'metric'
);

CREATE TABLE `data_source_bookmarks` (
  `bookmark_id` int PRIMARY KEY AUTO_INCREMENT,
  `user_id` char(36) NOT NULL,
  `osm_query` json NOT NULL,
  `name` varchar(100) NOT NULL,
  `created_at` datetime DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE `exports` (
  `export_id` int PRIMARY KEY AUTO_INCREMENT,
  `user_id` char(36) NOT NULL,
  `membership_id` char(36) NOT NULL,
  `map_id` char(36) NOT NULL,
  `file_path` varchar(255) NOT NULL,
  `file_size` bigint,
  `type_id` char(36) NOT NULL,
  `quota_type` enum(included,extra) NOT NULL DEFAULT 'included',
  `generated_at` datetime DEFAULT (CURRENT_TIMESTAMP)
);

ALTER TABLE `users` ADD FOREIGN KEY (`role_id`) REFERENCES `user_roles` (`role_id`);

ALTER TABLE `users` ADD FOREIGN KEY (`account_status_id`) REFERENCES `account_statuses` (`status_id`);

ALTER TABLE `organizations` ADD FOREIGN KEY (`owner_user_id`) REFERENCES `users` (`user_id`);

ALTER TABLE `memberships` ADD FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`);

ALTER TABLE `memberships` ADD FOREIGN KEY (`org_id`) REFERENCES `organizations` (`org_id`);

ALTER TABLE `memberships` ADD FOREIGN KEY (`plan_id`) REFERENCES `plans` (`plan_id`);

ALTER TABLE `memberships` ADD FOREIGN KEY (`status_id`) REFERENCES `membership_statuses` (`status_id`);

ALTER TABLE `organization_members` ADD FOREIGN KEY (`org_id`) REFERENCES `organizations` (`org_id`);

ALTER TABLE `organization_members` ADD FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`);

ALTER TABLE `organization_members` ADD FOREIGN KEY (`members_role_id`) REFERENCES `organization_members_types` (`type_id`);

ALTER TABLE `organization_members` ADD FOREIGN KEY (`invited_by`) REFERENCES `users` (`user_id`);

ALTER TABLE `organization_locations` ADD FOREIGN KEY (`org_id`) REFERENCES `organizations` (`org_id`);

ALTER TABLE `organization_locations` ADD FOREIGN KEY (`organization_locations_status_id`) REFERENCES `organization_locations_status` (`status_id`);

ALTER TABLE `map_templates` ADD FOREIGN KEY (`created_by`) REFERENCES `users` (`user_id`);

ALTER TABLE `maps` ADD FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`);

ALTER TABLE `maps` ADD FOREIGN KEY (`org_id`) REFERENCES `organizations` (`org_id`);

ALTER TABLE `maps` ADD FOREIGN KEY (`template_id`) REFERENCES `map_templates` (`template_id`);

ALTER TABLE `layers` ADD FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`);

ALTER TABLE `layers` ADD FOREIGN KEY (`layer_type_id`) REFERENCES `layer_types` (`layer_type_id`);

ALTER TABLE `layers` ADD FOREIGN KEY (`source_id`) REFERENCES `layer_sources` (`source_type_id`);

ALTER TABLE `map_layers` ADD FOREIGN KEY (`map_id`) REFERENCES `maps` (`map_id`);

ALTER TABLE `map_layers` ADD FOREIGN KEY (`layer_id`) REFERENCES `layers` (`layer_id`);

ALTER TABLE `annotations` ADD FOREIGN KEY (`type_id`) REFERENCES `annotation_types` (`type_id`);

ALTER TABLE `annotations` ADD FOREIGN KEY (`map_id`) REFERENCES `maps` (`map_id`);

ALTER TABLE `bookmarks` ADD FOREIGN KEY (`map_id`) REFERENCES `maps` (`map_id`);

ALTER TABLE `bookmarks` ADD FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`);

ALTER TABLE `collaborations` ADD FOREIGN KEY (`target_type_id`) REFERENCES `collaboration_target_types` (`target_type_id`);

ALTER TABLE `collaborations` ADD FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`);

ALTER TABLE `collaborations` ADD FOREIGN KEY (`permission_id`) REFERENCES `collaboration_permissions` (`permission_id`);

ALTER TABLE `collaborations` ADD FOREIGN KEY (`invited_by`) REFERENCES `users` (`user_id`);

ALTER TABLE `transactions` ADD FOREIGN KEY (`payment_gateway_id`) REFERENCES `payment_gateways` (`gateway_id`);

ALTER TABLE `transactions` ADD FOREIGN KEY (`membership_id`) REFERENCES `memberships` (`membership_id`);

ALTER TABLE `transactions` ADD FOREIGN KEY (`export_id`) REFERENCES `exports` (`export_id`);

ALTER TABLE `notifications` ADD FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`);

ALTER TABLE `support_tickets` ADD FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`);

ALTER TABLE `support_tickets` ADD FOREIGN KEY (`status_id`) REFERENCES `ticket_statuses` (`status_id`);

ALTER TABLE `map_history` ADD FOREIGN KEY (`map_id`) REFERENCES `maps` (`map_id`);

ALTER TABLE `map_history` ADD FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`);

ALTER TABLE `user_access_tools` ADD FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`);

ALTER TABLE `user_access_tools` ADD FOREIGN KEY (`tool_id`) REFERENCES `access_tools` (`tool_id`);

ALTER TABLE `user_favorite_templates` ADD FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`);

ALTER TABLE `user_favorite_templates` ADD FOREIGN KEY (`template_id`) REFERENCES `map_templates` (`template_id`);

ALTER TABLE `comments` ADD FOREIGN KEY (`map_id`) REFERENCES `maps` (`map_id`);

ALTER TABLE `comments` ADD FOREIGN KEY (`layer_id`) REFERENCES `layers` (`layer_id`);

ALTER TABLE `comments` ADD FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`);

ALTER TABLE `user_preferences` ADD FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`);

ALTER TABLE `data_source_bookmarks` ADD FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`);

ALTER TABLE `exports` ADD FOREIGN KEY (`user_id`) REFERENCES `users` (`user_id`);

ALTER TABLE `exports` ADD FOREIGN KEY (`membership_id`) REFERENCES `memberships` (`membership_id`);

ALTER TABLE `exports` ADD FOREIGN KEY (`map_id`) REFERENCES `maps` (`map_id`);

ALTER TABLE `exports` ADD FOREIGN KEY (`type_id`) REFERENCES `export_types` (`type_id`);