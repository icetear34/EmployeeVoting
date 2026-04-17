// ═══════════════════════════════════════════════════════════════════════════
// employee.js - 員工端共用 JavaScript
// ═══════════════════════════════════════════════════════════════════════════

/**
 * 員工端工具模組
 */
const EmployeeUtil = {
  /**
   * 檢查員工登入狀態
   * @returns {Promise<object|null>} 員工資料或 null
   */
  async checkAuth() {
    const session = CookieUtil.get('employee_session');
    if (!session) {
      return null;
    }

    try {
      const response = await ApiUtil.get('/employee-auth/me');
      sessionStorage.setItem('employee_data', JSON.stringify(response));
      return response;
    } catch (error) {
      if (error.status === 401) {
        CookieUtil.remove('employee_session');
        sessionStorage.removeItem('employee_data');
        return null;
      }
      // 若網路問題，嘗試使用快取
      const cached = sessionStorage.getItem('employee_data');
      return cached ? JSON.parse(cached) : null;
    }
  },

  /**
   * 取得快取的員工資料
   * @returns {object|null}
   */
  getCachedData() {
    const cached = sessionStorage.getItem('employee_data');
    return cached ? JSON.parse(cached) : null;
  },

  /**
   * 儲存員工資料到快取
   * @param {object} data 
   */
  cacheData(data) {
    sessionStorage.setItem('employee_data', JSON.stringify(data));
  },

  /**
   * 清除員工登入資料
   */
  clearAuth() {
    CookieUtil.remove('employee_session');
    sessionStorage.removeItem('employee_data');
  },

  /**
   * 導向到登入頁
   */
  redirectToLogin() {
    window.location.href = 'login.html';
  },

  /**
   * 導向到投票頁
   */
  redirectToVote() {
    window.location.href = 'vote.html';
  },

  /**
   * 員工登入
   * @param {string} employeeNo 員工編號
   * @param {string} birthDate 生日 (YYYY-MM-DD)
   * @param {string} captchaKey 驗證碼 Key
   * @param {string} captchaCode 驗證碼
   * @returns {Promise<object>} 登入回應
   */
  async login(employeeNo, birthDate, captchaKey, captchaCode) {
    const response = await ApiUtil.post('/employee-auth/login', {
      employeeNo,
      birthDate,
      captchaKey,
      captchaCode
    });

    if (response.token) {
      CookieUtil.set('employee_session', response.token, 1);
    }

    if (response.employee) {
      this.cacheData(response);
    }

    return response;
  },

  /**
   * 員工登出
   * @returns {Promise<void>}
   */
  async logout() {
    try {
      await ApiUtil.post('/employee-auth/logout', {});
    } catch (error) {
      // 忽略登出錯誤
    }
    this.clearAuth();
    this.redirectToLogin();
  },

  /**
   * 批次投票
   * @param {Array<{activityId: string, candidateId: string}>} votes 
   * @returns {Promise<object>}
   */
  async submitVotes(votes) {
    return await ApiUtil.post('/employee-vote/submit-batch', { votes });
  },

  /**
   * 單一投票
   * @param {string} activityId 
   * @param {string} candidateId 
   * @returns {Promise<object>}
   */
  async submitVote(activityId, candidateId) {
    return await ApiUtil.post('/employee-vote/submit', { activityId, candidateId });
  }
};

/**
 * 候選人卡片渲染器
 */
const CandidateRenderer = {
  /**
   * 建立候選人卡片 HTML
   * @param {object} candidate 候選人資料
   * @param {boolean} selected 是否已選擇
   * @returns {string} HTML
   */
  createCardHtml(candidate, selected = false) {
    const avatarContent = candidate.imageUrl
      ? `<img src="${candidate.imageUrl}" alt="${UIUtil.escapeHtml(candidate.name)}">`
      : (candidate.emoji || '👤');

    return `
      <div class="candidate-card${selected ? ' selected' : ''}" data-candidate-id="${candidate.id}">
        <div class="cand-avatar">${avatarContent}</div>
        <div class="cand-info">
          <div class="cand-name">${UIUtil.escapeHtml(candidate.name)}</div>
          ${candidate.description ? `<div class="cand-desc">${UIUtil.escapeHtml(candidate.description)}</div>` : ''}
        </div>
      </div>
    `;
  },

  /**
   * 建立候選人卡片 DOM 元素
   * @param {object} candidate 候選人資料
   * @param {boolean} selected 是否已選擇
   * @param {function} onClick 點擊回調
   * @returns {HTMLElement}
   */
  createCard(candidate, selected = false, onClick = null) {
    const card = document.createElement('div');
    card.className = 'candidate-card' + (selected ? ' selected' : '');
    card.dataset.candidateId = candidate.id;

    const avatarContent = candidate.imageUrl
      ? `<img src="${candidate.imageUrl}" alt="${UIUtil.escapeHtml(candidate.name)}">`
      : (candidate.emoji || '👤');

    card.innerHTML = `
      <div class="cand-avatar">${avatarContent}</div>
      <div class="cand-info">
        <div class="cand-name">${UIUtil.escapeHtml(candidate.name)}</div>
        ${candidate.description ? `<div class="cand-desc">${UIUtil.escapeHtml(candidate.description)}</div>` : ''}
      </div>
    `;

    if (onClick) {
      card.onclick = () => onClick(candidate);
    }

    return card;
  }
};

/**
 * 結果條渲染器
 */
const ResultBarRenderer = {
  /**
   * 建立結果條元素
   * @param {Array<{name: string, percent: number}>} bars 
   * @param {number} totalVotes 總票數 (可選)
   * @returns {HTMLElement}
   */
  create(bars, totalVotes = null) {
    const wrap = document.createElement('div');
    wrap.className = 'result-bar-wrap';

    let headerHtml = '<div class="rb-header"><span>得票佔比</span>';
    if (totalVotes) {
      headerHtml += `<span>共 ${totalVotes} 票</span>`;
    }
    headerHtml += '</div>';

    wrap.innerHTML = headerHtml + bars.map(bar => `
      <div class="rb-row">
        <div class="rb-name" title="${UIUtil.escapeHtml(bar.name)}">${UIUtil.escapeHtml(bar.name)}</div>
        <div class="rb-track">
          <div class="rb-fill" style="width:0%" data-pct="${bar.percent}"></div>
        </div>
        <div class="rb-pct">${bar.percent}%</div>
      </div>
    `).join('');

    return wrap;
  },

  /**
   * 動畫顯示進度條
   * @param {HTMLElement} container 
   * @param {number} delay 延遲毫秒
   */
  animate(container, delay = 60) {
    setTimeout(() => {
      container.querySelectorAll('.rb-fill').forEach(el => {
        el.style.width = el.dataset.pct + '%';
      });
    }, delay);
  },

  /**
   * 從候選人資料計算並建立結果條
   * @param {Array<{name: string, votes: number}>} candidates 
   * @returns {{element: HTMLElement, totalVotes: number}|null}
   */
  createFromCandidates(candidates) {
    const totalVotes = candidates.reduce((sum, c) => sum + (c.votes || 0), 0);
    if (totalVotes === 0) return null;

    const bars = candidates.map(c => ({
      name: c.name,
      percent: Math.round((c.votes || 0) / totalVotes * 100)
    }));

    return {
      element: this.create(bars, totalVotes),
      totalVotes
    };
  }
};

/**
 * 活動狀態判斷
 */
const ActivityStatusUtil = {
  /**
   * 判斷活動是否已結束
   * @param {object} activity 
   * @returns {boolean}
   */
  isEnded(activity) {
    return new Date(activity.endTime) < new Date();
  },

  /**
   * 判斷活動是否已開始
   * @param {object} activity 
   * @returns {boolean}
   */
  isStarted(activity) {
    return new Date(activity.startTime) <= new Date();
  },

  /**
   * 判斷活動是否進行中
   * @param {object} activity 
   * @returns {boolean}
   */
  isActive(activity) {
    const now = new Date();
    return new Date(activity.startTime) <= now && now < new Date(activity.endTime);
  },

  /**
   * 取得活動狀態文字和 CSS class
   * @param {object} activity 
   * @param {boolean} hasVoted 
   * @returns {{text: string, className: string}}
   */
  getStatus(activity, hasVoted = false) {
    const isEnded = this.isEnded(activity);
    
    if (hasVoted) {
      return {
        text: isEnded ? '已結束' : '已投票',
        className: 'badge-voted'
      };
    }
    
    if (isEnded) {
      return {
        text: '已結束',
        className: 'badge-ended'
      };
    }
    
    if (!this.isStarted(activity)) {
      return {
        text: '未開始',
        className: 'badge-pending'
      };
    }
    
    return {
      text: '進行中',
      className: 'badge-active'
    };
  }
};
