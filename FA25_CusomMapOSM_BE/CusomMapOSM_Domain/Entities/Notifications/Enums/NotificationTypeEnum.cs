namespace CusomMapOSM_Domain.Entities.Notifications.Enums;

public enum NotificationTypeEnum
{
    // Transaction notifications
    TransactionCompleted = 1,
    TransactionFailed = 2,
    PaymentProcessed = 3,
    TransactionPending = 4,

    // Membership notifications
    MembershipCreated = 10,
    MembershipExpired = 11,
    MembershipExpirationWarning = 12,
    MembershipPlanChanged = 13,

    // Quota notifications
    QuotaWarning = 20,
    QuotaExceeded = 21,
    QuotaReset = 22,

    // Export notifications
    ExportCompleted = 30,
    ExportFailed = 31,

    // User notifications
    Welcome = 40,
    ProfileUpdated = 41,

    // Organization notifications
    OrganizationInvitation = 50,
    OrganizationJoined = 51,
    OrganizationLeft = 52,

    // System notifications
    SystemMaintenance = 60,
    SystemUpdate = 61,
    SecurityAlert = 62
}
