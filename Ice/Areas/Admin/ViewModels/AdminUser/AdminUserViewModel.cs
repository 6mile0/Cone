using Ice.Enums;

namespace Ice.Areas.Admin.ViewModels.AdminUser;

public class AdminUserViewModel
{
    public required long Id { get; init; }
    
    public required string FullName { get; init; }
    
    public required TutorTypes TutorType { get; init; }
    
    public required DateTimeOffset CreatedAt { get; init; }
    
    public required DateTimeOffset UpdatedAt { get; init; }
}