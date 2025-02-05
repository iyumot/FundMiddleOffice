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


public partial class ShareClassViewModel : ObservableObject
{
    [ObservableProperty]
    public partial int Id { get; set; }

    [ObservableProperty]
    public required partial string Name { get; set; }



}
public abstract partial class ContractRelatedFlowViewModel : FlowViewModel, IElementChangable
{
    /// <summary>
    /// 定稿合同
    /// </summary>
    [ObservableProperty]
    public partial PredefinedFileViewModel? Contract { get; set; }

    /// <summary>
    /// 募集账户函
    /// </summary>
    [ObservableProperty]
    public partial PredefinedFileViewModel? CollectionAccount { get; set; }


    /// <summary>
    /// 托管账户函
    /// </summary>
    [ObservableProperty]
    public partial PredefinedFileViewModel? CustodyAccount { get; set; }


    [ObservableProperty]
    public partial ObservableCollection<ShareClassViewModel> Shares { get; set; }


    [ObservableProperty]
    public partial bool IsDividingShare { get; set; }



    [ObservableProperty]
    public partial PredefinedFileViewModel? RiskDisclosureDocument { get; set; }

    /// <summary>
    /// 份额分类
    /// </summary>
    [ObservableProperty]
    public partial bool ModifyShareClass { get; set; }


    /// <summary>
    /// 份额类型有变动
    /// </summary>
    private bool _shareChanged;


    [SetsRequiredMembers]
#pragma warning disable CS9264 // 退出构造函数时，不可为 null 的属性必须包含非 null 值。请考虑添加 ‘required’ 修饰符，或将属性声明为可为 null，或添加 ‘[field: MaybeNull, AllowNull]’ 特性。
    public ContractRelatedFlowViewModel(ContractFlow flow, Mutable<ShareClass[]>? shareClass) : base(flow)
#pragma warning restore CS9264 // 退出构造函数时，不可为 null 的属性必须包含非 null 值。请考虑添加 ‘required’ 修饰符，或将属性声明为可为 null，或添加 ‘[field: MaybeNull, AllowNull]’ 特性。
    {
        Contract = new(FundId, FlowId, "合同定稿" ,flow.ContractFile?.Path, "Contracts", nameof(ContractFlow.ContractFile));

        RiskDisclosureDocument = new(FundId, FlowId, "风险揭示书", flow.RiskDisclosureDocument?.Path,  "Contracts", nameof(ContractFlow.RiskDisclosureDocument));
         
        CollectionAccount = new(FundId, FlowId, "募集账户函", flow.CollectionAccountFile?.Path,  "Accounts", nameof(ContractFlow.CollectionAccountFile));

        CustodyAccount = new(FundId, FlowId, "托管账户函", flow.CustodyAccountFile?.Path, "Accounts", nameof(ContractFlow.CustodyAccountFile));



        if (flow is ContractFinalizeFlow)
        {
            // 如果没有设置过shareclass，设一个默认值 
            if (shareClass?.Value is null || shareClass.Value.Length == 0)
            {
                shareClass!.SetValue([new ShareClass { Id = IdGenerator.GetNextId(nameof(ShareClass)), Name = FundElements.SingleShareKey }], flow.Id);
                using var db = new BaseDatabase();
                var el = db.GetCollection<FundElements>().FindOne(x => x.FundId == FundId);
                el.ShareClasses = shareClass;
                db.GetCollection<FundElements>().Update(el);
            }
        }

        InitShare(shareClass);
    }
     
    public void InitShare(Mutable<ShareClass[]>? shareClass = null)
    {
        if (shareClass is null)
        {
            using var db = new BaseDatabase();
            shareClass = db.GetCollection<FundElements>().FindOne(x => x.FundId == FundId)?.ShareClasses;
        }

        if (shareClass is not null && shareClass.GetValue(FlowId).Value is ShareClass[] shares)
            Shares = new ObservableCollection<ShareClassViewModel>(shares.Select(x => new ShareClassViewModel { Id = x.Id, Name = x.Name }));
        else
            throw new Exception(); //Shares = new ObservableCollection<ShareClassViewModel>([new ShareClassViewModel { Id = IdGenerator.GetNextId(nameof(ShareClass)), Name = FundElements.SingleShareKey }]);

    }




    [RelayCommand]
    public void DivideShares()
    {
        IsDividingShare = true;
        _shareChanged = true;

        ///最大5类
        if (Shares.Count > 5) return;

        if (Shares.Count == 1)
            Shares[0].Name = "A";

        ShareClassViewModel newitem = new ShareClassViewModel { Id = IdGenerator.GetNextId(nameof(ShareClass)), Name = ((char)('A' + Shares.Count)).ToString() };
        Shares.Add(newitem);
    }

    [RelayCommand]
    public void DeleteShare(ShareClassViewModel s)
    {
        if (Shares.Count > 1)
            Shares.Remove(s);

        if (Shares.Count == 1)
            Shares[0].Name = FundElements.SingleShareKey;
    }

    [RelayCommand]
    public void ConfirmShares()
    {
        using var db = new BaseDatabase();
        var elements = db.GetCollection<FundElements>().FindOne(x => x.FundId == FundId);

        //// 同步份额相关的要素 
        var rem = Shares.Select(x => (x.Id, x.Name)).ToArray();
        var old = elements.ShareClasses is null ? [] : elements.ShareClasses.GetValue(FlowId) is var dd && dd.FlowId != FlowId ? [] : dd.Value?.Select(x => (x.Id, x.Name)).ToArray() ?? [];

        var remove = old.ExceptBy(rem.Select(x => x.Id), x => x.Id).ToArray();
        var add = rem.ExceptBy(old.Select(x => x.Id), x => x.Id).ToArray();
        var change = rem.Where(x => old.Any(y => y.Id == x.Id && y.Name != x.Name)).ToArray();


        if ((remove.Length != 0 && MessageBoxResult.Cancel == HandyControl.Controls.MessageBox.Show($"此操作将会删除份额[{string.Join(',', remove.Select(x => x.Name))}]相关的要素", "危险操作提示", MessageBoxButton.OKCancel)))
        {
            var shareClass = elements.ShareClasses;

            InitShare(shareClass);

            return;
        }


        elements.ShareClassChange(FlowId, add, remove, change);


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

        InitShare();
    }

    //partial void OnCollectionAccountChanged(FileInfo? oldValue, FileInfo? newValue)
    //{
    //    if (!Initialized) return;

    //    if (newValue?.Exists ?? false)
    //    {
    //        SaveFile<ContractFlow>(newValue, "Accounts", x => x.CollectionAccountFile, x => x.CollectionAccountFile = new FundFileInfo("募集账户函"));
    //    }

    //}

    //partial void OnCustodyAccountChanged(FileInfo? oldValue, FileInfo? newValue)
    //{
    //    if (!Initialized) return;

    //    if (newValue?.Exists ?? false)
    //    {
    //        SaveFile<ContractFlow>(newValue, "Accounts", x => x.CustodyAccountFile, x => x.CustodyAccountFile = new FundFileInfo("托管账户函"));
    //    }
    //}

    //partial void OnRiskDisclosureDocumentChanged(FileInfo? oldValue, FileInfo? newValue)
    //{
    //    if (!Initialized) return;

    //    if (newValue?.Exists ?? false)
    //    {
    //        SaveFile<ContractFlow>(newValue, "Contracts", x => x.RiskDisclosureDocument, x => x.RiskDisclosureDocument = new FundFileInfo("风险揭示书"));
    //    }
    //}

    //partial void OnContractChanged(FileInfo? oldValue, FileInfo? newValue)
    //{
    //    if (!Initialized) return;

    //    if (newValue?.Exists ?? false)
    //    {
    //        SaveFile<ContractFlow>(newValue, "Contracts", x => x.ContractFile, x => x.ContractFile = new FundFileInfo("定稿合同"));
    //    }

    //}
}
