using Cone.Areas.Admin.Dtos.Res;

namespace Cone.Services.NotificationService;

public interface INotificationService
{
    Task NotifyTicketCreatedAsync(long ticketId, string title, string studentGroupName, string assignedStaffName);
    Task NotifyStaffStatusAsync(StaffStatusResDto staffStatus, CancellationToken cancellationToken);
}