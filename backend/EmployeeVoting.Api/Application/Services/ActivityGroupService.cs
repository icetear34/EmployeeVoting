using EmployeeVoting.Api.Application.Interfaces;
using EmployeeVoting.Api.Domain.Entities;
using EmployeeVoting.Api.Domain.Exceptions;
using EmployeeVoting.Api.Dtos.Admin;

namespace EmployeeVoting.Api.Application.Services
{
    /// <summary>
    /// 活動分區服務實作
    /// </summary>
    public class ActivityGroupService : IActivityGroupService
    {
        private readonly IActivityGroupRepository _groupRepo;
        private readonly IAdminUserGroupRepository _userGroupRepo;
        private readonly IAdminUserRepository _adminUserRepo;
        private readonly IVoteActivityRepository _activityRepo;

        public ActivityGroupService(
            IActivityGroupRepository groupRepo,
            IAdminUserGroupRepository userGroupRepo,
            IAdminUserRepository adminUserRepo,
            IVoteActivityRepository activityRepo)
        {
            _groupRepo = groupRepo;
            _userGroupRepo = userGroupRepo;
            _adminUserRepo = adminUserRepo;
            _activityRepo = activityRepo;
        }

        public async Task<IEnumerable<ActivityGroupResponse>> GetAllAsync()
        {
            var groups = await _groupRepo.GetAllAsync();
            var result = new List<ActivityGroupResponse>();

            foreach (var g in groups)
            {
                var members = await _userGroupRepo.GetByGroupIdAsync(g.Id);
                var activities = await _activityRepo.GetAllAsync(includeDeleted: false);
                var actCount = activities.Count(a => a.ActivityGroupId == g.Id);

                result.Add(new ActivityGroupResponse
                {
                    Id = g.Id,
                    Name = g.Name,
                    Description = g.Description,
                    CreatedAt = g.CreatedAt,
                    CreatedBy = g.CreatedBy,
                    AdminCount = members.Count(),
                    ActivityCount = actCount
                });
            }
            return result;
        }

        public async Task<ActivityGroupDetailResponse?> GetByIdAsync(Guid id)
        {
            var group = await _groupRepo.GetByIdAsync(id);
            if (group == null) return null;

            var memberLinks = await _userGroupRepo.GetByGroupIdAsync(id);
            var memberList = new List<AdminUserResponse>();
            foreach (var link in memberLinks)
            {
                var user = await _adminUserRepo.GetByIdAsync(link.AdminUserId);
                if (user != null)
                {
                    memberList.Add(new AdminUserResponse
                    {
                        Id = user.Id,
                        Username = user.Account,
                        Name = user.DisplayName,
                        Role = user.Role,
                        IsEnabled = user.IsEnabled,
                        CreatedAt = user.CreatedAt,
                        UpdatedAt = user.UpdatedAt
                    });
                }
            }

            var activities = await _activityRepo.GetAllAsync(includeDeleted: false);
            var actCount = activities.Count(a => a.ActivityGroupId == id);

            return new ActivityGroupDetailResponse
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                CreatedAt = group.CreatedAt,
                CreatedBy = group.CreatedBy,
                AdminCount = memberList.Count,
                ActivityCount = actCount,
                Members = memberList
            };
        }

        public async Task<ActivityGroupResponse> CreateAsync(CreateActivityGroupRequest request, string createdBy)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("分區名稱不可為空");

            var group = new ActivityGroup
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Description = request.Description?.Trim() ?? string.Empty,
                CreatedAt = DateTime.Now,
                CreatedBy = createdBy,
                IsDeleted = false
            };

            await _groupRepo.CreateAsync(group);

            // 設定成員：每個 adminUser 加入此分區
            if (request.AdminUserIds?.Any() == true)
            {
                foreach (var adminId in request.AdminUserIds)
                {
                    var existing = (await _userGroupRepo.GetByAdminUserIdAsync(adminId))
                        .Select(x => x.ActivityGroupId).ToList();
                    if (!existing.Contains(group.Id))
                    {
                        existing.Add(group.Id);
                    }
                    await _userGroupRepo.SetGroupsForAdminAsync(adminId, existing);
                }
            }

            return new ActivityGroupResponse
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                CreatedAt = group.CreatedAt,
                CreatedBy = group.CreatedBy,
                AdminCount = request.AdminUserIds?.Count ?? 0,
                ActivityCount = 0
            };
        }

        public async Task<ActivityGroupResponse> UpdateAsync(Guid id, UpdateActivityGroupRequest request)
        {
            var group = await _groupRepo.GetByIdAsync(id)
                ?? throw new NotFoundException("分區不存在");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("分區名稱不可為空");

            group.Name = request.Name.Trim();
            group.Description = request.Description?.Trim() ?? string.Empty;
            await _groupRepo.UpdateAsync(group);

            var members = await _userGroupRepo.GetByGroupIdAsync(id);
            var activities = await _activityRepo.GetAllAsync(includeDeleted: false);

            return new ActivityGroupResponse
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                CreatedAt = group.CreatedAt,
                CreatedBy = group.CreatedBy,
                AdminCount = members.Count(),
                ActivityCount = activities.Count(a => a.ActivityGroupId == id)
            };
        }

        public async Task DeleteAsync(Guid id)
        {
            var group = await _groupRepo.GetByIdAsync(id)
                ?? throw new NotFoundException("分區不存在");

            await _groupRepo.SoftDeleteAsync(id);
            // 刪除所有成員關聯
            await _userGroupRepo.RemoveAllByGroupIdAsync(id);
        }

        public async Task SetMembersAsync(Guid groupId, IEnumerable<Guid> adminUserIds)
        {
            var group = await _groupRepo.GetByIdAsync(groupId)
                ?? throw new NotFoundException("分區不存在");

            // 先取得目前成員
            var currentMembers = (await _userGroupRepo.GetByGroupIdAsync(groupId))
                .Select(x => x.AdminUserId).ToHashSet();

            var newIds = adminUserIds.ToHashSet();

            // 移除不在新列表中的成員
            foreach (var oldMemberId in currentMembers.Where(id => !newIds.Contains(id)))
            {
                var userGroups = (await _userGroupRepo.GetByAdminUserIdAsync(oldMemberId))
                    .Select(x => x.ActivityGroupId)
                    .Where(gId => gId != groupId)
                    .ToList();
                await _userGroupRepo.SetGroupsForAdminAsync(oldMemberId, userGroups);
            }

            // 加入新成員
            foreach (var newMemberId in newIds.Where(id => !currentMembers.Contains(id)))
            {
                var userGroups = (await _userGroupRepo.GetByAdminUserIdAsync(newMemberId))
                    .Select(x => x.ActivityGroupId).ToList();
                if (!userGroups.Contains(groupId))
                {
                    userGroups.Add(groupId);
                }
                await _userGroupRepo.SetGroupsForAdminAsync(newMemberId, userGroups);
            }
        }
    }
}
