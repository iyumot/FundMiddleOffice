clear
Write-Host ����ʼ���"
# publish.ps1
# ��Ĭ������ֻ����ɹ���ʧ��
# ������Ŀ�� E:\nexus\x64 �� E:\nexus\x86

$SolutionRoot = Get-Location
$NexusRoot = "E:\nexus"

# Ҫ��������Ŀ�б�
$ProjectsToPublish = @(

    "FMO.PeiXunAssist",
    "DatabaseViewer",
    "FMO.TemplateManager",
    "FMO.LearnAssist" ,
   "FundMiddleOffice"
)

# Ŀ��ƽ̨ӳ��
$Runtimes = @(
    @{ Runtime = "win-x64"; Folder = "x64" }    #@{ Runtime = "win-x86"; Folder = "x86" }
)

# �������Ŀ¼
$OutputDirs = $Runtimes | ForEach-Object { Join-Path $NexusRoot $_.Folder }
foreach ($dir in $OutputDirs) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir | Out-Null
    }
}

# ����������Ŀ�ļ�
$AllProjects = Get-ChildItem -Path $SolutionRoot -Recurse -Include *.csproj, *.vbproj

# ����ÿ����Ŀ
foreach ($projectName in $ProjectsToPublish) {
    $projectFile = $AllProjects | Where-Object { $_.BaseName -eq $projectName }

    if (-not $projectFile) {
        Write-Host "?? δ�ҵ���Ŀ: $projectName" -ForegroundColor Yellow
        continue
    }

    $projectPath = $projectFile.FullName
    $projectDir = $projectFile.DirectoryName

    foreach ($rt in $Runtimes) {
        $runtime = $rt.Runtime
        $publishPath = Join-Path $NexusRoot $rt.Folder
        Write-Host " $projectName ($runtime) ... " -NoNewline

        # ���������в���
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
            # ʹ�� Start-Process ��ȫ������̣������������
            $process = Start-Process -FilePath "dotnet" -ArgumentList $args `
                -WorkingDirectory $projectDir `
                -Wait `
                -WindowStyle Hidden `
                -PassThru

            if ($process.ExitCode -eq 0) {
                Write-Host " �ɹ�" -ForegroundColor Green
            } else {
                Write-Host " ʧ��" -ForegroundColor Red
            }
        } catch {
            Write-Host "? �쳣" -ForegroundColor Red
        }
    }
}

# ========== ����ȫ����ɺ�ͳһ����������ļ��� ==========
Write-Host "?? �������� x64 �� x86 �еĿ������ļ���..." -ForegroundColor Cyan

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
                    Write-Host "? ��ɾ�����ļ���: $subDir\$lang" -ForegroundColor Gray
                }
            } catch {
                Write-Host "?? �޷������ļ���: $folder" -ForegroundColor Yellow
            }
        }
    }
}

Write-Host " ������ɣ�" -ForegroundColor Yellow
 