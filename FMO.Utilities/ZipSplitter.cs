using FMO.Models;
using ICSharpCode.SharpZipLib.Zip;

namespace FMO.Utilities;

public class ZipSplitter
{

    /// <summary>
    /// 分卷压缩
    /// </summary>
    /// <param name="filePaths">文件路径数组</param>
    /// <param name="outputFolder">输出文件夹</param>
    /// <param name="baseFileName">输出基础文件名（不含扩展名）</param>
    /// <param name="splitSizeBytes">每个分卷大小（字节）</param>
    /// <returns>所有生成的分卷文件完整路径列表</returns>
    public static string[] CreateSplitZip(string[] filePaths, string outputFolder, string baseFileName, long splitSizeBytes)
    {
        // 输出目录校验
        if (!Directory.Exists(outputFolder))
            Directory.CreateDirectory(outputFolder);

        var volumePaths = new List<string>();

        // 初始化分卷流
        using var splitStream = new SplitZipStream(outputFolder, baseFileName, splitSizeBytes, volumePaths);
        using var zipStream = new ZipOutputStream(splitStream);
        zipStream.SetLevel(9); // 最优压缩
        zipStream.UseZip64 = UseZip64.Off;

        // 批量添加文件
        foreach (var filePath in filePaths)
        {
            if (!File.Exists(filePath)) continue;

            var entryName = Path.GetFileName(filePath);
            var entry = new ZipEntry(entryName) { DateTime = DateTime.Now };

            zipStream.PutNextEntry(entry);

            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            fs.CopyTo(zipStream);

            zipStream.CloseEntry();
        }

        zipStream.Finish();
        return volumePaths.ToArray();
    }



    public static string[] CreateSplitZip(FileMeta[] files, string outputDir, string baseFileName, long splitSizeBytes)
    {
        // 确保输出目录存在
        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        // 第一个分卷路径
        string firstVolumePath = Path.Combine(outputDir, $"{baseFileName}.01.zip");
        var volumePaths = new List<string>();

        // 分卷流
        using (var splitStream = new SplitZipStream(outputDir, baseFileName, splitSizeBytes, volumePaths))
        using (var zipStream = new ZipOutputStream(splitStream))
        {
            zipStream.SetLevel(9); // 最高压缩
            zipStream.UseZip64 = UseZip64.Off;

            foreach (var file in files)
            {
                if (file is null || !file.Exists) continue;

                string entryName = file.Name;
                var entry = new ZipEntry(entryName)
                {
                    DateTime = DateTime.Now
                };

                zipStream.PutNextEntry(entry);

                using (var fs = file.OpenRead())
                {
                    fs?.CopyTo(zipStream);
                }

                zipStream.CloseEntry();
            }

            zipStream.Finish();

        }

        return volumePaths.ToArray();
    }
}

// 分卷流核心（自动生成 .01.zip / .02.zip ...）
/// <summary>
/// 自动分卷流（自动切割并记录分卷路径）
/// </summary>
public class SplitZipStream : Stream
{
    private readonly string _outputFolder;
    private readonly string _baseFileName;
    private readonly long _maxVolumeSize;
    private readonly List<string> _volumeList;

    private Stream _currentStream = null!; // 解决 CS8618
    private long _currentSize;
    private int _volumeIndex = 1;

    public SplitZipStream(string outputFolder, string baseFileName, long maxVolumeSize, List<string> volumeList)
    {
        _outputFolder = outputFolder ?? throw new ArgumentNullException(nameof(outputFolder));
        _baseFileName = baseFileName ?? throw new ArgumentNullException(nameof(baseFileName));
        _maxVolumeSize = maxVolumeSize;
        _volumeList = volumeList ?? throw new ArgumentNullException(nameof(volumeList));

        CreateNewVolume();
    }

    private void CreateNewVolume()
    {
        var fileName = $"{_baseFileName}.{_volumeIndex:D2}.zip";
        var fullPath = Path.Combine(_outputFolder, fileName);

        _currentStream?.Dispose();
        _currentStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        _currentSize = 0;

        _volumeList.Add(fullPath);
        _volumeIndex++;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        int remaining = count;
        while (remaining > 0)
        {
            long canWrite = _maxVolumeSize - _currentSize;
            if (canWrite <= 0)
            {
                CreateNewVolume();
                canWrite = _maxVolumeSize;
            }

            int writeLen = Math.Min(remaining, (int)canWrite);
            _currentStream.Write(buffer, offset + count - remaining, writeLen);

            _currentSize += writeLen;
            remaining -= writeLen;
        }
    }

    protected override void Dispose(bool disposing)
    {
        _currentStream?.Dispose();
        base.Dispose(disposing);
    }

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => throw new NotSupportedException();
    public override long Position { get; set; }

    public override void Flush() => _currentStream.Flush();
    public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
}