namespace CusomMapOSM_API.Constants;

public static class Routes
{
    public const string ApiBase = "api/v1";
    public const string Health = "health";
    public static class Prefix
    {
        public const string Auth = "auth";
        public const string Organization = "organizations";
        public const string PointOfInterest = "points-of-interest";
        public const string StoryMap = "story-map";
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
        public const string UpdateSegmentZone = "{mapId:guid}/segments/{segmentId:guid}/zones/{zoneId:guid}";
        public const string DeleteSegmentZone = "{mapId:guid}/segments/{segmentId:guid}/zones/{zoneId:guid}";
        
        // Segment layers
        public const string GetSegmentLayers = "{mapId:guid}/segments/{segmentId:guid}/layers";
        public const string CreateSegmentLayer = "{mapId:guid}/segments/{segmentId:guid}/layers";
        public const string UpdateSegmentLayer = "{mapId:guid}/segments/{segmentId:guid}/layers/{layerId:guid}";
        public const string DeleteSegmentLayer = "{mapId:guid}/segments/{segmentId:guid}/layers/{layerId:guid}";
        
        // Timeline
        public const string GetTimeline = "{mapId:guid}/timeline";
        public const string CreateTimelineStep = "{mapId:guid}/timeline";
        public const string UpdateTimelineStep = "{mapId:guid}/timeline/{stepId:guid}";
        public const string DeleteTimelineStep = "{mapId:guid}/timeline/{stepId:guid}";
    }

    public static class PoiEndpoints
    {
        // Map-level POIs
        public const string GetMapPois = "{mapId:guid}";
        public const string CreateMapPoi = "{mapId:guid}";
        
        // Segment-level POIs
        public const string GetSegmentPois = "{mapId:guid}/segments/{segmentId:guid}";
        public const string CreateSegmentPoi = "{mapId:guid}/segments/{segmentId:guid}";
        
        // POI management
        public const string UpdatePoi = "{poiId:guid}";
        public const string DeletePoi = "{poiId:guid}";
    }
}
