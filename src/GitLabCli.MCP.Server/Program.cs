using GitLabCli.MCP.Server.Middleware;
using GitLabCli.MCP.Server.Services.Implementations;
using GitLabCli.Shared.Configuration;
using GitLabCli.Shared.Interfaces;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// 配置 Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/gitlabcli-server-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// 配置選項
builder.Services.Configure<GitLabOptions>(
    builder.Configuration.GetSection(GitLabOptions.SectionName));
builder.Services.Configure<McpOptions>(
    builder.Configuration.GetSection(McpOptions.SectionName));
builder.Services.Configure<GitOptions>(
    builder.Configuration.GetSection(GitOptions.SectionName));

// 註冊服務
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

// 註冊自訂服務
builder.Services.AddScoped<IGitLabService, GitLabService>();
builder.Services.AddScoped<IGitService, GitService>();

// 添加控制器和 API 探索
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "GitLab CLI MCP Server API", 
        Version = "v1",
        Description = "基於 MCP (Model Context Protocol) 的 GitLab CLI 工具 Server API"
    });
    
    // 包含 XML 註解
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// 配置 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 配置 HTTP 請求管線
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GitLab CLI MCP Server API v1");
        c.RoutePrefix = string.Empty; // 將 Swagger UI 設為根路徑
    });
}

app.UseCors("AllowAll");
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseRouting();
app.MapControllers();

// 健康檢查端點
app.MapGet("/health", () => new { Status = "Healthy", Timestamp = DateTime.UtcNow });

app.Run();

// 確保在應用程式關閉時清理 Serilog
Log.CloseAndFlush();
