$ErrorActionPreference = "Stop"

dotnet restore SmartOpsDesk.sln
dotnet build SmartOpsDesk.sln --configuration Release
dotnet run --project tests\SmartOpsDesk.Tests\SmartOpsDesk.Tests.csproj --configuration Release --no-build

Remove-Item -Recurse -Force "$PSScriptRoot\publish", "$PSScriptRoot\release" -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path "$PSScriptRoot\publish", "$PSScriptRoot\release" | Out-Null

dotnet publish SmartOpsDesk.csproj `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:EnableCompressionInSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    --output "$PSScriptRoot\publish\SmartOpsDesk-win-x64"

@"
SmartOpsDesk 使用说明
=====================

1. 解压 SmartOpsDesk-win-x64.zip。
2. 双击 SmartOpsDesk.exe 启动。
3. 登录账号：
   dev / 123456
   pm / 123456
   impl / 123456
   ops / 123456
4. 登录后点击“连接设置”，可填写 Supabase、SQL Server 和 AI 接口配置。

数据默认保存在：
%APPDATA%\SmartOpsDesk

如果 Windows 提示未知发布者，请选择“更多信息”后继续运行。
"@ | Set-Content -Encoding UTF8 "$PSScriptRoot\publish\SmartOpsDesk-win-x64\使用说明.txt"

Compress-Archive -Path "$PSScriptRoot\publish\SmartOpsDesk-win-x64\*" -DestinationPath "$PSScriptRoot\release\SmartOpsDesk-win-x64.zip" -Force

Write-Host "发布完成：$PSScriptRoot\release\SmartOpsDesk-win-x64.zip"
