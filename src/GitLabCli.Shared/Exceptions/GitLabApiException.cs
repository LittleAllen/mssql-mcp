namespace GitLabCli.Shared.Exceptions;

/// <summary>
/// GitLab API 相關例外
/// </summary>
public class GitLabApiException : Exception
{
    /// <summary>
    /// HTTP 狀態碼
    /// </summary>
    public int StatusCode { get; }
    
    /// <summary>
    /// 錯誤代碼
    /// </summary>
    public string ErrorCode { get; }
    
    /// <summary>
    /// 建構函式
    /// </summary>
    /// <param name="statusCode">HTTP 狀態碼</param>
    /// <param name="errorCode">錯誤代碼</param>
    /// <param name="message">錯誤訊息</param>
    public GitLabApiException(int statusCode, string errorCode, string message) 
        : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
    
    /// <summary>
    /// 建構函式
    /// </summary>
    /// <param name="statusCode">HTTP 狀態碼</param>
    /// <param name="errorCode">錯誤代碼</param>
    /// <param name="message">錯誤訊息</param>
    /// <param name="innerException">內部例外</param>
    public GitLabApiException(int statusCode, string errorCode, string message, Exception innerException) 
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
}
