using System.ComponentModel.DataAnnotations;
using Ice.Enums;

namespace Ice.Db.Models;

public class Tickets
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    public required long StudentGroupId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }
    
    [Required]
    public required TicketStatus Status { get; set; } = TicketStatus.Open;
    
    [Required]
    public required DateTimeOffset CreatedAt { get; set; }
    
    [ConcurrencyCheck]
    public required DateTimeOffset UpdatedAt { get; set; }
}