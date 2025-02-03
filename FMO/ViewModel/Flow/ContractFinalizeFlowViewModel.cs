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
        if (!string.IsNullOrWhiteSpace(flow.ContractFile?.Path))
            Contract = new FileInfo(flow.ContractFile.Path);


        if (!string.IsNullOrWhiteSpace(flow.CollectionAccountFile?.Path))
            CollectionAccount = new FileInfo(flow.CollectionAccountFile.Path);


        if (!string.IsNullOrWhiteSpace(flow.CustodyAccountFile?.Path))
            CustodyAccount = new FileInfo(flow.CustodyAccountFile.Path);


        if (shareClass is not null && shareClass.GetValue(FlowId).Value is ShareClass[] shares)
            Shares = new ObservableCollection<string>(shares.Select(x => x.Name));
        else
            Shares = new ObservableCollection<string>([FundElements.SingleShareKey]);


        Initialized = true;
    }



     


}


public class FundShareChangedMessage
{
    public int FundId { get; set; }

    public int FlowId { get; set; }

}