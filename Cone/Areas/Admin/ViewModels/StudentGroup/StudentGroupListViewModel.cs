using System.Collections.Immutable;

namespace Cone.Areas.Admin.ViewModels.StudentGroup;

public class StudentGroupListViewModel
{
    public ImmutableList<StudentGroupViewModel> StudentGroupList { get; init; } = [];
}