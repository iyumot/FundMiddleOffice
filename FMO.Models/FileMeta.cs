namespace FMO.Models;

using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;



/// 使用hardlink来存储，所有文件都存储在hardlink目录下
/// <param name="Name"></param>
/// <param name="Time"></param>
/// <param name="Hash"></param>
public record FileMeta(string Id, string Name, DateTime Time, string Hash)
{

    public bool Exists => File.Exists(@$"hardlink\{Name}");



    public static FileMeta Create(FileInfo fi)
    {
        string hash;
        using (var md5 = MD5.Create())
        using (var stream = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
            hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();

        string id = Guid.NewGuid().ToString();
        var hard = @$"hardlink\{id}";
        if (!File.Exists(hard))
            File.Copy(fi.FullName, hard, true);

        return new FileMeta(id, fi.Name, fi.LastWriteTime, hash);
    }

    public static FileMeta Create(string path) => Create(new FileInfo(path));


    // 导入Windows API函数
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);

    public static bool Create(string sourcePath, string linkPath)
    {
        if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(linkPath))
            return false;

        bool result = CreateHardLink(linkPath, sourcePath, IntPtr.Zero);

        if (!result)
        {
            // 获取详细错误信息
            int errorCode = Marshal.GetLastWin32Error();
            throw new System.ComponentModel.Win32Exception(errorCode);
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


public class SealedFile : SimpleFile
{
    public FileMeta? Sealed { get; set; }
}