using System.ComponentModel.DataAnnotations;

namespace Cone.Db.Models;

public class Assignments
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    public required string Name { get; set; }
    
    [Required]
    public required string Description { get; set; }
    
    [Required]
    public required int SortOrder { get; set; }
    
    [Required]
    public required DateTimeOffset CreatedAt { get; set; }
    
    [ConcurrencyCheck]
    public required DateTimeOffset UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<StudentGroupAssignmentsProgress>? StudentGroupAssignmentsProgress { get; set; }
    public ICollection<TicketAssignments>? TicketAssignments { get; set; }
}