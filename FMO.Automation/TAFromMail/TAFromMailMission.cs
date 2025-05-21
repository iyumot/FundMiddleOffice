using ExcelDataReader;
using FMO.Models;
using MimeKit;
using Serilog;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

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
            GetField(head, domain);






        }
        else
        {
            //确认函
        }
    }

    private void GetField(object[] head, string domain)
    {
        var r = GetField(head.Select(x => x?.ToString() ?? "").ToList());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="head"></param>
    /// <returns></returns>
    private bool GetField(List<string> head)
    {
        ///产品代码	产品名称	客户名称	业务类型	申请日期	确认日期	确认金额	确认净额	确认份额	确认结果	单位净值	累计净值	手续费	业绩报酬	
        ///归管理人费用	归基金资产费用	归销售机构费用	手续费折扣率	确认总金额（含费）	申请金额	申请份额	剩余份额	销售机构代码	销售机构名称	直销渠道名称
        ///基金账号	交易账号	证件类型	证件号码	客户类型	分红方式	返回信息	账户利息	主产品名称	主产品代码	申请单号	确认流水号

        FieldIndex idx = new();
        bool failed = false;
        // 主产品
        idx.MainName = GetFieldByRegex(head, "主产品名称", ref failed);
        idx.MainCode = GetFieldByRegex(head, "主产品代码", ref failed);
        failed = false;

        idx.Name = GetFieldByRegex(head, "产品名称", ref failed);
        idx.Code = GetFieldByRegex(head, "产品代码", ref failed);

        idx.Type = GetFieldByRegex(head, "业务类型", ref failed);

        idx.RequestDate = GetFieldByRegex(head, "申请日期", ref failed);
        idx.RequestAmount = GetFieldByRegex(head, "申请金额", ref failed);
        idx.RequestShare = GetFieldByRegex(head, "申请份额", ref failed);

        idx.ConfirmDate = GetFieldByRegex(head, "确认日期", ref failed);
        idx.ConfirmShare = GetFieldByRegex(head, "确认份额", ref failed);
        idx.ConfirmNetAmount = GetFieldByRegex(head, "确认净额", ref failed);
        idx.ConfirmAmount = GetFieldByRegex(head, "确认金额", ref failed);

        idx.Type = GetFieldByRegex(head, "手续费", ref failed);
        idx.Type = GetFieldByRegex(head, "业绩报酬", ref failed);

        idx.Type = GetFieldByRegex(head, "证件号码", ref failed);
        idx.Type = GetFieldByRegex(head, "客户名称", ref failed);

        idx.Type = GetFieldByRegex(head, "申请单号", ref failed);
        idx.Type = GetFieldByRegex(head, "确认流水号", ref failed);




        return !failed;
    }

    private int GetFieldByRegex(List<string> head, string regex, ref bool failed)
    {
        var id = -1;
        var sel = head.Where(x => Regex.IsMatch(x, regex)).ToArray();
        if (sel.Length == 1)
        {
            id = head.IndexOf(sel[0]);
            head.RemoveAt(id);
        }
        else
        {
            Log.Error($"TA 表头解析异常，多个匹配 {regex} {string.Join(',', head)}");
            failed = true;
        }
        return id;
    }


    internal struct FieldIndex
    {
        public FieldIndex()
        {
        }

        public int Code { get; set; } = -1;

        public int Name { get; set; } = -1;

        public int MainCode { get; set; } = -1;

        public int MainName { get; set; } = -1;

        public int Type { get; set; } = -1;

        public int RequestDate { get; set; } = -1;

        public int ConfirmDate { get; set; } = -1;

        public int RequestShare { get; set; } = -1;

        public int RequestAmount { get; set; } = -1;

        public int ConfirmShare { get; set; } = -1;

        public int ConfirmAmount { get; set; } = -1;

        public int ConfirmNetAmount { get; set; } = -1;

        public int Fee { get; set; } = -1;

        public int PerformanceFee { get; set; } = -1;

        public int Saler { get; set; } = -1;

        public int Identity { get; set; } = -1;

        public int RequestId { get; set; } = -1;

        public int ConfirmId { get; set; } = -1;

    }
}


public class FieldInfo<TE, TP>
{

    public required string Header { get; set; }

    public int Index { get; set; } = -1;

    public required Func<object, TP> GetValue { get; set; }

    public required Action<TE, TP> SetValue { get; set; }

    public FieldInfo()
    {
    }

    [SetsRequiredMembers]
    public FieldInfo(string header, Func<object, TP> getValue, Action<TE, TP> setValue)
    {
        Header = header;
        GetValue = getValue;
        SetValue = setValue;
    }
}

public class FieldGather<T>
{



}



public class TAFieldGather : FieldGather<TransferRecord>
{
    public FieldInfo<TransferRecord, string?> FundName { get; } = new("主产品名称", o => o switch { string s => s, _ => o.ToString() }, (x, y) => x.FundName = y);









    public bool CheckField(List<string> fields)
    {
        FundName.Index = fields.IndexOf(FundName.Header);





    }

}