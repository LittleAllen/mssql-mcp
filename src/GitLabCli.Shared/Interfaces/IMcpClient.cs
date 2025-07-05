using GitLabCli.Shared.Models.Responses;

namespace GitLabCli.Shared.Interfaces;

/// <summary>
/// MCP 客戶端介面
/// </summary>
public interface IMcpClient
{
    /// <summary>
    /// 呼叫 MCP Server API
    /// </summary>
    /// <typeparam name="TRequest">請求類型</typeparam>
    /// <typeparam name="TResponse">回應類型</typeparam>
    /// <param name="endpoint">API 端點</param>
    /// <param name="request">請求資料</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>回應資料</returns>
    Task<ApiResponse<TResponse>> CallApiAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// GET 請求
    /// </summary>
    /// <typeparam name="TResponse">回應類型</typeparam>
    /// <param name="endpoint">API 端點</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>回應資料</returns>
    Task<ApiResponse<TResponse>> GetAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// POST 請求
    /// </summary>
    /// <typeparam name="TRequest">請求類型</typeparam>
    /// <typeparam name="TResponse">回應類型</typeparam>
    /// <param name="endpoint">API 端點</param>
    /// <param name="request">請求資料</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>回應資料</returns>
    Task<ApiResponse<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// PUT 請求
    /// </summary>
    /// <typeparam name="TRequest">請求類型</typeparam>
    /// <typeparam name="TResponse">回應類型</typeparam>
    /// <param name="endpoint">API 端點</param>
    /// <param name="request">請求資料</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>回應資料</returns>
    Task<ApiResponse<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// DELETE 請求
    /// </summary>
    /// <typeparam name="TResponse">回應類型</typeparam>
    /// <param name="endpoint">API 端點</param>
    /// <param name="cancellationToken">取消權杖</param>
    /// <returns>回應資料</returns>
    Task<ApiResponse<TResponse>> DeleteAsync<TResponse>(string endpoint, CancellationToken cancellationToken = default);
}
