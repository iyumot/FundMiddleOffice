using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.TPL;
using FMO.Utilities;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace FMO;

public partial class RegistrationFlowViewModel : FlowViewModel
{
    /// <summary>
    /// 备案承诺函
    /// </summary>
    [ObservableProperty]
    public partial FlowFileViewModel CommitmentLetter { get; set; }


    [ObservableProperty]
    public partial FlowFileViewModel SealedCommitmentLetter { get; set; }


    /// <summary>
    /// 招募说明书
    /// </summary>
    [ObservableProperty]
    public partial FlowFileViewModel Prospectus { get; set; }


    [ObservableProperty]
    public partial FlowFileViewModel SealedProspectus { get; set; }


    /// <summary>
    /// 基金合同 用印
    /// </summary>
    [ObservableProperty]
    public partial FlowFileViewModel SealedContract { get; set; }




    /// <summary>
    /// 募集账户监督协议
    /// </summary>
    [ObservableProperty]
    public partial FlowFileViewModel SealedAccountOversightProtocol { get; set; }

    /// <summary>
    /// 外包服务协议
    /// </summary>
    [ObservableProperty]
    public partial FlowFileViewModel SealedOutsourcingServicesAgreement { get; set; }

    /// <summary>
    /// 投资者明细
    /// </summary>
    [ObservableProperty]
    public partial FlowFileViewModel SealedInvestorList { get; set; }

    /// <summary>
    /// 产品结构图
    /// </summary>
    [ObservableProperty]
    public partial FlowFileViewModel StructureGraph { get; set; }


    [ObservableProperty]
    public partial FlowFileViewModel SealedStructureGraph { get; set; }

    /// <summary>
    /// 嵌套承诺函
    /// </summary>
    [ObservableProperty]
    public partial FlowFileViewModel NestedCommitmentLetter { get; set; }


    [ObservableProperty]
    public partial FlowFileViewModel SealedNestedCommitmentLetter { get; set; }


    [ObservableProperty]
    public partial FlowFileViewModel RegistrationLetter { get; set; }






    [SetsRequiredMembers]
    public RegistrationFlowViewModel(RegistrationFlow flow) : base(flow)
    {
        CommitmentLetter = new FlowFileViewModel(FundId, FlowId, "备案承诺函", flow.CommitmentLetter?.Path, "Registration", nameof(RegistrationFlow.CommitmentLetter)) { Filter = "文本文档|*.doc;*.docx;*.wps;*.pdf;" };
        SealedCommitmentLetter = new FlowFileViewModel(FundId, FlowId, "备案承诺函", flow.SealedCommitmentLetter?.Path, "Registration", nameof(RegistrationFlow.SealedCommitmentLetter));
        SealedCommitmentLetter.Filter = "PDF (*.pdf)|*.pdf;";

        Prospectus = new FlowFileViewModel(FundId, FlowId, "招募说明书", flow.Prospectus?.Path, "Registration", nameof(RegistrationFlow.Prospectus)) { Filter = "文本文档|*.doc;*.docx;*.wps;*.pdf;" };
        SealedProspectus = new FlowFileViewModel(FundId, FlowId, "招募说明书", flow.SealedProspectus?.Path, "Registration", nameof(RegistrationFlow.SealedProspectus));
        SealedProspectus.Filter = "PDF (*.pdf)|*.pdf;";

        SealedContract = new FlowFileViewModel(FundId, FlowId, "基金合同", flow.SealedContract?.Path, "Registration", nameof(RegistrationFlow.SealedContract));
        SealedContract.Filter = "PDF (*.pdf)|*.pdf;";

        SealedAccountOversightProtocol = new FlowFileViewModel(FundId, FlowId, "募集账户监督协议", flow.SealedAccountOversightProtocol?.Path, "Registration", nameof(RegistrationFlow.SealedAccountOversightProtocol));
        SealedAccountOversightProtocol.Filter = "PDF (*.pdf)|*.pdf;";

        SealedOutsourcingServicesAgreement = new FlowFileViewModel(FundId, FlowId, "外包服务协议", flow.SealedOutsourcingServicesAgreement?.Path, "Registration", nameof(RegistrationFlow.SealedOutsourcingServicesAgreement));
        SealedOutsourcingServicesAgreement.Filter = "PDF (*.pdf)|*.pdf;";

        SealedInvestorList = new FlowFileViewModel(FundId, FlowId, "投资者明细", flow.SealedInvestorList?.Path, "Registration", nameof(RegistrationFlow.SealedInvestorList));
        SealedInvestorList.Filter = "PDF (*.pdf)|*.pdf;";


        StructureGraph = new FlowFileViewModel(FundId, FlowId, "产品结构图", flow.StructureGraph?.Path, "Registration", nameof(RegistrationFlow.StructureGraph)) { Filter = "文本文档|*.doc;*.docx;*.wps;*.pdf;" };
        SealedStructureGraph = new FlowFileViewModel(FundId, FlowId, "产品结构图", flow.SealedStructureGraph?.Path, "Registration", nameof(RegistrationFlow.SealedStructureGraph));
        SealedStructureGraph.Filter = "PDF (*.pdf)|*.pdf;";



        NestedCommitmentLetter = new FlowFileViewModel(FundId, FlowId, "嵌套承诺函", flow.NestedCommitmentLetter?.Path, "Registration", nameof(RegistrationFlow.NestedCommitmentLetter)) { Filter = "文本文档|*.doc;*.docx;*.wps;*.pdf;" };
        SealedNestedCommitmentLetter = new FlowFileViewModel(FundId, FlowId, "嵌套承诺函", flow.SealedNestedCommitmentLetter?.Path, "Registration", nameof(RegistrationFlow.SealedNestedCommitmentLetter));
        SealedNestedCommitmentLetter.Filter = "PDF (*.pdf)|*.pdf;";


        RegistrationLetter = new FlowFileViewModel(FundId, FlowId, "备案函", flow.RegistrationLetter?.Path, "Registration", nameof(RegistrationFlow.RegistrationLetter)) { Filter = "PDF (*.pdf)|*.pdf;" };


    }


    [RelayCommand]
    public void GenerateFile(FlowFileViewModel v)
    {
        switch (v.Name)
        {
            case "备案承诺函":
                try
                {
                    using var db = DbHelper.Base();
                    var fund = db.GetCollection<Fund>().FindById(FundId);
                    string path = @$"{FundHelper.GetFolder(FundId)}\Registration\{fund.Name}_备案承诺函.docx";
                    var fi = new FileInfo(path);
                    if (!fi.Directory!.Exists) fi.Directory.Create();

                    if (WordTpl.GenerateRegisterAnounce(fund, path))
                    {
                        if (CommitmentLetter.File?.Exists ?? false)
                            CommitmentLetter.File.Delete();
                        CommitmentLetter.SetFile(new System.IO.FileInfo(path));
                    }
                    else HandyControl.Controls.Growl.Error("生成备案承诺函失败，请查看Log，检查模板是否存在");
                }
                catch { }
                break;
            default:
                break;
        }
    }

}
