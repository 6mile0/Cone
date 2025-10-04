namespace Ice.Areas.Admin.ViewModels.Assignment;

public class AssignmentDetailViewModel
{
    public long Id { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required int SortOrder { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }
}