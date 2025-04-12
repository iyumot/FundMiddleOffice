
using FMO.Models;
using Serilog;

namespace FMO.IO.Trustee.CMSChina.Json.TransferRequestJson;


// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
public class Root
{
    public int pageNo { get; set; }
    public int pageSize { get; set; }
    public List<Item> rows { get; set; }
    public int total { get; set; }
}

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
public class Item
{
    public object ID { get; set; }
    public string VC_CPDM { get; set; }
    public string VC_CPMC { get; set; }
    public string VC_TZRMC { get; set; }
    public string VC_YWLX { get; set; }
    public string VC_SQRQ { get; set; }
    public string EN_SQJE { get; set; }
    public string EN_SQFE { get; set; }
    public string VC_XSJGMC { get; set; }
    public string VC_KHLX { get; set; }
    public string VC_ZJLX { get; set; }
    public string VC_ZJHM { get; set; }
    public string VC_JYZH { get; set; }
    public string VC_JJZH { get; set; }
    public string VC_YYKFR { get; set; }
    public string VC_QRJG { get; set; }
    public string VC_QRRQ { get; set; }
    public string EN_SXFY { get; set; }
    public string EN_YJZKL { get; set; }
    public string L_SCGMBZ { get; set; }
    public object AMOUNT_REDEMPTION_FLAG { get; set; }
    public int RN { get; set; }
    public string VC_SQDH { get; set; }
    public object VC_YWDM { get; set; }

    public TransferRequest ToObject(string pid)
    {
        var obj = new TransferRequest
        {
            CustomerName = VC_TZRMC,
            CustomerIdentity = VC_ZJHM,
            FundName = VC_CPMC,
            FundCode = VC_CPDM,
            CreateDate = DateOnly.FromDateTime(DateTime.Now),
            RequestType = VC_YWLX switch { "认购"=> RequestType.Subscription, "申购" => RequestType.Purchase, "赎回" or "金额赎回" => RequestType.Redemption, _ => RequestType.UNK },
            RequestDate = DateOnly.ParseExact(VC_SQRQ, "yyyyMMdd"),
            RequestShare = EN_SQFE switch { "-" or "" or null => 0, _ => decimal.Parse(EN_SQFE) },
            RequestAmount = decimal.Parse(EN_SQJE),
            Source = pid,
            Agency = VC_XSJGMC,
            Fee = EN_SXFY switch { "-" or "" or null => 0, _ => decimal.Parse(EN_SXFY) },
            ExternalId = VC_SQDH, 
        };

        if(obj.RequestType == RequestType.UNK)
            Log.Error($"未知的类型 {VC_YWLX}");
        return obj;
    }
}



#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。