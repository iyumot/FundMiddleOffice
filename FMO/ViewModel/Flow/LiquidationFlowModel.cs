using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Shared;
using FMO.TPL;
using FMO.Utilities;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace FMO;

public partial class LiquidationFlowViewModel : FlowViewModel
{

    public FileViewModel  LiquidationReport { get; }
                         
    public FileViewModel  SealedLiquidationReport { get; set; }
                         
    public FileViewModel  CommitmentLetter { get; }
                         
    public FileViewModel  SealedCommitmentLetter { get; }
                         
    public FileViewModel  InvestorSheet { get; }
                         
    public FileViewModel  LiquidationSheet { get; }




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

        SealedLiquidationReport = new()
        {
            Label = "清算报告",
            SaveFolder = FundHelper.GetFolder(FundId, "Liquidation"),
            GetProperty = x => x switch { LiquidationFlow f => f.SealedLiquidationReport, _ => null },
            SetProperty = (x, y) => { if (x is LiquidationFlow f) f.SealedLiquidationReport = y; },
        }; SealedLiquidationReport.Init(flow);


        CommitmentLetter = new()
        {
            Label = "清算承诺函",
            SaveFolder = FundHelper.GetFolder(FundId, "Liquidation"),
            GetProperty = x => x switch { LiquidationFlow f => f.CommitmentLetter, _ => null },
            SetProperty = (x, y) => { if (x is LiquidationFlow f) f.CommitmentLetter = y; },
        }; CommitmentLetter.Init(flow);

        SealedCommitmentLetter = new()
        {
            Label = "清算承诺函",
            SaveFolder = FundHelper.GetFolder(FundId, "Liquidation"),
            SpecificFileName = x =>
            {
                using var db = DbHelper.Base();
                var fund = db.GetCollection<Fund>().FindById(FundId);

                return $"{fund.Name}_清算承诺函_{Date:yyyy年MM月dd日}{x}";
            },
            GetProperty = x => x switch { LiquidationFlow f => f.SealedCommitmentLetter, _ => null },
            SetProperty = (x, y) => { if (x is LiquidationFlow f) f.SealedCommitmentLetter = y; },
        }; SealedCommitmentLetter.Init(flow);


        InvestorSheet = new()
        {
            Label = "投资者信息表",
            SaveFolder = FundHelper.GetFolder(FundId, "Liquidation"),
            GetProperty = x => x switch { LiquidationFlow f => f.InvestorSheet, _ => null },
            SetProperty = (x, y) => { if (x is LiquidationFlow f) f.InvestorSheet = y; },
        }; InvestorSheet.Init(flow);


        LiquidationSheet = new()
        {
            Label = "清算情况表",
            SaveFolder = FundHelper.GetFolder(FundId, "Liquidation"),
            GetProperty = x => x switch { LiquidationFlow f => f.LiquidationSheet, _ => null },
            SetProperty = (x, y) => { if (x is LiquidationFlow f) f.LiquidationSheet = y; },
        }; LiquidationSheet.Init(flow);



        Initialized = true;
    }


    [RelayCommand]
    public void GenerateFile(FileViewModel v)
    {

        if (v == CommitmentLetter)
        {
            try
            {
                using var db = DbHelper.Base();
                var fund = db.GetCollection<Fund>().FindById(FundId);
                string path = @$"{v.SaveFolder}\{fund.Name}_清算承诺函.docx";
                var fi = new FileInfo(path);
                if (!fi.Directory!.Exists) fi.Directory.Create();

                if (Tpl.Generate(path, Tpl.GetPath("清算承诺函.docx"), new { Name = fund.Name, Code = fund.Code, Date = Date?.ToString("yyyy年MM月dd日") ?? "yyyy年MM月dd日" }))
                {
                    if (CommitmentLetter.File?.Exists ?? false)
                        CommitmentLetter.File.Delete();
                    SetFile(v, path);
                }
                else HandyControl.Controls.Growl.Error("生成承诺函失败，请查看Log，检查模板是否存在");
            }
            catch { }
        }
    }


    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        //
        if (e.PropertyName == nameof(LiquidationFlowViewModel.IsReadOnly) && IsReadOnly && Date is DateTime d && d != default)
        {
            using var db = DbHelper.Base();
            if (db.GetCollection<Fund>().FindById(FundId) is Fund f)
            {
                if (f.Status < FundStatus.StartLiquidation)
                    f.Status = FundStatus.StartLiquidation;

                var old = f.ClearDate;

                f.ClearDate = DateOnly.FromDateTime(Date.Value);
                db.GetCollection<Fund>().Update(f);

                DataTracker.OnEntityChanged(new EntityChanged<Fund, DateOnly>(f, nameof(Fund.ClearDate), old, f.ClearDate));
                WeakReferenceMessenger.Default.Send(new EntityChangedMessage<Fund, FundStatus>(f, nameof(Fund.Status), f.Status));
                WeakReferenceMessenger.Default.Send(new EntityChangedMessage<Fund, DateOnly>(f, nameof(Fund.ClearDate), f.ClearDate));
            }
        }
    }
}