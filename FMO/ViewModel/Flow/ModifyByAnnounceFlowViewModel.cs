using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using FMO.Utilities;

namespace FMO;

public partial class ModifyByAnnounceFlowViewModel : FlowViewModel
{
    [ObservableProperty]
    public partial FlowFileViewModel?  Announcement { get; set; }

    [ObservableProperty]
    public partial FlowFileViewModel? SealedAnnouncement { get; set; }

    [SetsRequiredMembers]
    public ModifyByAnnounceFlowViewModel(ModifyByAnnounceFlow flow) : base(flow)
    {
        Announcement = new(FundId, FlowId, "变更公告", flow.Announcement?.Path, "Announcement", nameof(ContractModifyFlowViewModel.Announcement)) { Filter = "文档 (*.pdf,*.doc,*.docx)|*.pdf;*.doc;*.docx;" };
        SealedAnnouncement = new(FundId, FlowId, "变更公告", flow.SealedAnnouncement?.Path, "Announcement", nameof(ContractModifyFlowViewModel.SealedAnnouncement))
        {
            Filter = "PDF (*.pdf)|*.pdf;",
            SpecificFileName = () =>
            {
                using var db = DbHelper.Base();
                var fund = db.GetCollection<Fund>().FindById(FundId);

                return $"{fund.Name}_变更公告_{Date:yyyy年MM月dd日}.pdf";
            }
        };
    }
}
