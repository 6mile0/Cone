using System.ComponentModel.DataAnnotations;

namespace Ice.Db.Models;

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
}