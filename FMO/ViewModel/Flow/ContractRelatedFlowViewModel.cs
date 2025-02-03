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

public abstract partial class ContractRelatedFlowViewModel : FlowViewModel, IElementChangable
{
    /// <summary>
    /// 定稿合同
    /// </summary>
    [ObservableProperty]
    public partial FileInfo? Contract { get; set; }

    /// <summary>
    /// 募集账户函
    /// </summary>
    [ObservableProperty]
    public partial FileInfo? CollectionAccount { get; set; }


    /// <summary>
    /// 托管账户函
    /// </summary>
    [ObservableProperty]
    public partial FileInfo? CustodyAccount { get; set; }


    [ObservableProperty]
    public partial ObservableCollection<string> Shares { get; set; }


    [ObservableProperty]
    public partial bool IsDividingShare { get; set; }

    /// <summary>
    /// 如何删除后，留下的
    /// </summary>
    private string? RemainShare { get; set; }

    /// <summary>
    /// 份额类型有变动
    /// </summary>
    private bool _shareChanged;


    [SetsRequiredMembers]
    public ContractRelatedFlowViewModel(ContractFlow flow, Mutable<ShareClass[]>? shareClass) : base(flow)
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
    }




    partial void OnCollectionAccountChanged(FileInfo? oldValue, FileInfo? newValue)
    {
        if (!Initialized) return;

        if (newValue?.Exists ?? false)
        {

            SaveFile<ContractFinalizeFlow>(newValue, "Accounts", x => x.CollectionAccountFile, x => x.CollectionAccountFile = new FundFileInfo { Name = "募集账户函" });

        }

    }


    [RelayCommand]
    public void DivideShares()
    {
        IsDividingShare = true;
        _shareChanged = true;

        ///最大5类
        if (Shares.Count > 5) return;

        if (Shares.Count == 1)
            Shares[0] = "A";
        Shares.Add(((char)('A' + Shares.Count)).ToString());
    }

    [RelayCommand]
    public void DeleteShare(string s)
    {
        if (s == FundElements.SingleShareKey)
            return;

        Shares.Remove(s);
        if (Shares.Count == 1)
        {
            RemainShare = Shares[0];
            Shares[0] = FundElements.SingleShareKey;
        }
    }

    [RelayCommand]
    public void ConfirmShares()
    {

        using var db = new BaseDatabase();
        var elements = db.GetCollection<FundElements>().FindOne(x => x.FundId == FundId);

        //// 同步份额相关的要素 
        var rem = Shares.Count == 1 ? [RemainShare ?? Shares[0]] : Shares.ToArray();
        var old = elements.ShareClasses is null ? [] : elements.ShareClasses.GetValue(FlowId) is var dd && dd.FlowId != FlowId ? [] : dd.Value?.Select(x => x.Name).ToArray() ?? [];

        var remove = old.Except(rem).ToArray();
        var add = rem.Except(old).ToArray();


        if ((remove.Length != 0 && !(remove.Length == 1 && remove[0] == FundElements.SingleShareKey)) && MessageBoxResult.Cancel == HandyControl.Controls.MessageBox.Show("减少份额种类将会删除对应份额相关的要素", "", MessageBoxButton.OKCancel))
        {
            if (elements.ShareClasses is not null && elements.ShareClasses.GetValue(FlowId) is var d && d.Value is ShareClass[] shares)
                Shares = new ObservableCollection<string>(shares.Select(x => x.Name));
            else
                Shares = new ObservableCollection<string>([FundElements.SingleShareKey]);

            return;
        }


        elements.ShareClassChange(FlowId, Shares.ToArray(), add, remove);
       

        db.GetCollection<FundElements>().Update(elements);

        WeakReferenceMessenger.Default.Send(new FundShareChangedMessage { FundId = FundId, FlowId = FlowId });


        _shareChanged = false;
        IsDividingShare = false;
    }


    /// <summary>
    /// 如果拆分份额但未保存，则恢复为原始值
    /// </summary>
    /// <param name="value"></param>
    partial void OnIsDividingShareChanged(bool value)
    {
        if (!_shareChanged || value) return;

        var db = new BaseDatabase();
        var shareClass = db.GetCollection<FundElements>().FindOne(x => x.FundId == FundId)?.ShareClasses;

        if (shareClass is not null && shareClass.GetValue(FlowId).Value is ShareClass[] shares)
            Shares = new ObservableCollection<string>(shares.Select(x => x.Name));
        else
            Shares = new ObservableCollection<string>([FundElements.SingleShareKey]);
    }
}
