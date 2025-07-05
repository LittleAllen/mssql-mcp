using GitLabCli.Shared.Exceptions;
using GitLabCli.Shared.Models.Responses;
using System.Net;
using System.Text.Json;

namespace GitLabCli.MCP.Server.Middleware;

/// <summary>
/// 全域錯誤處理中介軟體
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "處理請求時發生未預期的錯誤: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            GitLabApiException gitLabEx => new ApiResponse<object>
            {
                Success = false,
                Message = gitLabEx.Message,
                ErrorCode = gitLabEx.ErrorCode,
                Timestamp = DateTime.UtcNow
            },
            GitOperationException gitOpEx => new ApiResponse<object>
            {
                Success = false,
                Message = gitOpEx.Message,
                ErrorCode = $"GIT_{gitOpEx.OperationType.ToUpper()}_ERROR",
                Timestamp = DateTime.UtcNow
            },
            ArgumentNullException argNullEx => new ApiResponse<object>
            {
                Success = false,
                Message = $"必要參數不能為空: {argNullEx.ParamName}",
                ErrorCode = "INVALID_ARGUMENT",
                Timestamp = DateTime.UtcNow
            },
            ArgumentException argEx => new ApiResponse<object>
            {
                Success = false,
                Message = argEx.Message,
                ErrorCode = "INVALID_ARGUMENT",
                Timestamp = DateTime.UtcNow
            },
            _ => new ApiResponse<object>
            {
                Success = false,
                Message = "發生內部伺服器錯誤",
                ErrorCode = "INTERNAL_SERVER_ERROR",
                Timestamp = DateTime.UtcNow
            }
        };

        context.Response.StatusCode = exception switch
        {
            GitLabApiException gitLabEx => gitLabEx.StatusCode,
            ArgumentNullException => (int)HttpStatusCode.BadRequest,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
