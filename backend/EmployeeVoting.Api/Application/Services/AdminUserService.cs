using EmployeeVoting.Api.Application.Interfaces;
using EmployeeVoting.Api.Domain.Entities;
using EmployeeVoting.Api.Domain.Exceptions;
using EmployeeVoting.Api.Dtos.Admin;

namespace EmployeeVoting.Api.Application.Services
{
    /// <summary>
    /// 管理者帳號服務實作
    /// </summary>
    public class AdminUserService : IAdminUserService
    {
        private readonly IAdminUserRepository _adminUserRepository;
        private readonly IAdminUserGroupRepository _userGroupRepository;
        private readonly IActivityGroupRepository _activityGroupRepository;

        public AdminUserService(
            IAdminUserRepository adminUserRepository,
            IAdminUserGroupRepository userGroupRepository,
            IActivityGroupRepository activityGroupRepository)
        {
            _adminUserRepository = adminUserRepository;
            _userGroupRepository = userGroupRepository;
            _activityGroupRepository = activityGroupRepository;
        }

        /// <inheritdoc/>
        public async Task<PagedResult<AdminUserResponse>> GetPagedAsync(AdminUserQueryRequest query)
        {
            var all = (await _adminUserRepository.GetAllAsync()).ToList();

            // 關鍵字過濾（帳號 or 姓名）
            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                var kw = query.Keyword.Trim().ToLower();
                all = all.Where(u =>
                    u.Account.ToLower().Contains(kw) ||
                    u.DisplayName.ToLower().Contains(kw)
                ).ToList();
            }

            var totalCount = all.Count;
            var page = Math.Max(1, query.Page);
            var pageSize = Math.Clamp(query.PageSize, 1, 100);

            var paged = all
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var items = new List<AdminUserResponse>();
            foreach (var u in paged)
            {
                var resp = await ToResponseWithGroupsAsync(u);
                items.Add(resp);
            }

            return new PagedResult<AdminUserResponse>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        /// <inheritdoc/>
        public async Task<AdminUserResponse?> GetByIdAsync(Guid id)
        {
            var user = await _adminUserRepository.GetByIdAsync(id);
            return user == null ? null : await ToResponseWithGroupsAsync(user);
        }

        /// <inheritdoc/>
        public async Task<AdminUserResponse> CreateAsync(CreateAdminUserRequest request)
        {
            // 驗證
            if (string.IsNullOrWhiteSpace(request.Username))
                throw new ValidationException("帳號不可為空");
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("姓名不可為空");
            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                throw new ValidationException("密碼長度至少 6 個字元");

            // 帳號重複檢查
            if (await _adminUserRepository.AccountExistsAsync(request.Username.Trim()))
                throw new ValidationException("帳號已存在");

            var user = new AdminUser
            {
                Account = request.Username.Trim(),
                Password = request.Password,
                DisplayName = request.Name.Trim(),
                Role = string.IsNullOrWhiteSpace(request.Role) ? "admin" : request.Role,
                IsEnabled = request.IsEnabled
            };

            await _adminUserRepository.CreateAsync(user);

            return ToResponse(user);
        }

        /// <inheritdoc/>
        public async Task<AdminUserResponse> UpdateAsync(Guid id, UpdateAdminUserRequest request)
        {
            var user = await _adminUserRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("帳號不存在");

            if (string.IsNullOrWhiteSpace(request.Username))
                throw new ValidationException("帳號不可為空");
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("姓名不可為空");

            // 帳號重複檢查（排除自身）
            if (await _adminUserRepository.AccountExistsAsync(request.Username.Trim(), id))
                throw new ValidationException("帳號已被其他人使用");

            user.Account = request.Username.Trim();
            user.DisplayName = request.Name.Trim();
            user.Role = string.IsNullOrWhiteSpace(request.Role) ? "admin" : request.Role;
            user.IsEnabled = request.IsEnabled;

            // 若有傳入密碼則更新
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                if (request.Password.Length < 6)
                    throw new ValidationException("密碼長度至少 6 個字元");
                user.Password = request.Password;
            }

            await _adminUserRepository.UpdateAsync(user);

            return ToResponse(user);
        }

        /// <inheritdoc/>
        public async Task ResetPasswordAsync(Guid id, ResetAdminPasswordRequest request)
        {
            var user = await _adminUserRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("帳號不存在");

            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
                throw new ValidationException("密碼長度至少 6 個字元");

            user.Password = request.NewPassword;
            await _adminUserRepository.UpdateAsync(user);
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(Guid id)
        {
            var user = await _adminUserRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("帳號不存在");

            // 防止刪除最後一個管理員
            var all = (await _adminUserRepository.GetAllAsync()).ToList();
            if (all.Count <= 1)
                throw new ValidationException("無法刪除最後一個管理者帳號");

            // 直接由 Repository 刪除（AdminUser 尚無軟刪除，使用硬刪除）
            await _adminUserRepository.DeleteAsync(id);
        }

        // ─── 私有輔助 ────────────────────────────────────────────────────
        private static AdminUserResponse ToResponse(AdminUser u) => new()
        {
            Id = u.Id,
            Username = u.Account,
            Name = u.DisplayName,
            Role = string.IsNullOrWhiteSpace(u.Role) ? "admin" : u.Role,
            IsEnabled = u.IsEnabled,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt
        };

        private async Task<AdminUserResponse> ToResponseWithGroupsAsync(AdminUser u)
        {
            var resp = ToResponse(u);
            var userGroups = await _userGroupRepository.GetByAdminUserIdAsync(u.Id);
            var groups = new List<ActivityGroupResponse>();
            foreach (var ug in userGroups)
            {
                var g = await _activityGroupRepository.GetByIdAsync(ug.ActivityGroupId);
                if (g != null)
                {
                    groups.Add(new ActivityGroupResponse
                    {
                        Id = g.Id,
                        Name = g.Name,
                        Description = g.Description,
                        CreatedAt = g.CreatedAt,
                        CreatedBy = g.CreatedBy
                    });
                }
            }
            resp.Groups = groups;
            return resp;
        }
    }
}
