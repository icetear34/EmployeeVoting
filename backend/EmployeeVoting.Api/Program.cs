using EmployeeVoting.Api.Application.Interfaces;
using EmployeeVoting.Api.Application.Services;
using EmployeeVoting.Api.Controllers;
using EmployeeVoting.Api.Infrastructure.Configuration;
using EmployeeVoting.Api.Infrastructure.Persistence;
using EmployeeVoting.Api.Infrastructure.Persistence.Repositories;
using EmployeeVoting.Api.Infrastructure.Persistence.TypeHandlers;
using EmployeeVoting.Api.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 初始化 Dapper TypeHandlers
DapperTypeHandlers.Initialize();

// 設定 Options Pattern
builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection(AppSettings.SectionName));

// 設定 SQLite 連線
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=EmployeeVoting.db";
builder.Services.AddSingleton<IDbConnectionFactory>(new SqliteConnectionFactory(connectionString));
builder.Services.AddSingleton<DatabaseInitializer>();

// 註冊 Infrastructure Services
builder.Services.AddSingleton<ICaptchaImageGenerator, CaptchaImageGenerator>();

// 註冊 Repositories
builder.Services.AddScoped<IAdminUserRepository, AdminUserRepository>();
builder.Services.AddScoped<ISessionTokenRepository, SessionTokenRepository>();
builder.Services.AddScoped<ICaptchaSessionRepository, CaptchaSessionRepository>();
builder.Services.AddScoped<IVoteActivityRepository, VoteActivityRepository>();

// 註冊 Application Services
builder.Services.AddScoped<ICaptchaService, CaptchaService>();
builder.Services.AddScoped<IAdminAuthService, AdminAuthService>();

// 註冊 Action Filter
builder.Services.AddScoped<EmployeeVoting.Api.Controllers.AdminAuthFilter>();
builder.Services.AddScoped<IVoteActivityService, VoteActivityService>();

// 加入控制器
builder.Services.AddControllers();

// 設定 CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 設定 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "員工投票系統 API", 
        Version = "v1",
        Description = "Employee Voting System API - .NET 8 + Dapper + SQLite"
    });
});

var app = builder.Build();

// 初始化資料庫
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    dbInitializer.Initialize();
    dbInitializer.SeedDefaultAdmin();
}

// 開發環境啟用 Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();

app.Run();
