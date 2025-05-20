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
                        WorkOnSheet(mss, mime.Sender.Domain);
                    }
                    else if (Regex.IsMatch(ent.Name, ".pdf", RegexOptions.IgnoreCase))
                    {
                        using var ss = ent.Open();
                        var mss = new MemoryStream();
                        ss.CopyTo(mss);
                        WorkOnPdf(mss, mime.Sender.Domain);
                    }
                }
            }
            else if (Regex.IsMatch(filepath, ".xls|.xlsx", RegexOptions.IgnoreCase))
            {
                var ms = new MemoryStream();
                item.Content.DecodeTo(ms);

                WorkOnSheet(ms, mime.Sender.Domain);
            }
            else if (Regex.IsMatch(filepath, ".pdf", RegexOptions.IgnoreCase))
            {
                var ms = new MemoryStream();
                item.Content.DecodeTo(ms);

                WorkOnSheet(ms, mime.Sender.Domain);
            }
        }





    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mss"></param>
    /// <param name="domain">识别托管</param>
    private void WorkOnPdf(MemoryStream mss, string domain)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="domain">识别托管</param>
    private void WorkOnSheet(MemoryStream stream, string domain)
    {
        if (stream is null || stream.Length == 0) return;

        if (stream.CanSeek)
            stream.Seek(0, SeekOrigin.Begin);

        var reader = ExcelReaderFactory.CreateReader(stream);

        reader.Read();
        var head = new object[reader.FieldCount];
        reader.GetValues(head);

        // 列表、确认函
        if (head.Length > 6)
        {
            //列表
           // GetField(head);


             



        }
        else
        {
            //确认函
        }
    }



    internal struct FieldIndex
    {
        public int Code { get; set; }

        public int Name { get; set; }

        public int MainCode { get; set; }

        public int MainName { get; set; }

        public int Type { get; set; }

        public int RequestDate { get; set; }

        public int ConfirmDate { get; set; }

        public int RequestShare { get; set; }

        public int RequestAmount { get; set; }

        public int ConfirmShare { get; set; }

        public int ConfirmAmount { get; set; }
        
        public int ConfirmNetAmount { get; set; }

        public int Fee { get; set; }

        public int PerformanceFee { get; set; }

        public int Saler { get; set; }

        public int Identity { get; set; }

        public int RequestId { get; set; }

        public int ConfirmId { get; set; }

    }
}
