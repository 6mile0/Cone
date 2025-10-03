using Ice.Db.Models;

namespace Ice.Areas.Student.Dtos.Res;

public class AddTicketResDto
{
    public required Tickets? Ticket { get; init; }
    public required AdminUsers Admin { get; init; }
}