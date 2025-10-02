using System.ComponentModel.DataAnnotations;

namespace Ice.Db.Models;

public class StudentGroups
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public required string GroupName { get; set; }
    
    [Required]
    public required DateTimeOffset CreatedAt { get; set; }
    
    [ConcurrencyCheck]
    public required DateTimeOffset UpdatedAt { get; set; }
}