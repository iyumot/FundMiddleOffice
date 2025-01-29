namespace FMO.Models;

public class FundElements
{
    public const string SingleShareKey = "单一份额";

    public int Id { get; set; }

    public required int FundId { get; set; }


    /// <summary>
    /// 名称
    /// </summary>
    public Mutable<string>? FullName { get; set; }

    /// <summary>
    /// 简称
    /// </summary>
    public Mutable<string>? ShortName { get; set; }



    /// <summary>
    /// 运作方式
    /// </summary> 
    public Mutable<DataExtra<FundMode>>? FundModeInfo { get; set; }

    /// <summary>
    /// 封闭期
    /// </summary>
    public Mutable<SealingRule>? SealingRule { get; set; }

    /// <summary>
    /// 锁定期
    /// </summary>
    //public Mutable<SealingRule>? LockingRule { get; set; }



    /// <summary>
    /// 风险等级
    /// </summary>
    public Mutable<RiskLevel?>? RiskLevel { get; set; }

    /// <summary>
    /// 存续期
    /// </summary>
    public Mutable<int?>? DurationInMonths { get; set; }




    /// <summary>
    /// 结束日期
    /// </summary>
    public Mutable<DateOnly?>? ExpirationDate { get; set; }


    /// <summary>
    /// 主募集账户
    /// </summary>
    public Mutable<BankAccount>? CollectionAccount { get; set; }


    /// <summary>
    /// 主托管账户
    /// </summary>
    public Mutable<BankAccount>? CustodyAccount { get; set; }


    /// <summary>
    /// 份额类别
    /// </summary>
    public Mutable<ShareClass[]>? ShareClasses { get; set; }

    /// <summary>
    /// 止损线
    /// </summary>
    public Mutable<decimal?>? StopLine { get; set; }

    /// <summary>
    /// 预警线
    /// </summary>
    public Mutable<decimal?>? WarningLine { get; set; }

    /// <summary>
    /// 开放日规则
    /// </summary>
    public Mutable<string>? OpenDayInfo { get; set; }


    /// <summary>
    /// 托管费
    /// </summary>
    public Mutable<ValueWithEnum<FundFeeType, decimal>>? TrusteeFee { get; set; }

    /// <summary>
    /// 托管保底
    /// </summary>
    public Mutable<ValueWithBoolean<decimal>>? TrusteeGuaranteedFee { get; set; }


    /// <summary>
    /// 外包费
    /// </summary>
    public Mutable<ValueWithEnum<FundFeeType, decimal>>? OutsourcingFee { get; set; }


    /// <summary>
    /// 外包费保底
    /// </summary>
    public Mutable<ValueWithBoolean<decimal>>? OutsourcingGuaranteedFee { get; set; }


    public PortionMutable<ValueWithEnum<SealingType, int>>? LockingRule { get; set; }


    /// <summary>
    /// 管理费
    /// </summary>
    public PortionMutable<ValueWithEnum<FundFeeType, decimal>>? ManageFee { get; set; }







    public static FundElements Create(int fundid)
    {
        return new FundElements
        {
            FundId = fundid,

        };
    }


    public bool Init()
    {
        bool changed = false;

        if (FullName is null)
        { changed = true; FullName = new Mutable<string>(nameof(FullName)); }

        if (ShortName is null)
        { changed = true; ShortName = new Mutable<string>(nameof(ShortName)); }

        if (RiskLevel is null)
        { changed = true; RiskLevel = new Mutable<RiskLevel?>(nameof(RiskLevel)); }

        if (DurationInMonths is null)
        { changed = true; DurationInMonths = new Mutable<int?>(nameof(DurationInMonths)); }

        if (ExpirationDate is null)
        { changed = true; ExpirationDate = new Mutable<DateOnly?>(nameof(ExpirationDate)); }

        if (CollectionAccount is null)
        { changed = true; CollectionAccount = new Mutable<BankAccount>(nameof(CollectionAccount)); }

        if (CustodyAccount is null)
        { changed = true; CustodyAccount = new Mutable<BankAccount>(nameof(CustodyAccount)); }


        if (ShareClasses is null)
        { changed = true; ShareClasses = new Mutable<ShareClass[]>(nameof(ShareClasses)); }


        if (StopLine is null)
        { changed = true; StopLine = new Mutable<decimal?>(nameof(StopLine)); }


        if (WarningLine is null)
        { changed = true; WarningLine = new Mutable<decimal?>(nameof(WarningLine)); }

        if (FundModeInfo is null)
        { changed = true; FundModeInfo = new Mutable<DataExtra<FundMode>>(nameof(FundModeInfo)); }

        if (SealingRule is null)
        { changed = true; SealingRule = new Mutable<SealingRule>(nameof(SealingRule)); }


        if (LockingRule is null)
        { changed = true; LockingRule = new (nameof(LockingRule)); }

        if (OpenDayInfo is null)
        { changed = true; OpenDayInfo = new Mutable<string>(nameof(OpenDayInfo)); }




        if (TrusteeFee is null)
        { changed = true; TrusteeFee = new (nameof(TrusteeFee)); }


        if (TrusteeGuaranteedFee is null)
        { changed = true; TrusteeGuaranteedFee = new(nameof(TrusteeGuaranteedFee)); }

        if (OutsourcingFee is null)
        { changed = true; OutsourcingFee = new(nameof(OutsourcingFee)); }


        if (OutsourcingGuaranteedFee is null)
        { changed = true; OutsourcingGuaranteedFee = new(nameof(OutsourcingGuaranteedFee)); }

        if (ManageFee is null)
        { changed = true; ManageFee = new PortionMutable<ValueWithEnum<FundFeeType, decimal>>(nameof(ManageFee)); }



        return changed;
    }


    /// <summary>
    /// 删除份额相关的要素 
    /// </summary>
    /// <param name="flowid"></param>
    /// <param name="share"></param>
    public void RemoveShareRelated(int flowid, string share)
    {
        LockingRule?.RemoveValue(share, flowid);
        ManageFee?.RemoveValue(share, flowid);
    }


    private void CopyFromDefault<T1, T2>(PortionMutable<ValueWithEnum<T1, T2>> mutable, int flowId, string[] add) where T1 : struct, Enum
    {
        if (mutable!.GetValue(flowId) is var d && d.FlowId == flowId && d.Value?.FirstOrDefault().Value is ValueWithEnum<T1, T2> r)
            foreach (var item in add)
                d.Value[item] = r;
    }

    private void AddShareRelated(int flowId, string[] add)
    {
        CopyFromDefault(ManageFee!, flowId, add);
        CopyFromDefault(LockingRule!, flowId, add);

    }



    private void SetElementAsDefault<T>(PortionMutable<T>? portion, int flowid)
    {
        if (portion is null) return;

        (var id, var v) = portion!.GetValue(flowid);
        if (id != flowid) return;
        if (v is null || v.Count != 1) return;

        var sin = v.First();
        v[SingleShareKey] = sin.Value;
        v.Remove(sin.Key);
    }


    private void SetAsDefault(int flowid)
    {
        foreach (var p in GetType().GetProperties())
        {
            if (p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(PortionMutable<>))
            {
                var genericArg = p.PropertyType.GetGenericArguments()[0];
                var method = typeof(FundElements).GetMethod("SetElementAsDefault", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);//, new[] { typeof(PortionMutable<>), typeof(int) });
                var genericMethod = method!.MakeGenericMethod(genericArg);
                genericMethod.Invoke(this, new object[] { p.GetValue(this)!, flowid });
            }
        }
    }



    public void ShareClassChange(int flowId, string[] newshares, string[] add, string[] remove)
    {
        // 从单一份额变成多份额，复制值
        if (ShareClasses is not null && ShareClasses.GetValue(flowId) is var iv && iv.FlowId == flowId && iv.Value?.Length == 1)
            AddShareRelated(flowId, add);

        foreach (var item in remove)
            RemoveShareRelated(flowId, item);

        if (ShareClasses is null)
            ShareClasses = new(nameof(FundElements.ShareClasses), newshares.Select(x => new ShareClass { Name = x }).ToArray());
        else
            ShareClasses.SetValue(newshares.Select(x => new ShareClass { Name = x }).ToArray(), flowId);


        if (newshares.Length == 1)
            SetAsDefault(flowId);
    }

}