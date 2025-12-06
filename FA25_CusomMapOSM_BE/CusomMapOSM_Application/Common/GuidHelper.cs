namespace CusomMapOSM_Application.Common;

public static class GuidHelper
{
    public static Guid? ParseNullableGuid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        return Guid.TryParse(value, out var guid) ? guid : null;
    }
}