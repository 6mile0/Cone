using Ice.Enums;

namespace Ice.Areas.Admin.ViewModels.Assignment;

public class AssignmentProgressViewModel
{
    public required long AssignmentId { get; init; }

    public required string AssignmentName { get; init; }

    public required string Status { get; init; }

    public required AssignmentProgress StatusEnum { get; init; }
}
