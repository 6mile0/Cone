using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Ice.Db.Models;

[Index(nameof(TicketId), nameof(AdminUserId), IsUnique = true)]
public class TicketAdminUsers
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    public required long TicketId { get; set; }
    
    [Required]
    public required long AdminUserId { get; set; }
    
    [Required]
    public required DateTimeOffset CreatedAt { get; set; }
    
    [ConcurrencyCheck]
    public required DateTimeOffset UpdatedAt { get; set; }
    
    // Navigation properties
    public Tickets Ticket { get; set; } = null!;
    public AdminUsers AdminUser { get; set; } = null!;
}