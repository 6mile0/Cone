using System.ComponentModel.DataAnnotations;
using Cone.Enums;

namespace Cone.Db.Models;

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
    public required TicketStatus Status { get; set; } = TicketStatus.InProgress;
    
    [MaxLength(2000)]
    public string? Remark { get; set; } = string.Empty;
    
    [Required]
    public required DateTimeOffset CreatedAt { get; set; }
    
    [ConcurrencyCheck]
    public required DateTimeOffset UpdatedAt { get; set; }
    
    // Navigation property
    public StudentGroups StudentGroup { get; set; }　= null!;
    public TicketAdminUsers? TicketAdminUser { get; set; }
}