namespace CusomMapOSM_Infrastructure.Databases;

public static class SeedDataConstants
{
    // User Roles
    public static readonly Guid StaffRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid RegisteredUserRoleId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    public static readonly Guid AdminRoleId = Guid.Parse("00000000-0000-0000-0000-000000000003");

    // Account Statuses
    public static readonly Guid ActiveStatusId = Guid.Parse("00000000-0000-0000-0000-000000000004");
    public static readonly Guid InactiveStatusId = Guid.Parse("00000000-0000-0000-0000-000000000005");
    public static readonly Guid SuspendedStatusId = Guid.Parse("00000000-0000-0000-0000-000000000006");
    public static readonly Guid PendingVerificationStatusId = Guid.Parse("00000000-0000-0000-0000-000000000007");

    // Annotation Types
    public static readonly Guid MarkerTypeId = Guid.Parse("00000000-0000-0000-0000-000000000008");
    public static readonly Guid LineTypeId = Guid.Parse("00000000-0000-0000-0000-000000000009");
    public static readonly Guid PolygonTypeId = Guid.Parse("00000000-0000-0000-0000-000000000010");
    public static readonly Guid CircleTypeId = Guid.Parse("00000000-0000-0000-0000-000000000011");
    public static readonly Guid RectangleTypeId = Guid.Parse("00000000-0000-0000-0000-000000000012");
    public static readonly Guid TextLabelTypeId = Guid.Parse("00000000-0000-0000-0000-000000000013");

    // Collaboration Permissions
    public static readonly Guid ViewPermissionId = Guid.Parse("00000000-0000-0000-0000-000000000014");
    public static readonly Guid EditPermissionId = Guid.Parse("00000000-0000-0000-0000-000000000015");
    public static readonly Guid ManagePermissionId = Guid.Parse("00000000-0000-0000-0000-000000000016");

    // Collaboration Target Types
    public static readonly Guid MapTargetTypeId = Guid.Parse("00000000-0000-0000-0000-000000000017");
    public static readonly Guid LayerTargetTypeId = Guid.Parse("00000000-0000-0000-0000-000000000018");
    public static readonly Guid OrganizationTargetTypeId = Guid.Parse("00000000-0000-0000-0000-000000000019");

    // Export Types
    public static readonly Guid PdfExportTypeId = Guid.Parse("00000000-0000-0000-0000-000000000020");
    public static readonly Guid PngExportTypeId = Guid.Parse("00000000-0000-0000-0000-000000000021");
    public static readonly Guid SvgExportTypeId = Guid.Parse("00000000-0000-0000-0000-000000000022");
    public static readonly Guid GeoJsonExportTypeId = Guid.Parse("00000000-0000-0000-0000-000000000023");
    public static readonly Guid MbtilesExportTypeId = Guid.Parse("00000000-0000-0000-0000-000000000024");

    // Layer Sources
    public static readonly Guid OpenStreetMapSourceTypeId = Guid.Parse("00000000-0000-0000-0000-000000000025");
    public static readonly Guid UserUploadSourceTypeId = Guid.Parse("00000000-0000-0000-0000-000000000026");
    public static readonly Guid ExternalApiSourceTypeId = Guid.Parse("00000000-0000-0000-0000-000000000027");
    public static readonly Guid DatabaseSourceTypeId = Guid.Parse("00000000-0000-0000-0000-000000000028");
    public static readonly Guid WebServiceSourceTypeId = Guid.Parse("00000000-0000-0000-0000-000000000029");

    // Membership Statuses
    public static readonly Guid ActiveMembershipStatusId = Guid.Parse("00000000-0000-0000-0000-000000000030");
    public static readonly Guid ExpiredMembershipStatusId = Guid.Parse("00000000-0000-0000-0000-000000000031");
    public static readonly Guid SuspendedMembershipStatusId = Guid.Parse("00000000-0000-0000-0000-000000000032");
    public static readonly Guid PendingPaymentMembershipStatusId = Guid.Parse("00000000-0000-0000-0000-000000000033");
    public static readonly Guid CancelledMembershipStatusId = Guid.Parse("00000000-0000-0000-0000-000000000034");

    // Organization Location Statuses
    public static readonly Guid ActiveOrganizationLocationStatusId = Guid.Parse("00000000-0000-0000-0000-000000000035");
    public static readonly Guid InactiveOrganizationLocationStatusId = Guid.Parse("00000000-0000-0000-0000-000000000036");
    public static readonly Guid UnderConstructionOrganizationLocationStatusId = Guid.Parse("00000000-0000-0000-0000-000000000037");
    public static readonly Guid TemporaryClosedOrganizationLocationStatusId = Guid.Parse("00000000-0000-0000-0000-000000000038");

    // Organization Member Types
    public static readonly Guid OwnerOrganizationMemberTypeId = Guid.Parse("00000000-0000-0000-0000-000000000039");
    public static readonly Guid AdminOrganizationMemberTypeId = Guid.Parse("00000000-0000-0000-0000-000000000040");
    public static readonly Guid MemberOrganizationMemberTypeId = Guid.Parse("00000000-0000-0000-0000-000000000041");
    public static readonly Guid ViewerOrganizationMemberTypeId = Guid.Parse("00000000-0000-0000-0000-000000000042");

    // Ticket Statuses
    public static readonly Guid OpenTicketStatusId = Guid.Parse("00000000-0000-0000-0000-000000000043");
    public static readonly Guid InProgressTicketStatusId = Guid.Parse("00000000-0000-0000-0000-000000000044");
    public static readonly Guid WaitingForCustomerTicketStatusId = Guid.Parse("00000000-0000-0000-0000-000000000045");
    public static readonly Guid ResolvedTicketStatusId = Guid.Parse("00000000-0000-0000-0000-000000000046");
    public static readonly Guid ClosedTicketStatusId = Guid.Parse("00000000-0000-0000-0000-000000000047");

    // Payment Gateways
    public static readonly Guid VnPayPaymentGatewayId = Guid.Parse("00000000-0000-0000-0000-000000000048");
    public static readonly Guid PayPalPaymentGatewayId = Guid.Parse("00000000-0000-0000-0000-000000000049");
    public static readonly Guid StripePaymentGatewayId = Guid.Parse("00000000-0000-0000-0000-000000000050");
    public static readonly Guid BankTransferPaymentGatewayId = Guid.Parse("00000000-0000-0000-0000-000000000051");
}