using Ice.Areas.Admin.Dtos.Req;
using Ice.Db.Models;

namespace Ice.Services.AssignmentService;

public interface IAssignmentService
{
    /// <summary>
    /// Get all assignments.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A list of all assignments.</returns>
    Task<IReadOnlyList<Assignments>> GetAllAssignmentsAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Get an assignment by its ID.
    /// </summary>
    /// <param name="assignmentId">The ID of the assignment to retrieve.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The assignment with the specified ID, or null if not found.</returns>
    Task<Assignments?> GetAssignmentByIdAsync(long assignmentId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Get the assignments by student group ID.
    /// </summary>
    /// <param name="studentGroupId">The ID of the student group.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A list of assignments for the specified student group.</returns>
    Task<IReadOnlyList<Assignments>> GetAssignmentsByStudentGroupIdAsync(long studentGroupId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Get the tasks released for each group.
    /// </summary>
    /// <param name="groupId">The ID of the group.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A list of assignments for the specified group.</returns>
    Task<IReadOnlyList<Assignments>> GetReleasedAssignmentByGroupId(long groupId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Create a new assignment.
    /// </summary>
    /// <param name="addAssignmentDto">The assignment to create.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The created assignment.</returns>
    Task<Assignments> CreateAssignmentAsync(AddAssignmentDto addAssignmentDto, CancellationToken cancellationToken);
    
    /// <summary>
    /// Edit an existing assignment.
    /// </summary>
    /// <param name="assignment">The assignment to edit.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The updated assignment.</returns>
    Task<Assignments> EditAssignmentAsync(Assignments assignment, CancellationToken cancellationToken);
    
    /// <summary>
    /// Delete an assignment.
    /// </summary>
    /// <param name="assignmentId">The ID of the assignment to delete.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteAssignmentAsync(long assignmentId, CancellationToken cancellationToken);

    /// <summary>
    /// Assign an assignment to student groups.
    /// </summary>
    /// <param name="assignmentId">The ID of the assignment.</param>
    /// <param name="studentGroupIds">The IDs of student groups to assign.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AssignToStudentGroupsAsync(long assignmentId, List<long> studentGroupIds, CancellationToken cancellationToken);

    /// <summary>
    /// Unassign an assignment from a student group.
    /// </summary>
    /// <param name="assignmentId">The ID of the assignment.</param>
    /// <param name="studentGroupId">The ID of the student group to unassign.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UnassignFromStudentGroupAsync(long assignmentId, long studentGroupId, CancellationToken cancellationToken);

    /// <summary>
    /// Get student groups assigned to an assignment.
    /// </summary>
    /// <param name="assignmentId">The ID of the assignment.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A list of assigned student groups.</returns>
    Task<IReadOnlyList<StudentGroups>> GetAssignedStudentGroupsAsync(long assignmentId, CancellationToken cancellationToken);
}