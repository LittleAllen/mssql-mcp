using GitLabCli.Shared.Interfaces;
using GitLabCli.Shared.Models.Requests;
using GitLabCli.Shared.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace GitLabCli.MCP.Server.Controllers;

/// <summary>
/// Git 操作 API 控制器
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class GitController : ControllerBase
{
    private readonly IGitService _gitService;
    private readonly ILogger<GitController> _logger;

    public GitController(IGitService gitService, ILogger<GitController> logger)
    {
        _gitService = gitService;
        _logger = logger;
    }

    /// <summary>
    /// 處理 Push 操作
    /// </summary>
    /// <param name="request">Push 請求</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>Push 操作結果</returns>
    [HttpPost("push")]
    [ProducesResponseType(typeof(ApiResponse<PushResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PushResult>>> ProcessPush([FromBody] PushRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(ApiResponse<object>.CreateError("Push 請求不能為空", "INVALID_REQUEST"));
            }

            if (request.ProjectId <= 0)
            {
                return BadRequest(ApiResponse<object>.CreateError("專案 ID 無效", "INVALID_PROJECT_ID"));
            }

            if (string.IsNullOrEmpty(request.BranchName))
            {
                return BadRequest(ApiResponse<object>.CreateError("分支名稱不能為空", "INVALID_BRANCH_NAME"));
            }

            _logger.LogInformation("收到 Push 請求，專案 ID: {ProjectId}，分支: {BranchName}，Commit 數量: {CommitCount}", 
                request.ProjectId, request.BranchName, request.Commits.Count);
            
            var result = await _gitService.ProcessPushAsync(request, cancellationToken);
            
            return Ok(ApiResponse<PushResult>.CreateSuccess(result, "Push 操作完成"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Push 操作失敗");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                ApiResponse<object>.CreateError($"Push 操作失敗: {ex.Message}", "PUSH_ERROR"));
        }
    }

    /// <summary>
    /// 處理 Pull 操作
    /// </summary>
    /// <param name="request">Pull 請求</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>Pull 操作結果</returns>
    [HttpPost("pull")]
    [ProducesResponseType(typeof(ApiResponse<PullResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PullResult>>> ProcessPull([FromBody] PullRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(ApiResponse<object>.CreateError("Pull 請求不能為空", "INVALID_REQUEST"));
            }

            if (request.ProjectId <= 0)
            {
                return BadRequest(ApiResponse<object>.CreateError("專案 ID 無效", "INVALID_PROJECT_ID"));
            }

            if (string.IsNullOrEmpty(request.BranchName))
            {
                return BadRequest(ApiResponse<object>.CreateError("分支名稱不能為空", "INVALID_BRANCH_NAME"));
            }

            if (string.IsNullOrEmpty(request.LocalPath))
            {
                return BadRequest(ApiResponse<object>.CreateError("本地路徑不能為空", "INVALID_LOCAL_PATH"));
            }

            _logger.LogInformation("收到 Pull 請求，專案 ID: {ProjectId}，分支: {BranchName}，本地路徑: {LocalPath}", 
                request.ProjectId, request.BranchName, request.LocalPath);
            
            var result = await _gitService.ProcessPullAsync(request, cancellationToken);
            
            return Ok(ApiResponse<PullResult>.CreateSuccess(result, "Pull 操作完成"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pull 操作失敗");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                ApiResponse<object>.CreateError($"Pull 操作失敗: {ex.Message}", "PULL_ERROR"));
        }
    }

    /// <summary>
    /// 取得倉庫狀態
    /// </summary>
    /// <param name="repositoryPath">倉庫路徑</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>倉庫狀態</returns>
    [HttpGet("status")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> GetRepositoryStatus([FromQuery] string repositoryPath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(repositoryPath))
            {
                return BadRequest(ApiResponse<object>.CreateError("倉庫路徑不能為空", "INVALID_REPOSITORY_PATH"));
            }

            _logger.LogInformation("收到取得倉庫狀態請求，路徑: {RepositoryPath}", repositoryPath);
            
            var status = await _gitService.GetRepositoryStatusAsync(repositoryPath, cancellationToken);
            
            return Ok(ApiResponse<object>.CreateSuccess(status, "倉庫狀態取得成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得倉庫狀態失敗");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                ApiResponse<object>.CreateError($"取得倉庫狀態失敗: {ex.Message}", "GET_STATUS_ERROR"));
        }
    }

    /// <summary>
    /// 取得本地分支清單
    /// </summary>
    /// <param name="repositoryPath">倉庫路徑</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>分支清單</returns>
    [HttpGet("branches")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> GetLocalBranches([FromQuery] string repositoryPath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(repositoryPath))
            {
                return BadRequest(ApiResponse<object>.CreateError("倉庫路徑不能為空", "INVALID_REPOSITORY_PATH"));
            }

            _logger.LogInformation("收到取得本地分支清單請求，路徑: {RepositoryPath}", repositoryPath);
            
            var branches = await _gitService.GetLocalBranchesAsync(repositoryPath, cancellationToken);
            
            return Ok(ApiResponse<object>.CreateSuccess(branches, "本地分支清單取得成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得本地分支清單失敗");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                ApiResponse<object>.CreateError($"取得本地分支清單失敗: {ex.Message}", "GET_BRANCHES_ERROR"));
        }
    }

    /// <summary>
    /// 切換分支
    /// </summary>
    /// <param name="request">切換分支請求</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>操作結果</returns>
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> CheckoutBranch([FromBody] CheckoutBranchRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(ApiResponse<object>.CreateError("切換分支請求不能為空", "INVALID_REQUEST"));
            }

            if (string.IsNullOrEmpty(request.RepositoryPath))
            {
                return BadRequest(ApiResponse<object>.CreateError("倉庫路徑不能為空", "INVALID_REPOSITORY_PATH"));
            }

            if (string.IsNullOrEmpty(request.BranchName))
            {
                return BadRequest(ApiResponse<object>.CreateError("分支名稱不能為空", "INVALID_BRANCH_NAME"));
            }

            _logger.LogInformation("收到切換分支請求，路徑: {RepositoryPath}，分支: {BranchName}", 
                request.RepositoryPath, request.BranchName);
            
            var success = await _gitService.CheckoutBranchAsync(request.RepositoryPath, request.BranchName, cancellationToken);
            
            return Ok(ApiResponse<object>.CreateSuccess(new { Success = success }, 
                success ? "分支切換成功" : "分支切換失敗"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "切換分支失敗");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                ApiResponse<object>.CreateError($"切換分支失敗: {ex.Message}", "CHECKOUT_ERROR"));
        }
    }

    /// <summary>
    /// 解決衝突
    /// </summary>
    /// <param name="request">解決衝突請求</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>操作結果</returns>
    [HttpPost("resolve-conflicts")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> ResolveConflicts([FromBody] ResolveConflictsRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(ApiResponse<object>.CreateError("解決衝突請求不能為空", "INVALID_REQUEST"));
            }

            if (string.IsNullOrEmpty(request.RepositoryPath))
            {
                return BadRequest(ApiResponse<object>.CreateError("倉庫路徑不能為空", "INVALID_REPOSITORY_PATH"));
            }

            if (request.ConflictFiles == null || !request.ConflictFiles.Any())
            {
                return BadRequest(ApiResponse<object>.CreateError("衝突檔案清單不能為空", "INVALID_CONFLICT_FILES"));
            }

            _logger.LogInformation("收到解決衝突請求，路徑: {RepositoryPath}，策略: {Strategy}，檔案數量: {FileCount}", 
                request.RepositoryPath, request.Strategy, request.ConflictFiles.Count);
            
            var success = await _gitService.ResolveConflictsAsync(request.RepositoryPath, request.ConflictFiles, request.Strategy, cancellationToken);
            
            return Ok(ApiResponse<object>.CreateSuccess(new { Success = success }, 
                success ? "衝突解決成功" : "衝突解決失敗"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "解決衝突失敗");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                ApiResponse<object>.CreateError($"解決衝突失敗: {ex.Message}", "RESOLVE_CONFLICTS_ERROR"));
        }
    }
}

/// <summary>
/// 切換分支請求
/// </summary>
public class CheckoutBranchRequest
{
    /// <summary>
    /// 倉庫路徑
    /// </summary>
    public string RepositoryPath { get; set; } = string.Empty;
    
    /// <summary>
    /// 分支名稱
    /// </summary>
    public string BranchName { get; set; } = string.Empty;
}

/// <summary>
/// 解決衝突請求
/// </summary>
public class ResolveConflictsRequest
{
    /// <summary>
    /// 倉庫路徑
    /// </summary>
    public string RepositoryPath { get; set; } = string.Empty;
    
    /// <summary>
    /// 衝突檔案清單
    /// </summary>
    public List<string> ConflictFiles { get; set; } = new();
    
    /// <summary>
    /// 解決策略
    /// </summary>
    public string Strategy { get; set; } = "manual";
}
