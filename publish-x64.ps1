clear
Write-Host “开始打包"
# publish.ps1
# 静默发布：只输出成功或失败
# 发布项目到 E:\nexus\x64 和 E:\nexus\x86

$SolutionRoot = Get-Location
$NexusRoot = "E:\nexus"

# 要发布的项目列表
$ProjectsToPublish = @(

    "FMO.PeiXunAssist",
    "DatabaseViewer",
    "FMO.TemplateManager",
    "FMO.LearnAssist" ,
   "FundMiddleOffice"
)

# 目标平台映射
$Runtimes = @(
    @{ Runtime = "win-x64"; Folder = "x64" }    #@{ Runtime = "win-x86"; Folder = "x86" }
)

# 创建输出目录
$OutputDirs = $Runtimes | ForEach-Object { Join-Path $NexusRoot $_.Folder }
foreach ($dir in $OutputDirs) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir | Out-Null
    }
}

# 查找所有项目文件
$AllProjects = Get-ChildItem -Path $SolutionRoot -Recurse -Include *.csproj, *.vbproj

# 遍历每个项目
foreach ($projectName in $ProjectsToPublish) {
    $projectFile = $AllProjects | Where-Object { $_.BaseName -eq $projectName }

    if (-not $projectFile) {
        Write-Host "?? 未找到项目: $projectName" -ForegroundColor Yellow
        continue
    }

    $projectPath = $projectFile.FullName
    $projectDir = $projectFile.DirectoryName

    foreach ($rt in $Runtimes) {
        $runtime = $rt.Runtime
        $publishPath = Join-Path $NexusRoot $rt.Folder
        Write-Host " $projectName ($runtime) ... " -NoNewline

        # 构造命令行参数
        $args = @(
            "publish", $projectPath,
            "--framework", "net9.0-windows",
            "--runtime", $runtime,
            "--self-contained", "true",
            "--output", $publishPath,
            "-c", "Release",
            "--nologo",
            "-p:PublishSingleFile=false",
            "-p:IncludeNativeLibrariesForSelfExtract=true",
            "-p:DebugType=None",
            "-p:DebugSymbols=false",
            "-p:SupportedCultures=zh-Hans",
            "-p:SatelliteResourceLanguages=zh-Hans"
        )

        try {
            # 使用 Start-Process 完全隔离进程，彻底屏蔽输出
            $process = Start-Process -FilePath "dotnet" -ArgumentList $args `
                -WorkingDirectory $projectDir `
                -Wait `
                -WindowStyle Hidden `
                -PassThru

            if ($process.ExitCode -eq 0) {
                Write-Host " 成功" -ForegroundColor Green
            } else {
                Write-Host " 失败" -ForegroundColor Red
            }
        } catch {
            Write-Host "? 异常" -ForegroundColor Red
        }
    }
}

# ========== 发布全部完成后，统一清理空语言文件夹 ==========
Write-Host "?? 正在清理 x64 和 x86 中的空语言文件夹..." -ForegroundColor Cyan

$languageFolders = @("zh-Hans", "zh-CN", "en", "fr", "de", "ja", "ko", "ru", "es", "it", "pt", "tr", "cs", "pl", "pt-BR")
$outputSubDirs = @("x64", "x86")

foreach ($subDir in $outputSubDirs) {
    $rootDir = Join-Path $NexusRoot $subDir
    if (-not (Test-Path $rootDir)) { continue }

    foreach ($lang in $languageFolders) {
        $folder = Join-Path $rootDir $lang
        if (Test-Path $folder) {
            try {
                $items = Get-ChildItem $folder -ErrorAction SilentlyContinue
                if ($items.Count -eq 0) {
                    Remove-Item $folder -Recurse -Force
                    Write-Host "? 已删除空文件夹: $subDir\$lang" -ForegroundColor Gray
                }
            } catch {
                Write-Host "?? 无法处理文件夹: $folder" -ForegroundColor Yellow
            }
        }
    }
}

Write-Host " 发布完成！" -ForegroundColor Yellow
 