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

        public VoteActivityService(IVoteActivityRepository voteActivityRepository)
        {
            _voteActivityRepository = voteActivityRepository;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<VoteActivityListItem>> GetAllActivitiesAsync()
        {
            var activities = await _voteActivityRepository.GetAllAsync(includeDeleted: false);
            
            return activities.Select(a => new VoteActivityListItem
            {
                Id = a.Id,
                ActivityCode = a.ActivityCode,
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

            return MapToDetailResponse(activity);
        }

        /// <inheritdoc/>
        public async Task<VoteActivityDetailResponse> CreateActivityAsync(CreateVoteActivityRequest request, string createdBy)
        {
            // 驗證請求
            ValidateActivityRequest(request.Name, request.StartTime, request.EndTime);

            // 產生活動代號
            var activityCode = await _voteActivityRepository.GenerateActivityCodeAsync();

            var activity = new VoteActivity
            {
                Id = Guid.NewGuid(),
                ActivityCode = activityCode,
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy,
                IsDeleted = false
            };

            await _voteActivityRepository.CreateAsync(activity);

            return MapToDetailResponse(activity);
        }

        /// <inheritdoc/>
        public async Task<VoteActivityDetailResponse> UpdateActivityAsync(Guid id, UpdateVoteActivityRequest request)
        {
            var activity = await _voteActivityRepository.GetByIdAsync(id);
            
            if (activity == null || activity.IsDeleted)
            {
                throw new NotFoundException("活動不存在");
            }

            // 驗證請求
            ValidateActivityRequest(request.Name, request.StartTime, request.EndTime);

            // 更新欄位
            activity.Name = request.Name.Trim();
            activity.Description = request.Description?.Trim();
            activity.StartTime = request.StartTime;
            activity.EndTime = request.EndTime;

            await _voteActivityRepository.UpdateAsync(activity);

            return MapToDetailResponse(activity);
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

        /// <summary>
        /// 驗證活動請求
        /// </summary>
        private void ValidateActivityRequest(string name, DateTime startTime, DateTime endTime)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ValidationException("活動名稱不可為空");
            }

            if (name.Trim().Length > 100)
            {
                throw new ValidationException("活動名稱不可超過 100 字");
            }

            if (endTime <= startTime)
            {
                throw new ValidationException("結束時間必須大於開始時間");
            }
        }

        /// <summary>
        /// 取得活動狀態文字
        /// </summary>
        private static string GetActivityStatus(DateTime startTime, DateTime endTime)
        {
            var now = DateTime.UtcNow;

            if (now < startTime)
            {
                return "未開始";
            }
            else if (now >= startTime && now <= endTime)
            {
                return "進行中";
            }
            else
            {
                return "已結束";
            }
        }

        /// <summary>
        /// 轉換為詳情回應
        /// </summary>
        private static VoteActivityDetailResponse MapToDetailResponse(VoteActivity activity)
        {
            return new VoteActivityDetailResponse
            {
                Id = activity.Id,
                ActivityCode = activity.ActivityCode,
                Name = activity.Name,
                Description = activity.Description,
                StartTime = activity.StartTime,
                EndTime = activity.EndTime,
                Status = GetActivityStatus(activity.StartTime, activity.EndTime),
                CreatedAt = activity.CreatedAt,
                CreatedBy = activity.CreatedBy
            };
        }
    }
}
