﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;

namespace FMO;

public partial class ContractFinalizeFlowViewModel : FlowViewModel, IElementChangable
{
    private const string SingleShareName = "单一份额";

    /// <summary>
    /// 定稿合同
    /// </summary>
    [ObservableProperty]
    public partial FileInfo? Contract { get; set; }


    [ObservableProperty]
    public partial FileInfo? RiskDisclosureDocument { get; set; }


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


    [SetsRequiredMembers]
    public ContractFinalizeFlowViewModel(ContractFinalizeFlow flow, Mutable<ShareClass[]>? shareClass) : base(flow)
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
            Shares = new ObservableCollection<string>([SingleShareName]);


        Initialized = true;
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

        ///最大5类
        if (Shares.Count > 5) return;

        if (Shares.Count == 1)
            Shares[0] = "A";
        Shares.Add(((char)('A' + Shares.Count)).ToString());
    }

    [RelayCommand]
    public void DeleteShare(string s)
    {
        if (s == SingleShareName)
            return;

        Shares.Remove(s);
        if (Shares.Count == 1)
        {
            RemainShare = Shares[0];
            Shares[0] = SingleShareName;
        }
    }

    [RelayCommand]
    public void ConfirmShares()
    {
        IsDividingShare = false;

        using var db = new BaseDatabase();
        var elements = db.GetCollection<FundElements>().FindOne(x=>x.FundId == FundId);

        if(elements.ShareClasses is not null && elements.ShareClasses.GetValue(FlowId) is var d && d.Value is not null && d.Value.Length>= Shares.Count && (!d.Value?.Select(x=>x.Name).ToArray().SequenceEqual(Shares)?? false ))
        {
            if(MessageBoxResult.Cancel == MessageBox.Show("减少份额种类将会删除对应份额相关的要素", "", MessageBoxButton.OKCancel))
            {  
                if (d.Value is ShareClass[] shares)
                    Shares = new ObservableCollection<string>(shares.Select(x => x.Name));
                else
                    Shares = new ObservableCollection<string>([SingleShareName]);

                return;
            }
        }





        if (elements.ShareClasses is null)
            elements.ShareClasses = new(nameof(FundElements.ShareClasses), Shares.Select(x => new ShareClass { Name = x }).ToArray());
        else
            elements.ShareClasses.SetValue(Shares.Select(x => new ShareClass { Name = x }).ToArray(), FlowId);

        //elements.Remove()

        db.GetCollection<FundElements>().Update(elements);

        WeakReferenceMessenger.Default.Send(new FundShareChangedMessage { FundId = FundId, FlowId = FlowId });
    }

}


public class FundShareChangedMessage
{
    public int FundId { get; set; }

    public int FlowId { get; set; }

}