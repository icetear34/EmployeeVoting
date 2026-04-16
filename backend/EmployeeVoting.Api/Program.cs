using EmployeeVoting.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 設定 SQLite 連線
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=EmployeeVoting.db";
builder.Services.AddSingleton<IDbConnectionFactory>(new SqliteConnectionFactory(connectionString));
builder.Services.AddSingleton<DatabaseInitializer>();

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
