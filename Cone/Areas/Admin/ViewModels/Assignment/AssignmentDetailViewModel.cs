using Cone.Areas.Admin.ViewModels.StudentGroup;

namespace Cone.Areas.Admin.ViewModels.Assignment;

public class AssignmentDetailViewModel
{
    public long Id { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required int SortOrder { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }

    public List<StudentGroupViewModel> AssignedStudentGroups { get; init; } = [];

    public List<StudentGroupViewModel> UnassignedStudentGroups { get; init; } = [];

    public List<StudentGroupViewModel> NotStartedGroups { get; init; } = [];

    public List<StudentGroupViewModel> InProgressGroups { get; init; } = [];

    public List<StudentGroupViewModel> CompletedGroups { get; init; } = [];
}

public class StudentGroupWithStatusViewModel
{
    public required long Id { get; init; }

    public required string GroupName { get; init; }

    public required string Status { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }
}