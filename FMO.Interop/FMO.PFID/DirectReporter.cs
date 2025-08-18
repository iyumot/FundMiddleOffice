
#define TEST_PFID

using FMO.Logging;
using FMO.Models;
using FMO.Utilities;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Text.Json;


namespace FMO.AMAC.Direct;


public enum DirectFileType
{
    Unk,

    [Description("私募月报")] PB0001,


    [Description("私募季报")] PB0002,


    [Description("私募年报")] PB0003,


    [Description("私募半年报")] PB0004,



    [Description("私募产品运行表")] RS0001,

    [Description("私募产品清算报告")] RS0002,

    [Description("私募基金年度财务监测报告")] RS0010,

    [Description("量化私募基金运行报表")] RS0030,

    [Description("规模以上证券类管理人报表")] RS0031,

    [Description("不动产私募投资基金监测报表")] RS0050,

    /// <summary>
    /// RS0051, //不动产私募投资基金监测报表（托管人）
    /// </summary>
    ///[Description("不动产私募投资基金监测报表")] RS0051,  
}



public class DirectReporter
{

#if TEST_PFID
    public const string OperationUploadUrl = "http://amrs-test.amac.org.cn:8280/pmg/v1/report/direct-file.do";
    public const string DisclosureUploadUrl = "http://pfid-test.amac.org.cn:8480/pof/v1/report/direct-file.do";


    public const string OperationResultUrl = "http://amrs-test.amac.org.cn:8280/pmg/v1/report/direct-query.do";
    public const string DisclosureResultUrl = "http://pfid-test.amac.org.cn:8480/pof/v1/report/direct-query.do";

    public const string OperationSubmitUrl = "http://amrs-test.amac.org.cn:8280/pmg/v1/report/direct-submit.do";
    public const string DisclosureSubmitUrl = "http://pfid-test.amac.org.cn:8480/pof/v1/report/direct-submit.do";

#elif TEST_PFID2

    public const string OperationUploadUrl = "http://amrs-stage.amac.org.cn:8180/pmg/v1/report/direct-file.do";
    public const string DisclosureUploadUrl = "http://pfid-stage.amac.org.cn:8380/pof/v1/report/direct-file.do";


    public const string OperationResultUrl = "http://amrs-stage.amac.org.cn:8180/pmg/v1/report/direct-query.do";
    public const string DisclosureResultUrl = "http://pfid-stage.amac.org.cn:8380/pof/v1/report/direct-query.do";

    public const string OperationSubmitUrl = "http://amrs-stage.amac.org.cn:8180/pmg/v1/report/direct-submit.do";
    public const string DisclosureSubmitUrl = "http://pfid-stage.amac.org.cn:8380/pof/v1/report/direct-submit.do";

#else
    public const string OperationUploadUrl = "https://amrs.amac.org.cn/pmg/v1/report/direct-file.do";
    public const string DisclosureUploadUrl = "https://pfid.amac.org.cn/pof/v1/report/direct-file.do";


    public const string OperationResultUrl = "https://amrs.amac.org.cn/pmg/v1/report/direct-query.do";
    public const string DisclosureResultUrl = "https://pfid.amac.org.cn/pof/v1/report/direct-query.do";

    public const string OperationSubmitUrl = "https://amrs.amac.org.cn/pmg/v1/report/direct-submit.do";
    public const string DisclosureSubmitUrl = "https://pfid.amac.org.cn/pof/v1/report/direct-submit.do";

#endif

    public static async Task<(bool Success, string? Error)> UploadReport(FundPeriodicReport report, AmacReportAccount acc) => await UploadReport(report, x => x.Excel?.File, acc);
    public static async Task<(bool Success, string? Error)> UploadReport(FundQuarterlyUpdate report, AmacReportAccount acc) => await UploadReport(report, x => x.Operation?.File, acc);

    public static async Task<(bool Success, string? Error)> UploadReport<T>(T report, Func<T, FileMeta?> file, AmacReportAccount acc) where T : IPeriodical
    {
        var type = report.Type switch
        {
            PeriodicReportType.MonthlyReport => DirectFileType.PB0001,
            PeriodicReportType.QuarterlyReport => DirectFileType.PB0002,
            PeriodicReportType.SemiAnnualReport => DirectFileType.PB0004,
            PeriodicReportType.AnnualReport => DirectFileType.PB0003,
            PeriodicReportType.QuarterlyUpdate => DirectFileType.RS0001,
            _ => DirectFileType.Unk
        };

        if (report.FundCode is null) return (false, "基金备案编码为空");
        if (type == DirectFileType.Unk) return (false, "未知的报告类型");
        if (file(report) is not FileMeta fm || !fm.Exists) return (false, "文件不存在");


        // 生成zip
        var path = Path.GetTempFileName();
        try
        {
            using (var archive = new ZipArchive(File.Create(path), ZipArchiveMode.Create))
            {
                var entry = archive.CreateEntry($"CN_{report.FundCode}_{type}_{report.PeriodEnd:yyyy-MM-dd}.xlsx");
                using var entryStream = entry.Open();
                using var fs = fm.OpenRead();
                if (fs is null) return (false, "无法读取文件");

                fs.CopyTo(entryStream);
                entryStream.Flush();
            }

            using var db = DbHelper.Base();
            var manager = db.GetCollection<Manager>().Query().First();
            var result = await UploadFile(type, path, report.PeriodEnd, acc, manager.Name, report.FundCode);

            // 关联
            if (result?.ProcessCode switch { "00" or "100" => true, _ => false })
            {
                db.GetCollection<AmacDirectHandle>().Upsert(new AmacDirectHandle(report.Id, type, result!.Handle!));
                return (true, "");
            }
            return (false, result?.ProcessMessage);
        }
        catch (Exception ex) { LogEx.Error(ex); return (false, "上报失败"); }
        finally { File.Delete(path); }
    }


    /// <summary>
    /// 参数验证在调用前完成
    /// </summary>
    /// <param name="fileType"></param>
    /// <param name="filePath"></param>
    /// <param name="reportEndDate"></param>
    /// <param name="acc"></param>
    /// <param name="managerName"></param>
    /// <param name="entityCode"></param>
    /// <returns></returns>
    public static async Task<DirectFileResponse?> UploadFile(DirectFileType fileType, string filePath, DateOnly reportEndDate, AmacReportAccount acc, string managerName, string entityCode)
    {
        string UserName = acc.Name;
        string DirectPwd = acc.Password;
        string PublicKey = acc.Key;

        //"02b794148d8f48b1d174a7df482e7fe31794c36427ec922c54015785e25aeeb436"; //pmg
        //"02ca905207424c8a03a733458cf079230c55f25cc1ef14750a2b24a36ffe1f1744";// acc.Key;


        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "imgfornote");

        // 生成认证头部
        var pwd = Sm3Utils.Encrypt32(DirectPwd);
        var salt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var sign = Sm3Utils.Encrypt(UserName + salt + pwd);


        HttpRequestMessage request = new HttpRequestMessage { Method = HttpMethod.Post };
        request.Headers.Add("userName", UserName);
        request.Headers.Add("salt", salt);
        request.Headers.Add("sign", sign);
        request.Headers.Add("User-Agent", "imgfornote");



        // 读取文件内容
        var fileBytes = File.ReadAllBytes(filePath);
        var base64File = Convert.ToBase64String(fileBytes);

        // 计算校验和
        var checksum = Sm3Utils.Encrypt(fileBytes);
        checksum = Sm2Utils.Encrypt(PublicKey, checksum!);

        // 构建请求体
        var requestContent = new
        {
            entityCode = entityCode,
            reportType = $"{fileType}",
            reportEndDate = $"{reportEndDate:yyyy-MM-dd}",
            xbrlFileName = $"CN_{entityCode}_{fileType}_{reportEndDate:yyyy-MM-dd}.zip",
            subCompany = managerName,
            body = base64File,
            checksum = checksum,
        };

        var jsonOptions = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Default,
            WriteIndented = false,
        };
        var json = JsonSerializer.Serialize(requestContent, jsonOptions);

        // 发送请求
        request.RequestUri = new Uri(fileType < DirectFileType.RS0001 ? DisclosureUploadUrl : OperationUploadUrl);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        await PrintHttpRequestMessageAsync(request);

        var response = await httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        Debug.WriteLine(responseContent);

        // 解析响应
        var result = JsonSerializer.Deserialize<DirectFileResponse>(responseContent);
        return result;
    }


    public static async Task<IList<ValidationInfo>> QueryResult(IPeriodical periodical, AmacReportAccount acc)
    {
        using var db = DbHelper.Base();
        var id = db.GetCollection<AmacDirectHandle>().FindById(periodical.Id);
        var results = await QueryResult(id, acc);
        db.GetCollection<AmacDirectHandle>().Update(id with { ResultInfo = results });
        return results;
    }

    private static async Task<IList<ValidationInfo>> QueryResult(AmacDirectHandle handle, AmacReportAccount acc)
    {
        string UserName = acc.Name;
        string DirectPwd = acc.Password;
        string PublicKey = acc.Key;

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "imgfornote");

        // 生成认证头部
        var pwd = Sm3Utils.Encrypt32(DirectPwd);
        var salt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        var sign = Sm3Utils.Encrypt(UserName + salt + pwd);


        HttpRequestMessage request = new HttpRequestMessage { Method = HttpMethod.Post };
        request.Headers.Add("userName", UserName);
        request.Headers.Add("salt", salt);
        request.Headers.Add("sign", sign);
        request.Headers.Add("User-Agent", "imgfornote");

        var jsonOptions = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // 关键：不转义中文
            WriteIndented = false // 是否格式化（可选）
        };
        var json = JsonSerializer.Serialize(new { handle = new string[] { handle.Handle } }, jsonOptions);

        // 发送请求
        request.RequestUri = new Uri(handle.FileType < DirectFileType.RS0001 ? DisclosureResultUrl : OperationResultUrl);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");


        var response = await httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<List<ValidationResultItem>>(responseContent);

        if (result?.FirstOrDefault() is ValidationResultItem root)
        {
            return [..root.verifyMessage.children.Where(x=>x.children is not null).
               SelectMany(x => x.children.Select(y => new ValidationInfo { Level = y.deepLevel, Message = y.description }))];
        }

        return [new ValidationInfo { Level = "Error", Message = "未获取到返回信息" }];
    }

    // <summary>
    /// 异步打印 HttpRequestMessage 的详细信息（用于调试）
    /// </summary>
    public static async Task PrintHttpRequestMessageAsync(HttpRequestMessage request)
    {
        Debug.WriteLine("============== HTTP REQUEST ==============");

        // 1. 请求方法和 URL
        Debug.WriteLine($"Method: {request.Method}");
        Debug.WriteLine($"URL: {request.RequestUri}");

        // 2. Headers
        Debug.WriteLine("Headers:");
        foreach (var header in request.Headers)
        {
            Debug.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
        }

        // 如果有 Content，也打印 Content-Type 和 Content Headers
        if (request.Content != null)
        {
            var contentType = request.Content.Headers.ContentType;
            if (contentType != null)
            {
                Debug.WriteLine($"  Content-Type: {contentType}");
            }
        }

        // 3. Body
        Debug.WriteLine("Body:");
        if (request.Content != null)
        {
            // 读取 body（注意：读取后需要重新设置，否则发送时可能为空）
            var body = await request.Content.ReadAsStringAsync();
            Debug.WriteLine(body);
        }
        else
        {
            Debug.WriteLine("<empty>");
        }

        Debug.WriteLine("==========================================");
    }
}

public record AmacDirectHandle(int Id, DirectFileType FileType, string Handle, IList<ValidationInfo>? ResultInfo = null);
