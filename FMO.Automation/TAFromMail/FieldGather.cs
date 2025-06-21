using System.Diagnostics.CodeAnalysis;
using FMO.Models;
using FMO.Utilities;

namespace FMO.Schedule;


public abstract class FieldGather
{


    public static string StringValue(object obj) => obj switch { string s => s, _ => obj?.ToString() ?? "" };

    public static decimal DecimalValue(object obj)
    {
        return obj switch { double s => (decimal)s, string s => decimal.Parse(s), _ => -1 };
    }

    public static DateOnly DateOnlyValue(object o) => o switch { DateTime d => DateOnly.FromDateTime(d), DateOnly d => d, _ => DateTimeHelper.TryParse(o?.ToString(), out var d) ? d : default };

}

public abstract class FieldGather<T> : FieldGather
{

    public abstract void Fill(T obj, IList<object> values);



    protected void Check<TP, TE>(FieldInfo<TE, TP> field, IList<string> values) => field.Index = values.IndexOf(field.Header);

    protected void Fill<TE, TP>(FieldInfo<TE, TP> field, TE obj, IList<object> values)
    {
        if (field.Index >= 0 && values.Count > field.Index)
        {
            var v = field.GetValue(values[field.Index]);
            field.SetValue(obj, v);
        }
    }

}





public class FieldInfo<TE, TP>
{

    public required string Header { get; set; }

    public int Index { get; set; } = -1;

    public bool IsNecessary { get; set; } = true;

    public required Func<object, TP> GetValue { get; set; }

    public required Action<TE, TP> SetValue { get; set; }

    public FieldInfo()
    {
    }

    [SetsRequiredMembers]
    public FieldInfo(string header, Func<object, TP> getValue, Action<TE, TP> setValue, bool necessary = true)
    {
        Header = header;
        GetValue = getValue;
        SetValue = setValue;
        IsNecessary = necessary;
    }
}



public class TAFieldGather : FieldGather<TransferRecord>
{
    public FieldInfo<TransferRecord, string?> FundName { get; init; } = new FieldInfo<TransferRecord, string?>("产品名称", o => o switch { string s => s, _ => o.ToString() }, (x, y) => x.FundName = y);
    public FieldInfo<TransferRecord, string?> FundCode { get; init; } = new FieldInfo<TransferRecord, string?>("产品代码", o => o switch { string s => s, _ => o.ToString() }, (x, y) => x.FundCode = y);
    public FieldInfo<TransferRecord, TARecordType> Type { get; init; } = new FieldInfo<TransferRecord, TARecordType>("业务类型", o => ParseType(o?.ToString()), (x, y) => x.Type = y);
    public FieldInfo<TransferRecord, DateOnly> RequestDate { get; init; } = new FieldInfo<TransferRecord, DateOnly>("申请日期", o => DateOnlyValue(o), (x, y) => x.RequestDate = y);
    public FieldInfo<TransferRecord, decimal> RequestAmount { get; init; } = new FieldInfo<TransferRecord, decimal>("申请金额", o => DecimalValue(o), (x, y) => x.RequestAmount = y);
    public FieldInfo<TransferRecord, decimal> RequestShare { get; init; } = new FieldInfo<TransferRecord, decimal>("申请份额", o => DecimalValue(o), (x, y) => x.RequestShare = y);

    public FieldInfo<TransferRecord, DateOnly> ConfirmedDate { get; init; } = new FieldInfo<TransferRecord, DateOnly>("确认日期", o => DateOnlyValue(o), (x, y) => x.ConfirmedDate = y);
    public FieldInfo<TransferRecord, decimal> ConfirmedShare { get; init; } = new ("确认份额", o => DecimalValue(o), (x, y) => x.ConfirmedShare = y);
    public FieldInfo<TransferRecord, decimal> ConfirmedAmount { get; init; } = new FieldInfo<TransferRecord, decimal>("确认金额", o => DecimalValue(o), (x, y) => x.ConfirmedAmount = y);
    public FieldInfo<TransferRecord, decimal> ConfirmedNetAmount { get; init; } = new FieldInfo<TransferRecord, decimal>("确认净额", o => DecimalValue(o), (x, y) => x.ConfirmedNetAmount = y);


    public FieldInfo<TransferRecord, decimal> Fee { get; init; } = new FieldInfo<TransferRecord, decimal>("手续费", o => DecimalValue(o), (x, y) => x.Fee = y);
    public FieldInfo<TransferRecord, decimal> PerformanceFee { get; init; } = new FieldInfo<TransferRecord, decimal>("业绩报酬", o => DecimalValue(o), (x, y) => x.PerformanceFee = y);

    public FieldInfo<TransferRecord, string> CustomerIdentity { get; init; } = new FieldInfo<TransferRecord, string>("证件号码", o => o switch { string s => s, _ => o.ToString()! }, (x, y) => x.CustomerIdentity = y);
    public FieldInfo<TransferRecord, string> CustomerName { get; init; } = new FieldInfo<TransferRecord, string>("客户名称", o => StringValue(o), (x, y) => x.CustomerName = y);


    public FieldInfo<TransferRecord, string> ExternalRequestId { get; init; } = new FieldInfo<TransferRecord, string>("申请单号", o => StringValue(o), (x, y) => x.ExternalRequestId = y, false);
    public FieldInfo<TransferRecord, string> ExternalId { get; init; } = new FieldInfo<TransferRecord, string>("确认流水号", o => StringValue(o), (x, y) => x.ExternalId = y);


    public bool CheckField(IList<string> fields)
    {
        Check(FundName, fields);
        Check(FundCode, fields);
        Check(Type, fields);
        Check(RequestDate, fields);
        Check(RequestAmount, fields);
        Check(RequestShare, fields);
        Check(ConfirmedDate, fields);
        Check(ConfirmedShare, fields);
        Check(ConfirmedAmount, fields);
        Check(ConfirmedNetAmount, fields);
        Check(Fee, fields);
        Check(PerformanceFee, fields);
        Check(CustomerIdentity, fields);
        Check(CustomerName, fields);
        Check(ExternalRequestId, fields);
        Check(ExternalId, fields);

        return FundName.Index >= 0 && FundCode.Index >= 0 && Type.Index >= 0 && RequestDate.Index >= 0 && RequestAmount.Index >= 0 && RequestShare.Index >= 0 &&
            ConfirmedDate.Index >= 0 && ConfirmedShare.Index >= 0 && ConfirmedAmount.Index >= 0 && ConfirmedNetAmount.Index >= 0 && Fee.Index >= 0 &&
            PerformanceFee.Index >= 0 && CustomerIdentity.Index >= 0 && CustomerName.Index >= 0 && ExternalRequestId.Index >= 0 && ExternalId.Index >= 0;
    }


    public override void Fill(TransferRecord obj, IList<object> values)
    {
        Fill(FundName, obj, values);
        Fill(FundCode, obj, values);
        Fill(Type, obj, values);
        Fill(RequestDate, obj, values);
        Fill(RequestAmount, obj, values);
        Fill(RequestShare, obj, values);
        Fill(ConfirmedDate, obj, values);
        Fill(ConfirmedShare, obj, values);
        Fill(ConfirmedAmount, obj, values);
        Fill(ConfirmedNetAmount, obj, values);
        Fill(Fee, obj, values);
        Fill(PerformanceFee, obj, values);
        Fill(CustomerIdentity, obj, values);
        Fill(CustomerName, obj, values);
        Fill(ExternalRequestId, obj, values);
        Fill(ExternalId, obj, values);

    }


    public string[] Failed()
    {
        List<string> errors = new List<string>();
        if (FundName.Index == -1) errors.Add("FundName");
        if (FundCode.Index == -1) errors.Add("FundCode");
        if (Type.Index == -1) errors.Add("Type");
        if (RequestDate.Index == -1) errors.Add("RequestDate");
        if (RequestAmount.Index == -1) errors.Add("RequestAmount");
        if (RequestShare.Index == -1) errors.Add("RequestShare");
        if (ConfirmedDate.Index == -1) errors.Add("ConfirmedDate");
        if (ConfirmedShare.Index == -1) errors.Add("ConfirmedShare");
        if (ConfirmedAmount.Index == -1) errors.Add("ConfirmedAmount");
        if (ConfirmedNetAmount.Index == -1) errors.Add("ConfirmedNetAmount");
        if (Fee.Index == -1) errors.Add("Fee");
        if (PerformanceFee.Index == -1) errors.Add("PerformanceFee");
        if (CustomerIdentity.Index == -1) errors.Add("CustomerIdentity");
        if (CustomerName.Index == -1) errors.Add("CustomerName");
        if (ExternalRequestId.Index == -1) errors.Add("ExternalRequestId");
        if (ExternalId.Index == -1) errors.Add("ExternalId");
        return errors.ToArray();
    }


    public static TARecordType ParseType(string? str)
    {
        switch (str)
        {
            case "认购结果":
                return TARecordType.Subscription;

            case "申购确认":
                return TARecordType.Purchase;

            case string s when s.Contains("强制赎回"):
                return TARecordType.ForceRedemption;

            case string s when s.Contains("赎回"):
                return TARecordType.Redemption;

            case string s when s.Contains("调增"):
                return TARecordType.Increase;

            case string s when s.Contains("调减"):
                return TARecordType.Decrease;

            case string s when s.Contains("转入"):
                return TARecordType.MoveIn;

            case string s when s.Contains("转出"):
                return TARecordType.MoveOut;

            case string s when s.Contains("分红"):
                return TARecordType.Distribution;

            default:
                return TARecordType.UNK;
        }
    }

}



/// <summary>
/// string -> property
/// string[]
/// object[]
/// </summary>
/// <typeparam name="T"></typeparam>
public class FieldParser<T>
{




}

 
