using GitLabCli.Shared.Interfaces;
using GitLabCli.Shared.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace GitLabCli.MCP.Server.Controllers;

/// <summary>
/// GitLab 專案管理 API 控制器
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class GitLabController : ControllerBase
{
    private readonly IGitLabService _gitLabService;
    private readonly ILogger<GitLabController> _logger;

    public GitLabController(IGitLabService gitLabService, ILogger<GitLabController> logger)
    {
        _gitLabService = gitLabService;
        _logger = logger;
    }

    /// <summary>
    /// 取得專案清單
    /// </summary>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>專案清單</returns>
    [HttpGet("projects")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<object>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> GetProjects(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("收到取得專案清單請求");
            
            var projects = await _gitLabService.GetProjectsAsync(cancellationToken);
            
            return Ok(ApiResponse<object>.CreateSuccess(projects, "專案清單取得成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得專案清單失敗");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                ApiResponse<object>.CreateError($"取得專案清單失敗: {ex.Message}", "GET_PROJECTS_ERROR"));
        }
    }

    /// <summary>
    /// 取得專案資訊
    /// </summary>
    /// <param name="projectId">專案 ID</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>專案資訊</returns>
    [HttpGet("projects/{projectId:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> GetProject(int projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("收到取得專案資訊請求，專案 ID: {ProjectId}", projectId);
            
            var project = await _gitLabService.GetProjectAsync(projectId, cancellationToken);
            
            return Ok(ApiResponse<object>.CreateSuccess(project, "專案資訊取得成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得專案資訊失敗，專案 ID: {ProjectId}", projectId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                ApiResponse<object>.CreateError($"取得專案資訊失敗: {ex.Message}", "GET_PROJECT_ERROR"));
        }
    }

    /// <summary>
    /// 取得分支清單
    /// </summary>
    /// <param name="projectId">專案 ID</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>分支清單</returns>
    [HttpGet("projects/{projectId:int}/branches")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> GetBranches(int projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("收到取得分支清單請求，專案 ID: {ProjectId}", projectId);
            
            var branches = await _gitLabService.GetBranchesAsync(projectId, cancellationToken);
            
            return Ok(ApiResponse<object>.CreateSuccess(branches, "分支清單取得成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得分支清單失敗，專案 ID: {ProjectId}", projectId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                ApiResponse<object>.CreateError($"取得分支清單失敗: {ex.Message}", "GET_BRANCHES_ERROR"));
        }
    }

    /// <summary>
    /// 取得分支資訊
    /// </summary>
    /// <param name="projectId">專案 ID</param>
    /// <param name="branchName">分支名稱</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>分支資訊</returns>
    [HttpGet("projects/{projectId:int}/branches/{branchName}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> GetBranch(int projectId, string branchName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("收到取得分支資訊請求，專案 ID: {ProjectId}，分支: {BranchName}", projectId, branchName);
            
            var branch = await _gitLabService.GetBranchAsync(projectId, branchName, cancellationToken);
            
            return Ok(ApiResponse<object>.CreateSuccess(branch, "分支資訊取得成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得分支資訊失敗，專案 ID: {ProjectId}，分支: {BranchName}", projectId, branchName);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                ApiResponse<object>.CreateError($"取得分支資訊失敗: {ex.Message}", "GET_BRANCH_ERROR"));
        }
    }

    /// <summary>
    /// 建立分支
    /// </summary>
    /// <param name="projectId">專案 ID</param>
    /// <param name="request">建立分支請求</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>建立的分支資訊</returns>
    [HttpPost("projects/{projectId:int}/branches")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> CreateBranch(int projectId, [FromBody] CreateBranchRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(request.BranchName))
            {
                return BadRequest(ApiResponse<object>.CreateError("分支名稱不能為空", "INVALID_BRANCH_NAME"));
            }

            if (string.IsNullOrEmpty(request.SourceBranch))
            {
                return BadRequest(ApiResponse<object>.CreateError("來源分支不能為空", "INVALID_SOURCE_BRANCH"));
            }

            _logger.LogInformation("收到建立分支請求，專案 ID: {ProjectId}，分支: {BranchName}，來源分支: {SourceBranch}", 
                projectId, request.BranchName, request.SourceBranch);
            
            var branch = await _gitLabService.CreateBranchAsync(projectId, request.BranchName, request.SourceBranch, cancellationToken);
            
            return CreatedAtAction(nameof(GetBranch), new { projectId, branchName = request.BranchName }, 
                ApiResponse<object>.CreateSuccess(branch, "分支建立成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "建立分支失敗，專案 ID: {ProjectId}，分支: {BranchName}", projectId, request.BranchName);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                ApiResponse<object>.CreateError($"建立分支失敗: {ex.Message}", "CREATE_BRANCH_ERROR"));
        }
    }

    /// <summary>
    /// 取得 Commit 清單
    /// </summary>
    /// <param name="projectId">專案 ID</param>
    /// <param name="branchName">分支名稱</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>Commit 清單</returns>
    [HttpGet("projects/{projectId:int}/branches/{branchName}/commits")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> GetCommits(int projectId, string branchName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("收到取得 Commit 清單請求，專案 ID: {ProjectId}，分支: {BranchName}", projectId, branchName);
            
            var commits = await _gitLabService.GetCommitsAsync(projectId, branchName, cancellationToken);
            
            return Ok(ApiResponse<object>.CreateSuccess(commits, "Commit 清單取得成功"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得 Commit 清單失敗，專案 ID: {ProjectId}，分支: {BranchName}", projectId, branchName);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                ApiResponse<object>.CreateError($"取得 Commit 清單失敗: {ex.Message}", "GET_COMMITS_ERROR"));
        }
    }
}

/// <summary>
/// 建立分支請求
/// </summary>
public class CreateBranchRequest
{
    /// <summary>
    /// 分支名稱
    /// </summary>
    public string BranchName { get; set; } = string.Empty;
    
    /// <summary>
    /// 來源分支
    /// </summary>
    public string SourceBranch { get; set; } = string.Empty;
}
