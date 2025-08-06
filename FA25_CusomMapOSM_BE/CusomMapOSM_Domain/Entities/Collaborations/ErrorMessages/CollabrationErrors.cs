using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Domain.Entities.Collabrations.ErrorMessages;

public class CollabrationErrors
{
    public const string CollaborationNotFound = "Collaboration not found";
    public const string CollaborationAlreadyExists = "Collaboration already exists";
    public const string CollaborationNotValid = "Collaboration is not valid";
}

public class CollabrationTargetTypeErrors
{
    public const string CollaborationTargetTypeNotFound = "Collaboration target type not found";
    public const string CollaborationTargetTypeAlreadyExists = "Collaboration target type already exists";
    public const string CollaborationTargetTypeNotValid = "Collaboration target type is not valid";
}

public class CollabrationPermissionErrors
{
    public const string CollaborationPermissionNotFound = "Collaboration permission not found";
    public const string CollaborationPermissionAlreadyExists = "Collaboration permission already exists";
    public const string CollaborationPermissionNotValid = "Collaboration permission is not valid";
}