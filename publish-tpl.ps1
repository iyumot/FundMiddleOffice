clear
Write-Host “开始打包 所有模板"


# BuildAndPackageTemplates.ps1
# 修正版：仅收集构建输出目录（bin\Release\net9.0\）中的 tpl.dll 和 .xlsx 文件

$Root = Get-Location
$TemplatesDir = Join-Path $Root "Templates"
$OutputDir = Join-Path $Root "deftpl"

if (-not (Test-Path $TemplatesDir)) {
    Write-Error " Templates 目录不存在: $TemplatesDir"
    exit 1
}

if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

$Projects = Get-ChildItem -Path $TemplatesDir -Directory

if ($Projects.Count -eq 0) {
    Write-Warning " Templates 中没有子项目。"
    exit 0
}

$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:DOTNET_NOLOGO = "1"

foreach ($Project in $Projects) {
    $ProjectName = $Project.Name
    $ProjectPath = $Project.FullName
    $TargetFramework = "net9.0"
    $OutputBinDir = Join-Path $ProjectPath "bin\Release\$TargetFramework"

    Write-Host " 开始构建项目: $ProjectName (.NET $TargetFramework, Release)" -ForegroundColor Cyan

    # 查找 .csproj
    $CspojFile = Get-ChildItem -Path $ProjectPath -Filter "*.csproj" -File | Select-Object -First 1
    if (-not $CspojFile) {
        Write-Warning " 未找到 .csproj 文件，跳过: $ProjectName"
        continue
    }

    # 清理旧输出
    $BinDir = Join-Path $ProjectPath "bin"
    $ObjDir = Join-Path $ProjectPath "obj"
    if (Test-Path $BinDir) { Remove-Item $BinDir -Recurse -Force -ErrorAction SilentlyContinue }
    if (Test-Path $ObjDir) { Remove-Item $ObjDir -Recurse -Force -ErrorAction SilentlyContinue }

    # 构建参数
    $BuildArgs = @(
        "build"
        $CspojFile.FullName
        "--configuration", "Release"
        "--framework", $TargetFramework
        "--no-self-contained"
        "/p:PublishSingleFile=false"
        "/p:IncludeAllContentForSelfExtract=false"
        "/p:DebugType=None"
        "/p:Optimize=true"
        "/nologo"
        "/v:quiet"
    )

    # 执行构建
    dotnet @BuildArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Error " 构建失败: $ProjectName"
        continue
    }

    # 检查输出目录是否存在
    if (-not (Test-Path $OutputBinDir)) {
        Write-Error " 构建输出目录不存在: $OutputBinDir"
        continue
    }

    # ? 只收集输出目录中的 tpl.dll 和 .xlsx 文件
    $DllFile = Join-Path $OutputBinDir "tpl.dll"
    if (-not (Test-Path $DllFile)) {
        Write-Error " 未生成 tpl.dll: $DllFile"
        continue
    }

    # ? 只从输出目录找 .xlsx
    $XlsxInOutput = Get-ChildItem -Path $OutputBinDir -Filter "*.xlsx" -File | ForEach-Object { $_.FullName }
    $PackFiles = @($DllFile) + $XlsxInOutput

    if ($PackFiles.Count -eq 1) {
        Write-Warning " 输出目录中没有 .xlsx 文件: $OutputBinDir"
    }

    # 准备压缩包
    $ZipFilePath = Join-Path $OutputDir "$ProjectName.zip"
    if (Test-Path $ZipFilePath) { Remove-Item $ZipFilePath -Force }

    $TempDir = Join-Path $env:TEMP "PackTemp_$ProjectName"
    if (Test-Path $TempDir) { Remove-Item $TempDir -Recurse -Force }
    New-Item -ItemType Directory -Path $TempDir -Force | Out-Null

    # 复制文件（避免重名）
    foreach ($file in $PackFiles) {
        $fileName = Split-Path $file -Leaf
        $dest = Join-Path $TempDir $fileName
        $i = 1
        $originalDest = $dest
        while (Test-Path $dest) {
            $ext = [System.IO.Path]::GetExtension($fileName)
            $fn = [System.IO.Path]::GetFileNameWithoutExtension($fileName)
            $dest = Join-Path $TempDir "$fn`_$i$ext"
            $i++
        }
        Copy-Item $file $dest
    }

    # 压缩
    try {
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        [System.IO.Compression.ZipFile]::CreateFromDirectory($TempDir, $ZipFilePath)
        Write-Host " 打包成功: $ZipFilePath" -ForegroundColor Green
    }
    catch {
        Write-Error " 压缩失败: $_"
    }
    finally {
        Remove-Item $TempDir -Recurse -Force -ErrorAction SilentlyContinue
    }

    Start-Sleep -Milliseconds 200
}

Write-Host " 所有项目打包完成！输出目录: $OutputDir" -ForegroundColor Yellow