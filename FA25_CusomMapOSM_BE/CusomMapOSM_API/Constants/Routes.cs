namespace CusomMapOSM_API.Constants;

public static class Routes
{
    public const string ApiBase = "api/v1";
    public const string Health = "health";
    public static class Prefix
    {
        public const string Auth = "auth";
        public const string Organization = "organizations";
        public const string Location = "locations";
        public const string StoryMap = "storymaps";
        public const string Animations = "animations";
        public const string Maps = "maps";
        public const string Usage = "usage";
        public const string Payment = "payment";
        public const string Notifications = "notifications";
        public const string Faqs = "faqs";
        public const string SupportTickets = "support-tickets";
        public const string SystemAdmin = "admin";
        public const string OrganizationAdmin = "organization-admin";
        public const string Osm = "osm";
        public const string Workspace = "workspaces";
        public const string User = "user";
        public const string Layers = "layers";
    }

    public static class AuthEndpoints
    {
        public const string Login = "login";
        public const string VerifyEmail = "verify-email";
        public const string VerifyOtp = "verify-otp";
        public const string Logout = "logout";
        public const string ResetPasswordVerify = "reset-password-verify";
        public const string ResetPassword = "reset-password";
    }

    public static class OrganizationsEndpoints
    {
        public const string GetAll = "";
        public const string GetById = "{id:guid}";
        public const string Create = "";
        public const string Update = "{id:guid}";
        public const string Delete = "{id:guid}";

        // Organization invitations
        public const string InviteMember = "invite-member";
        public const string AcceptInvite = "accept-invite";
        public const string GetMyInvitations = "my-invitations";
        public const string RejectInvite = "invites/reject";
        public const string CancelInvite = "invites/cancel";

        // Organization members
        public const string GetOrganizationMembers = "{orgId:guid}/members";
        public const string UpdateMemberRole = "members/role";
        public const string RemoveMember = "members/remove";

        // User-specific endpoints
        public const string GetMyOrganizations = "my-organizations";
        public const string TransferOwnership = "{orgId:guid}/ownership";

        // Bulk student account creation
        public const string BulkCreateStudents = "bulk-create-students";
    }
    public static class StoryMapEndpoints
    {
        // Story map segments
        public const string GetSegments = "{mapId:guid}/segments";
        public const string CreateSegment = "{mapId:guid}/segments";
        public const string UpdateSegment = "{mapId:guid}/segments/{segmentId:guid}";
        public const string DeleteSegment = "{mapId:guid}/segments/{segmentId:guid}";

        // Segment zones
        public const string GetSegmentZones = "{mapId:guid}/segments/{segmentId:guid}/zones";
        public const string CreateSegmentZone = "{mapId:guid}/segments/{segmentId:guid}/zones";
        public const string UpdateSegmentZone = "{mapId:guid}/segments/{segmentId:guid}/zones/{segmentZoneId:guid}";
        public const string DeleteSegmentZone = "{mapId:guid}/segments/{segmentId:guid}/zones/{segmentZoneId:guid}";
        public const string MoveZoneToSegment = "{mapId:guid}/segments/{fromSegmentId:guid}/zones/{segmentZoneId:guid}/move-to/{toSegmentId:guid}";

        // Segment layers
        public const string GetSegmentLayers = "{mapId:guid}/segments/{segmentId:guid}/layers";
        public const string CreateSegmentLayer = "{mapId:guid}/segments/{segmentId:guid}/layers";
        public const string UpdateSegmentLayer = "{mapId:guid}/segments/{segmentId:guid}/layers/{layerId:guid}";
        public const string DeleteSegmentLayer = "{mapId:guid}/segments/{segmentId:guid}/layers/{layerId:guid}";
        public const string MoveLayerToSegment = "{mapId:guid}/segments/{fromSegmentId:guid}/layers/{segmentLayerId:guid}/move-to/{toSegmentId:guid}";

        // Segment locations (POIs)
        public const string GetSegmentLocations = "{mapId:guid}/segments/{segmentId:guid}/locations";
        public const string GetMapLocations = "{mapId:guid}/locations";
        public const string CreateSegmentLocation = "{mapId:guid}/segments/{segmentId:guid}/locations";
        public const string UpdateSegmentLocation = "{mapId:guid}/segments/{segmentId:guid}/locations/{locationId:guid}";
        public const string DeleteSegmentLocation = "{mapId:guid}/segments/{segmentId:guid}/locations/{locationId:guid}";
        public const string MoveLocationToSegment = "{mapId:guid}/segments/{fromSegmentId:guid}/locations/{locationId:guid}/move-to/{toSegmentId:guid}";

        // Timeline
        public const string GetTimeline = "{mapId:guid}/timeline";
        public const string CreateTimelineStep = "{mapId:guid}/timeline";
        public const string UpdateTimelineStep = "{mapId:guid}/timeline/{stepId:guid}";
        public const string DeleteTimelineStep = "{mapId:guid}/timeline/{stepId:guid}";

        // Segment transitions
        public const string GetSegmentTransitions = "{mapId:guid}/transitions";
        public const string CreateSegmentTransition = "{mapId:guid}/transitions";
        public const string UpdateSegmentTransition = "{mapId:guid}/transitions/{transitionId:guid}";
        public const string DeleteSegmentTransition = "{mapId:guid}/transitions/{transitionId:guid}";
        public const string PreviewTransition = "{mapId:guid}/preview-transition";

        // Story Element Layers
        public const string GetStoryElementLayers = "story-elements/{elementId:guid}/layers";
        public const string CreateStoryElementLayer = "story-elements/layers";
        public const string UpdateStoryElementLayer = "story-elements/layers/{storyElementLayerId:guid}";
        public const string DeleteStoryElementLayer = "story-elements/layers/{storyElementLayerId:guid}";

        // Zone Master Data
        public const string GetZones = "zones";
        public const string GetZone = "zones/{zoneId:guid}";
        public const string GetZonesByParent = "zones/parent/{parentZoneId:guid}";
        public const string SearchZones = "zones/search";
        public const string CreateZone = "zones";
        public const string UpdateZone = "zones/{zoneId:guid}";
        public const string DeleteZone = "zones/{zoneId:guid}";
        public const string SyncZonesFromOSM = "zones/sync-osm";

        // Location Search
        public const string SearchLocations = "locations/search";

        // Route Search
        public const string SearchRoutes = "routes/search";
        public const string SearchRouteBetweenLocations = "routes/between-locations";
        public const string SearchRouteWithMultipleLocations = "routes/multiple-locations";

        // Route Animations
        public const string GetRouteAnimations = "{mapId:guid}/segments/{segmentId:guid}/route-animations";
        public const string GetRouteAnimation = "{mapId:guid}/segments/{segmentId:guid}/route-animations/{routeAnimationId:guid}";
        public const string CreateRouteAnimation = "{mapId:guid}/segments/{segmentId:guid}/route-animations";
        public const string UpdateRouteAnimation = "{mapId:guid}/segments/{segmentId:guid}/route-animations/{routeAnimationId:guid}";
        public const string DeleteRouteAnimation = "{mapId:guid}/segments/{segmentId:guid}/route-animations/{routeAnimationId:guid}";
        public const string MoveRouteToSegment = "{mapId:guid}/segments/{fromSegmentId:guid}/route-animations/{routeAnimationId:guid}/move-to/{toSegmentId:guid}";

        // Timeline Transitions
        public const string GetTimelineTransitions = "{mapId:guid}/timeline-transitions";
        public const string GetTimelineTransition = "{mapId:guid}/timeline-transitions/{transitionId:guid}";
        public const string CreateTimelineTransition = "{mapId:guid}/timeline-transitions";
        public const string UpdateTimelineTransition = "{mapId:guid}/timeline-transitions/{transitionId:guid}";
        public const string DeleteTimelineTransition = "{mapId:guid}/timeline-transitions/{transitionId:guid}";
        public const string GenerateTransition = "{mapId:guid}/timeline-transitions/generate";

        // Animated Layers
        public const string GetAnimatedLayers = "{mapId:guid}/animated-layers";
        public const string GetAnimatedLayer = "{mapId:guid}/animated-layers/{layerId:guid}";
        public const string CreateAnimatedLayer = "{mapId:guid}/animated-layers";
        public const string UpdateAnimatedLayer = "{mapId:guid}/animated-layers/{layerId:guid}";
        public const string DeleteAnimatedLayer = "{mapId:guid}/animated-layers/{layerId:guid}";
        public const string AttachAnimatedLayerToSegment = "{mapId:guid}/animated-layers/{layerId:guid}/attach-segment/{segmentId:guid}";

        // Animated Layer Presets
        public const string GetAnimatedLayerPresets = "animated-layer-presets";
        public const string GetAnimatedLayerPreset = "animated-layer-presets/{presetId:guid}";
        public const string SearchAnimatedLayerPresets = "animated-layer-presets/search";
        public const string CreateAnimatedLayerPreset = "animated-layer-presets";
        public const string UpdateAnimatedLayerPreset = "animated-layer-presets/{presetId:guid}";
        public const string DeleteAnimatedLayerPreset = "animated-layer-presets/{presetId:guid}";
        public const string DuplicateAnimatedLayerPreset = "animated-layer-presets/{presetId:guid}/duplicate";
        public const string CreateAnimatedLayerFromPreset = "animated-layer-presets/{presetId:guid}/create-layer";

        // Enhanced Segment Operations
        public const string GetSegmentEnhanced = "{mapId:guid}/segments/{segmentId:guid}";
        public const string DuplicateSegment = "{mapId:guid}/segments/{segmentId:guid}/duplicate";
        public const string ReorderSegments = "{mapId:guid}/segments/reorder";
    }

    public static class LocationEndpoints
    {
        // Map-level POIs
        public const string GetMapLocations = "{mapId:guid}";
        public const string CreateMapLocation = "{mapId:guid}";

        // Segment-level POIs
        public const string GetSegmentLocations = "{mapId:guid}/segments/{segmentId:guid}";
        public const string CreateSegmentLocation = "{mapId:guid}/segments/{segmentId:guid}";

        // POI management
        public const string UpdateLocation = "{locationId:guid}";
        public const string DeleteLocation = "{locationId:guid}";
        public const string UploadLocationAudio = "locations/upload-audio";
        public const string UpdateLocationDisplayConfig = "{locationId:guid}/display-config";
        public const string UpdateLocationInteractionConfig = "{locationId:guid}/interaction-config";
        public const string MoveLocationToSegment = "{mapId:guid}/segments/{fromSegmentId:guid}/locations/{locationId:guid}/move-to/{toSegmentId:guid}";
        public const string GetZoneLocations = "zones/{zoneId:guid}/locations";
        public const string GetSegmentLocationsWithoutZone = "segments/{segmentId:guid}/locations/without-zone";
        
    }

    public static class AnimationEndpoints
    {
        public const string GetByLayer = "layers/{layerId:guid}";
        public const string Create = "";
        public const string GetById = "{animationId:guid}";
        public const string Update = "{animationId:guid}";
        public const string Delete = "{animationId:guid}";
        public const string GetActive = "active";
    }

    public static class OsmEndpoints
    {
        public const string Search = "search";
        public const string ReverseGeocode = "reverse";
        public const string Geocode = "geocode";
        public const string ElementDetail = "elements/{osmType}/{osmId:long}";
    }

    public static class WorkspaceEndpoints
    {
        public const string GetAll = "";
        public const string GetById = "{id:guid}";
        public const string Create = "";
        public const string Update = "{id:guid}";
        public const string Delete = "{id:guid}";
        public const string GetByOrganization = "organization/{orgId:guid}";
        public const string GetMyWorkspaces = "my-workspaces";
        public const string GetWorkspaceMaps = "{workspaceId:guid}/maps";
        public const string AddMapToWorkspace = "{workspaceId:guid}/maps";
        public const string RemoveMapFromWorkspace = "{workspaceId:guid}/maps/{mapId:guid}";
    }

    public static class UserEndpoints
    {
        public const string GetMe = "me";
        public const string GetMyMembership = "me/membership/{orgId:guid}";
        public const string UpdatePersonalInfo = "me/personal-info";
    }

    public static class LayerEndpoints
    {
        public const string GetAvailable = "available";
        public const string GetById = "{layerId:guid}";
        public const string GetByMap = "map/{mapId:guid}";
        public const string Create = "";
        public const string Update = "{layerId:guid}";
        public const string Delete = "{layerId:guid}";
    }
}
