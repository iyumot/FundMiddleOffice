using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using System.Diagnostics.CodeAnalysis;

namespace FMO;

public partial class RegistrationFlowViewModel : FlowViewModel
{ 

    [ObservableProperty]
    public partial PredefinedFileViewModel? CommitmentLetter { get; set; }


    [ObservableProperty]
    public partial PredefinedFileViewModel? SealedCommitmentLetter { get; set; }




    [SetsRequiredMembers]
    public RegistrationFlowViewModel(RegistrationFlow flow) : base(flow)
    {
        CommitmentLetter = new PredefinedFileViewModel(FundId, FlowId, "备案承诺函", flow.CommitmentLetter?.Path, "Registration", nameof(RegistrationFlow.CommitmentLetter)) { Filter = "文本文档|*.doc;*.docx;*.wps;*.pdf;"};
        SealedCommitmentLetter = new PredefinedFileViewModel(FundId, FlowId, "备案承诺函", flow.SealedCommitmentLetter?.Path, "Registration", nameof(RegistrationFlow.SealedCommitmentLetter));
        SealedCommitmentLetter.Filter = "PDF (*.pdf)|*.pdf;";
    }
}
