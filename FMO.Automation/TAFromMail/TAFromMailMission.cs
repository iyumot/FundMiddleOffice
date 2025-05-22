using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using ExcelDataReader;
using FMO.Models;
using FMO.Utilities;
using MimeKit;
using Serilog;

namespace FMO.Schedule;

/// <summary>
/// 
/// </summary>
public class MailMissionRecord
{
    public required string Id { get; set; }

    public DateTime Time { get; set; }
    public bool HasError { get; internal set; }
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
        var coll = db.GetCollection<MailMissionRecord>(nameof(TAFromMailMission));
        var worked = coll.FindAll().ToArray();

        var work = IgnoreHistory ? files : files.ExceptBy(worked.Select(x => x.Id), x => x.Name);

        foreach (var f in work)
        {
            try
            {
                WorkOne(f);
                coll.Insert(new MailMissionRecord { Id = f.Name, Time = DateTime.Now });
            }
            catch (Exception ex)
            {
                Log.Error($"TA From Mail {ex}");
            }
        }

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
    public static bool WorkOnSheet(Stream stream, string domain)
    {
        if (stream is null || stream.Length == 0) return false;

        if (stream.CanSeek)
            stream.Seek(0, SeekOrigin.Begin);

        var reader = ExcelReaderFactory.CreateReader(stream);

        reader.Read();
        var head = new object[reader.FieldCount];
        reader.GetValues(head);

        List<TransferRecord> records = new List<TransferRecord>();

        // 列表、确认函
        if (head.Length > 6)
        {
            //列表 
            SheetParser parser = SheetParser.Create(domain);
            var ta = parser.ParseTASheet(reader);
            records.AddRange(ta);
        }
        else
        {
            //确认函
            SheetParser parser = SheetParser.Create(domain);
            var ta = parser.ParseTAConfirm(reader);
            records.AddRange(ta);
        }

        // 更新
        using var db = DbHelper.Base();
        foreach (var rec in records)
        {
            // 校验
            if (rec.Type == TARecordType.UNK || (rec.ConfirmedShare == 0 && rec.ConfirmedAmount == 0))
            {
                Log.Error($"TA Bad Data {rec.PrintProperties()}");
                continue;
            }

            // 通过平台获取的不会被更新
            var old = db.GetCollection<TransferRecord>().FindOne(x => x.ExternalId == rec.ExternalId);

            if (old is null)
                old = db.GetCollection<TransferRecord>().FindOne(x => x.CustomerIdentity == rec.CustomerIdentity && x.ConfirmedDate == rec.ConfirmedDate &&
                            x.FundCode == rec.FundCode && x.ConfirmedShare == rec.ConfirmedShare && x.ConfirmedAmount == rec.ConfirmedAmount);

            if (old is not null && (old.Source != "manual" && !string.IsNullOrWhiteSpace(old.Source)))
                continue;

            rec.Id = old?.Id ?? 0;
            db.GetCollection<TransferRecord>().Upsert(rec);
        }


        return true;
    }



    private static void PostHandle(TransferRecord r)
    {
        // 后处理
        // 对应fund id
        using var db = DbHelper.Base();
        var fund = db.FindFund(r.FundCode);

        // 没找到对应的fund
        if (fund is null)
        {
            Log.Error($"{r.FundName}({r.FundCode}) {r.CustomerName} {r.ConfirmedDate} 未找到对应基金");
            return;
        }

        if (string.IsNullOrWhiteSpace(r.FundName))
        {
            Log.Error($"{r.FundName}({r.FundCode}) {r.CustomerName} {r.ConfirmedDate} 数据异常");
            return;
        }
        r.FundId = fund.Id;

        // 有子份额的
        var m = Regex.Match(r.FundName, @$"{fund.Name}(\w+)");
        if (m.Success)
            r.ShareClass = m.Groups[1].Value;

        r.Source = "Mail";
    }



    public static TAFieldGather[] GetFieldGather(string domain)
    {
        switch (domain)
        {
            case "cstisc.com":
                return [new() { Fee = new("总费用", x => FieldGather.DecimalValue(x), (x, y) => x.Fee = y), ExternalId = new("TA确认号", o => FieldGather.StringValue(o), (x, y) => x.ExternalId = y) }];


        }

        return [new()];
    }





}

