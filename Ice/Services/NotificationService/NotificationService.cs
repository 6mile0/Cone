using System.Text.Json;
using Ice.Areas.Admin.Dtos.Res;
using Lib.AspNetCore.ServerSentEvents;

namespace Ice.Services.NotificationService;

public class NotificationService(IServerSentEventsService serverSentEventsService) : INotificationService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task NotifyTicketCreatedAsync(
        long ticketId,
        string title,
        string studentGroupName,
        string assignedStaffName)
    {
        var message = JsonSerializer.Serialize(new
        {
            type = "ticket-created",
            ticketId,
            title,
            studentGroupName,
            assignedStaffName,
            timestamp = DateTime.UtcNow
        }, SerializerOptions);

        await serverSentEventsService.SendEventAsync(message);
    }

    public async Task NotifyStaffStatusAsync(StaffStatusResDto staffStatus)
    {
        var message = JsonSerializer.Serialize(new
        {
            type = "staff-status",
            data = staffStatus
        }, SerializerOptions);

        await serverSentEventsService.SendEventAsync(message);
    }
}
