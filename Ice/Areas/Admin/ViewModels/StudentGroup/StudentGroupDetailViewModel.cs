using System.Collections.Immutable;
using Ice.Db.Models;

namespace Ice.Areas.Admin.ViewModels.StudentGroup;

public class StudentGroupDetailViewModel
{
    public required long Id { get; init; }

    public required string GroupName { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }

    public required ImmutableList<AssignmentProgressViewModel> AssignmentProgress { get; init; }

    public required ImmutableList<TicketViewModel> Tickets { get; init; }
}

public class AssignmentProgressViewModel
{
    public required long AssignmentId { get; init; }

    public required string AssignmentName { get; init; }

    public required string Status { get; init; }
}

public class TicketViewModel
{
    public required long Id { get; init; }

    public required string Title { get; init; }

    public required string Status { get; init; }
    
    public required AdminUsers? AssignedTo { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
}
