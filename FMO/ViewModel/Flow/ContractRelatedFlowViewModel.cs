using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.PDF;
using FMO.Shared;
using FMO.Utilities;
using Serilog;
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

    [ObservableProperty]
    public partial string? Requirement { get; set; }

    public ShareClassViewModel() { }

    [SetsRequiredMembers]
    public ShareClassViewModel(ShareClass s)
    {
        Id = s.Id;
        Name = s.Name;
        Requirement = s.Requirement;
    }
}



public abstract partial class ContractRelatedFlowViewModel : FlowViewModel, IElementChangable, IFileSetter
{
    /// <summary>
    /// 定稿合同
    /// </summary>
    //[ObservableProperty]
    //    public partial FlowFileViewModel? Contract { get; set; }


    public FileViewModel Contract { get; }

    /// <summary>
    /// 募集账户函
    /// </summary>
    public FileViewModel CollectionAccount { get; set; }


    /// <summary>
    /// 托管账户函
    /// </summary>
    public FileViewModel CustodyAccount { get; set; }


    [ObservableProperty]
    public partial ObservableCollection<ShareClassViewModel> Shares { get; set; }


    [ObservableProperty]
    public partial bool IsDividingShare { get; set; }


    public FileViewModel RiskDisclosureDocument { get; set; }

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
        //Contract = new(FundId, FlowId, "合同定稿", flow.ContractFile?.Path, "Contract", nameof(ContractFlow.ContractFile));

        Contract = new()
        {
            Label = "合同定稿",
            SaveFolder = FundHelper.GetFolder(FundId, "Contract"),
            GetProperty = x => x switch { ContractFlow f => f.ContractFile, _ => null },
            SetProperty = (x, y) => { if (x is ContractFlow f) f.ContractFile = y; },
            Filter = "文本|*.docx;*.doc;*.pdf"
        };
        Contract.Init(flow);

        RiskDisclosureDocument = new()
        {
            Label = "风险揭示书",
            SaveFolder = FundHelper.GetFolder(FundId, "Contract"),
            GetProperty = x => x switch { ContractFlow f => f.RiskDisclosureDocument, _ => null },
            SetProperty = (x, y) => { if (x is ContractFlow f) f.RiskDisclosureDocument = y; },
            Filter = "文本|*.docx;*.doc;*.pdf"
        };
        RiskDisclosureDocument.Init(flow);

        CollectionAccount = new()
        {
            Label = "募集账户函",
            SaveFolder = FundHelper.GetFolder(FundId, "Account"),
            GetProperty = x => x switch { ContractFlow f => f.CollectionAccountFile, _ => null },
            SetProperty = (x, y) => { if (x is not ContractFlow f) return; f.CollectionAccountFile = y; UpdateElement(y?.Path is null ? null : new FileInfo(y.Path), x => x.CollectionAccount, FundAccountType.Collection); },
            Filter = "文本|*.docx;*.doc;*.pdf"
        };
        CollectionAccount.Init(flow);

        CustodyAccount = new()
        {
            Label = "托管账户函",
            SaveFolder = FundHelper.GetFolder(FundId, "Account"),
            GetProperty = x => x switch { ContractFlow f => f.CustodyAccountFile, _ => null },
            SetProperty = (x, y) => { if (x is not ContractFlow f) return; f.CustodyAccountFile = y; UpdateElement(y?.Path is null ? null : new FileInfo(y.Path), x => x.CustodyAccount, FundAccountType.Custody); },
            Filter = "文本|*.docx;*.doc;*.pdf"
        };
        CustodyAccount.Init(flow);


        if (flow is ContractFinalizeFlow)
        {
            // 如果没有设置过shareclass，设一个默认值 
            if (shareClass?.Value is null || shareClass.Value.Length == 0)
            {
                shareClass!.SetValue([new ShareClass { Id = IdGenerator.GetNextId(nameof(ShareClass)), Name = FundElements.SingleShareKey }], flow.Id);
                using var db = DbHelper.Base();
                var el = db.GetCollection<FundElements>().FindById(FundId);
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
            using var db = DbHelper.Base();
            shareClass = db.GetCollection<FundElements>().FindById(FundId)?.ShareClasses;
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
        using var db = DbHelper.Base();
        var elements = db.GetCollection<FundElements>().FindById(FundId);

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


    //[RelayCommand]
    //public void ChooseFile(FileViewModel<ContractFlow> file)
    //{
    //    var fd = new OpenFileDialog();
    //    fd.Filter = file.Filter;
    //    if (fd.ShowDialog() != true)
    //        return;

    //    SetFile(file, fd.FileName);
    //}


    //public void SetFile(IFileViewModel? file, string path)
    //{
    //    if (file is FileViewModel<ContractFlow> ff)
    //    {
    //        ff.File = new FileInfo(path);

    //        using var db = DbHelper.Base();
    //        var flow = db.GetCollection<FundFlow>().FindById(FlowId) as ContractFlow;
    //        if (flow is ContractFlow f)
    //        {
    //            ff.SetProperty(flow, ff.Build());
    //            db.GetCollection<FundFlow>().Update(flow);
    //        }
    //    }
    //}




    //[RelayCommand]
    //public void Clear(FileViewModel<ContractFlow> file)
    //{
    //    if (file is null) return;

    //    var r = HandyControl.Controls.MessageBox.Show("是否删除文件", "提示", MessageBoxButton.YesNoCancel);
    //    if (r == MessageBoxResult.Cancel) return;

    //    if (r == MessageBoxResult.Yes) file.File?.Delete();

    //    using var db = DbHelper.Base();
    //    var flow = db.GetCollection<FundFlow>().FindById(FlowId) as ContractFlow;
    //    file.SetProperty(flow!, null);
    //    db.GetCollection<FundFlow>().Update(flow!);
    //    file.File = null;
    //}


    /// <summary>
    /// 如果拆分份额但未保存，则恢复为原始值
    /// </summary>
    /// <param name="value"></param>
    partial void OnIsDividingShareChanged(bool value)
    {
        if (!_shareChanged || value) return;

        InitShare();
    }



    private void UpdateElement(FileInfo? x, Func<FundElements, Mutable<BankAccount>> property, FundAccountType accountType)
    {
        Task.Run(() =>
        {
            try
            {
                if (x?.Exists ?? false)
                {
                    using var fs = x.OpenRead();
                    var ac = PdfHelper.GetAccountInfo(fs);

                    if (ac is not null)
                    {
                        using var db = DbHelper.Base();
                        var ele = db.GetCollection<FundElements>().FindById(FundId);
                        property(ele).SetValue(ac.First(), FlowId);
                        db.GetCollection<FundElements>().Update(ele);
                        WeakReferenceMessenger.Default.Send(new ElementChangedBackgroundMessage(FundId, FlowId));
                        WeakReferenceMessenger.Default.Send(new FundAccountChangedMessage(FundId, accountType));
                    }
                }
                else
                {
                    using var db = DbHelper.Base();
                    var ele = db.GetCollection<FundElements>().FindById(FundId);
                    property(ele).RemoveValue(FlowId);
                    db.GetCollection<FundElements>().Update(ele);
                    WeakReferenceMessenger.Default.Send(new ElementChangedBackgroundMessage(FundId, FlowId));
                    WeakReferenceMessenger.Default.Send(new FundAccountChangedMessage(FundId, accountType));
                }
                Log.Information($"设置 {accountType} 账户成功 {FundId}.{FlowId}");
            }
            catch (Exception e) { Log.Error($"设置 {accountType} 账户出错 {FundId}.{FlowId} {e}"); }
        });
    }

}
