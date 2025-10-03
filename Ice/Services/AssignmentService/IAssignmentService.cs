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
    /// Get the tasks released for each group.
    /// </summary>
    /// <param name="groupId">The ID of the group.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A list of assignments for the specified group.</returns>
    Task<IReadOnlyList<Assignments>> GetAssignmentsByGroupIdAsync(long groupId, CancellationToken cancellationToken);
    
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
}