namespace Ice.Areas.Admin.ViewModels.StudentGroup;

public class StudentGroupViewModel
{
    public required long Id { get; init; }
    
    public required string GroupName { get; init; }
    
    public required DateTimeOffset CreatedAt { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }

    public int TicketCount { get; init; }
}