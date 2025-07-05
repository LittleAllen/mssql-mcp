namespace GitLabCli.Shared.Tests.TestHelpers;

/// <summary>
/// 測試基礎類別
/// </summary>
public abstract class TestBase
{
    /// <summary>
    /// 測試資料建構器
    /// </summary>
    protected TestDataBuilder TestData => new();
}

/// <summary>
/// 測試資料建構器
/// </summary>
public class TestDataBuilder
{
    /// <summary>
    /// 建立專案資訊測試資料
    /// </summary>
    public ProjectInfoBuilder ProjectInfo() => new();
    
    /// <summary>
    /// 建立分支資訊測試資料
    /// </summary>
    public BranchInfoBuilder BranchInfo() => new();
    
    /// <summary>
    /// 建立提交資訊測試資料
    /// </summary>
    public CommitInfoBuilder CommitInfo() => new();
}
