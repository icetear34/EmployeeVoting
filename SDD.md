以下為整合你補充條件後的修正版 SDD。

````markdown id="m7sdd1"
# 員工投票系統 SDD
版本：v0.2  
狀態：Draft  
技術基線：Frontend HTML + CSS + JavaScript / Backend .NET 8 Web API / SQLite

---

# 1. 文件目的

本文件定義「員工投票系統」之軟體設計規格，作為需求確認、系統設計、開發拆分、測試驗收與後續維護之依據。

本系統目標：

1. 提供員工以工號、生日 8 碼、驗證碼登入後進行單次投票。
2. 若員工僅符合一個有效活動，登入後直接進入該活動；若同時符合多個有效活動，則先進入活動選擇頁。
3. 提供管理後台建立投票活動、匯入投票名單、建立候選人、查看開票結果。
4. 前台採純 HTML 頁面，使用 JavaScript 呼叫 API 並渲染內容。
5. 後端以 .NET 8 / C# 建構，資料庫使用 SQLite。
6. 以前端 Cookie 儲存 session 資訊，各頁面先以 Cookie 檢查登入狀態，再由後端做實際驗證。
7. 採 SDD 模式完整定義畫面、流程、資料模型、API、驗證規則與權限。

---

# 2. 系統範圍

## 2.1 In Scope

### 前台
- 員工登入頁
- 員工活動選擇頁
- 驗證碼檢核
- 投票頁
- 已投票結果頁
- Cookie 登入狀態維持
- 前端頁面存取檢查

### 後台
- 管理者登入
- 投票活動管理
- 候選人管理
- 可投票名單匯入
- 開票統計
- 圓餅圖顯示得票比例
- 管理者帳號管理

### 後端
- 員工登入驗證
- 管理者登入驗證
- 驗證碼驗證
- 活動有效性驗證
- 投票名單驗證
- 重複投票防呆
- 候選人資料維護
- 投票資料統計
- Cookie 對應之後端授權驗證

## 2.2 Out of Scope
- LDAP / AD 整合
- Email / 簡訊通知
- 多語系
- 複雜 RBAC 權限模型
- 大量高併發分散式架構
- 第三方登入
- 手機 App
- 匯出 CSV / Excel
- 顯示每位員工是否已投票明細
- HR 主檔生日同步匯入

---

# 3. 名詞定義

| 名詞 | 說明 |
|---|---|
| 投票活動 | 一次完整投票事件，具有名稱、時間範圍、活動代號 |
| 候選人 | 供員工投票選擇的對象 |
| 可投票名單 | 有資格投票的員工清單 |
| 已投票 | 同一活動中該員工已完成投票，不可再次投票 |
| 活動代號 | 系統產生之唯一識別碼 |
| 驗證碼 | 登入頁顯示之圖形或文字驗證碼，用於阻擋機器請求 |
| 有效活動 | 目前時間落在活動起訖範圍內，且該員工在可投票名單中的活動 |

---

# 4. 系統角色

## 4.1 員工使用者
可執行：
- 進入登入頁
- 使用工號、生日、驗證碼登入
- 查看自己符合資格的有效活動
- 選擇活動並進行投票
- 查看自己在該活動是否已投票

不可執行：
- 查看完整票數
- 修改活動
- 建立候選人
- 匯入名單
- 修改管理資料

## 4.2 管理者
可執行：
- 登入後台
- 建立／修改投票活動
- 設定投票起訖時間
- 匯入可投票名單
- 建立／修改候選人資訊
- 查看投票統計與得票圖
- 管理管理者帳號

---

# 5. 使用情境總覽

## 5.1 員工登入與活動導流流程
1. 使用者進入登入頁。
2. 輸入工號、生日 8 碼（yyyymmdd）、驗證碼。
3. 系統驗證登入資料。
4. 後端查詢該工號於目前有效期間內且名單匹配的活動。
5. 若無符合活動，登入失敗並提示無可參與活動。
6. 若僅有一個符合活動，直接建立 session 並進入該活動投票頁。
7. 若有多個符合活動，建立員工登入 session，進入活動選擇頁。
8. 使用者於活動選擇頁選擇其要進入的活動。
9. 系統更新目前活動上下文後進入投票頁。

## 5.2 員工投票流程
1. 員工進入投票頁。
2. 若該活動已投票，僅顯示「已投票」狀態。
3. 若尚未投票，顯示候選人卡片。
4. 使用者點選任一卡片，卡片外框標示為選取狀態。
5. 點擊底部 100% 寬確認投票按鈕。
6. 系統跳出確認視窗：「確定投給 XXX 候選人？」
7. 使用者確認後送出投票。
8. 後端寫入票數並將該使用者標記為已投票。
9. 前台顯示「已完成投票」。

## 5.3 管理者建立活動流程
1. 管理者登入後台。
2. 建立投票活動，輸入名稱、時間範圍。
3. 系統產生活動代號。
4. 管理者進入活動內容頁。
5. 透過頁簽查看：
   - 基本資料
   - 可投票名單
   - 候選人管理
   - 開票頁面
   - 得票圖

## 5.4 匯入可投票名單流程
1. 管理者進入活動的「可投票名單」頁簽。
2. 於大型文字框貼上名單。
3. 每行格式：`工號 空格 生日`
4. 範例：`1233C00 1897/01/02`
5. 系統解析、驗證格式、寫入名單。
6. 顯示匯入成功筆數、失敗筆數、錯誤明細。

---

# 6. 功能需求

---

## 6.1 前台登入頁

### 6.1.1 欄位
- 工號
- 生日 8 碼（yyyymmdd）
- 驗證碼輸入框
- 驗證碼圖片或文字
- 登入按鈕

### 6.1.2 驗證規則
- 工號：必填，不可空白，最大長度建議 20
- 生日：必填，固定 8 碼，只允許數字，格式 `yyyymmdd`
- 驗證碼：必填，需與伺服器發送內容一致
- 工號 + 生日需存在於至少一個有效活動的可投票名單中

### 6.1.3 成功條件
- 驗證成功後建立前台 Cookie：
  - `employee_session`
  - `role=employee`

### 6.1.4 登入後導向規則
- 若僅有 1 個有效活動：直接進入該活動頁面
- 若有 2 個以上有效活動：導向活動選擇頁面
- 若 0 個有效活動：登入失敗，提示目前無可參與投票活動

### 6.1.5 失敗情境
- 驗證碼錯誤
- 工號不存在
- 工號與生日不匹配
- 不在任何有效活動名單中
- 無有效活動
- session 建立失敗

---

## 6.2 前台活動選擇頁

### 6.2.1 顯示條件
- 員工登入成功後，符合兩個以上有效活動

### 6.2.2 顯示內容
- 活動名稱
- 活動代號
- 開始時間
- 結束時間
- 狀態（固定為進行中或可投票）

### 6.2.3 操作規則
- 使用者點擊某活動後，系統設定目前活動上下文
- 設定完成後導向該活動投票頁
- 活動選擇前不得直接進入投票頁

---

## 6.3 前台投票頁

### 6.3.1 畫面規則
- 頁面載入時依 Cookie 檢查登入狀態
- 再由後端 API 驗證 session 合法性
- 顯示活動名稱
- 若已投票，僅顯示已投票畫面
- 若未投票，顯示候選人卡片清單

### 6.3.2 卡片顯示內容
每個候選人卡片包含：
- 圖片
- 姓名
- 介紹文字

### 6.3.3 卡片互動規則
- 點擊卡片任一區域即視為選取
- 同時間只能選取一位候選人
- 被選取卡片需有明顯外框或高亮狀態
- 再次點選其他卡片時，前一張取消選取

### 6.3.4 投票按鈕
- 固定於頁面底部或位於最下方
- 寬度 100%
- 未選擇候選人前不可送出，或點擊後提示「請先選擇候選人」

### 6.3.5 確認視窗
內容：
- 標題：確認投票
- 文字：`確定投給 {候選人姓名}？`
- 按鈕：
  - 取消
  - 確認送出

### 6.3.6 投票送出規則
後端必須再次驗證：
- Session 合法
- 活動仍在投票期間
- 該使用者在可投票名單內
- 尚未投票
- 候選人屬於此活動
- 候選人狀態有效
- 候選人圖片存在且有效

### 6.3.7 送出後結果
- 成功：顯示「投票完成」
- 失敗：顯示對應錯誤訊息

---

## 6.4 已投票頁

### 6.4.1 顯示條件
- 員工於該活動已存在投票紀錄

### 6.4.2 顯示內容
- 活動名稱
- 已完成投票提示
- 不顯示投給哪位候選人
- 不可再次選擇候選人
- 不可再次送出

---

## 6.5 後台管理登入

### 6.5.1 登入方式
- 帳號
- 密碼
- 驗證碼

### 6.5.2 成功後
建立 Cookie：
- `admin_session`
- `role=admin`

### 6.5.3 驗證規則
- 帳號必填
- 密碼必填
- 驗證碼必填
- 後端進行帳號密碼比對

### 6.5.4 密碼儲存規則
- 依目前決策，管理者密碼直接存於資料表，不另加密
- 此為明確規格決策，不再擴充雜湊機制

### 6.5.5 風險說明
此設計安全性低，屬明確接受風險之規格決策。  
系統文件需註明：
- DB 內容一旦外洩，管理者密碼將直接外洩
- IT 直接改表即等同可直接重設密碼
- 正式環境不建議沿用此設計

---

## 6.6 投票活動列表

### 6.6.1 功能
- 顯示所有投票活動
- 可搜尋活動名稱
- 可查看狀態：未開始／進行中／已結束
- 可新增活動
- 可進入活動內容頁

### 6.6.2 欄位
- 活動代號
- 活動名稱
- 開始時間
- 結束時間
- 狀態
- 建立時間
- 操作按鈕

### 6.6.3 新增活動
輸入：
- 活動名稱
- 開始時間
- 結束時間

系統產生：
- 活動代號

### 6.6.4 活動代號規則
建議格式：
`VOTE-YYYYMMDD-XXXX`

例如：
`VOTE-20260416-0001`

規則：
- 唯一
- 不可重複

---

## 6.7 活動內容頁籤

活動內容頁採頁簽形式，至少包含以下頁籤：

1. 基本資料
2. 可投票名單
3. 候選人管理
4. 開票頁面
5. 得票圖

---

## 6.8 基本資料頁籤

### 顯示／可編輯內容
- 活動名稱
- 活動代號（唯讀）
- 開始時間
- 結束時間
- 狀態（系統推導）
- 建立時間（唯讀）
- 建立人（唯讀）

### 修改規則
- 活動尚未開始時可修改基本資料
- 活動進行中允許延長截止時間
- 活動進行中不允許將截止時間縮短到早於目前時間
- 活動進行中不允許將開始時間改到未來
- 活動已結束後不允許再修改起訖時間

---

## 6.9 可投票名單頁籤

### 6.9.1 匯入方式
使用大型文字框，每行一筆：
`工號 空格 生日`

範例：
`1233C00 1897/01/02`

### 6.9.2 解析規則
- 以換行分隔
- 以第一個空白切分工號與生日
- 生日允許格式：
  - `yyyy/MM/dd`
  - `yyyy-MM-dd`
- 匯入後統一轉為 `yyyyMMdd`

### 6.9.3 驗證規則
- 工號不可空白
- 生日需為合法日期
- 同一活動中工號不可重複
- 已有投票紀錄之工號不可直接刪除，除非資料層另行處理

### 6.9.4 顯示內容
- 匯入總筆數
- 成功筆數
- 失敗筆數
- 錯誤列表
- 現有名單清單

### 6.9.5 名單清單欄位
- 工號
- 生日
- 匯入時間

### 6.9.6 不提供功能
- 不提供「是否已投票」明細查詢
- 不提供每位員工投票狀態列表頁

---

## 6.10 候選人管理頁籤

### 6.10.1 候選人欄位
- 圖片
- 姓名
- 介紹
- 排序號
- 啟用狀態

### 6.10.2 建立規則
- 每位候選人屬於單一活動
- 姓名必填
- 介紹可為多行文字
- 圖片必填，不允許空白
- 圖片格式限定 jpg / jpeg / png / webp
- 建議限制檔案大小 2MB 以內

### 6.10.3 修改規則
- 可更新姓名、介紹、排序、啟用狀態
- 可重新上傳圖片
- 不允許移除圖片後存檔
- 若已有投票紀錄，不允許刪除候選人，只能停用

### 6.10.4 顯示規則
- 以前台最終排序為準
- 未啟用候選人不出現在前台

---

## 6.11 開票頁面

### 6.11.1 顯示內容
- 活動總可投票人數
- 已投票人數
- 未投票人數
- 投票率
- 各候選人得票數
- 各候選人得票百分比

### 6.11.2 開票資料列欄位
- 候選人姓名
- 得票數
- 得票比例
- 排名

### 6.11.3 排序
- 依得票數由高到低
- 得票數相同時依建立順序或排序號

### 6.11.4 不提供功能
- 不提供匯出 CSV
- 不提供匯出 Excel

---

## 6.12 得票圖頁籤

### 6.12.1 圖表類型
- 圓餅圖

### 6.12.2 資料來源
- 該活動所有有效投票紀錄

### 6.12.3 顯示內容
- 各候選人名稱
- 各候選人得票佔比
- Tooltip 顯示票數與比例
- 無資料時顯示空狀態

---

## 6.13 管理者帳號管理

### 6.13.1 功能
- 建立管理者
- 修改管理者帳號資訊
- 修改管理者密碼
- 啟用／停用帳號

### 6.13.2 密碼規範
- 必須符合一定複雜度規範
- 建議規則：
  - 長度至少 8 碼
  - 至少 1 碼大寫英文字母
  - 至少 1 碼小寫英文字母
  - 至少 1 碼數字
  - 至少 1 碼特殊字元

### 6.13.3 重設方式
- 不另外提供後台「重設密碼流程」
- 管理者密碼重設由 IT 直接修改資料表處理

---

# 7. 非功能需求

## 7.1 安全性
- 前端 Cookie 僅保存必要 session 識別資訊，不可信任其內容作最終授權判斷
- 所有實際權限與投票資格皆由後端再次驗證
- 員工前台與管理後台登入皆必須使用驗證碼
- API 必須防止重複送票
- 雖管理者密碼不加密為既定規格，但需於系統文件中明列風險

## 7.2 可用性
- 前台畫面簡潔
- 卡片可於桌面與手機顯示
- 錯誤訊息需明確可理解

## 7.3 一致性
- 投票提交需具原子性
- 一次請求只能成功寫入一筆該員工於該活動之投票

## 7.4 效能
- 單活動候選人數通常不高，SQLite 足以支援
- 匯入名單應支援至少數千筆資料
- 開票頁面應可在可接受時間內完成統計

## 7.5 可維護性
- 前後端分離
- 後端分層設計
- API 命名一致
- DB schema 清楚可擴充至多活動、多管理者

---

# 8. 系統架構

## 8.1 整體架構

### Frontend
- HTML
- CSS
- Vanilla JavaScript
- 以 JS 呼叫後端 API 並渲染畫面
- 使用 Cookie 保存登入狀態

### Backend
- ASP.NET Core .NET 8 Web API
- C#
- 驗證、授權、業務邏輯、資料存取
- 圖片上傳與靜態檔案服務

### Database
- SQLite

## 8.2 後端分層建議

1. Presentation
   - Controllers
   - Request / Response DTOs

2. Application
   - Use Cases / Services
   - 業務流程
   - 驗證規則彙整

3. Domain
   - Entities
   - Domain Rules

4. Infrastructure
   - EF Core / SQLite
   - Repository
   - File Storage
   - Captcha Provider

---

# 9. 資料模型設計

## 9.1 VoteActivity
投票活動

| 欄位 | 型別 | 說明 |
|---|---|---|
| Id | Guid | PK |
| ActivityCode | string | 活動代號，唯一 |
| Name | string | 活動名稱 |
| StartTime | datetime | 開始時間 |
| EndTime | datetime | 結束時間 |
| CreatedAt | datetime | 建立時間 |
| CreatedBy | string | 建立人 |
| IsDeleted | bool | 軟刪除 |

---

## 9.2 Candidate
候選人

| 欄位 | 型別 | 說明 |
|---|---|---|
| Id | Guid | PK |
| VoteActivityId | Guid | FK |
| Name | string | 候選人姓名 |
| Description | string | 介紹 |
| ImagePath | string | 圖片路徑，必填 |
| SortOrder | int | 排序 |
| IsEnabled | bool | 啟用狀態 |
| CreatedAt | datetime | 建立時間 |

---

## 9.3 EligibleVoter
可投票名單

| 欄位 | 型別 | 說明 |
|---|---|---|
| Id | Guid | PK |
| VoteActivityId | Guid | FK |
| EmployeeNo | string | 工號 |
| BirthDate | string | 標準化後 yyyyMMdd |
| CreatedAt | datetime | 匯入時間 |

唯一索引建議：
- `(VoteActivityId, EmployeeNo)`

---

## 9.4 VoteRecord
投票紀錄

| 欄位 | 型別 | 說明 |
|---|---|---|
| Id | Guid | PK |
| VoteActivityId | Guid | FK |
| CandidateId | Guid | FK |
| EmployeeNo | string | 工號 |
| VotedAt | datetime | 投票時間 |
| ClientIp | string | 投票來源 IP |
| UserAgent | string | 使用者代理 |

唯一索引建議：
- `(VoteActivityId, EmployeeNo)`

---

## 9.5 AdminUser
管理者帳號

| 欄位 | 型別 | 說明 |
|---|---|---|
| Id | Guid | PK |
| Account | string | 登入帳號 |
| Password | string | 明文密碼 |
| DisplayName | string | 顯示名稱 |
| IsEnabled | bool | 啟用狀態 |
| CreatedAt | datetime | 建立時間 |
| UpdatedAt | datetime | 更新時間 |

唯一索引建議：
- `(Account)`

---

## 9.6 SessionToken
伺服器端 session 管理

| 欄位 | 型別 | 說明 |
|---|---|---|
| Id | Guid | PK |
| SessionToken | string | 唯一 token |
| Role | string | employee/admin |
| EmployeeNo | string | 員工工號，可空 |
| AdminUserId | Guid | 管理者 Id，可空 |
| CurrentVoteActivityId | Guid | 目前選定活動 Id，可空 |
| ExpireAt | datetime | 到期時間 |
| CreatedAt | datetime | 建立時間 |
| IsRevoked | bool | 是否失效 |

備註：
- 員工登入成功後若有多活動可選，先建立 employee session
- 待員工選定活動後再寫入 `CurrentVoteActivityId`

---

## 9.7 CaptchaSession
驗證碼資料

| 欄位 | 型別 | 說明 |
|---|---|---|
| Id | Guid | PK |
| CaptchaId | string | 對外識別碼 |
| Code | string | 驗證值 |
| Purpose | string | employee-login / admin-login |
| ExpireAt | datetime | 到期時間 |
| IsUsed | bool | 是否已使用 |
| CreatedAt | datetime | 建立時間 |

---

# 10. Cookie 與授權設計

## 10.1 員工端 Cookie
建議只存：
- `employee_session`

後端透過 session token 還原：
- employee_no
- role
- current_activity_id

## 10.2 管理端 Cookie
- `admin_session`

## 10.3 前端頁面檢查
- 頁面載入先檢查 Cookie 是否存在
- 若不存在則跳回登入頁
- 若存在則呼叫 `/api/auth/me` 驗證是否合法
- 不合法則清除 Cookie 並跳回登入頁

## 10.4 活動上下文規則
- 員工若有多個有效活動，session 初始不綁定單一活動
- 進入活動選擇頁後，選擇活動才設定 `CurrentVoteActivityId`
- 投票頁需檢查 `CurrentVoteActivityId` 是否存在

---

# 11. 驗證碼設計

## 11.1 基本需求
- 前台登入必須使用驗證碼
- 後台登入必須使用驗證碼
- 使用者輸入後與伺服器儲存值比對

## 11.2 建議方案
- 後端產生隨機碼
- 存於 captcha store 或資料表
- 前端顯示對應圖片或簡易 SVG
- 驗證成功或失敗後即失效
- 重新整理可換一組
- 驗證碼需設定短時效，例如 3~5 分鐘

---

# 12. API 規格草案

---

## 12.1 驗證碼

### 12.1.1 取得員工登入驗證碼
`GET /api/employee-auth/captcha`

Response:
```json id="cap001"
{
  "captchaId": "string",
  "imageBase64": "string"
}
````

### 12.1.2 取得管理者登入驗證碼

`GET /api/admin-auth/captcha`

Response:

```json id="cap002"
{
  "captchaId": "string",
  "imageBase64": "string"
}
```

---

## 12.2 前台認證

### 12.2.1 員工登入

`POST /api/employee-auth/login`

Request:

```json id="empl001"
{
  "employeeNo": "1233C00",
  "birthDate": "18970102",
  "captchaId": "string",
  "captchaCode": "ABCD"
}
```

Response 200：

```json id="empl002"
{
  "sessionToken": "string",
  "employeeNo": "1233C00",
  "activityCount": 2,
  "directEnter": false,
  "currentActivity": null
}
```

單一活動直接進入時：

```json id="empl003"
{
  "sessionToken": "string",
  "employeeNo": "1233C00",
  "activityCount": 1,
  "directEnter": true,
  "currentActivity": {
    "activityId": "guid",
    "activityCode": "VOTE-20260416-0001",
    "activityName": "2026 模範員工票選",
    "hasVoted": false
  }
}
```

### 12.2.2 取得目前登入資訊

`GET /api/employee-auth/me`

Response:

```json id="empl004"
{
  "employeeNo": "1233C00",
  "currentActivity": {
    "activityId": "guid",
    "activityCode": "VOTE-20260416-0001",
    "activityName": "2026 模範員工票選",
    "hasVoted": false
  }
}
```

### 12.2.3 取得可選活動清單

`GET /api/employee-auth/available-activities`

Response:

```json id="empl005"
{
  "activities": [
    {
      "activityId": "guid",
      "activityCode": "VOTE-20260416-0001",
      "activityName": "2026 模範員工票選",
      "startTime": "2026-05-01T09:00:00",
      "endTime": "2026-05-03T18:00:00",
      "hasVoted": false
    }
  ]
}
```

### 12.2.4 選擇活動

`POST /api/employee-auth/select-activity`

Request:

```json id="empl006"
{
  "activityId": "guid"
}
```

Response:

```json id="empl007"
{
  "success": true,
  "activityId": "guid",
  "activityName": "2026 模範員工票選",
  "hasVoted": false
}
```

### 12.2.5 登出

`POST /api/employee-auth/logout`

Response 204

---

## 12.3 前台投票

### 12.3.1 取得候選人列表

`GET /api/employee-vote/candidates`

Response:

```json id="vote001"
{
  "activityCode": "VOTE-20260416-0001",
  "activityName": "2026 模範員工票選",
  "hasVoted": false,
  "candidates": [
    {
      "id": "guid",
      "name": "王小明",
      "description": "介紹文字",
      "imageUrl": "/uploads/candidates/xxx.jpg"
    }
  ]
}
```

### 12.3.2 送出投票

`POST /api/employee-vote/submit`

Request:

```json id="vote002"
{
  "candidateId": "guid"
}
```

Response 200：

```json id="vote003"
{
  "success": true,
  "message": "投票完成"
}
```

---

## 12.4 後台認證

### 12.4.1 管理者登入

`POST /api/admin-auth/login`

Request:

```json id="admin001"
{
  "account": "admin",
  "password": "P@ssw0rd",
  "captchaId": "string",
  "captchaCode": "ABCD"
}
```

Response:

```json id="admin002"
{
  "sessionToken": "string",
  "account": "admin",
  "displayName": "系統管理員"
}
```

### 12.4.2 取得後台登入資訊

`GET /api/admin-auth/me`

### 12.4.3 登出

`POST /api/admin-auth/logout`

---

## 12.5 投票活動管理

### 12.5.1 取得活動列表

`GET /api/admin/vote-activities`

### 12.5.2 建立活動

`POST /api/admin/vote-activities`

Request:

```json id="act001"
{
  "name": "2026 模範員工票選",
  "startTime": "2026-05-01T09:00:00",
  "endTime": "2026-05-03T18:00:00"
}
```

### 12.5.3 取得活動詳情

`GET /api/admin/vote-activities/{id}`

### 12.5.4 修改活動

`PUT /api/admin/vote-activities/{id}`

---

## 12.6 可投票名單管理

### 12.6.1 匯入名單

`POST /api/admin/vote-activities/{id}/eligible-voters/import`

Request:

```json id="list001"
{
  "rawText": "1233C00 1897/01/02\n1233C01 1990/12/31"
}
```

Response:

```json id="list002"
{
  "total": 2,
  "successCount": 2,
  "failedCount": 0,
  "errors": []
}
```

### 12.6.2 查詢名單

`GET /api/admin/vote-activities/{id}/eligible-voters`

---

## 12.7 候選人管理

### 12.7.1 取得候選人列表

`GET /api/admin/vote-activities/{id}/candidates`

### 12.7.2 新增候選人

`POST /api/admin/vote-activities/{id}/candidates`

採 `multipart/form-data`

欄位：

* name
* description
* sortOrder
* isEnabled
* image

規則：

* image 必填

### 12.7.3 修改候選人

`PUT /api/admin/candidates/{candidateId}`

### 12.7.4 停用候選人

`PATCH /api/admin/candidates/{candidateId}/disable`

---

## 12.8 統計與開票

### 12.8.1 開票資料

`GET /api/admin/vote-activities/{id}/result-summary`

Response:

```json id="res001"
{
  "totalEligibleVoters": 100,
  "totalVoted": 65,
  "totalNotVoted": 35,
  "voteRate": 65.0,
  "candidates": [
    {
      "candidateId": "guid",
      "name": "王小明",
      "voteCount": 30,
      "votePercent": 46.15
    }
  ]
}
```

### 12.8.2 得票圖資料

`GET /api/admin/vote-activities/{id}/result-chart`

---

## 12.9 管理者管理

### 12.9.1 管理者列表

`GET /api/admin/users`

### 12.9.2 建立管理者

`POST /api/admin/users`

### 12.9.3 修改管理者

`PUT /api/admin/users/{id}`

### 12.9.4 修改密碼

`PATCH /api/admin/users/{id}/password`

### 12.9.5 啟用／停用帳號

`PATCH /api/admin/users/{id}/status`

---

# 13. API 狀態碼規範

| 狀態碼 | 用途             |
| --- | -------------- |
| 200 | 查詢成功、操作成功      |
| 201 | 建立成功           |
| 204 | 無內容成功，例如登出     |
| 400 | 請求格式錯誤、欄位錯誤    |
| 401 | 未登入或登入失效       |
| 403 | 無權限            |
| 404 | 找不到資源          |
| 409 | 衝突，例如重複投票、重複帳號 |
| 422 | 業務規則不符，例如活動已結束 |
| 500 | 系統未預期錯誤        |

---

# 14. 業務規則

## 14.1 投票規則

1. 同一活動中，一位員工只能投一次。
2. 員工必須存在於該活動可投票名單。
3. 活動必須在有效投票時間內。
4. 候選人必須屬於該活動且為啟用狀態。
5. 候選人必須有圖片。
6. 投票送出後不得修改。
7. 前台不顯示員工具體投給誰。

## 14.2 活動規則

1. 活動代號唯一。
2. 開始時間不得晚於結束時間。
3. 活動進行中允許延長截止時間。
4. 活動進行中不得將結束時間縮短到早於當前時間。
5. 活動已結束後不可修改時間。
6. 有投票紀錄後，不可任意刪除活動。

## 14.3 候選人規則

1. 姓名必填。
2. 圖片必填。
3. 同活動下姓名建議不允許重複。
4. 已有投票紀錄之候選人不得物理刪除，改用停用。

## 14.4 管理者規則

1. 密碼需符合複雜度規範。
2. 密碼直接存資料表，不做額外加密。
3. 重設密碼由 IT 直接修改資料表，不提供額外重設流程。

---

# 15. UI 規格摘要

## 15.1 前台登入頁

元件：

* 工號輸入框
* 生日輸入框
* 驗證碼圖片
* 驗證碼輸入框
* 登入按鈕

## 15.2 前台活動選擇頁

元件：

* 活動卡片或活動列表
* 活動名稱
* 活動代號
* 起訖時間
* 進入按鈕或整列可點選

## 15.3 前台投票頁

元件：

* 活動名稱
* 候選人卡片清單
* 卡片選取狀態
* 底部全寬送出按鈕
* 確認投票彈窗

## 15.4 已投票頁

元件：

* 已投票訊息
* 活動名稱

## 15.5 後台活動頁

元件：

* 活動列表表格
* 新增活動按鈕
* 活動頁籤內容

---

# 16. 前端路由建議

## 員工端

* `/employee/login`
* `/employee/activities`
* `/employee/vote`
* `/employee/voted`

## 管理端

* `/admin/login`
* `/admin/activities`
* `/admin/activities/{id}`
* `/admin/users`

---

# 17. 異常情境設計

## 17.1 員工登入異常

* 驗證碼失效
* 驗證碼錯誤
* 工號不存在
* 生日不符
* 無符合資格之有效活動

## 17.2 活動選擇異常

* 員工選擇了不屬於自己的活動
* 所選活動剛好失效或結束
* session 已過期

## 17.3 投票異常

* session 過期
* 未先選擇活動
* 重複投票
* 非法 candidateId
* 活動已關閉
* 後端寫入失敗

## 17.4 匯入異常

* 空白資料
* 格式錯誤
* 重複工號
* 不合法日期

## 17.5 後台異常

* 驗證碼錯誤
* 帳號重複
* 密碼不符複雜度
* 帳號停用
* 無權限操作

---

# 18. 稽核與紀錄建議

建議記錄以下資訊：

* 管理者登入成功／失敗
* 員工登入成功／失敗
* 名單匯入紀錄
* 候選人異動紀錄
* 活動異動紀錄
* 投票送出時間與來源 IP

注意：

* 不記錄員工明文生日於一般操作 log
* 不記錄驗證碼明文於一般 log
* 不在一般 log 中輸出 session token
* 管理者密碼若採明文儲存，DB 權限需嚴格限制

---

# 19. 測試案例摘要

## 19.1 員工登入

* 正確工號 + 正確生日 + 正確驗證碼
* 錯誤生日
* 錯誤驗證碼
* 非名單內工號
* 無有效活動
* 多活動時正確導向活動選擇頁
* 單活動時直接進入投票頁

## 19.2 活動選擇

* 顯示多筆符合活動
* 選擇合法活動成功
* 選擇非法活動失敗
* 已投票活動仍可進入但顯示已投票頁

## 19.3 投票

* 正常完成投票
* 不選候選人直接送出
* 重複投票
* 非法 candidateId
* session 過期後送出

## 19.4 名單匯入

* 正確資料
* 日期格式混用
* 重複工號
* 空白行
* 不合法日期

## 19.5 候選人管理

* 上傳合法圖片
* 上傳過大圖片
* 缺少姓名
* 缺少圖片
* 停用已有票數候選人

## 19.6 管理者管理

* 建立符合密碼規則帳號
* 建立不符合密碼規則帳號
* 停用管理者後不可登入
* 使用驗證碼錯誤無法登入

---

# 20. 實作決策結論

## 20.1 已定案事項

1. 員工登入後直接依有效活動決定流向：

   * 單一有效活動：直接進入
   * 多個有效活動：進入活動選擇頁
2. 驗證碼前台與後台都需要。
3. 候選人不允許沒有圖片。
4. 活動進行中允許延長截止時間。
5. 管理者重設密碼由 IT 直接修改資料表。
6. 管理者密碼不另加密。
7. 不提供每位員工是否已投票明細清單。
8. 不提供匯出 CSV / Excel。
9. 生日來源暫不考慮 HR 匯入。

## 20.2 風險接受事項

1. 管理者密碼明文存放屬高風險設計。
2. IT 可直接改表，代表系統外部流程必須補足控管。
3. 若未來系統從內網用途轉為正式公開用途，必須重做管理者認證安全機制。

---

# 21. MVP 建議範圍

## 前台

* 員工登入
* 驗證碼
* 活動選擇
* 候選人卡片選擇
* 確認投票
* 已投票頁

## 後台

* 管理者登入
* 驗證碼
* 活動建立
* 候選人建立
* 名單匯入
* 開票表格
* 圓餅圖

## 暫不做

* 匯出報表
* 每人投票明細
* 進階權限
* 忘記密碼流程
* 即時推播

---

# 22. 建議資料夾結構

```text id="tree001"
/src
  /Frontend
    /employee
      login.html
      activities.html
      vote.html
      voted.html
      employee.js
      employee.css
    /admin
      login.html
      activities.html
      activity-detail.html
      users.html
      admin.js
      admin.css

  /Backend
    /Controllers
    /Application
    /Domain
    /Infrastructure
    /Dtos
    /Entities
    /Migrations
    Program.cs
```

---

# 23. 結論

本系統為一套內部用途、快速交付導向的輕量投票系統。
核心設計已明確定案如下：

1. 員工登入後依有效活動數量決定直接進入或先選活動。
2. 前後台皆需驗證碼。
3. 候選人圖片為必填。
4. 活動進行中可延長截止時間。
5. 管理者密碼採明文存表，重設由 IT 直接改表。
6. 不提供投票明細與匯出功能。

此版本已可作為後續拆 API、DB Schema、頁面與開發任務的正式基礎文件。

```

這一版已把你列出的 8 點補充全部併入正式規格。

下一步最合理的是直接往下展成兩份：
1. 開發用任務拆解版
2. API DTO + SQLite 建表 SQL 初稿
```
