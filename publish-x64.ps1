# 配置参数
$SolutionDir = Split-Path $PSScriptRoot -Parent
$OutputDir = Join-Path $SolutionDir "PublishOutput"
$Projects = @(
    "FMO\FundMiddleOffice.csproj",
    "DatabaseViewer\DatabaseViewer.csproj"
    # 添加更多项目路径
)

# 创建输出目录
if (!(Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir | Out-Null
}

Write-Host "================================" -ForegroundColor Green
Write-Host "开始发布所有项目到: $OutputDir" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green

# 发布每个项目
foreach ($projectPath in $Projects) {
    $fullProjectPath = Join-Path $SolutionDir $projectPath
    $projectName = [System.IO.Path]::GetFileNameWithoutExtension($projectPath)
    
    Write-Host "正在发布 $projectName..." -ForegroundColor Yellow
    
    # 执行发布命令
    $publishArgs = "publish", "$fullProjectPath", "-c", "Release", "-o", "$OutputDir", "--no-restore"
    & dotnet @publishArgs
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ $projectName 发布失败！" -ForegroundColor Red
        exit 1
    } else {
        Write-Host "✅ $projectName 发布成功！" -ForegroundColor Green
    }
}

Write-Host "================================" -ForegroundColor Green
Write-Host "🎉 所有项目发布完成！" -ForegroundColor Green
Write-Host "输出位置: $OutputDir" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green

# 可选：打开输出目录
# explorer $OutputDir

Pause