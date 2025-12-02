using System.ComponentModel.DataAnnotations;
using Cone.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cone.Db.Models;

[Index(nameof(StudentGroupId), nameof(AssignmentId), IsUnique = true)]
public class StudentGroupAssignmentsProgress
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    public required long StudentGroupId { get; set; }
    
    [Required]
    public required long AssignmentId { get; set; }

    [Required] public required AssignmentProgress Status { get; set; } = AssignmentProgress.NotStarted;
    
    [Required]
    public required DateTimeOffset CreatedAt { get; set; }
    
    [ConcurrencyCheck]
    public required DateTimeOffset UpdatedAt { get; set; }
    
    // Navigation properties
    public StudentGroups? StudentGroup { get; set; }
    public Assignments? Assignment { get; set; }
}