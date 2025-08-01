clear
Write-Host ����ʼ��� ����ģ��"


# BuildAndPackageTemplates.ps1
# �����棺���ռ��������Ŀ¼��bin\Release\net9.0\���е� tpl.dll �� .xlsx �ļ�

$Root = Get-Location
$TemplatesDir = Join-Path $Root "Templates"
$OutputDir = Join-Path $Root "deftpl"

if (-not (Test-Path $TemplatesDir)) {
    Write-Error " Templates Ŀ¼������: $TemplatesDir"
    exit 1
}

if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

$Projects = Get-ChildItem -Path $TemplatesDir -Directory

if ($Projects.Count -eq 0) {
    Write-Warning " Templates ��û������Ŀ��"
    exit 0
}

$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
$env:DOTNET_NOLOGO = "1"

foreach ($Project in $Projects) {
    $ProjectName = $Project.Name
    $ProjectPath = $Project.FullName
    $TargetFramework = "net9.0"
    $OutputBinDir = Join-Path $ProjectPath "bin\Release\$TargetFramework"

    Write-Host " ��ʼ������Ŀ: $ProjectName (.NET $TargetFramework, Release)" -ForegroundColor Cyan

    # ���� .csproj
    $CspojFile = Get-ChildItem -Path $ProjectPath -Filter "*.csproj" -File | Select-Object -First 1
    if (-not $CspojFile) {
        Write-Warning " δ�ҵ� .csproj �ļ�������: $ProjectName"
        continue
    }

    # ��������
    $BinDir = Join-Path $ProjectPath "bin"
    $ObjDir = Join-Path $ProjectPath "obj"
    if (Test-Path $BinDir) { Remove-Item $BinDir -Recurse -Force -ErrorAction SilentlyContinue }
    if (Test-Path $ObjDir) { Remove-Item $ObjDir -Recurse -Force -ErrorAction SilentlyContinue }

    # ��������
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

    # ִ�й���
    dotnet @BuildArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Error " ����ʧ��: $ProjectName"
        continue
    }

    # ������Ŀ¼�Ƿ����
    if (-not (Test-Path $OutputBinDir)) {
        Write-Error " �������Ŀ¼������: $OutputBinDir"
        continue
    }

    # ? ֻ�ռ����Ŀ¼�е� tpl.dll �� .xlsx �ļ�
    $DllFile = Join-Path $OutputBinDir "tpl.dll"
    if (-not (Test-Path $DllFile)) {
        Write-Error " δ���� tpl.dll: $DllFile"
        continue
    }

    # ? ֻ�����Ŀ¼�� .xlsx
    $XlsxInOutput = Get-ChildItem -Path $OutputBinDir -Filter "*.xlsx" -File | ForEach-Object { $_.FullName }
    $PackFiles = @($DllFile) + $XlsxInOutput

    if ($PackFiles.Count -eq 1) {
        Write-Warning " ���Ŀ¼��û�� .xlsx �ļ�: $OutputBinDir"
    }

    # ׼��ѹ����
    $ZipFilePath = Join-Path $OutputDir "$ProjectName.zip"
    if (Test-Path $ZipFilePath) { Remove-Item $ZipFilePath -Force }

    $TempDir = Join-Path $env:TEMP "PackTemp_$ProjectName"
    if (Test-Path $TempDir) { Remove-Item $TempDir -Recurse -Force }
    New-Item -ItemType Directory -Path $TempDir -Force | Out-Null

    # �����ļ�������������
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

    # ѹ��
    try {
        Add-Type -AssemblyName System.IO.Compression.FileSystem
        [System.IO.Compression.ZipFile]::CreateFromDirectory($TempDir, $ZipFilePath)
        Write-Host " ����ɹ�: $ZipFilePath" -ForegroundColor Green
    }
    catch {
        Write-Error " ѹ��ʧ��: $_"
    }
    finally {
        Remove-Item $TempDir -Recurse -Force -ErrorAction SilentlyContinue
    }

    Start-Sleep -Milliseconds 200
}

Write-Host " ������Ŀ�����ɣ����Ŀ¼: $OutputDir" -ForegroundColor Yellow