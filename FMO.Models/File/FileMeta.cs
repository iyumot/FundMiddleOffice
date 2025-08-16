namespace FMO.Models;

using FMO.Logging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;



/// 使用hardlink来存储，所有文件都存储在hardlink目录下
/// <param name="Name"></param>
/// <param name="Time"></param>
/// <param name="Hash"></param>
public record FileMeta(string Id, string Name, DateTime Time, string Hash)
{

    public bool Exists => File.Exists(@$"files\hardlink\{Id}");


    public static FileMeta Create(FileInfo fi)
    {
        string hash;
        using (var md5 = MD5.Create())
        using (var stream = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();

        string id = Guid.NewGuid().ToString();
        Directory.CreateDirectory("files\\hardlink");
        var hard = @$"files\hardlink\{id}";
        if (!File.Exists(hard))
            File.Copy(fi.FullName, hard, true);

        return new FileMeta(id, fi.Name, fi.LastWriteTime, hash);
    }

    public static FileMeta Create(FileInfo fi, string desireName)
    {
        string hash;
        using (var md5 = MD5.Create())
        using (var stream = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();

        string id = Guid.NewGuid().ToString();
        Directory.CreateDirectory("files\\hardlink");
        var hard = @$"files\hardlink\{id}";
        if (!File.Exists(hard))
            File.Copy(fi.FullName, hard, true);

        // 目录同名
        //var di = new FileInfo(desire).Directory!;
        //if (!di.Exists) di.Create();
        //else desire = GetSafeFileName(di, fi.Name);


        //FileMeta.CreateHardLink(@$"hardlink\{id}", desire);

        return new FileMeta(id, desireName, fi.LastWriteTime, hash);
    }

    public static FileMeta Create(string path, string desireName) => Create(new FileInfo(path), desireName);


    public static string GetSafeFileName(DirectoryInfo di, string filename)
    {
        string name = Path.GetFileNameWithoutExtension(filename), ext = Path.GetExtension(filename);

        for (int i = 0; i < 20; i++)
        {
            var path = Path.Combine(di.Name, i == 0 ? filename : $"{name}（{i + 1}）{ext}");
            if (!System.IO.File.Exists(path))
                return path;
        }
        return Path.Combine(di.Name, $"{name}{DateTime.Now.Ticks.GetHashCode()}{ext}");
    }


    // 导入Windows API函数
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

    public static bool CreateHardLink(string sourcePath, string linkPath)
    {
        if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(linkPath))
            return false;

        var tarfolder = new FileInfo(linkPath).Directory;
        if (tarfolder is not null && !tarfolder.Exists) tarfolder.Create();

        bool result = CreateHardLink(linkPath, sourcePath, IntPtr.Zero);

        if (!result)
        {
            // 获取详细错误信息
            LogEx.Error($"{Marshal.GetLastWin32Error()}");
        }

        return result;
    }

    // Windows API：创建符号链接
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLinkFlags dwFlags);

    [Flags]
    private enum SymbolicLinkFlags
    {
        // 文件软连接
        File = 0,
        // 目录软连接
        Directory = 1
    }

    /// <summary>
    /// 创建文件软连接
    /// </summary>
    public static bool CreateFileLink(string linkPath, string targetPath)
    {
        if (!File.Exists(targetPath))
            return false;

        return CreateSymbolicLink(linkPath, targetPath, SymbolicLinkFlags.File);
    }
}



public class DualFileMeta
{
    public FileMeta? Normal { get; set; }

    public FileMeta? Another { get; set; }
}

public class SimpleFile
{
    public string? Label { get; set; }

    public FileMeta? File { get; set; }
}

public class MultiFile
{
    public string? Label { get; set; }

    public List<FileMeta>? Files { get; set; }
}


public class DualFile : SimpleFile
{
    public FileMeta? Another { get; set; }
}


public class MultiDualFile
{
    public string? Label { get; set; }

    public List<DualFileMeta>? Files { get; set; }

}
