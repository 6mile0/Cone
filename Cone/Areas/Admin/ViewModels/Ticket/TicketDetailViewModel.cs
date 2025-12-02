using Cone.Areas.Admin.ViewModels.AdminUser;

namespace Cone.Areas.Admin.ViewModels.Ticket;

public class TicketDetailViewModel
{
    public required long Id { get; init; }

    public required string Title { get; init; }

    public required string Status { get; init; }

    public required string? Remark { get; init; }

    public required long StudentGroupId { get; init; }

    public required string StudentGroupName { get; init; }

    public required AdminUserViewModel? AssignedTo { get; init; }

    public required List<AdminUserViewModel> AdminUsers { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }
}