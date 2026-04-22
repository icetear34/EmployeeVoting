using EmployeeVoting.Api.Dtos.Employee;

namespace EmployeeVoting.Api.Application.Interfaces
{
    /// <summary>
    /// 員工投票服務介面
    /// </summary>
    public interface IEmployeeVoteService
    {
        Task<List<ActivityWithCandidatesDto>> GetActivitiesForEmployeeAsync(string employeeNo);
        Task<(VoteResponse response, string? errorCode)> SubmitBatchVoteAsync(string employeeNo, SubmitBatchVoteRequest request, string clientIp, string userAgent);
        Task<ActivityResultBarsResponse?> GetResultBarsAsync(Guid activityId, string employeeNo);
    }
}
