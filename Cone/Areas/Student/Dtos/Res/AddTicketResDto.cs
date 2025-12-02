using Cone.Db.Models;

namespace Cone.Areas.Student.Dtos.Res;

public class AddTicketResDto
{
    public required Tickets? Ticket { get; init; }
    public required AdminUsers Admin { get; init; }
}