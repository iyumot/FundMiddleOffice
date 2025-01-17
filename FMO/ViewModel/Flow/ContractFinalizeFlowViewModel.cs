﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Models;
using FMO.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;

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



    public ObservableCollection<string> Shares { get; set; }


    [ObservableProperty]
    public partial bool IsDividingShare { get; set; }



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
            Shares[0] = SingleShareName;
    }

    [RelayCommand]
    public void ConfirmShares()
    {
        IsDividingShare = false;

        using var db = new BaseDatabase();
        var elements = db.GetCollection<FundElements>().FindById(FundId);

        if (elements.ShareClasses is null)
            elements.ShareClasses = new(nameof(FundElements.ShareClasses), Shares.Select(x => new ShareClass { Name = x }).ToArray());

        else
            elements.ShareClasses.SetValue(Shares.Select(x => new ShareClass { Name = x }).ToArray(), FlowId);

        db.GetCollection<FundElements>().Update(elements);
    }

}
