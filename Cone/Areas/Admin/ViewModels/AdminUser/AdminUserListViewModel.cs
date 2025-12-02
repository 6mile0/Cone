namespace Cone.Areas.Admin.ViewModels.AdminUser;

public class AdminUserListViewModel
{
    public required IReadOnlyList<AdminUserViewModel> AdminUsers { get; init; }
}