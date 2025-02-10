using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using System.Diagnostics.CodeAnalysis;

namespace FMO;

public partial class LiquidationFlowViewModel:FlowViewModel
{

    [ObservableProperty]
    public partial PredefinedFileViewModel? LiquidationReport { get; set; }

    [SetsRequiredMembers]
    public LiquidationFlowViewModel(LiquidationFlow flow) : base(flow)
    {
        LiquidationReport = new(FundId, FlowId, "清算报告", flow.LiquidationReport?.Path, "Liquidation", nameof(LiquidationFlow.LiquidationReport)); 

        Initialized = true;
    }
}