param(
    [switch]$NoBuild,
    [switch]$Verbose
)

clear
Write-Host "开始打包" -ForegroundColor Cyan

$SolutionRoot = Get-Location
$NexusRoot = "D:\fmo"

$ProjectsToPublish = @(
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
        "--framework", "net10.0-windows"
        "--runtime", $Runtime
        "--self-contained", "false"
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

    if ($Verbose) {
        $args += "--verbosity"; $args += "minimal"
    }

    try {
        # 直接调用 dotnet，捕获标准输出和错误输出
        $output = & dotnet @args 2>&1

        # 获取最后的退出码
        $exitCode = $LASTEXITCODE

        # 将输出合并为字符串用于匹配
        $outputStr = $output -join "`n"

        # 判断是否“项目已是最新”
        if ($outputStr -match "up\s+to\s+date|最新|最新版本") {
            Write-Host "跳过（项目已是最新）" -ForegroundColor Gray
            continue
        }

        # 检查结果
        if ($exitCode -eq 0) {
            Write-Host "成功" -ForegroundColor Green
        } else {
            Write-Host "失败 (退出码: $exitCode)" -ForegroundColor Red
            if ($Verbose) {
                Write-Host $outputStr
            }
        }
    } catch {
        Write-Host "失败 (异常: $($_.Exception.Message))" -ForegroundColor Red
        if ($Verbose) {
            Write-Host ($_.ScriptStackTrace)
        }
    }
}

# ========== 清理空的语言文件夹 ==========
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