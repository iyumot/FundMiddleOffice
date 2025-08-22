using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Utilities;
using MimeKit;
using Serilog;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace FMO.Schedule;

public class DisclosureFromMailMission : MailMission
{
    public int Interval { get; set; } = 6;

    protected override void SetNextRun()
    {
        NextRun = (LastRun ?? DateTime.Now).AddHours(Interval);
        if (NextRun < DateTime.Now) NextRun = DateTime.Now.AddHours(Interval);
    }

    //public void Test() => WorkOverride();
   
    protected override bool WorkOverride()
    {
        // 获取所有缓存  
        var di = new DirectoryInfo(@$"files\mailcache\{MailName}");
        if (!di.Exists)
        {
            WeakReferenceMessenger.Default.Send(new MissionWorkMessage(Id, "邮件缓存文件夹不存在"));
            Log.Error($"Mission[{Id}] 邮件缓存文件夹不存在");
            return false;
        }

        // 获取所有文件
        var files = di.GetFiles();
        using var db = new MissionDatabase();
        // 排除估值表，加快效率
        var cat = db.GetCollection<MailCategoryInfo>().Query().Where(x => x.Category == MailCategory.ValueSheet && x.IsSealed).Select(x => x.Id).ToList();//.FindAll().Where(x => x.Category.HasFlag(MailCategory.Disclosure)).Select(x => x.Id).ToArray();

        var coll = db.GetCollection<MailMissionRecord>($"dfm_{Id}");
        var worked = coll.FindAll().ExceptBy(cat, x => x.Id).ToArray();

        var work = IgnoreHistory ? files : files.ExceptBy(worked.Select(x => x.Id), x => x.Name).ToArray();

        var log = $"{(IgnoreHistory ? "全部重解析" : $"已解析{worked.Length}个")}\n待处理邮件 {work.Length} 个";

        double unit = 100.0 / work.Length;
        double progress = 0;

        Dictionary<string, int> fundmap;
        using (var mdb = DbHelper.Base())
            fundmap = mdb.GetCollection<Fund>().Query().Select(x => new { x.Id, x.Code }).ToArray().DistinctBy(x => x.Code).ToDictionary(x => x.Code!, x => x.Id);

        Parallel.ForEach(work, f => //)
        //foreach (var f in work.AsEnumerable().Reverse())
        {
            try
            {
                WorkOne(f, fundmap, log);
                coll.Upsert(new MailMissionRecord { Id = f.Name, Time = DateTime.Now });
            }
            catch (Exception ex)
            {
                Log.Error($"TA From Mail {ex}");
            }

            progress += unit;
            WeakReferenceMessenger.Default.Send(new MissionProgressMessage { Id = Id, Progress = progress });
        }
        );


        log += $"完成";

        using (var mdb = new MissionDatabase())
            mdb.GetCollection<MissionRecord>().Insert(new MissionRecord { MissionId = Id, Time = DateTime.Now, Record = log });

        return true;
    }

    private void WorkOne(FileInfo f, Dictionary<string, int> fundmap, string log)
    {
        using MimeMessage mime = LoadMail(f.FullName);

        if (DetermineCategory(mime) switch { MailCategory.Unk or MailCategory.Disclosure => false, _ => true })
            return;


        List<ParsedInfo> reports = [];


        foreach (MimePart item in mime.Attachments)
        {
            var filepath = item.FileName;
            switch (Path.GetExtension(filepath).ToLower())
            {
                case ".zip":
                    using (var ms = Copy(item.Content))
                        HandleZip(filepath, ms, reports, log);
                    continue;

                case ".xlsx":
                case ".doc":
                case ".docx":
                case ".pdf":
                case ".xbrl":
                    using (var ms = Copy(item.Content))
                        HandleRealFile(filepath, ms, reports, log);
                    continue;
            }
        }

        if (reports.Count == 0) return;

        // 合并成报告
        using var db = DbHelper.Base();
        db.BeginTrans();
        foreach (var fund in reports.GroupBy(x => x.Code))
        {
            if (!fundmap.TryGetValue(fund.Key, out int fundId)) continue;

            foreach (var type in fund.AsEnumerable().GroupBy(x => x.Type))
            {
                foreach (var r in type.AsEnumerable().GroupBy(x => x.Date))
                {
                    // 只处理周期报告
                    if (type.Key >= PeriodicReportType.MonthlyReport && type.Key <= PeriodicReportType.AnnualReport)
                    {
                        FundPeriodicReport fp = new FundPeriodicReport { FundId = fundId, FundCode = fund.Key, Type = type.Key, PeriodEnd = r.Key };

                        foreach (var item in r)
                        {
                            switch (Path.GetExtension(item.FileName))
                            {
                                case ".xlsx":
                                    fp.Excel = new SimpleFile { File = FileMeta.Create(item.Stream, item.FileName) };
                                    break;

                                case ".doc":
                                case ".docx":
                                    fp.Word = new SimpleFile { File = FileMeta.Create(item.Stream, item.FileName) };
                                    break;

                                case ".xbrl":
                                    fp.Xbrl = new SimpleFile { File = FileMeta.Create(item.Stream, item.FileName) };
                                    break;

                                case ".pdf":
                                    fp.Pdf = new SimpleFile { File = FileMeta.Create(item.Stream, item.FileName) };
                                    break;
                            }
                            item.Stream.Dispose();
                        }
                        db.GetCollection<FundPeriodicReport>().Upsert(fp);
                        log += $"\n{fund.Key}: {type.Key} {r.Key}";
                    }
                    else if (type.Key == PeriodicReportType.QuarterlyUpdate)
                    {
                        FundQuarterlyUpdate fp = new() { FundId = fundId, FundCode = fund.Key, PeriodEnd = r.Key };
                        foreach (var item in r)
                        {
                            if (Regex.IsMatch(item.FileName, "投资[者人]"))
                                fp.Investor = new SimpleFile { File = FileMeta.Create(item.Stream, item.FileName) };
                            else if (item.FileName.Contains("运行"))
                                fp.Operation = new SimpleFile { File = FileMeta.Create(item.Stream, item.FileName) };

                            item.Stream.Dispose();
                        }
                        db.GetCollection<FundQuarterlyUpdate>().Upsert(fp);
                        log += $"\n{fund.Key}: {type.Key} {r.Key}";
                    }
                }
            }
        }

        db.Commit();
    }

    private MemoryStream Copy(IMimeContent c)
    {
        var ms = new MemoryStream();
        c.DecodeTo(ms);
        ms.Position = 0;
        return ms;
    }

    private void HandleRealFile(string path, Stream stream, List<ParsedInfo> reports, string log)
    {
        // 解析文件名
        // 备案号
        var m = Regex.Match(path, "S[0-9A-Z]{5}", RegexOptions.IgnoreCase);
        if (!m.Success)
        {
            log += $"\n{path} 解析失败，未找到备案号";
            return;
        }
        string code = m.Value;

        m = Regex.Match(path, @"(\d{4})[年\.](\d{1,2})[月\.](\d{1,2})日?|\d{8}", RegexOptions.IgnoreCase);
        if (!m.Success || DateTimeHelper.TryFindDate(m.Value) is not DateOnly date)
        {
            log += $"\n{path} 解析失败，未找到日期";
            return;
        }

        var ms = new MemoryStream();
        stream.CopyTo(ms);

        switch (path)
        {
            case var p when p.Contains("投资者信息"):
                reports.Add(new ParsedInfo(PeriodicReportType.QuarterlyUpdate, code, date, path, ms));
                break;

            case var p when p.Contains("运行信息"):
                reports.Add(new ParsedInfo(PeriodicReportType.QuarterlyUpdate, code, date, path, ms));
                break;

            case var p when p.Contains("月报"):
                reports.Add(new ParsedInfo(PeriodicReportType.MonthlyReport, code, date, path, ms));
                break;

            case var p when p.Contains("季报"):
                reports.Add(new ParsedInfo(PeriodicReportType.QuarterlyReport, code, date, path, ms));
                break;

            case var p when p.Contains("半年报"):
                reports.Add(new ParsedInfo(PeriodicReportType.SemiAnnualReport, code, date, path, ms));
                break;

            case var p when p.Contains("年报"):
                reports.Add(new ParsedInfo(PeriodicReportType.AnnualReport, code, date, path, ms));
                break;
            default:
                break;
        }

    }

    private void HandleZip(string path, Stream stream, List<ParsedInfo> reports, string log)
    {
        using var zip = new ZipArchive(stream);
        foreach (var ent in zip.Entries)
        {
            switch (Path.GetExtension(ent.Name).ToLower())
            {
                case ".zip":
                    HandleZip(ent.Name, ent.Open(), reports, log);
                    continue;

                case ".xlsx":
                case ".doc":
                case ".docx":
                case ".pdf":
                case ".xbrl":
                    HandleRealFile(ent.Name, ent.Open(), reports, log);
                    continue;
            }
        }
    }




    enum ParseType
    {
        InvestorInfo,
        Operation,

        /// <summary>
        /// 每月报告
        /// </summary>
        Monthly,
        /// <summary>
        /// 每季度报告
        /// </summary>
        Quarterly,
        /// <summary>
        /// 半年报告
        /// </summary>
        SemiAnnual,
        /// <summary>
        /// 年度报告
        /// </summary>
        Annual,

    }

    record ParsedInfo(PeriodicReportType Type, string Code, DateOnly Date, string FileName, Stream Stream);
}


