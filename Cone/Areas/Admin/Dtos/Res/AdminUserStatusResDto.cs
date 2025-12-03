namespace Cone.Areas.Admin.Dtos.Res;

public class AdminUserStatusResDto
{
    public long Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public bool IsWorking { get; init; }
    public IReadOnlyList<CurrentTicketDto> CurrentTickets { get; init; } = [];
}

public class CurrentTicketDto
{
    public long Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string StudentGroupName { get; init; } = string.Empty;
}