namespace CusomMapOSM_Application.Common.Errors.Interactions;

/// <summary>
/// Error codes and messages for interaction operations
/// </summary>
public static class InteractionErrors
{
    public static Error NotFound(Guid interactionId) => Error.NotFound(
        "Interaction.NotFound",
        $"Interaction with ID '{interactionId}' was not found or has been deleted");

    public static Error FeatureNotFound(Guid featureId) => Error.NotFound(
        "Interaction.FeatureNotFound",
        $"Feature with ID '{featureId}' was not found");

    public static Error MapNotFound(Guid mapId) => Error.NotFound(
        "Interaction.MapNotFound",
        $"Map with ID '{mapId}' was not found");

    public static Error FileSizeExceeded(long maxSizeBytes, long actualSizeBytes) => Error.ValidationError(
        "Interaction.FileSizeExceeded",
        $"File size ({actualSizeBytes / 1024 / 1024}MB) exceeds the maximum allowed size ({maxSizeBytes / 1024 / 1024}MB) for your membership plan");

    public static Error QuotaExceeded(int currentCount, int maxAllowed) => Error.Conflict(
        "Interaction.QuotaExceeded",
        $"Cannot create more interactions. Current: {currentCount}, Maximum allowed: {maxAllowed} per map for your membership plan");

    public static Error UnsupportedContentType(string contentType, string planName) => Error.Forbidden(
        "Interaction.UnsupportedContentType",
        $"Content type '{contentType}' is not supported by your '{planName}' membership plan. Upgrade to access this feature");

    public static Error UnsupportedFileType(string fileExtension, string[] allowedTypes) => Error.ValidationError(
        "Interaction.UnsupportedFileType",
        $"File type '{fileExtension}' is not supported. Allowed types: {string.Join(", ", allowedTypes)}");

    public static Error FileUploadFailed(string fileName, string reason) => Error.Problem(
        "Interaction.FileUploadFailed",
        $"Failed to upload file '{fileName}': {reason}");

    public static Error ContentStorageFailed(string reason) => Error.Problem(
        "Interaction.ContentStorageFailed",
        $"Failed to store interaction content: {reason}");

    public static Error Unauthorized(Guid userId, Guid interactionId) => Error.Unauthorized(
        "Interaction.Unauthorized",
        $"User '{userId}' is not authorized to modify interaction '{interactionId}'");

    public static Error NoFileProvided() => Error.ValidationError(
        "Interaction.NoFileProvided",
        "A media file is required for this interaction type");

    public static Error InvalidActionType(string interactionType, string actionType) => Error.ValidationError(
        "Interaction.InvalidActionType",
        $"Action type '{actionType}' is not compatible with interaction type '{interactionType}'");

    public static Error ContentNotFound(Guid interactionId) => Error.NotFound(
        "Interaction.ContentNotFound",
        $"Content for interaction '{interactionId}' was not found in MongoDB");

    public static Error InvalidDisplayOrder(int displayOrder, int maxAllowed) => Error.ValidationError(
        "Interaction.InvalidDisplayOrder",
        $"Display order {displayOrder} exceeds maximum allowed value of {maxAllowed}");

    public static Error MongoConnectionFailed(string reason) => Error.Problem(
        "Interaction.MongoConnectionFailed",
        $"Failed to connect to MongoDB: {reason}");

    public static Error FirebaseStorageConnectionFailed(string reason) => Error.Problem(
        "Interaction.FirebaseStorageConnectionFailed",
        $"Failed to connect to Firebase Storage: {reason}");

    public static Error InactiveInteraction(Guid interactionId) => Error.Forbidden(
        "Interaction.InactiveInteraction",
        $"Interaction '{interactionId}' is currently inactive and cannot be accessed");
}
