namespace Modules.Identity.Contracts.Notifications;

public interface IHubContextWrapper
{
    Task SendToGroupAsync(string groupName, string method, object? arg = null);
}
