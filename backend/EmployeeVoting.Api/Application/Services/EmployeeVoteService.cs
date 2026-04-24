using EmployeeVoting.Api.Application.Interfaces;
using EmployeeVoting.Api.Common;
using EmployeeVoting.Api.Domain.Entities;
using EmployeeVoting.Api.Dtos.Employee;

namespace EmployeeVoting.Api.Application.Services
{
    /// <summary>
    /// 員工投票服務實作
    /// </summary>
    public class EmployeeVoteService : IEmployeeVoteService
    {
        private readonly IVoteActivityRepository _voteActivityRepository;
        private readonly ICandidateRepository _candidateRepository;
        private readonly IEligibleVoterRepository _eligibleVoterRepository;
        private readonly IVoteRecordRepository _voteRecordRepository;

        public EmployeeVoteService(
            IVoteActivityRepository voteActivityRepository,
            ICandidateRepository candidateRepository,
            IEligibleVoterRepository eligibleVoterRepository,
            IVoteRecordRepository voteRecordRepository)
        {
            _voteActivityRepository = voteActivityRepository;
            _candidateRepository = candidateRepository;
            _eligibleVoterRepository = eligibleVoterRepository;
            _voteRecordRepository = voteRecordRepository;
        }

        public async Task<List<ActivityWithCandidatesDto>> GetActivitiesForEmployeeAsync(string employeeNo)
        {
            // 取得此工號所在的所有名單
            var voters = (await _eligibleVoterRepository.GetByEmployeeNoAsync(employeeNo)).ToList();
            if (voters.Count == 0) return new List<ActivityWithCandidatesDto>();

            var now = DateTime.UtcNow;
            var result = new List<ActivityWithCandidatesDto>();

            foreach (var voter in voters)
            {
                var activity = await _voteActivityRepository.GetByIdAsync(voter.VoteActivityId);

                // 跳過已刪除
                if (activity == null || activity.IsDeleted) continue;

                var isActive = activity.StartTime <= now && activity.EndTime >= now;
                var hasVoted = await _voteRecordRepository.HasVotedAsync(activity.Id, employeeNo);
                var isViewable = IsResultViewable(activity);

                // 顯示條件：進行中的活動，或已結束且已投票且結果可查閱
                if (!isActive && !(hasVoted && isViewable)) continue;

                var candidates = (await _candidateRepository.GetByActivityIdAsync(activity.Id))
                    .Where(c => c.IsEnabled)
                    .OrderBy(c => c.SortOrder)
                    .ToList();

                // 若已投票且結果可查閱，計算佔比
                List<ResultBarDto>? resultBars = null;
                if (hasVoted && isViewable)
                {
                    resultBars = await BuildResultBarsAsync(activity.Id, candidates);
                }

                result.Add(new ActivityWithCandidatesDto
                {
                    ActivityId = activity.Id,
                    ActivityName = activity.Name,
                    Description = activity.Description,
                    StartTime = activity.StartTime,
                    EndTime = activity.EndTime,
                    HasVoted = hasVoted,
                    Candidates = candidates.Select(c => new CandidateDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        ImageUrl = string.IsNullOrWhiteSpace(c.ImagePath) ? "" : $"{c.ImagePath}"
                    }).ToList(),
                    ResultBars = resultBars
                });
            }

            return result;
        }

        public async Task<(VoteResponse response, string? errorCode)> SubmitBatchVoteAsync(
            string employeeNo, SubmitBatchVoteRequest request, string clientIp, string userAgent)
        {
            if (request.Votes == null || request.Votes.Count == 0)
            {
                return (new VoteResponse { Success = false, Message = "請選擇候選人" }, ErrorCodes.ValidationFailed);
            }

            var now = DateTime.UtcNow;
            var records = new List<VoteRecord>();

            foreach (var vote in request.Votes)
            {
                // 驗證活動
                var activity = await _voteActivityRepository.GetByIdAsync(vote.ActivityId);
                if (activity == null || activity.IsDeleted || activity.StartTime > now || activity.EndTime < now)
                {
                    return (new VoteResponse { Success = false, Message = "活動不在進行中" }, ErrorCodes.ActivityNotActive);
                }

                // 驗證在名單內
                var voter = await _eligibleVoterRepository.FindAsync(vote.ActivityId, employeeNo);
                if (voter == null)
                {
                    return (new VoteResponse { Success = false, Message = "您不在此活動的投票名單中" }, ErrorCodes.NotInVoterList);
                }

                // 驗證未重複投票
                var hasVoted = await _voteRecordRepository.HasVotedAsync(vote.ActivityId, employeeNo);
                if (hasVoted)
                {
                    return (new VoteResponse { Success = false, Message = "您已投過此活動的票" }, ErrorCodes.AlreadyVoted);
                }

                // 驗證候選人
                var candidates = await _candidateRepository.GetByActivityIdAsync(vote.ActivityId);
                var candidate = candidates.FirstOrDefault(c => c.Id == vote.CandidateId && c.IsEnabled);
                if (candidate == null)
                {
                    return (new VoteResponse { Success = false, Message = "候選人不存在或已停用" }, ErrorCodes.InvalidCandidate);
                }

                records.Add(new VoteRecord
                {
                    Id = Guid.NewGuid(),
                    VoteActivityId = vote.ActivityId,
                    CandidateId = vote.CandidateId,
                    EmployeeNo = employeeNo,
                    VotedAt = now,
                    ClientIp = clientIp,
                    UserAgent = userAgent
                });
            }

            // 批次寫入
            try
            {
                await _voteRecordRepository.BatchCreateAsync(records);
            }
            catch
            {
                return (new VoteResponse { Success = false, Message = "投票失敗，請稍後再試" }, ErrorCodes.BatchVoteFailed);
            }

            return (new VoteResponse { Success = true, Message = "投票成功" }, null);
        }

        public async Task<ActivityResultBarsResponse?> GetResultBarsAsync(Guid activityId, string employeeNo)
        {
            var activity = await _voteActivityRepository.GetByIdAsync(activityId);
            if (activity == null || activity.IsDeleted) return null;

            // 確認已投票
            var hasVoted = await _voteRecordRepository.HasVotedAsync(activityId, employeeNo);
            if (!hasVoted) return null;

            // 確認可查閱
            if (!IsResultViewable(activity)) return null;

            var candidates = (await _candidateRepository.GetByActivityIdAsync(activityId))
                .Where(c => c.IsEnabled)
                .OrderBy(c => c.SortOrder)
                .ToList();

            var bars = await BuildResultBarsAsync(activityId, candidates);
            return new ActivityResultBarsResponse { Candidates = bars };
        }

        // --- 私有輔助方法 ---

        private static bool IsResultViewable(VoteActivity activity)
        {
            if (!activity.IsResultViewable) return false;
            var now = DateTime.UtcNow;
            if (activity.ResultViewStartTime.HasValue && now < activity.ResultViewStartTime.Value) return false;
            if (activity.ResultViewEndTime.HasValue && now > activity.ResultViewEndTime.Value) return false;
            return true;
        }

        private async Task<List<ResultBarDto>> BuildResultBarsAsync(Guid activityId, List<Candidate> candidates)
        {
            var allVotes = (await _voteRecordRepository.GetByActivityIdAsync(activityId)).ToList();
            var totalVotes = allVotes.Count;

            return candidates.Select(c =>
            {
                var count = allVotes.Count(v => v.CandidateId == c.Id);
                var percent = totalVotes > 0 ? Math.Round((decimal)count / totalVotes * 100, 1) : 0m;
                return new ResultBarDto { Name = c.Name, Percent = percent };
            }).ToList();
        }
    }
}
