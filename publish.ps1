dotnet restore SmartOpsDesk.sln
dotnet build SmartOpsDesk.sln --configuration Release
dotnet run --project tests/SmartOpsDesk.Tests/SmartOpsDesk.Tests.csproj --configuration Release --no-build
dotnet publish SmartOpsDesk.csproj --configuration Release --runtime win-x64 --self-contained false --output publish
Write-Host "发布完成：$PSScriptRoot\publish"
