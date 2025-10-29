namespace CusomMapOSM_Domain.Entities.Workspaces.ErrorMessages;

public class WorkspaceErrors
{
    public const string WorkspaceNotFound = "Workspace not found";
    public const string WorkspaceAlreadyExists = "Workspace already exists";
    public const string WorkspaceNotValid = "Workspace is not valid";
    public const string WorkspaceCreationFailed = "Failed to create the workspace due to an internal error";
    public const string WorkspaceUpdateFailed = "Failed to update the workspace due to an internal error";
    public const string WorkspaceDeletionFailed = "Failed to delete the workspace due to an internal error";
    public const string InvalidWorkspaceId = "The provided workspace ID is invalid";
    public const string WorkspaceNameEmpty = "Workspace name cannot be empty";
    public const string UnauthorizedAccess = "Unauthorized access to the workspace";
    public const string WorkspaceLimitExceeded = "Workspace limit exceeded for this organization";
}
