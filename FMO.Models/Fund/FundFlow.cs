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
    public List<FundFileInfo>? CustomFiles { get; set; }

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
    public FundFileInfo? ContractFile { get; set; }


    /// <summary>
    /// 风险揭示书
    /// </summary>
    public FundFileInfo? RiskDisclosureDocument { get; set; }

    /// <summary>
    /// 募集账户函
    /// </summary>
    public FundFileInfo? CollectionAccountFile { get; set; }

    /// <summary>
    /// 托管账户函
    /// </summary>
    public FundFileInfo? CustodyAccountFile { get; set; }

}





/// <summary>
/// 合同定稿工作流
/// </summary>
public class ContractFinalizeFlow : ContractFlow
{



    //份额、要素

    public override string Name { get => "合同定稿"; set { } }
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
    /// 实缴出资证明
    /// </summary>
    public FundFileInfo? PaidInCapitalProof { get; set; }

    /// <summary>
    /// 成立公告
    /// </summary>
    public FundFileInfo? EstablishmentAnnouncement { get; set; }



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
    public FundFileInfo? CommitmentLetter { get; set; }

    /// <summary>
    /// 用印版
    /// </summary>
    public FundFileInfo? SealedCommitmentLetter { get; set; }


    /// <summary>
    /// 招募说明书
    /// </summary>
    public FundFileInfo? Prospectus { get; set; }

    public FundFileInfo? SealedProspectus { get; set; }

    /// <summary>
    /// 用印的基金合同
    /// </summary>
    public FundFileInfo? SealedContract { get; set; }


    /// <summary>
    /// 募集账户监督协议
    /// </summary>
    public FundFileInfo? SealedAccountOversightProtocol { get; set; }

    /// <summary>
    /// 外包服务协议
    /// </summary>
    public FundFileInfo? SealedOutsourcingServicesAgreement { get; set; }

    /// <summary>
    /// 投资者明细
    /// </summary>
    public FundFileInfo? SealedInvestorList { get; set; }

    /// <summary>
    /// 产品结构图
    /// </summary>
    public FundFileInfo? StructureGraph { get; set; }


    public FundFileInfo? SealedStructureGraph { get; set; }

    /// <summary>
    /// 嵌套承诺函
    /// </summary>
    public FundFileInfo? NestedCommitmentLetter { get; set; }

    public FundFileInfo? SealedNestedCommitmentLetter { get; set; }

    /// <summary>
    /// 备案函
    /// </summary>
    public FundFileInfo? RegistrationLetter { get; set; }

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
    public FundFileInfo? RegistrationLetter { get; set; }


    /// <summary>
    /// 成立公告
    /// </summary>
    public FundFileInfo? Announcement { get; set; }

    public FundFileInfo? SealedAnnouncement { get; set; }

    //份额、要素

    public override string Name { get => "合同变更"; set { } }
}


public class LiquidationFlow : FundFlow
{
    /// <summary>
    /// 清算报告
    /// </summary>
    public FundFileInfo? LiquidationReport { get; set; }






    public override string Name { get => "基金清算"; set { } }
}