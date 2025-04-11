
using FMO.Models;

namespace FMO.IO.Trustee.CMSChina.Json.TransferRecordJson;


#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
public class Root
{
    public int pageNo { get; set; }
    public int pageSize { get; set; }
    public object limitMin { get; set; }
    public object limitMax { get; set; }
    public int total { get; set; }
    public List<Item> rows { get; set; }
    public object rows1 { get; set; }
    public Rows4 rows4 { get; set; }
    public object displayShow { get; set; }
}

public class Item
{
    public object RN { get; set; }
    public string ID { get; set; }
    public string EN_SQJE { get; set; }
    public string EN_SQFE { get; set; }
    public string EN_QRJE { get; set; }
    public string EN_QRFE { get; set; }
    public string EN_FHBL { get; set; }
    public string EN_ZCFY { get; set; }
    public string EN_DLFY { get; set; }
    public string EN_SXFL { get; set; }
    public string EN_YJBC { get; set; }
    public string EN_ZHLX { get; set; }
    public string VC_CPDM { get; set; }
    public string VC_CPMC { get; set; }
    public string VC_DJZH { get; set; }
    public string VC_FHDM { get; set; }
    public string VC_FHXX { get; set; }
    public object VC_GHRQ { get; set; }
    public string VC_INVTP { get; set; }
    public string VC_KHLX { get; set; }
    public string VC_JGBM { get; set; }
    public string VC_KHBM { get; set; }
    public string VC_KHMC { get; set; }
    public string VC_QRLS { get; set; }
    public string VC_QRRQ { get; set; }
    public string VC_SJLX { get; set; }
    public string VC_SQDH { get; set; }
    public string VC_SQRQ { get; set; }
    public string VC_XMBH { get; set; }
    public string VC_XSJG { get; set; }
    public string VC_XSJGMC { get; set; }
    public string VC_XSQD { get; set; }
    public string VC_YWLXDM { get; set; }
    public string VC_ZJHM { get; set; }
    public object VC_ZXQDMC { get; set; }
    public string CONFIRM_TOTAL { get; set; }
    public string VC_YWLX { get; set; }
    public string EN_QRJJE { get; set; }
    public string EN_DWJZ { get; set; }
    public string EN_LJDWJZ { get; set; }
    public string EN_SXFY { get; set; }
    public string EN_SXFY1 { get; set; }
    public string EN_SYFE { get; set; }
    public string VC_ZJLX { get; set; }
    public object VC_FHFS { get; set; }
    public string MFUNDNAME { get; set; }
    public string MFUNDCODE { get; set; }
    public object VC_JZHD { get; set; }
    public object VC_DFXGJG { get; set; }
    public object VC_DFJJZH { get; set; }
    public object VC_DFJYZH { get; set; }
    public string VC_GLRMC { get; set; }
    public object VC_MBJJDM { get; set; }
    public object VC_TAFQBZ { get; set; }
    public string VC_TAQRH { get; set; }
    public string EN_ZFY { get; set; }
    public string EN_FHJZ { get; set; }
    public string CONFIRM_DATA_TYPE { get; set; }
    public object F_QRBL { get; set; }
    public object VC_ZDZKL { get; set; }
    public object VC_DFJJMC { get; set; }
    public object VC_JESHFS { get; set; }
    public object VC_WBYWLX { get; set; }
    public object VC_YSQZK { get; set; }
    public object VC_HSFZK { get; set; }
    public string OVALUE1 { get; set; }
    public string CLEARING_DATE { get; set; }

    internal Models.TransferRecord ToObject(string identifier)
    {
        TARecordType type = TARecordType.UNK;
        switch (VC_YWLX)
        {
            case "认购结果":
                type = TARecordType.Subscription;
                break;
            case "申购确认":
                type = TARecordType.Purchase;
                break;
            case string s when s.Contains("强制赎回"):
                type = TARecordType.ForceRedemption;
                break;
            case string s when s.Contains("赎回"):
                type = TARecordType.Redemption;
                break;
            case string s when s.Contains("调增"):
                type = TARecordType.Increase;
                break;
            case string s when s.Contains("调减"):
                type = TARecordType.Decrease;
                break;
            case string s when s.Contains("转入"):
                type = TARecordType.MoveIn;
                break;
            case string s when s.Contains("转出"):
                type = TARecordType.MoveOut;
                break;
            default:
                break;
        }


        var obj = new Models.TransferRecord
        {
            CustomerName = VC_KHMC,
            CustomerIdentity = VC_ZJHM,
            FundName = MFUNDNAME,
            FundCode = MFUNDCODE,
            Agency = VC_XSJGMC,
            Type = type,
            RequestDate = DateOnly.Parse(VC_SQRQ),
            RequestShare = decimal.Parse(EN_SQJE),
            RequestAmount = decimal.Parse(EN_SQFE),
            CreateDate = DateOnly.Parse(VC_QRRQ),
            ConfirmedDate = DateOnly.Parse(VC_QRRQ),
            ConfirmedShare = decimal.Parse(EN_QRFE),
            ConfirmedAmount = decimal.Parse(EN_QRJE),
            ConfirmedNetAmount = decimal.Parse(EN_QRJJE),
            ExternalId = VC_QRLS,
            ExternalRequestId = VC_SQDH,
            Source = identifier,
            Fee = decimal.Parse(EN_SXFY),
            PerformanceFee = decimal.Parse(EN_YJBC),
        };


        // 有子份额
        if (VC_CPMC.Length > MFUNDNAME.Length)
            obj.ShareClass = VC_CPMC[MFUNDNAME.Length..];

        return obj;
    }
}

public class Rows4
{
    public object RN { get; set; }
    public object ID { get; set; }
    public string EN_SQJE { get; set; }
    public string EN_SQFE { get; set; }
    public string EN_QRJE { get; set; }
    public string EN_QRFE { get; set; }
    public object EN_FHBL { get; set; }
    public object EN_ZCFY { get; set; }
    public object EN_DLFY { get; set; }
    public object EN_SXFL { get; set; }
    public object EN_YJBC { get; set; }
    public object EN_ZHLX { get; set; }
    public object VC_CPDM { get; set; }
    public string VC_CPMC { get; set; }
    public object VC_DJZH { get; set; }
    public object VC_FHDM { get; set; }
    public object VC_FHXX { get; set; }
    public object VC_GHRQ { get; set; }
    public object VC_INVTP { get; set; }
    public object VC_KHLX { get; set; }
    public object VC_JGBM { get; set; }
    public object VC_KHBM { get; set; }
    public object VC_KHMC { get; set; }
    public object VC_QRLS { get; set; }
    public object VC_QRRQ { get; set; }
    public object VC_SJLX { get; set; }
    public object VC_SQDH { get; set; }
    public object VC_SQRQ { get; set; }
    public object VC_XMBH { get; set; }
    public object VC_XSJG { get; set; }
    public object VC_XSJGMC { get; set; }
    public object VC_XSQD { get; set; }
    public object VC_YWLXDM { get; set; }
    public object VC_ZJHM { get; set; }
    public object VC_ZXQDMC { get; set; }
    public string CONFIRM_TOTAL { get; set; }
    public object VC_YWLX { get; set; }
    public string EN_QRJJE { get; set; }
    public object EN_DWJZ { get; set; }
    public object EN_LJDWJZ { get; set; }
    public object EN_SXFY { get; set; }
    public string EN_SXFY1 { get; set; }
    public object EN_SYFE { get; set; }
    public object VC_ZJLX { get; set; }
    public object VC_FHFS { get; set; }
    public object MFUNDNAME { get; set; }
    public object MFUNDCODE { get; set; }
    public object VC_JZHD { get; set; }
    public object VC_DFXGJG { get; set; }
    public object VC_DFJJZH { get; set; }
    public object VC_DFJYZH { get; set; }
    public object VC_GLRMC { get; set; }
    public object VC_MBJJDM { get; set; }
    public object VC_TAFQBZ { get; set; }
    public object VC_TAQRH { get; set; }
    public object EN_ZFY { get; set; }
    public object EN_FHJZ { get; set; }
    public object CONFIRM_DATA_TYPE { get; set; }
    public object F_QRBL { get; set; }
    public object VC_ZDZKL { get; set; }
    public object VC_DFJJMC { get; set; }
    public object VC_JESHFS { get; set; }
    public object VC_WBYWLX { get; set; }
    public object VC_YSQZK { get; set; }
    public object VC_HSFZK { get; set; }
    public object OVALUE1 { get; set; }
    public object CLEARING_DATE { get; set; }
}





#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。