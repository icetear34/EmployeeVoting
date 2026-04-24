using EmployeeVoting.Api.Application.Interfaces;
using EmployeeVoting.Api.Common;
using EmployeeVoting.Api.Domain.Entities;
using EmployeeVoting.Api.Domain.Exceptions;
using EmployeeVoting.Api.Dtos.Admin;

namespace EmployeeVoting.Api.Application.Services
{
    /// <summary>
    /// 投票活動服務實作
    /// </summary>
    public class VoteActivityService : IVoteActivityService
    {
        private readonly IVoteActivityRepository _voteActivityRepository;
        private readonly ICandidateRepository _candidateRepository;
        private readonly IEligibleVoterRepository _eligibleVoterRepository;
        private readonly IVoteRecordRepository _voteRecordRepository;

        public VoteActivityService(
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

        /// <inheritdoc/>
        public async Task<PagedResult<VoteActivityListItem>> GetActivitiesAsync(ActivityQueryRequest query)
        {
            var (items, total) = await _voteActivityRepository.GetPagedAsync(query);

            var page     = Math.Max(1, query.Page);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);

            return new PagedResult<VoteActivityListItem>
            {
                Items = items.Select(a => new VoteActivityListItem
                {
                    Id          = a.Id,
                    Name        = a.Name,
                    Description = a.Description,
                    StartTime   = a.StartTime,
                    EndTime     = a.EndTime,
                    Status      = GetActivityStatus(a.StartTime, a.EndTime),
                    CreatedAt   = a.CreatedAt
                }),
                TotalCount = total,
                Page       = page,
                PageSize   = pageSize
            };
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<VoteActivityListItem>> GetAllActivitiesAsync()
        {
            var activities = await _voteActivityRepository.GetAllAsync(includeDeleted: false);
            
            return activities.Select(a => new VoteActivityListItem
            {
                Id = a.Id,
                Name = a.Name,
                Description = a.Description,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = GetActivityStatus(a.StartTime, a.EndTime),
                CreatedAt = a.CreatedAt
            }).OrderByDescending(a => a.CreatedAt);
        }

        /// <inheritdoc/>
        public async Task<VoteActivityDetailResponse?> GetActivityByIdAsync(Guid id)
        {
            var activity = await _voteActivityRepository.GetByIdAsync(id);
            
            if (activity == null || activity.IsDeleted)
            {
                return null;
            }

            var response = MapToDetailResponse(activity);

            // 載入候選人（含得票數）
            var candidates = await _candidateRepository.GetByActivityIdAsync(id);
            var candidateItems = new List<CandidateListItem>();
            foreach (var c in candidates)
            {
                var voteCount = await _voteRecordRepository.GetCountByCandidateIdAsync(c.Id);
                candidateItems.Add(new CandidateListItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ImageUrl = c.ImagePath,
                    SortOrder = c.SortOrder,
                    IsEnabled = c.IsEnabled,
                    CreatedAt = c.CreatedAt,
                    VoteCount = voteCount
                });
            }
            response.Candidates = candidateItems;

            // 載入投票名單
            var voters = await _eligibleVoterRepository.GetByActivityIdAsync(id);
            response.EligibleVoters = voters.Select(v => new EligibleVoterListItem
            {
                Id = v.Id,
                EmployeeNo = v.EmployeeNo,
                Name = v.Name,
                Department = v.Department,
                BirthDate = v.BirthDate,
                CreatedAt = v.CreatedAt
            }).ToList();

            return response;
        }

        /// <inheritdoc/>
        public async Task<VoteActivityDetailResponse> CreateActivityAsync(CreateVoteActivityRequest request, string createdBy)
        {
            ValidateActivityRequest(request.ActivityName, request.StartTime, request.EndTime, request.IsResultViewable,request.ResultViewStartTime,request.ResultViewEndTime);

            var now = DateTime.Now;

            var activity = new VoteActivity
            {
                Id = Guid.NewGuid(),
                Name = request.ActivityName.Trim(),
                Description = request.Description?.Trim(),
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IsResultViewable = request.IsResultViewable,
                ResultViewStartTime = request.ResultViewStartTime,
                ResultViewEndTime = request.ResultViewEndTime,
                CreatedAt = now,
                CreatedBy = createdBy,
                IsDeleted = false
            };

            await _voteActivityRepository.CreateAsync(activity);

            // 建立候選人（如果有）
            if (request.Candidates?.Any() == true)
            {
                var candidates = request.Candidates.Select((c, idx) => new Candidate
                {
                    Id = Guid.NewGuid(),
                    VoteActivityId = activity.Id,
                    Name = c.Name.Trim(),
                    Description = c.Description?.Trim() ?? string.Empty,
                    ImagePath = c.ImagePath?.Trim() ?? string.Empty,
                    SortOrder = idx + 1,
                    IsEnabled = true,
                    CreatedAt = now
                });
                await _candidateRepository.BatchCreateAsync(candidates);
            }

            // 建立投票名單（如果有）
            if (request.EligibleVoters?.Any() == true)
            {
                var voters = request.EligibleVoters.Select(v => new EligibleVoter
                {
                    Id = Guid.NewGuid(),
                    VoteActivityId = activity.Id,
                    EmployeeNo = v.EmployeeNo.Trim(),
                    Name = v.Name?.Trim() ?? string.Empty,
                    Department = v.Department?.Trim() ?? string.Empty,
                    BirthDate = v.BirthDate.Trim(),
                    CreatedAt = now
                });
                await _eligibleVoterRepository.BatchCreateAsync(voters);
            }

            // 回傳完整詳情
            return (await GetActivityByIdAsync(activity.Id))!;
        }

        /// <inheritdoc/>
        public async Task<VoteActivityDetailResponse> UpdateActivityAsync(Guid id, UpdateVoteActivityRequest request)
        {
            var activity = await _voteActivityRepository.GetByIdAsync(id);
            
            if (activity == null || activity.IsDeleted)
            {
                throw new NotFoundException("活動不存在");
            }

            ValidateActivityRequest(request.Name, request.StartTime, request.EndTime, request.IsResultViewable, request.ResultViewStartTime, request.ResultViewEndTime);

            activity.Name = request.Name.Trim();
            activity.Description = request.Description?.Trim();
            activity.StartTime = request.StartTime;
            activity.EndTime = request.EndTime;
            activity.IsResultViewable = request.IsResultViewable;
            activity.ResultViewStartTime = request.ResultViewStartTime;
            activity.ResultViewEndTime = request.ResultViewEndTime;

            await _voteActivityRepository.UpdateAsync(activity);

            return (await GetActivityByIdAsync(id))!;
        }

        /// <inheritdoc/>
        public async Task DeleteActivityAsync(Guid id)
        {
            var activity = await _voteActivityRepository.GetByIdAsync(id);
            
            if (activity == null || activity.IsDeleted)
            {
                throw new NotFoundException("活動不存在");
            }

            await _voteActivityRepository.SoftDeleteAsync(id);
        }

        /// <inheritdoc/>
        public async Task UpdateCandidatesAsync(Guid activityId, UpdateCandidatesRequest request)
        {
            var activity = await _voteActivityRepository.GetByIdAsync(activityId);
            if (activity == null || activity.IsDeleted)
            {
                throw new NotFoundException("活動不存在");
            }

            var now = DateTime.Now;
            var candidates = request.Candidates.Select((c, idx) => new Candidate
            {
                Id = c.Id ?? Guid.NewGuid(),
                VoteActivityId = activityId,
                Name = c.Name.Trim(),
                Description = c.Description?.Trim() ?? string.Empty,
                ImagePath = c.ImagePath?.Trim() ?? string.Empty,
                SortOrder = c.SortOrder > 0 ? c.SortOrder : idx + 1,
                IsEnabled = true,
                CreatedAt = now
            });

            await _candidateRepository.ReplaceAllAsync(activityId, candidates);
        }

        /// <inheritdoc/>
        public async Task UpdateEligibleVotersAsync(Guid activityId, UpdateEligibleVotersRequest request)
        {
            var activity = await _voteActivityRepository.GetByIdAsync(activityId);
            if (activity == null || activity.IsDeleted)
            {
                throw new NotFoundException("活動不存在");
            }

            var now = DateTime.Now;
            var voters = request.Voters.Select(v => new EligibleVoter
            {
                Id = Guid.NewGuid(),
                VoteActivityId = activityId,
                EmployeeNo = v.EmployeeNo.Trim(),
                Name = v.Name?.Trim() ?? string.Empty,
                Department = v.Department?.Trim() ?? string.Empty,
                BirthDate = v.BirthDate.Trim(),
                CreatedAt = now
            });

            await _eligibleVoterRepository.ReplaceAllAsync(activityId, voters);
        }

        private void ValidateActivityRequest(
    string name,
    DateTime startTime,
    DateTime endTime,
    bool isResultViewable,
    DateTime? resultViewStartTime,
    DateTime? resultViewEndTime)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ValidationException("活動名稱不可為空");

            if (name.Trim().Length > 100)
                throw new ValidationException("活動名稱不可超過 100 字");

            if (endTime <= startTime)
                throw new ValidationException("結束時間必須大於開始時間");

            if (isResultViewable)
            {
                if (!resultViewStartTime.HasValue || !resultViewEndTime.HasValue)
                    throw new ValidationException("啟用結果查閱時，必須填寫查閱開始與結束時間");

                if (resultViewEndTime <= resultViewStartTime)
                    throw new ValidationException("結果查閱結束時間必須大於開始時間");
            }
        }

        private static string GetActivityStatus(DateTime startTime, DateTime endTime)
        {
            var now = DateTime.Now;
            if (now < startTime) return "未開始";
            if (now <= endTime) return "進行中";
            return "已結束";
        }

        private static VoteActivityDetailResponse MapToDetailResponse(VoteActivity activity)
        {
            return new VoteActivityDetailResponse
            {
                Id = activity.Id,
                Name = activity.Name,
                Description = activity.Description,
                StartTime = activity.StartTime,
                EndTime = activity.EndTime,
                Status = GetActivityStatus(activity.StartTime, activity.EndTime),
                CreatedAt = activity.CreatedAt,
                CreatedBy = activity.CreatedBy,
                IsResultViewable = activity.IsResultViewable,
                ResultViewStartTime = activity.ResultViewStartTime,
                ResultViewEndTime = activity.ResultViewEndTime
            };
        }
    }
}
