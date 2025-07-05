#!/bin/bash

# 測試執行腳本

echo "🧪 開始執行 GitLab CLI MCP 測試套件..."

# 建構專案
echo "📦 建構專案..."
dotnet build --configuration Release

if [ $? -ne 0 ]; then
    echo "❌ 建構失敗"
    exit 1
fi

# 執行單元測試
echo "🔬 執行單元測試..."
dotnet test tests/GitLabCli.Shared.Tests/GitLabCli.Shared.Tests.csproj --configuration Release --logger trx --collect:"XPlat Code Coverage"
dotnet test tests/GitLabCli.MCP.Server.Tests/GitLabCli.MCP.Server.Tests.csproj --configuration Release --logger trx --collect:"XPlat Code Coverage"
dotnet test tests/GitLabCli.MCP.Client.Tests/GitLabCli.MCP.Client.Tests.csproj --configuration Release --logger trx --collect:"XPlat Code Coverage"

# 執行整合測試
echo "🔗 執行整合測試..."
dotnet test tests/GitLabCli.Integration.Tests/GitLabCli.Integration.Tests.csproj --configuration Release --logger trx --collect:"XPlat Code Coverage"

# 產生程式碼覆蓋率報告
echo "📊 產生程式碼覆蓋率報告..."
dotnet tool restore
dotnet tool run reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"TestResults/CoverageReport" -reporttypes:Html

echo "✅ 測試執行完成！"
echo "📈 程式碼覆蓋率報告位於: TestResults/CoverageReport/index.html"
