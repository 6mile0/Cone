using Ice.Areas.Admin.Dtos.Req;
using Ice.Areas.Student.Dtos.Req;
using Ice.Areas.Student.Dtos.Res;
using Ice.Db.Models;

namespace Ice.Services.TicketService;

public interface ITicketService
{
    /// <summary>
    /// Gets all ticket,
    /// </summary>
    Task<IReadOnlyList<Tickets?>> GetAllTicketsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Get a ticket by its ID.
    /// </summary>
    Task<Tickets?> GetTicketByIdAsync(long ticketId, CancellationToken cancellationToken);

    /// <summary>
    /// Get tickets by student group ID.
    /// </summary>
    Task<IReadOnlyList<Tickets>> GetTicketsByStudentGroupIdAsync(long studentGroupId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Create a new ticket.
    /// </summary>
    Task<AddTicketResDto> CreateTicketAsync(AddTicketDto addTicketDto, CancellationToken cancellationToken);
    
    /// <summary>
    /// Update an existing ticket.
    /// </summary>
    Task<Tickets> UpdateTicketAsync(UpdateTicketReqDto req, CancellationToken cancellationToken);
    
    /// <summary>
    /// Delete a ticket by its ID.
    /// </summary>
    Task DeleteTicketAsync(long ticketId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Check if a student group can add a new ticket.
    /// </summary>
    Task<Tickets?> IsAbleAddTicketAsync(long studentGroupId, CancellationToken cancellationToken);
}