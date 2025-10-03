using System.ComponentModel.DataAnnotations;
using Ice.Enums;
using Microsoft.EntityFrameworkCore;

namespace Ice.Db.Models;

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
    
    // Navigation properties
    public StudentGroups? StudentGroup { get; set; }
    public Assignments? Assignment { get; set; }
}