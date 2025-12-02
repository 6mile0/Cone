using System.Collections.Immutable;
using Cone.Areas.Admin.ViewModels.Assignment;
using Cone.Areas.Admin.ViewModels.Ticket;

namespace Cone.Areas.Admin.ViewModels.StudentGroup;

public class StudentGroupDetailViewModel
{
    public required long Id { get; init; }

    public required string GroupName { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }

    public required ImmutableList<AssignmentProgressViewModel> AssignmentProgress { get; init; }

    public required ImmutableList<TicketViewModel> Tickets { get; init; }
}
