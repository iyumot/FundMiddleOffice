using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using FMO.Shared;
using FMO.Utilities;
using System.Diagnostics.CodeAnalysis;

namespace FMO;

public partial class LiquidationFlowViewModel : FlowViewModel
{

    [ObservableProperty]
    public partial FileViewModel? LiquidationReport { get; set; }

    [SetsRequiredMembers]
    public LiquidationFlowViewModel(LiquidationFlow flow) : base(flow)
    {
        LiquidationReport = new()
        {
            Label = "清算报告",
            SaveFolder = FundHelper.GetFolder(FundId, "Liquidation"),
            GetProperty = x => x switch { LiquidationFlow f => f.LiquidationReport, _ => null },
            SetProperty = (x, y) => { if (x is LiquidationFlow f) f.LiquidationReport = y; },
        }; LiquidationReport.Init(flow);





        Initialized = true;
    }
}