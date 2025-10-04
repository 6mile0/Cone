using System.ComponentModel.DataAnnotations;

namespace Ice.Areas.Admin.ViewModels.StudentGroup;

public class AddStudentGroupViewModel
{
    [Required(ErrorMessage = "班名は必須です")]
    public required string GroupName { get; init; }
}