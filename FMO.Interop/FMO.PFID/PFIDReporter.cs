
#define TEST_PFID

using FMO.Models;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;


namespace FMO.PFID;


public enum PFIDFileType
{

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



public class PFIDReporter
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






    /// <summary>
    /// 参数验证在调用前完成
    /// </summary>
    /// <param name="fileType"></param>
    /// <param name="filePath"></param>
    /// <param name="reportEndDate"></param>
    /// <param name="acc"></param>
    /// <param name="managerName"></param>
    /// <param name="managerCode"></param>
    /// <returns></returns>
    public async Task<string?> UploadFile(PFIDFileType fileType, string filePath, DateOnly reportEndDate, AmacReportAccount acc, string managerName, string managerCode)
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
            entityCode = managerCode,
            reportType = $"{fileType}",
            reportEndDate = $"{reportEndDate:yyyy-MM-dd}",
            xbrlFileName = $"CN_{managerCode}_{fileType}_{reportEndDate:yyyy-MM-dd}.zip",
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
        request.RequestUri = new Uri(fileType < PFIDFileType.RS0001 ? DisclosureUploadUrl : OperationUploadUrl);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        await PrintHttpRequestMessageAsync(request);

        var response = await httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        Debug.WriteLine(responseContent);

        // 解析响应
        var result = JsonSerializer.Deserialize<DirectFileResponse>(responseContent);
        return result?.ProcessCode == "100" ? result.Handle : null;
    }


    public async Task<string?> QueryResult(string handle, PFIDFileType fileType, AmacReportAccount acc)
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
        var json = JsonSerializer.Serialize(new { handle = new string[] { handle } }, jsonOptions);

        // 发送请求
        request.RequestUri = new Uri(fileType < PFIDFileType.RS0001 ? DisclosureResultUrl : OperationResultUrl);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        await PrintHttpRequestMessageAsync(request);

        var response = await httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        return responseContent;
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
