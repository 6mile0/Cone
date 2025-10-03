using System.Collections.Immutable;

namespace Ice.Areas.Admin.ViewModels.StudentGroup;

public class StudentGroupListViewModel
{
    public ImmutableList<StudentGroupViewModel> StudentGroupList { get; init; }
}