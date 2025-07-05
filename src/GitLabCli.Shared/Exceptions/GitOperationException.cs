namespace GitLabCli.Shared.Exceptions;

/// <summary>
/// Git 操作相關例外
/// </summary>
public class GitOperationException : Exception
{
    /// <summary>
    /// 操作類型
    /// </summary>
    public string OperationType { get; }
    
    /// <summary>
    /// 建構函式
    /// </summary>
    /// <param name="operationType">操作類型</param>
    /// <param name="message">錯誤訊息</param>
    public GitOperationException(string operationType, string message) 
        : base(message)
    {
        OperationType = operationType;
    }
    
    /// <summary>
    /// 建構函式
    /// </summary>
    /// <param name="operationType">操作類型</param>
    /// <param name="message">錯誤訊息</param>
    /// <param name="innerException">內部例外</param>
    public GitOperationException(string operationType, string message, Exception innerException) 
        : base(message, innerException)
    {
        OperationType = operationType;
    }
}
