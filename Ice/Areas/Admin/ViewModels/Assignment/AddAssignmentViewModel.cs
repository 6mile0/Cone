using System.ComponentModel.DataAnnotations;

namespace Ice.Areas.Admin.ViewModels.Assignment;

public class AddAssignmentViewModel
{
    [Required(ErrorMessage = "課題名は必須です")]
    [MaxLength(200, ErrorMessage = "課題名は200文字以内で入力してください")]
    public required string Name { get; init; }
    
    [Required(ErrorMessage = "完了要件(チェックポイント)は必須です")]
    public required string Description { get; init; }
}