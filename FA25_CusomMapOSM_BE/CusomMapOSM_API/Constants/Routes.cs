namespace CusomMapOSM_API.Constants;

public static class Routes
{
    public const string ApiBase = "api/v1";
    public const string Health = "health";
    public static class Prefix
    {
        public const string Auth = "auth";
        public const string Organization = "organizations";
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
        public const string Create = "";
        public const string GetById = "{id}";
        public const string Update = "{id}";
        public const string Delete = "{id}";
        public const string GetAll = "";
        public const string InviteMember = "invite-member";
        public const string AcceptInvite = "accept-invite";
        public const string GetMyInvitations = "my-invitations";
        public const string GetOrganizationMembers = "/{orgId:guid}/members";
        public const string UpdateMemberRole = "/members/role";
        public const string RemoveMember = "/members/remove";
        public const string RejectInvite = "/invites/reject";
        public const string CancelInvite = "/invites/cancel";
        public const string GetMyOrganizations = "/mine";
        public const string TransferOwnership = "/ownership/transfer";

    }
}