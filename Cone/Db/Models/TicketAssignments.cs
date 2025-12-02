using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Cone.Db.Models;

[Index(nameof(TicketId), nameof(AssignmentId), nameof(StudentGroupId), IsUnique = true)]
public class TicketAssignments
{
    [Key]
    public long Id { get; set; }

    [Required]
    public required long TicketId { get; set; }

    [Required]
    public required long AssignmentId { get; set; }

    [Required]
    public required long StudentGroupId { get; set; }

    [Required]
    public required DateTimeOffset CreatedAt { get; set; }

    [ConcurrencyCheck]
    public required DateTimeOffset UpdatedAt { get; set; }

    // Navigation properties
    public Tickets? Ticket { get; set; }
    public Assignments? Assignment { get; set; }
    public StudentGroups? StudentGroup { get; set; }
}