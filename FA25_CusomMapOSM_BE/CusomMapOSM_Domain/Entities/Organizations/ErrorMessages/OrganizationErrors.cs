using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CusomMapOSM_Domain.Entities.Organizations.ErrorMessages;

public class OrganizationErrors
{
    public const string OrganizationNotFound = "Organization not found.";
    public const string OrganizationAlreadyExists = "Organization already exists.";
    public const string InvalidOrganizationName = "Invalid organization name.";
    public const string InvalidOrganizationId = "Invalid organization ID.";
    public const string UnauthorizedAccess = "Unauthorized access to the organization.";
    public const string OrganizationLimitExceeded = "Organization limit exceeded.";
    public const string InvalidOrganizationType = "Invalid organization type.";
}

public class OrganizationLocationErrors
{
    public const string InvalidLatitude = "Invalid latitude value.";
    public const string InvalidLongitude = "Invalid longitude value.";
    public const string InvalidLocationFormat = "Invalid location format. Expected format: 'latitude,longitude'.";
    public const string LocationNotFound = "Location not found.";
}

public class OrganizationMemberErrors
{
    public const string MemberNotFound = "Member not found.";
    public const string InvalidMemberRole = "Invalid member role.";
    public const string MemberAlreadyExists = "Member already exists in the organization.";
}

public class OrganizationMemberTypeErrors
{
    public const string InvalidMemberType = "Invalid member type.";
    public const string MemberTypeNotFound = "Member type not found.";
    public const string MemberTypeAlreadyExists = "Member type already exists in the organization.";
}
