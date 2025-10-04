using Ice.Db.Models;

namespace Ice.Services.AssignmentStudentGroupService;

public interface IAssignmentStudentGroupService
{
    /// <summary>
    /// Get student groups not started an assignment.
    /// </summary>
    /// <param name="assignmentId">The ID of the assignment.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A list of student groups not started the assignment.</returns>
    Task<IReadOnlyList<StudentGroups>> GetNotStartedStudentGroupsAsync(long assignmentId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Get student groups in progress of an assignment.
    /// </summary>
    /// <param name="assignmentId">The ID of the assignment.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A list of student groups in progress of the assignment.</returns>
    Task<IReadOnlyList<StudentGroups>> GetInProgressStudentGroupsAsync(long assignmentId, CancellationToken cancellationToken);
    
    /// <summary>
    /// Get student groups completed an assignment.
    /// </summary>
    /// <param name="assignmentId">The ID of the assignment.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A list of student groups completed the assignment.</returns>
    Task<IReadOnlyList<StudentGroups>> GetCompletedStudentGroupsAsync(long assignmentId, CancellationToken cancellationToken);
}