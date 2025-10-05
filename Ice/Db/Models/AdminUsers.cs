using System.ComponentModel.DataAnnotations;
using Ice.Enums;

namespace Ice.Db.Models;

public class AdminUsers
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public required string FullName { get; set; }
    
    [Required]
    [EnumDataType(typeof(TutorTypes))]
    public required TutorTypes TutorType { get; set; }
    
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public required string Email { get; set; }
    
    [Required]
    public required DateTimeOffset CreatedAt { get; set; }
    
    [ConcurrencyCheck]
    public required DateTimeOffset UpdatedAt { get; set; }
}