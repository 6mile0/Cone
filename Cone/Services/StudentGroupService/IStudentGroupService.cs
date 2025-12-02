using Cone.Areas.Admin.Dtos.Req;
using Cone.Db.Models;

namespace Cone.Services.StudentGroupService;

public interface IStudentGroupService
{
    /// <summary>
    /// Gets all student groups.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A list of all student groups.</returns>
    Task<IReadOnlyList<StudentGroups>> GetAllStudentGroupsAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Get a student group by its ID.
    /// </summary>
    /// <param name="groupId">The ID of the student group to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The student group with the specified ID, or null if not found.</returns>
    Task<StudentGroups?> GetStudentGroupByIdAsync(long? groupId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Create a new student group.
    /// </summary>
    /// <param name="addStudentGroupDto">The student group to create.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The created student group.</returns>
    Task<StudentGroups> CreateStudentGroupAsync(AddStudentGroupDto addStudentGroupDto, CancellationToken cancellationToken);
    
    /// <summary>
    /// Edit an existing student group.
    /// </summary>
    /// <param name="studentGroup">The student group to edit.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The updated student group.</returns>
    Task<StudentGroups> EditStudentGroupAsync(StudentGroups studentGroup, CancellationToken cancellationToken);
    
    /// <summary>
    /// Delete a student group.
    /// </summary>
    /// <param name="groupId">The ID of the student group to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteStudentGroupAsync(long groupId, CancellationToken cancellationToken);

    /// <summary>
    /// Update the assignment progress status for a student group.
    /// </summary>
    /// <param name="updateAssignmentProgressDto">The DTO containing student group ID, assignment ID, and new status.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAssignmentProgressAsync(UpdateAssignmentProgressDto updateAssignmentProgressDto, CancellationToken cancellationToken);
}