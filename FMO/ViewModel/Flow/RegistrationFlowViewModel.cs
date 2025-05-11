using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Shared;
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
    public partial FileViewModel CommitmentLetter { get; set; }


    [ObservableProperty]
    public partial FileViewModel SealedCommitmentLetter { get; set; }


    /// <summary>
    /// 招募说明书
    /// </summary>
    [ObservableProperty]
    public partial FileViewModel Prospectus { get; set; }


    [ObservableProperty]
    public partial FileViewModel SealedProspectus { get; set; }


    /// <summary>
    /// 基金合同 用印
    /// </summary>
    [ObservableProperty]
    public partial FileViewModel SealedContract { get; set; }




    /// <summary>
    /// 募集账户监督协议
    /// </summary>
    [ObservableProperty]
    public partial FileViewModel SealedAccountOversightProtocol { get; set; }

    /// <summary>
    /// 外包服务协议
    /// </summary>
    [ObservableProperty]
    public partial FileViewModel SealedOutsourcingServicesAgreement { get; set; }

    /// <summary>
    /// 投资者明细
    /// </summary>
    // [ObservableProperty]
    //  public partial FileViewModel SealedInvestorList { get; set; }

    /// <summary>
    /// 产品结构图
    /// </summary>
    [ObservableProperty]
    public partial FileViewModel StructureGraph { get; set; }


    [ObservableProperty]
    public partial FileViewModel SealedStructureGraph { get; set; }

    /// <summary>
    /// 嵌套承诺函
    /// </summary>
    [ObservableProperty]
    public partial FileViewModel NestedCommitmentLetter { get; set; }


    [ObservableProperty]
    public partial FileViewModel SealedNestedCommitmentLetter { get; set; }


    [ObservableProperty]
    public partial FileViewModel RegistrationLetter { get; set; }






    [SetsRequiredMembers]
    public RegistrationFlowViewModel(RegistrationFlow flow) : base(flow)
    {
        CommitmentLetter = new()
        {
            Label = "备案承诺函",
            Filter = "文本文档|*.doc;*.docx;*.wps;*.pdf;",
            SaveFolder = FundHelper.GetFolder(FundId, "Registration"),
            GetProperty = x => x switch { RegistrationFlow f => f.CommitmentLetter, _ => null },
            SetProperty = (x, y) => { if (x is RegistrationFlow f) f.CommitmentLetter = y; },
        };
        CommitmentLetter.Init(flow);



        SealedCommitmentLetter = new()
        {
            Label = "备案承诺函",
            Filter = "PDF (*.pdf)|*.pdf;",
            SaveFolder = FundHelper.GetFolder(FundId, "Registration"),
            GetProperty = x => x switch { RegistrationFlow f => f.SealedCommitmentLetter, _ => null },
            SetProperty = (x, y) => { if (x is RegistrationFlow f) f.SealedCommitmentLetter = y; },
        };
        SealedCommitmentLetter.Init(flow);



        Prospectus = new()
        {
            Label = "招募说明书",
            Filter = "文本文档|*.doc;*.docx;*.wps;*.pdf;",
            SaveFolder = FundHelper.GetFolder(FundId, "Registration"),
            GetProperty = x => x switch { RegistrationFlow f => f.Prospectus, _ => null },
            SetProperty = (x, y) => { if (x is RegistrationFlow f) f.Prospectus = y; },
        }; Prospectus.Init(flow);

        SealedProspectus = new()
        {
            Label = "招募说明书",
            Filter = "PDF (*.pdf)|*.pdf;",
            SaveFolder = FundHelper.GetFolder(FundId, "Registration"),
            GetProperty = x => x switch { RegistrationFlow f => f.SealedProspectus, _ => null },
            SetProperty = (x, y) => { if (x is RegistrationFlow f) f.SealedProspectus = y; },
        };
        SealedProspectus.Init(flow); 

        SealedContract = new()
        {
            Label = "基金合同",
            Filter = "PDF (*.pdf)|*.pdf;",
            SaveFolder = FundHelper.GetFolder(FundId, "Registration"),
            GetProperty = x => x switch { RegistrationFlow f => f.SealedContract, _ => null },
            SetProperty = (x, y) => { if (x is RegistrationFlow f) f.SealedContract = y; },
        }; SealedContract.Init(flow);

        SealedAccountOversightProtocol = new()
        {
            Label = "募集账户监督协议",
            Filter = "PDF (*.pdf)|*.pdf;",
            SaveFolder = FundHelper.GetFolder(FundId, "Registration"),
            GetProperty = x => x switch { RegistrationFlow f => f.SealedAccountOversightProtocol, _ => null },
            SetProperty = (x, y) => { if (x is RegistrationFlow f) f.SealedAccountOversightProtocol = y; },
        }; SealedAccountOversightProtocol.Init(flow);

        SealedOutsourcingServicesAgreement = new()
        {
            Label = "外包服务协议",
            Filter = "PDF (*.pdf)|*.pdf;",
            SaveFolder = FundHelper.GetFolder(FundId, "Registration"),
            GetProperty = x => x switch { RegistrationFlow f => f.SealedOutsourcingServicesAgreement, _ => null },
            SetProperty = (x, y) => { if (x is RegistrationFlow f) f.SealedOutsourcingServicesAgreement = y; },
        }; SealedOutsourcingServicesAgreement.Init(flow);



        StructureGraph = new()
        {
            Label = "产品结构图",
            SaveFolder = FundHelper.GetFolder(FundId, "Registration"),
            GetProperty = x => x switch { RegistrationFlow f => f.StructureGraph, _ => null },
            SetProperty = (x, y) => { if (x is RegistrationFlow f) f.StructureGraph = y; },
        };
        StructureGraph.Init(flow);

        SealedStructureGraph = new()
        {
            Label = "产品结构图",
            Filter = "PDF (*.pdf)|*.pdf;",
            SaveFolder = FundHelper.GetFolder(FundId, "Registration"),
            GetProperty = x => x switch { RegistrationFlow f => f.SealedStructureGraph, _ => null },
            SetProperty = (x, y) => { if (x is RegistrationFlow f) f.SealedStructureGraph = y; },
        }; SealedStructureGraph.Init(flow);



        NestedCommitmentLetter = new()
        {
            Label = "嵌套承诺函",
            SaveFolder = FundHelper.GetFolder(FundId, "Registration"),
            GetProperty = x => x switch { RegistrationFlow f => f.NestedCommitmentLetter, _ => null },
            SetProperty = (x, y) => { if (x is RegistrationFlow f) f.NestedCommitmentLetter = y; },
        }; NestedCommitmentLetter.Init(flow);
        SealedNestedCommitmentLetter = new()
        {
            Label = "嵌套承诺函",
            Filter = "PDF (*.pdf)|*.pdf;",
            SaveFolder = FundHelper.GetFolder(FundId, "Registration"),
            GetProperty = x => x switch { RegistrationFlow f => f.SealedNestedCommitmentLetter, _ => null },
            SetProperty = (x, y) => { if (x is RegistrationFlow f) f.SealedNestedCommitmentLetter = y; },
        }; SealedNestedCommitmentLetter.Init(flow);


        RegistrationLetter = new()
        {
            Label = "备案函",
            SaveFolder = FundHelper.GetFolder(FundId, "Registration"),
            GetProperty = x => x switch { RegistrationFlow f => f.RegistrationLetter, _ => null },
            SetProperty = (x, y) => { if (x is RegistrationFlow f) f.RegistrationLetter = y; },
        }; RegistrationLetter.Init(flow);
        RegistrationLetter.Init(flow);
    }


    [RelayCommand]
    public void GenerateFile(FileViewModel v)
    {

        if (v == CommitmentLetter)
        {
            try
            {
                using var db = DbHelper.Base();
                var fund = db.GetCollection<Fund>().FindById(FundId);
                string path = @$"{v.SaveFolder}\{fund.Name}_备案承诺函.docx";
                var fi = new FileInfo(path);
                if (!fi.Directory!.Exists) fi.Directory.Create();

                if (WordTpl.GenerateRegisterAnounce(fund, path))
                {
                    if (CommitmentLetter.File?.Exists ?? false)
                        CommitmentLetter.File.Delete();
                    SetFile(v, path);
                }
                else HandyControl.Controls.Growl.Error("生成备案承诺函失败，请查看Log，检查模板是否存在");
            }
            catch { }
        }
    }

}
