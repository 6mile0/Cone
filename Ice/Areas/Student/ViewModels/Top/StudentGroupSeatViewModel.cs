namespace Ice.Areas.Student.ViewModels.Top;

public class StudentGroupSeatViewModel
{
    public long Id { get; set; }
    public required string GroupName { get; set; }
    public int TicketCount { get; set; }
}