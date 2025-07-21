using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using FMO.Shared;
using FMO.Utilities;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text.RegularExpressions;

namespace FMO;

public partial class ModifyByAnnounceFlowViewModel : FlowViewModel
{
    [ObservableProperty]
    public partial FileViewModel? Announcement { get; set; }

    [ObservableProperty]
    public partial FileViewModel? SealedAnnouncement { get; set; }

    [SetsRequiredMembers]
    public ModifyByAnnounceFlowViewModel(ModifyByAnnounceFlow flow) : base(flow)
    {



        Announcement = new()
        {
            Label = "变更公告",
            SaveFolder = FundHelper.GetFolder(FundId, "Announcement"),
            GetProperty = x => x switch { ModifyByAnnounceFlow f => f.Announcement, _ => null },
            SetProperty = (x, y) => { if (x is ModifyByAnnounceFlow f) f.Announcement = y; },
        }; Announcement.Init(flow);


        SealedAnnouncement = new()
        {
            Label = "变更公告",
            Filter = "PDF (*.pdf)|*.pdf;",
            SpecificFileName = x =>
            {
                using var db = DbHelper.Base();
                var fund = db.GetCollection<Fund>().FindById(FundId);

                return $"{fund.Name}_变更公告_{Date:yyyy年MM月dd日}.pdf";
            },
            SaveFolder = FundHelper.GetFolder(FundId, "Announcement"),
            GetProperty = x => x switch { ModifyByAnnounceFlow f => f.SealedAnnouncement, _ => null },
            SetProperty = (x, y) => { if (x is ModifyByAnnounceFlow f) f.SealedAnnouncement = y; },
        }; SealedAnnouncement.Init(flow);

    }

     

}
