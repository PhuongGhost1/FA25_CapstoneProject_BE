namespace CusomMapOSM_Application.Interfaces.Services.User;

public interface ICurrentUserService
{
    Guid? GetUserId();
    string? GetEmail();
}



