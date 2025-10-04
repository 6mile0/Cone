using System.ComponentModel.DataAnnotations;

namespace Ice.Areas.Admin.ViewModels.Assignment;

public class UpdateAssignmentViewModel
{
    [Required(ErrorMessage = "課題名は必須です")]
    [MaxLength(100, ErrorMessage = "課題名は100文字以内で入力してください")]
    public required string Name { get; set; }

    [Required(ErrorMessage = "説明は必須です")]
    [MaxLength(1000, ErrorMessage = "説明は1000文字以内で入力してください")]
    public required string Description { get; set; }
}