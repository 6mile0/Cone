using System.ComponentModel.DataAnnotations;
using Ice.Enums;

namespace Ice.Areas.Admin.ViewModels.AdminUser;

public class AddAdminUserViewModel
{
    [Required(ErrorMessage = "名前は必須です")]
    public required string FullName { get; init; }

    [Required(ErrorMessage = "種別は必須です")]
    [EnumDataType(typeof(TutorTypes), ErrorMessage = "不正な種別です")]
    public required TutorTypes TutorType { get; init; }
    
    [Required(ErrorMessage = "メールアドレスは必須です")]
    [EmailAddress(ErrorMessage = "不正なメールアドレスです")]
    public required string Email { get; init; }
}