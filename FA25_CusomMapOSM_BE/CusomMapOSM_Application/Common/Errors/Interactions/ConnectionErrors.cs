namespace CusomMapOSM_Application.Common.Errors.Interactions;

/// <summary>
/// Error codes and messages for connection operations
/// </summary>
public static class ConnectionErrors
{
    public static Error NotFound(Guid connectionId) => Error.NotFound(
        "Connection.NotFound",
        $"Connection with ID '{connectionId}' was not found");

    public static Error FeatureNotFound(Guid featureId) => Error.NotFound(
        "Connection.FeatureNotFound",
        $"Feature with ID '{featureId}' was not found");

    public static Error MapNotFound(Guid mapId) => Error.NotFound(
        "Connection.MapNotFound",
        $"Map with ID '{mapId}' was not found");

    public static Error QuotaExceeded(int currentCount, int maxAllowed) => Error.Conflict(
        "Connection.QuotaExceeded",
        $"Cannot create more connections. Current: {currentCount}, Maximum allowed: {maxAllowed} per map for your membership plan");

    public static Error SelfConnection(Guid featureId) => Error.ValidationError(
        "Connection.SelfConnection",
        $"Cannot create a connection from a feature to itself (Feature ID: {featureId})");

    public static Error DuplicateConnection(Guid sourceId, Guid targetId) => Error.Conflict(
        "Connection.DuplicateConnection",
        $"A connection already exists between feature '{sourceId}' and '{targetId}'");

    public static Error FeaturesMustBeOnSameMap(Guid sourceMapId, Guid targetMapId) => Error.ValidationError(
        "Connection.FeaturesMustBeOnSameMap",
        $"Cannot connect features from different maps. Source map: '{sourceMapId}', Target map: '{targetMapId}'");

    public static Error InvalidPathPoints(string reason) => Error.ValidationError(
        "Connection.InvalidPathPoints",
        $"Invalid path points configuration: {reason}");

    public static Error InvalidStyleConfig(string reason) => Error.ValidationError(
        "Connection.InvalidStyleConfig",
        $"Invalid style configuration: {reason}");

    public static Error Unauthorized(Guid userId, Guid connectionId) => Error.Unauthorized(
        "Connection.Unauthorized",
        $"User '{userId}' is not authorized to modify connection '{connectionId}'");

    public static Error BulkDeletePartialFailure(int successCount, int failureCount) => Error.Problem(
        "Connection.BulkDeletePartialFailure",
        $"Bulk delete partially completed: {successCount} succeeded, {failureCount} failed");

    public static Error AnimationNotSupported(string planName) => Error.Forbidden(
        "Connection.AnimationNotSupported",
        $"Animated connections are not supported by your '{planName}' membership plan. Upgrade to access this feature");

    public static Error InvalidConnectionType(string connectionType) => Error.ValidationError(
        "Connection.InvalidConnectionType",
        $"Invalid connection type: '{connectionType}'. Valid types are: Line, Arrow, Dashed, Animated, Curved");

    public static Error PathTooComplex(int pointCount, int maxAllowed) => Error.ValidationError(
        "Connection.PathTooComplex",
        $"Path has too many waypoints ({pointCount}). Maximum allowed: {maxAllowed}");

    public static Error InvalidCoordinates(double lat, double lng) => Error.ValidationError(
        "Connection.InvalidCoordinates",
        $"Invalid coordinates: latitude={lat}, longitude={lng}. Lat must be between -90 and 90, lng between -180 and 180");

    public static Error NoConnectionsSelected() => Error.ValidationError(
        "Connection.NoConnectionsSelected",
        "No connections were selected for the bulk operation");

    public static Error CascadeDeleteFailed(Guid mapId, string reason) => Error.Problem(
        "Connection.CascadeDeleteFailed",
        $"Failed to delete connections for map '{mapId}': {reason}");
}
