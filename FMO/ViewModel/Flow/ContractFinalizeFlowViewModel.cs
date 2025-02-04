using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;

namespace FMO;

public partial class ContractFinalizeFlowViewModel : ContractRelatedFlowViewModel, IElementChangable
{ 


    [ObservableProperty]
    public partial FileInfo? RiskDisclosureDocument { get; set; }

     

    /// <summary>
    /// 如何删除后，留下的
    /// </summary>
    private string? RemainShare { get; set; }

    /// <summary>
    /// 份额类型有变动
    /// </summary>
    private bool _shareChanged;


    [SetsRequiredMembers]
    public ContractFinalizeFlowViewModel(ContractFinalizeFlow flow, Mutable<ShareClass[]>? shareClass) : base(flow, shareClass)
    {

        Initialized = true;
    }



     


}


public class FundShareChangedMessage
{
    public int FundId { get; set; }

    public int FlowId { get; set; }

}