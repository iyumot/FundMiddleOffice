param(
    [switch]$NoBuild,
    [switch]$Verbose
)

clear
Write-Host "开始打包" -ForegroundColor Cyan

$SolutionRoot = Get-Location
$NexusRoot = "D:\fmo"

$ProjectsToPublish = @(
    "FMO.PeiXunAssist",
    "DatabaseViewer",
    "FMO.FeeCalc",
    "FMO.TemplateManager",
    "FMO.LearnAssist",
    "FundMiddleOffice"
)

$Runtime = "win-x64"
$PublishPath = $NexusRoot

# 确保输出目录存在
if (-not (Test-Path $PublishPath)) {
    New-Item -ItemType Directory -Path $PublishPath -Force | Out-Null
}

# 查找所有项目
$AllProjects = Get-ChildItem -Path $SolutionRoot -Recurse -Include *.csproj, *.vbproj

foreach ($projectName in $ProjectsToPublish) {
    $projectFile = $AllProjects | Where-Object { $_.BaseName -eq $projectName }

    if (-not $projectFile) {
        Write-Host "?? 未找到项目: $projectName" -ForegroundColor Yellow
        continue
    }

    $projectPath = $projectFile.FullName
    $projectDir = $projectFile.DirectoryName

    Write-Host "发布 $projectName ($Runtime) ... " -NoNewline

    # 构建参数
    $args = @(
        "publish"
        $projectPath
        "--framework", "net9.0-windows"
        "--runtime", $Runtime
        "--self-contained", "true"
        "--output", $PublishPath
        "-c", "Release"
        "--nologo"
        "-p:PublishSingleFile=false"
        "-p:IncludeNativeLibrariesForSelfExtract=true"
        "-p:DebugType=None"
        "-p:DebugSymbols=false"
        "-p:SupportedCultures=zh-Hans"
        "-p:SatelliteResourceLanguages=zh-Hans"
    )

    if ($NoBuild) {
        $args += "--no-build"
        Write-Host "(无编译) " -ForegroundColor Gray -NoNewline
    }

    # 增加详细日志（可选）
    if ($Verbose) {
        $args += "--verbosity" ; $args += "minimal"  # 或 "normal"
    }

    try {
        # 捕获输出用于判断是否“实际构建”
        $outputLog = New-TemporaryFile

        $process = Start-Process -FilePath "dotnet" -ArgumentList $args `
            -WorkingDirectory $projectDir `
            -Wait `
            -WindowStyle Hidden `
            -PassThru `
            -RedirectStandardOutput $outputLog

        $output = Get-Content $outputLog -Raw

        # 清理临时文件
        Remove-Item $outputLog

        # 分析输出：MSBuild 通常会输出 "项目是最新的" 或 "Project is up to date"
        if ($output -match "up\s+to\s+date|最新|最新版本") {
            Write-Host "跳过（项目已是最新）" -ForegroundColor Gray
            continue
        }

        # 检查退出码
        if ($process.ExitCode -eq 0) {
            Write-Host "成功" -ForegroundColor Green
        } else {
            Write-Host "失败 (退出码: $($process.ExitCode))" -ForegroundColor Red
            if ($Verbose) {
                Write-Host $output
            }
        }
    } catch {
        Write-Host "失败 (异常: $($_.Exception.Message))" -ForegroundColor Red
    }
}

# ========== 清理空语言文件夹 ==========
Write-Host "`n清理空的语言文件夹..." -ForegroundColor Cyan

$languageFolders = @("zh-Hans", "zh-CN", "en", "fr", "de", "ja", "ko", "ru", "es", "it", "pt", "tr", "cs", "pl", "pt-BR")

foreach ($lang in $languageFolders) {
    $folder = Join-Path $PublishPath $lang
    if (Test-Path $folder) {
        try {
            $items = Get-ChildItem $folder -ErrorAction SilentlyContinue
            if ($null -eq $items -or $items.Count -eq 0) {
                Remove-Item $folder -Recurse -Force
                Write-Host "已删除空文件夹: $lang" -ForegroundColor Gray
            }
        } catch {
            Write-Host "无法清理: $folder" -ForegroundColor Yellow
        }
    }
}

Write-Host "`n发布完成！输出路径: $PublishPath" -ForegroundColor Yellow