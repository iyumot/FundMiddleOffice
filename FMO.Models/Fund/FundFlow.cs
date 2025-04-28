namespace FMO.Models;


public interface IElementChangable
{

}

/// <summary>
/// 流程
/// </summary>
public abstract class FundFlow
{
    public int Id { get; set; }

    public required int FundId { get; set; }

    /// <summary>
    /// 日期
    /// </summary>
    public DateOnly? Date { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public abstract string Name { get; set; }

    /// <summary>
    /// 自定义文件
    /// </summary>
    public List<FileStorageInfo>? CustomFiles { get; set; }

    /// <summary>
    /// 流程结束
    /// </summary>
    public bool Finished { get; set; }


}

public class FlowCollection
{
    public int FundId { get; set; }


    public List<FundFlow> Flows { get; set; } = new();
}

/// <summary>
/// 发起工作流
/// </summary>
public class InitiateFlow : FundFlow
{
    /// <summary>
    /// 要素文件
    /// </summary>
    public required VersionedFileInfo ElementFiles { get; set; }

    /// <summary>
    /// 定稿前历次合同文件
    /// </summary>
    public required VersionedFileInfo ContractFiles { get; set; }

    public override string Name { get => "项目发起"; set { } }
}




public abstract class ContractFlow : FundFlow
{
    /// <summary>
    /// 定稿合同文件
    /// </summary>
    public FileStorageInfo? ContractFile { get; set; }


    /// <summary>
    /// 风险揭示书
    /// </summary>
    public FileStorageInfo? RiskDisclosureDocument { get; set; }

    /// <summary>
    /// 募集账户函
    /// </summary>
    public FileStorageInfo? CollectionAccountFile { get; set; }

    /// <summary>
    /// 托管账户函
    /// </summary>
    public FileStorageInfo? CustodyAccountFile { get; set; }

}





/// <summary>
/// 合同定稿工作流
/// </summary>
public class ContractFinalizeFlow : ContractFlow
{



    //份额、要素

    public override string Name { get => "合同定稿"; set { } }
}

public struct DateRange
{
    public DateOnly Begin { get; set; }

    public DateOnly End { get; set; }

    public override string ToString()
    {
        return $"{Begin:yyyy-MM-dd} - {End:yyyy-MM-dd}";
    }

    public static bool operator ==(DateRange left, DateRange right)
    {
        return left.Begin == right.Begin && left.End == right.End;
    }

    public static bool operator !=(DateRange left, DateRange right)
    {
        return !(left == right);
    }

    public override bool Equals(object? obj)
    {
        return obj is DateRange other && this == other;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Begin, End);
    }
}

/// <summary>
/// 基金成立工作流
/// </summary>
public class SetupFlow : FundFlow
{
    /// <summary>
    /// 募集开始日期
    /// </summary>
    public DateOnly RaisingStartDate { get; set; }

    /// <summary>
    /// 募集结束日期
    /// </summary>
    public DateOnly RaisingEndDate { get; set; }

    /// <summary>
    /// 募集期
    /// </summary>
    public DateRange RasingPeriod { get; set; }


    public decimal InitialAsset { get; set; }


    /// <summary>
    /// 实缴出资证明
    /// </summary>
    public FileStorageInfo? PaidInCapitalProof { get; set; }

    /// <summary>
    /// 成立公告
    /// </summary>
    public FileStorageInfo? EstablishmentAnnouncement { get; set; }



    public override string Name { get => "基金成立"; set { } }
}

/// <summary>
/// 备案工作流
/// </summary>
public class RegistrationFlow : FundFlow
{

    /// <summary>
    /// 备案承诺函
    /// </summary>
    public FileStorageInfo? CommitmentLetter { get; set; }

    /// <summary>
    /// 用印版
    /// </summary>
    public FileStorageInfo? SealedCommitmentLetter { get; set; }


    /// <summary>
    /// 招募说明书
    /// </summary>
    public FileStorageInfo? Prospectus { get; set; }

    public FileStorageInfo? SealedProspectus { get; set; }

    /// <summary>
    /// 用印的基金合同
    /// </summary>
    public FileStorageInfo? SealedContract { get; set; }


    /// <summary>
    /// 募集账户监督协议
    /// </summary>
    public FileStorageInfo? SealedAccountOversightProtocol { get; set; }

    /// <summary>
    /// 外包服务协议
    /// </summary>
    public FileStorageInfo? SealedOutsourcingServicesAgreement { get; set; }

    /// <summary>
    /// 投资者明细
    /// </summary>
    public FileStorageInfo? SealedInvestorList { get; set; }

    /// <summary>
    /// 产品结构图
    /// </summary>
    public FileStorageInfo? StructureGraph { get; set; }


    public FileStorageInfo? SealedStructureGraph { get; set; }

    /// <summary>
    /// 嵌套承诺函
    /// </summary>
    public FileStorageInfo? NestedCommitmentLetter { get; set; }

    public FileStorageInfo? SealedNestedCommitmentLetter { get; set; }

    /// <summary>
    /// 备案函
    /// </summary>
    public FileStorageInfo? RegistrationLetter { get; set; }

    public override string Name { get => "基金备案"; set { } }


}

/// <summary>
/// 合同变更工作流
/// </summary>
public class ContractModifyFlow : ContractFlow
{


    /// <summary>
    /// 补充协议
    /// </summary>
    public VersionedFileInfo? SupplementaryFile { get; set; }

    /// <summary>
    /// 备案函
    /// </summary>
    public FileStorageInfo? RegistrationLetter { get; set; }


    /// <summary>
    /// 成立公告
    /// </summary>
    public FileStorageInfo? Announcement { get; set; }

    public FileStorageInfo? SealedAnnouncement { get; set; }


    public FileStorageInfo? CommitmentLetter { get; set; }

    //份额、要素

    public override string Name { get => "合同变更"; set { } }
}


public class ModifyByAnnounceFlow : FundFlow
{

    /// <summary>
    /// 公告
    /// </summary>
    public FileStorageInfo? Announcement { get; set; }

    public FileStorageInfo? SealedAnnouncement { get; set; }


    public override string Name { get => "合同变更"; set { } }
}









public class LiquidationFlow : FundFlow
{
    /// <summary>
    /// 清算报告
    /// </summary>
    public FileStorageInfo? LiquidationReport { get; set; }






    public override string Name { get => "基金清算"; set { } }
}