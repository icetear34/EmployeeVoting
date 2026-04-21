/**
 * 員工投票系統 - 共用 JavaScript 工具
 */

// ═══════════════════════════════════════════════════════════════════════════
// API 設定
// ═══════════════════════════════════════════════════════════════════════════
// 前後端整合部署，使用相對路徑
const API_BASE_URL = '/api';

// ═══════════════════════════════════════════════════════════════════════════
// Cookie 工具
// ═══════════════════════════════════════════════════════════════════════════
const CookieUtil = {
  set(name, value, days = 1) {
    const expires = new Date(Date.now() + days * 864e5).toUTCString();
    document.cookie = `${name}=${encodeURIComponent(value)}; expires=${expires}; path=/`;
  },

  get(name) {
    const match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
    return match ? decodeURIComponent(match[2]) : null;
  },

  remove(name) {
    document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/`;
  }
};

// ═══════════════════════════════════════════════════════════════════════════
// API 工具
// ═══════════════════════════════════════════════════════════════════════════
const ApiUtil = {
  async request(endpoint, options = {}) {
    const url = `${API_BASE_URL}${endpoint}`;
    const defaultHeaders = {
      'Content-Type': 'application/json',
    };

    const config = {
      ...options,
      headers: {
        ...defaultHeaders,
        ...options.headers,
      },
      credentials: 'include', // 自動帶上 HttpOnly Cookie
    };

    try {
      const response = await fetch(url, config);
      
      if (response.status === 204) {
        return { success: true };
      }

      const data = await response.json();

      if (!response.ok) {
        throw new ApiError(data.message || '請求失敗', response.status, data);
      }

      return data;
    } catch (error) {
      if (error instanceof ApiError) {
        throw error;
      }
      throw new ApiError('網路連線失敗，請檢查網路狀態', 0, null);
    }
  },

  get(endpoint) {
    return this.request(endpoint, { method: 'GET' });
  },

  post(endpoint, data) {
    return this.request(endpoint, {
      method: 'POST',
      body: JSON.stringify(data),
    });
  },

  put(endpoint, data) {
    return this.request(endpoint, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
  },

  patch(endpoint, data) {
    return this.request(endpoint, {
      method: 'PATCH',
      body: JSON.stringify(data),
    });
  },

  delete(endpoint) {
    return this.request(endpoint, { method: 'DELETE' });
  },

  // 用於上傳檔案（multipart/form-data）
  async upload(endpoint, formData) {
    const url = `${API_BASE_URL}${endpoint}`;

    try {
      const response = await fetch(url, {
        method: 'POST',
        body: formData,
        credentials: 'include',
      });

      const data = await response.json();

      if (!response.ok) {
        throw new ApiError(data.message || '上傳失敗', response.status, data);
      }

      return data;
    } catch (error) {
      if (error instanceof ApiError) {
        throw error;
      }
      throw new ApiError('上傳失敗，請檢查網路狀態', 0, null);
    }
  }
};

// API 錯誤類別
class ApiError extends Error {
  constructor(message, status, data) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.data = data;
  }
}

// ═══════════════════════════════════════════════════════════════════════════
// UI 工具
// ═══════════════════════════════════════════════════════════════════════════
const UIUtil = {
  // 顯示錯誤訊息
  showError(elementOrId, message) {
    const el = typeof elementOrId === 'string' 
      ? document.getElementById(elementOrId) 
      : elementOrId;
    if (el) {
      el.textContent = message;
      el.style.display = 'block';
      el.classList.add('show');
    }
  },

  // 隱藏錯誤訊息
  hideError(elementOrId) {
    const el = typeof elementOrId === 'string' 
      ? document.getElementById(elementOrId) 
      : elementOrId;
    if (el) {
      el.style.display = 'none';
      el.classList.remove('show');
    }
  },

  // 顯示/隱藏載入狀態
  setLoading(buttonEl, isLoading) {
    if (isLoading) {
      buttonEl.disabled = true;
      buttonEl.dataset.originalText = buttonEl.textContent;
      buttonEl.textContent = '處理中...';
    } else {
      buttonEl.disabled = false;
      buttonEl.textContent = buttonEl.dataset.originalText || buttonEl.textContent;
    }
  },

  // 顯示 Modal
  showModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
      modal.classList.add('show');
      document.body.style.overflow = 'hidden';
    }
  },

  // 關閉 Modal
  closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
      modal.classList.remove('show');
      document.body.style.overflow = '';
    }
  },

  // 格式化日期時間
  formatDateTime(dateString, includeTime = true) {
    if (!dateString) return '';
    const date = new Date(dateString);
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    
    if (!includeTime) {
      return `${y}/${m}/${d}`;
    }
    
    const h = String(date.getHours()).padStart(2, '0');
    const min = String(date.getMinutes()).padStart(2, '0');
    return `${y}/${m}/${d} ${h}:${min}`;
  },

  // 格式化為 input datetime-local 格式
  formatForInput(dateString) {
    if (!dateString) return '';
    const date = new Date(dateString);
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    const h = String(date.getHours()).padStart(2, '0');
    const min = String(date.getMinutes()).padStart(2, '0');
    return `${y}-${m}-${d}T${h}:${min}`;
  },

  // 計算活動狀態
  getActivityStatus(startTime, endTime) {
    const now = new Date();
    const start = new Date(startTime);
    const end = new Date(endTime);

    if (now < start) {
      return { status: 'pending', text: '未開始', className: 'badge-pending' };
    } else if (now > end) {
      return { status: 'ended', text: '已結束', className: 'badge-ended' };
    } else {
      return { status: 'active', text: '進行中', className: 'badge-active' };
    }
  },

  // Escape HTML
  escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
  }
};

// ═══════════════════════════════════════════════════════════════════════════
// 驗證碼工具
// ═══════════════════════════════════════════════════════════════════════════
const CaptchaUtil = {
  currentKey: null,
  localCode: null,

  // 從後端取得驗證碼（主要方法）
  async fetch(endpoint = '/admin-auth/captcha') {
    try {
      const data = await ApiUtil.get(endpoint);
      // 後端回傳: { captchaId: "xxx", imageBase64: "data:image/png;base64,..." }
      this.currentKey = data.captchaId;
      return {
        key: this.currentKey,
        imageDataUrl: data.imageBase64
      };
    } catch (error) {
      console.error('取得驗證碼失敗，使用本地模擬:', error);
      // 若後端不可用，使用前端模擬
      return this.generateLocal();
    }
  },

  // 前端模擬驗證碼（開發用）
  generateLocal() {
    const chars = 'ABCDEFGHJKLMNPQRSTUVWXYZ23456789';
    const code = Array.from({ length: 4 }, () => 
      chars[Math.floor(Math.random() * chars.length)]
    ).join('');
    
    this.currentKey = 'local-' + Date.now();
    this.localCode = code;
    
    // 建立簡易 canvas 圖片
    const canvas = document.createElement('canvas');
    canvas.width = 120;
    canvas.height = 40;
    const ctx = canvas.getContext('2d');
    
    // 背景
    ctx.fillStyle = '#f0f0f0';
    ctx.fillRect(0, 0, 120, 40);
    
    // 干擾線
    for (let i = 0; i < 4; i++) {
      ctx.strokeStyle = `hsl(${Math.random() * 360}, 50%, 70%)`;
      ctx.beginPath();
      ctx.moveTo(Math.random() * 120, Math.random() * 40);
      ctx.lineTo(Math.random() * 120, Math.random() * 40);
      ctx.stroke();
    }
    
    // 文字
    ctx.font = 'bold 24px Arial';
    ctx.fillStyle = '#333';
    for (let i = 0; i < code.length; i++) {
      ctx.save();
      ctx.translate(20 + i * 25, 28);
      ctx.rotate((Math.random() - 0.5) * 0.3);
      ctx.fillText(code[i], 0, 0);
      ctx.restore();
    }
    
    return {
      key: this.currentKey,
      imageDataUrl: canvas.toDataURL()
    };
  },

  // 驗證本地驗證碼（開發用）
  verifyLocal(inputCode) {
    return this.localCode && inputCode.toUpperCase() === this.localCode;
  },

  getKey() {
    return this.currentKey;
  }
};

// ═══════════════════════════════════════════════════════════════════════════
// 分頁工具
// ═══════════════════════════════════════════════════════════════════════════
const PaginationUtil = {
  render(containerId, currentPage, totalPages, onPageChange) {
    const container = document.getElementById(containerId);
    if (!container || totalPages <= 1) {
      if (container) container.innerHTML = '';
      return;
    }

    let html = `
      <button class="pagination-btn" ${currentPage <= 1 ? 'disabled' : ''} 
              onclick="${onPageChange}(${currentPage - 1})">上一頁</button>
    `;

    // 頁碼按鈕
    const maxVisible = 5;
    let startPage = Math.max(1, currentPage - Math.floor(maxVisible / 2));
    let endPage = Math.min(totalPages, startPage + maxVisible - 1);
    
    if (endPage - startPage < maxVisible - 1) {
      startPage = Math.max(1, endPage - maxVisible + 1);
    }

    if (startPage > 1) {
      html += `<button class="pagination-btn" onclick="${onPageChange}(1)">1</button>`;
      if (startPage > 2) html += `<span class="pagination-info">...</span>`;
    }

    for (let i = startPage; i <= endPage; i++) {
      html += `<button class="pagination-btn ${i === currentPage ? 'active' : ''}" 
                       onclick="${onPageChange}(${i})">${i}</button>`;
    }

    if (endPage < totalPages) {
      if (endPage < totalPages - 1) html += `<span class="pagination-info">...</span>`;
      html += `<button class="pagination-btn" onclick="${onPageChange}(${totalPages})">${totalPages}</button>`;
    }

    html += `
      <button class="pagination-btn" ${currentPage >= totalPages ? 'disabled' : ''} 
              onclick="${onPageChange}(${currentPage + 1})">下一頁</button>
    `;

    container.innerHTML = html;
  }
};

// ═══════════════════════════════════════════════════════════════════════════
// 匯出全域
// ═══════════════════════════════════════════════════════════════════════════
window.API_BASE_URL = API_BASE_URL;
window.CookieUtil = CookieUtil;
window.ApiUtil = ApiUtil;
window.ApiError = ApiError;
window.UIUtil = UIUtil;
window.CaptchaUtil = CaptchaUtil;
window.PaginationUtil = PaginationUtil;
