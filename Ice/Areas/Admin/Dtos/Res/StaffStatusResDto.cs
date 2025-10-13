namespace Ice.Areas.Admin.Dtos.Res;

public class StaffStatusResDto
{
    public IReadOnlyList<AdminUserStatusResDto> AdminUserStatuses { get; init; } = [];
    public int TotalTicketCount { get; init; }
    public int UnassignedTicketCount { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}