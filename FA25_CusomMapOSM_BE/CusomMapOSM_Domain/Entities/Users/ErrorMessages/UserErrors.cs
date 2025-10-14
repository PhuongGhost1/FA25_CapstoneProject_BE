using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Domain.Entities.Users.ErrorMessages;

public class UserErrors
{
    public const string NotFound = "User not found";
    public const string EmailExists = "Email address already registered";
    public const string InvalidCredentials = "Invalid credentials";
    public const string AccountLocked = "Account is locked";
    public const string RoleRequired = "User role is required";
    public const string StatusRequired = "Account status is required";
}


public class UserFavoriteTemplateErrors
{
    public const string NotFound = "User favorite template not found";
    public const string TemplateNotFound = "Template not found";
    public const string TemplateAlreadyExists = "Template already exists";
    public const string TemplateNotValid = "Template is not valid";
}

public class UserPreferenceErrors
{
    public const string NotFound = "User preference not found";
    public const string PreferenceNotFound = "Preference not found";
    public const string PreferenceAlreadyExists = "Preference already exists";
    public const string PreferenceNotValid = "Preference is not valid";
}

public class UserRoleErrors
{
    public const string NotFound = "User role not found";
    public const string DuplicateName = "Role name already exists";
}

public class AccountStatusErrors
{
    public const string NotFound = "Account status not found";
}