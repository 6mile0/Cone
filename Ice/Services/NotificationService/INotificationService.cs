using Ice.Areas.Admin.Dtos.Res;

namespace Ice.Services.NotificationService;

public interface INotificationService
{
    Task NotifyTicketCreatedAsync(long ticketId, string title, string studentGroupName, string assignedStaffName);
    Task NotifyStaffStatusAsync(StaffStatusResDto staffStatus);
}