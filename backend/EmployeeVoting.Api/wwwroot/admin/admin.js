// ═══════════════════════════════════════════════════════════════════════════
// admin.js - 管理端共用 JavaScript
// ═══════════════════════════════════════════════════════════════════════════

/**
 * 管理員工具模組
 */
const AdminUtil = {
  /**
   * 檢查管理員登入狀態
   * @returns {Promise<object|null>} 管理員資料或 null
   */
  async checkAuth() {
    try {
      const response = await ApiUtil.get('/admin-auth/me');
      sessionStorage.setItem('admin_data', JSON.stringify(response));
      return response;
    } catch (error) {
      if (error.status === 401) {
        sessionStorage.removeItem('admin_data');
        return null;
      }
      const cached = sessionStorage.getItem('admin_data');
      return cached ? JSON.parse(cached) : null;
    }
  },

  /**
   * 取得快取的管理員資料
   * @returns {object|null}
   */
  getCachedData() {
    const cached = sessionStorage.getItem('admin_data');
    return cached ? JSON.parse(cached) : null;
  },

  /**
   * 清除管理員登入資料
   */
  clearAuth() {
    CookieUtil.remove('admin_logged_in');
    sessionStorage.removeItem('admin_data');
  },

  /**
   * 導向到登入頁
   */
  redirectToLogin() {
    window.location.href = 'login.html';
  },

  /**
   * 管理員登入
   * @param {string} username 帳號
   * @param {string} password 密碼
   * @param {string} captchaKey 驗證碼 Key
   * @param {string} captchaCode 驗證碼
   * @returns {Promise<object>} 登入回應
   */
  async login(username, password, captchaKey, captchaCode) {
    const response = await ApiUtil.post('/admin-auth/login', {
      username,
      password,
      captchaKey,
      captchaCode
    });

    // 後端已設 HttpOnly Cookie，不需前端手動存 token
    if (response.admin) {
      sessionStorage.setItem('admin_data', JSON.stringify(response.admin));
    }

    return response;
  },

  /**
   * 管理員登出
   * @returns {Promise<void>}
   */
  async logout() {
    try {
      await ApiUtil.post('/admin-auth/logout', {});
    } catch (error) {
      // 忽略登出錯誤
    }
    this.clearAuth();
    this.redirectToLogin();
  }
};

/**
 * 活動管理 API
 */
const ActivityApi = {
  /**
   * 取得活動列表
   * @param {object} params 查詢參數
   * @returns {Promise<object>}
   */
  async list(params = {}) {
    const query = new URLSearchParams(params).toString();
    return await ApiUtil.get(`/admin/activities${query ? '?' + query : ''}`);
  },

  /**
   * 取得活動詳情
   * @param {string} id 
   * @returns {Promise<object>}
   */
  async get(id) {
    return await ApiUtil.get(`/admin/activities/${id}`);
  },

  /**
   * 建立活動
   * @param {object} data 
   * @returns {Promise<object>}
   */
  async create(data) {
    return await ApiUtil.post('/admin/activities', data);
  },

  /**
   * 更新活動
   * @param {string} id 
   * @param {object} data 
   * @returns {Promise<object>}
   */
  async update(id, data) {
    return await ApiUtil.put(`/admin/activities/${id}`, data);
  },

  /**
   * 刪除活動
   * @param {string} id 
   * @returns {Promise<void>}
   */
  async delete(id) {
    return await ApiUtil.delete(`/admin/activities/${id}`);
  },

  /**
   * 下載投票人員 CSV 範本（由後端提供，含工號/名稱/單位/生日）
   * @returns {Promise<void>}
   */
  async downloadVoterTemplate() {
    const response = await fetch(API_BASE_URL + '/admin/activities/voters/template', {
      method: 'GET',
      credentials: 'include'
    });

    if (!response.ok) {
      const text = await response.text();
      let msg = `下載失敗（HTTP ${response.status}）`;
      try { msg = JSON.parse(text)?.message || msg; } catch { /* ignore */ }
      throw new Error(msg);
    }

    const blob = await response.blob();
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'voters_template.csv';
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  },

  /**
   * 解析 CSV 投票名單（純解析，不存 DB）
   * 回傳陣列供前端暫存，儲存活動時一起送出
   * @param {File} file CSV 檔案
   * @returns {Promise<Array<{employeeNo, name, department, birthDate}>>}
   */
  async importVoters(file) {
    const formData = new FormData();
    formData.append('file', file);

    const response = await fetch(API_BASE_URL + '/admin/activities/voters/parse', {
      method: 'POST',
      credentials: 'include',
      body: formData
    });

    // 先確認有 body 再 parse JSON，避免空回應 crash
    let result;
    const contentType = response.headers.get('content-type') || '';
    if (contentType.includes('application/json')) {
      result = await response.json();
    } else {
      const text = await response.text();
      if (text) {
        try { result = JSON.parse(text); } catch { result = null; }
      }
    }

    if (!response.ok) {
      const msg = result?.message || `請求失敗（HTTP ${response.status}）`;
      const error = new Error(msg);
      error.status = response.status;
      throw error;
    }

    // 後端回傳 PascalCase，統一轉成 camelCase
    return (Array.isArray(result) ? result : []).map(v => ({
      employeeNo: v.EmployeeNo || v.employeeNo || '',
      name:       v.Name       || v.name       || '',
      department: v.Department || v.department || '',
      birthDate:  v.BirthDate  || v.birthDate  || ''
    }));
  }
};

/**
 * 使用者管理 API
 */
const UserApi = {
  /**
   * 取得使用者列表
   * @param {object} params 查詢參數
   * @returns {Promise<object>}
   */
  async list(params = {}) {
    const query = new URLSearchParams(params).toString();
    return await ApiUtil.get(`/admin/users${query ? '?' + query : ''}`);
  },

  /**
   * 取得使用者詳情
   * @param {string} id 
   * @returns {Promise<object>}
   */
  async get(id) {
    return await ApiUtil.get(`/admin/users/${id}`);
  },

  /**
   * 建立使用者
   * @param {object} data 
   * @returns {Promise<object>}
   */
  async create(data) {
    return await ApiUtil.post('/admin/users', data);
  },

  /**
   * 更新使用者
   * @param {string} id 
   * @param {object} data 
   * @returns {Promise<object>}
   */
  async update(id, data) {
    return await ApiUtil.put(`/admin/users/${id}`, data);
  },

  /**
   * 刪除使用者
   * @param {string} id 
   * @returns {Promise<void>}
   */
  async delete(id) {
    return await ApiUtil.delete(`/admin/users/${id}`);
  },

  /**
   * 重設密碼
   * @param {string} id 
   * @param {string} newPassword 
   * @returns {Promise<void>}
   */
  async resetPassword(id, newPassword) {
    return await ApiUtil.post(`/admin/users/${id}/reset-password`, { newPassword });
  }
};

/**
 * 表格工具
 */
const TableUtil = {
  /**
   * 建立空狀態 HTML
   * @param {string} message 
   * @param {number} colspan 
   * @returns {string}
   */
  emptyRow(message, colspan = 6) {
    return `<tr><td colspan="${colspan}" style="text-align:center;padding:40px;color:var(--ink-3)">${message}</td></tr>`;
  },

  /**
   * 建立載入中 HTML
   * @param {number} colspan 
   * @returns {string}
   */
  loadingRow(colspan = 6) {
    return this.emptyRow('載入中...', colspan);
  },

  /**
   * 建立錯誤 HTML
   * @param {string} message 
   * @param {number} colspan 
   * @returns {string}
   */
  errorRow(message, colspan = 6) {
    return `<tr><td colspan="${colspan}" style="text-align:center;padding:40px;color:var(--danger)">${message}</td></tr>`;
  }
};

/**
 * 檔案上傳工具
 */
const FileUtil = {
  /**
   * 下載 CSV 範本
   * @param {string} filename 
   * @param {string} content 
   */
  downloadCsv(filename, content) {
    const blob = new Blob(['\ufeff' + content], { type: 'text/csv;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
  },

  /**
   * 下載投票人員範本
   */
  downloadVoterTemplate() {
    const csv = 'employeeNo,name,department\nEMP001,王小明,研發部\nEMP002,李小華,行銷部';
    this.downloadCsv('voter_template.csv', csv);
  },

  /**
   * 讀取檔案為文字
   * @param {File} file 
   * @returns {Promise<string>}
   */
  readAsText(file) {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => resolve(reader.result);
      reader.onerror = () => reject(new Error('讀取檔案失敗'));
      reader.readAsText(file);
    });
  },

  /**
   * 讀取檔案為 Data URL
   * @param {File} file 
   * @returns {Promise<string>}
   */
  readAsDataUrl(file) {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => resolve(reader.result);
      reader.onerror = () => reject(new Error('讀取檔案失敗'));
      reader.readAsDataURL(file);
    });
  }
};

/**
 * 圖表工具
 */
const ChartUtil = {
  /**
   * 繪製甜甜圈圖
   * @param {HTMLCanvasElement} canvas 
   * @param {Array<{name: string, value: number}>} data 
   * @param {Array<string>} colors 
   */
  drawDonut(canvas, data, colors = null) {
    if (!canvas || !canvas.getContext) return;

    const defaultColors = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899', '#06b6d4', '#84cc16'];
    colors = colors || defaultColors;

    const ctx = canvas.getContext('2d');
    const size = Math.min(canvas.parentElement.clientWidth, 280);
    canvas.width = size;
    canvas.height = size;

    const centerX = size / 2;
    const centerY = size / 2;
    const radius = size / 2 - 20;

    const total = data.reduce((sum, item) => sum + item.value, 0);

    if (total === 0) {
      ctx.fillStyle = '#e5e7eb';
      ctx.beginPath();
      ctx.arc(centerX, centerY, radius, 0, 2 * Math.PI);
      ctx.fill();

      ctx.fillStyle = '#9ca3af';
      ctx.font = '14px sans-serif';
      ctx.textAlign = 'center';
      ctx.fillText('尚無資料', centerX, centerY);
      return;
    }

    let startAngle = -Math.PI / 2;

    data.forEach((item, idx) => {
      const pct = item.value / total;
      const endAngle = startAngle + pct * 2 * Math.PI;

      ctx.fillStyle = colors[idx % colors.length];
      ctx.beginPath();
      ctx.moveTo(centerX, centerY);
      ctx.arc(centerX, centerY, radius, startAngle, endAngle);
      ctx.closePath();
      ctx.fill();

      startAngle = endAngle;
    });

    // 中心白圓（甜甜圈效果）
    ctx.fillStyle = '#fff';
    ctx.beginPath();
    ctx.arc(centerX, centerY, radius * 0.5, 0, 2 * Math.PI);
    ctx.fill();
  }
};

/**
 * 驗證工具
 */
const ValidationUtil = {
  /**
   * 驗證必填欄位
   * @param {object} fields 欄位名稱與值的物件
   * @returns {{valid: boolean, message: string}}
   */
  required(fields) {
    for (const [name, value] of Object.entries(fields)) {
      if (!value || (typeof value === 'string' && !value.trim())) {
        return { valid: false, message: `請填寫${name}` };
      }
    }
    return { valid: true, message: '' };
  },

  /**
   * 驗證時間範圍
   * @param {string} startTime 
   * @param {string} endTime 
   * @returns {{valid: boolean, message: string}}
   */
  timeRange(startTime, endTime) {
    if (!startTime || !endTime) {
      return { valid: false, message: '請填寫開始和結束時間' };
    }
    if (new Date(endTime) <= new Date(startTime)) {
      return { valid: false, message: '結束時間必須晚於開始時間' };
    }
    return { valid: true, message: '' };
  },

  /**
   * 驗證密碼
   * @param {string} password 
   * @param {number} minLength 
   * @returns {{valid: boolean, message: string}}
   */
  password(password, minLength = 6) {
    if (!password) {
      return { valid: false, message: '請輸入密碼' };
    }
    if (password.length < minLength) {
      return { valid: false, message: `密碼長度至少 ${minLength} 個字元` };
    }
    return { valid: true, message: '' };
  },

  /**
   * 驗證密碼確認
   * @param {string} password 
   * @param {string} confirmPassword 
   * @returns {{valid: boolean, message: string}}
   */
  confirmPassword(password, confirmPassword) {
    if (password !== confirmPassword) {
      return { valid: false, message: '兩次輸入的密碼不一致' };
    }
    return { valid: true, message: '' };
  }
};


const AdminPage = {
    async requireAuth() {
        try {
            await ApiUtil.get('/admin-auth/me');
            return true;
        } catch (error) {
            sessionStorage.removeItem('admin_data');
            CookieUtil.remove('admin_logged_in');
            window.location.href = 'login.html';
            return false;
        }
    },

    async logout() {
        try {
            await ApiUtil.post('/admin-auth/logout', {});
        } catch (_) {
        }
        sessionStorage.removeItem('admin_data');
        CookieUtil.remove('admin_logged_in');
        window.location.href = 'login.html';
    }
};

function doLogout() {
    return AdminPage.logout();
}

window.doLogout = async function doLogout() {
  await AdminUtil.logout();
};
