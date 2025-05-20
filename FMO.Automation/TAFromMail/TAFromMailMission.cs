using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using ExcelDataReader;
using MimeKit;
using Serilog;

namespace FMO.Schedule;

/// <summary>
/// 
/// </summary>
public class TAMissionRecord
{
    public required string Id { get; set; }

    public DateTime Time { get; set; }
}

public class TAFromMailMission : Mission
{
    public string? Mail { get; set; }

    public int Interval { get; set; } = 15;

    public bool IgnoreHistory { get; set; }


    protected override void SetNextRun()
    {
        NextRun = (LastRun ?? DateTime.Now).AddMinutes(Interval);
        if (NextRun < DateTime.Now) NextRun = DateTime.Now.AddMinutes(Interval);
    }

    protected override bool WorkOverride()
    {
        // 获取所有缓存 
        var di = new DirectoryInfo(@$"files\mailcache\{Mail}");
        if (!di.Exists)
        {
            Log.Error($"Mission[{Id}] 邮件缓存文件夹不存在");
            return false;
        }

        // 获取所有文件
        var files = di.GetFiles();
        using var db = new MissionDatabase();
        var worked = db.GetCollection<TAMissionRecord>().FindAll();

        var work = IgnoreHistory ? files : files.ExceptBy(worked.Select(x => x.Id), x => x.Name);

        foreach (var f in work)
            WorkOne(f);

        return true;
    }

    private void WorkOne(FileInfo f)
    {
        using var fs = f.OpenRead();
        MimeMessage mime = new MimeMessage(fs);

        // 没有附件
        if (!mime.Attachments.Any()) return;

        foreach (MimePart item in mime.Attachments)
        {
            var filepath = item.FileName;
            if (filepath.ToLower().EndsWith(".zip"))
            {
                using var ms = new MemoryStream();
                item.Content.DecodeTo(ms);
                ZipArchive zip = new ZipArchive(ms);

                foreach (var ent in zip.Entries)
                {
                    if (Regex.IsMatch(ent.Name, ".xls|.xlsx", RegexOptions.IgnoreCase))
                    {
                        using var ss = ent.Open();
                        var mss = new MemoryStream();
                        ss.CopyTo(mss);
                        WorkOnSheet(mss);
                    }
                    else if (Regex.IsMatch(ent.Name, ".pdf", RegexOptions.IgnoreCase))
                    {
                        using var ss = ent.Open();
                        var mss = new MemoryStream();
                        ss.CopyTo(mss);
                        WorkOnPdf(mss);
                    }
                }
            }
            else if (Regex.IsMatch(filepath, ".xls|.xlsx", RegexOptions.IgnoreCase))
            {
                var ms = new MemoryStream();
                item.Content.DecodeTo(ms);

                WorkOnSheet(ms);
            }
            else if (Regex.IsMatch(filepath, ".pdf", RegexOptions.IgnoreCase))
            {
                var ms = new MemoryStream();
                item.Content.DecodeTo(ms);

                WorkOnSheet(ms);
            }
        }





    }

    private void WorkOnPdf(MemoryStream mss)
    {

    }

    private void WorkOnSheet(MemoryStream stream)
    {
        if (stream is null || stream.Length == 0) return ;

        if (stream.CanSeek)
            stream.Seek(0, SeekOrigin.Begin); 

        var reader = ExcelReaderFactory.CreateReader(stream);
        
    }
}
