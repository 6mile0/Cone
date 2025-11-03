using Ice.Areas.Admin.Dtos.Res;
using Ice.Enums;
using Ice.Services.AdminUserService;
using Ice.Services.TicketService;

namespace Ice.Services.NotificationService;

public class StaffStatusBroadcastService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<StaffStatusBroadcastService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Staff Status Broadcast Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await BroadcastStaffStatusAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when service is stopping
                break;
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "Error broadcasting staff status");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        logger.LogInformation("Staff Status Broadcast Service stopped");
    }

    private async Task BroadcastStaffStatusAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var adminUserService = scope.ServiceProvider.GetRequiredService<IAdminUserService>();
        var ticketService = scope.ServiceProvider.GetRequiredService<ITicketService>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var adminUsers = await adminUserService.GetAllAdminUsersAsync(cancellationToken);
        var tickets = await ticketService.GetAllTicketsAsync(cancellationToken);

        var adminUserStatuses = adminUsers.Select(adminUser =>
        {
            var assignedTickets = tickets.Where(t => t.TicketAdminUser?.AdminUserId == adminUser.Id).ToList();

            // Get all in-progress tickets (excluding Pending)
            var currentTickets = assignedTickets
                .Where(t => t.Status == TicketStatus.InProgress)
                .OrderBy(t => t.CreatedAt)
                .ToList();
            
            var currentTicketDtos = currentTickets.Select(t => new CurrentTicketDto
            {
                Id = t.Id,
                Title = t.Title
            }).ToList();

            return new AdminUserStatusResDto
            {
                Id = adminUser.Id,
                FullName = adminUser.FullName,
                IsWorking = currentTickets.Count > 0,
                CurrentTickets = currentTicketDtos
            };
        }).ToList();

        var staffStatus = new StaffStatusResDto
        {
            AdminUserStatuses = adminUserStatuses,
            TotalTicketCount = tickets.Count,
            UnassignedTicketCount = tickets.Count(t => t.TicketAdminUser == null),
            Timestamp = DateTime.UtcNow
        };

        await notificationService.NotifyStaffStatusAsync(staffStatus, cancellationToken);
    }
}