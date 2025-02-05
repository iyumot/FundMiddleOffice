using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using System.Diagnostics.CodeAnalysis;

namespace FMO;

public partial class RegistrationFlowViewModel : FlowViewModel
{
    /// <summary>
    /// 备案承诺函
    /// </summary>
    [ObservableProperty]
    public partial PredefinedFileViewModel CommitmentLetter { get; set; }


    [ObservableProperty]
    public partial PredefinedFileViewModel SealedCommitmentLetter { get; set; }


    /// <summary>
    /// 招募说明书
    /// </summary>
    [ObservableProperty]
    public partial PredefinedFileViewModel Prospectus { get; set; }


    [ObservableProperty]
    public partial PredefinedFileViewModel SealedProspectus { get; set; }


    /// <summary>
    /// 基金合同 用印
    /// </summary>
    [ObservableProperty]
    public partial PredefinedFileViewModel SealedContract { get; set; }




    /// <summary>
    /// 募集账户监督协议
    /// </summary>
    [ObservableProperty]
    public partial PredefinedFileViewModel SealedAccountOversightProtocol { get; set; }

    /// <summary>
    /// 外包服务协议
    /// </summary>
    [ObservableProperty]
    public partial PredefinedFileViewModel SealedOutsourcingServicesAgreement { get; set; }

    /// <summary>
    /// 投资者明细
    /// </summary>
    [ObservableProperty]
    public partial PredefinedFileViewModel SealedInvestorList { get; set; }

    /// <summary>
    /// 产品结构图
    /// </summary>
    [ObservableProperty]
    public partial PredefinedFileViewModel StructureGraph { get; set; }


    [ObservableProperty]
    public partial PredefinedFileViewModel SealedStructureGraph { get; set; }

    /// <summary>
    /// 嵌套承诺函
    /// </summary>
    [ObservableProperty]
    public partial PredefinedFileViewModel NestedCommitmentLetter { get; set; }


    [ObservableProperty]
    public partial PredefinedFileViewModel SealedNestedCommitmentLetter { get; set; }
     

    [ObservableProperty]
    public partial PredefinedFileViewModel RegistrationLetter { get; set; }






    [SetsRequiredMembers]
    public RegistrationFlowViewModel(RegistrationFlow flow) : base(flow)
    {
        CommitmentLetter = new PredefinedFileViewModel(FundId, FlowId, "备案承诺函", flow.CommitmentLetter?.Path, "Registration", nameof(RegistrationFlow.CommitmentLetter)) { Filter = "文本文档|*.doc;*.docx;*.wps;*.pdf;"};
        SealedCommitmentLetter = new PredefinedFileViewModel(FundId, FlowId, "备案承诺函", flow.SealedCommitmentLetter?.Path, "Registration", nameof(RegistrationFlow.SealedCommitmentLetter));
        SealedCommitmentLetter.Filter = "PDF (*.pdf)|*.pdf;";

        Prospectus = new PredefinedFileViewModel(FundId, FlowId, "招募说明书", flow.Prospectus?.Path, "Registration", nameof(RegistrationFlow.Prospectus)) { Filter = "文本文档|*.doc;*.docx;*.wps;*.pdf;" };
        SealedProspectus = new PredefinedFileViewModel(FundId, FlowId, "招募说明书", flow.SealedProspectus?.Path, "Registration", nameof(RegistrationFlow.SealedProspectus));
        SealedProspectus.Filter = "PDF (*.pdf)|*.pdf;";

        SealedContract = new PredefinedFileViewModel(FundId, FlowId, "基金合同", flow.SealedContract?.Path, "Registration", nameof(RegistrationFlow.SealedContract));
        SealedContract.Filter = "PDF (*.pdf)|*.pdf;";

        SealedAccountOversightProtocol = new PredefinedFileViewModel(FundId, FlowId, "募集账户监督协议", flow.SealedAccountOversightProtocol?.Path, "Registration", nameof(RegistrationFlow.SealedAccountOversightProtocol));
        SealedAccountOversightProtocol.Filter = "PDF (*.pdf)|*.pdf;";

        SealedOutsourcingServicesAgreement = new PredefinedFileViewModel(FundId, FlowId, "外包服务协议", flow.SealedOutsourcingServicesAgreement?.Path, "Registration", nameof(RegistrationFlow.SealedOutsourcingServicesAgreement));
        SealedOutsourcingServicesAgreement.Filter = "PDF (*.pdf)|*.pdf;";

        SealedInvestorList = new PredefinedFileViewModel(FundId, FlowId, "投资者明细", flow.SealedInvestorList?.Path, "Registration", nameof(RegistrationFlow.SealedInvestorList));
        SealedInvestorList.Filter = "PDF (*.pdf)|*.pdf;";


        StructureGraph = new PredefinedFileViewModel(FundId, FlowId, "产品结构图", flow.StructureGraph?.Path, "Registration", nameof(RegistrationFlow.StructureGraph)) { Filter = "文本文档|*.doc;*.docx;*.wps;*.pdf;" };
        SealedStructureGraph = new PredefinedFileViewModel(FundId, FlowId, "产品结构图", flow.SealedStructureGraph?.Path, "Registration", nameof(RegistrationFlow.SealedStructureGraph));
        SealedStructureGraph.Filter = "PDF (*.pdf)|*.pdf;";



        NestedCommitmentLetter = new PredefinedFileViewModel(FundId, FlowId, "嵌套承诺函", flow.NestedCommitmentLetter?.Path, "Registration", nameof(RegistrationFlow.NestedCommitmentLetter)) { Filter = "文本文档|*.doc;*.docx;*.wps;*.pdf;" };
        SealedNestedCommitmentLetter = new PredefinedFileViewModel(FundId, FlowId, "嵌套承诺函", flow.SealedNestedCommitmentLetter?.Path, "Registration", nameof(RegistrationFlow.SealedNestedCommitmentLetter));
        SealedNestedCommitmentLetter.Filter = "PDF (*.pdf)|*.pdf;";


        RegistrationLetter = new PredefinedFileViewModel(FundId, FlowId, "备案函", flow.RegistrationLetter?.Path, "Registration", nameof(RegistrationFlow.RegistrationLetter)) { Filter = "PDF (*.pdf)|*.pdf;" };
   

    }
}
