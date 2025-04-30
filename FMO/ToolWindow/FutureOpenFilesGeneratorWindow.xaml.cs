using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging; 
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using FMO.Models;
using FMO.TPL;
using FMO.Utilities;
using MiniSoftware;

namespace FMO;

/// <summary>
/// FutureOpenFilesGeneratorWindow.xaml 的交互逻辑
/// </summary>
public partial class FutureOpenFilesGeneratorWindow : Window
{
    public FutureOpenFilesGeneratorWindow()
    {
        InitializeComponent();
    }
}

public partial class FutureOpenFilesGeneratorWindowViewModel:ObservableObject
{
    public FutureOpenFilesGeneratorWindowViewModel()
    {
        using var db = DbHelper.Base();
        var members = db.GetCollection<Participant>().FindAll();

        OpenAgents = new(members.Where(x => x.Role.HasFlag(PersonRole.Agent)));
        OrderPlacers = new(members.Where(x => x.Role.HasFlag(PersonRole.OrderPlacer)));
        FundTransferors = new(members.Where(x => x.Role.HasFlag(PersonRole.FundTransferor)));
        ConfirmationPersons = new(members.Where(x => x.Role.HasFlag(PersonRole.ConfirmationPerson)));
    }

    public ObservableCollection<Participant> OpenAgents { get; set; }


    public ObservableCollection<Participant> OrderPlacers { get; set; }

    public ObservableCollection<Participant> FundTransferors { get; set; }

    public ObservableCollection<Participant> ConfirmationPersons { get; set; }


    public required string Company { get; set; }


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsParticipantValid))]
    [NotifyPropertyChangedFor(nameof(CanGenerate))]
    public partial Participant? OpenAgent { get; set; }


    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsParticipantValid))]
    [NotifyPropertyChangedFor(nameof(CanGenerate))]
    public partial Participant? OrderPlacer { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsParticipantValid))]
    [NotifyPropertyChangedFor(nameof(CanGenerate))]
    public partial Participant? FundTransferor { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsParticipantValid))]
    [NotifyPropertyChangedFor(nameof(CanGenerate))]
    public partial Participant? ConfirmationPerson { get; set; }


    public bool IsParticipantValid => OpenAgent is not null && OrderPlacer is not null && FundTransferor is not null && ConfirmationPerson is not null;

    /// <summary>
    /// 模板文件
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGenerate))]
    public partial string? TemplatePath { get; set; }

    /// <summary>
    /// 目标目录
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanGenerate))]
    public partial string? TargetFolder { get; set; }


    public bool CanGenerate => IsParticipantValid && File.Exists(TemplatePath) && Directory.Exists(TargetFolder);

    public int FundId { get; init; }

    [RelayCommand(CanExecute =nameof(CanGenerate))]
    public void Generate()
    {
        try
        {
            // 模板文件
            using var db = DbHelper.Base();
            var m = db.GetCollection<Models.Manager>().FindOne(x => x.IsMaster);
            var fund = db.GetCollection<Fund>().FindById(FundId);
            var daily = db.GetDailyCollection(fund.Id).Find(x => x.NetAsset > 0).MaxBy(x => x.Date)!;
            var ele = db.GetCollection<FundElements>().FindById(fund.Id);

            // 数据
            var obj = new
            {
                Manager = new
                {
                    Name = m.Name,
                    EnglishName = m.EnglishName,
                    LegalPerson = m.LegalAgent?.Name ?? m.ArtificialPerson,
                    ArtificialPerson = m.ArtificialPerson,
                    SetupDate = m.SetupDate,
                    ExpireDate = m.ExpireDate,
                    RegisterAddress = m.RegisterAddress,
                    OfficeAddress = m.OfficeAddress,
                    RegisterCapital = $"{m.RegisterCapital}万",
                    RealCapital = $"{m.RealCapital}万",
                    BusinessScope = m.BusinessScope,
                    Telephone = m.Telephone,
                },
                Fund = new
                {
                    Name = fund.Name,
                    NetAsset = $"{daily.NetAsset / 10000:N0}万",
                    CustodyAccount = ele.CustodyAccount.Value,
                    Duration = $"{ele.DurationInMonths}个月"
                }
                ,
                LegalPerson = new
                {
                    Name = m.LegalAgent?.Name,
                    IdType = m.LegalAgent?.IDType switch { IDType.IdentityCard or IDType.Unknown or null => "身份证", var x => EnumDescriptionTypeConverter.GetEnumDescription(x) },
                    Id = m.LegalAgent?.Id,
                    Phone = m.LegalAgent?.Phone,
                    Address = m.LegalAgent?.Address,
                },
                InvestmentManager = new
                {
                    Name = "杨博涵",
                    IdType = "身份证",
                    Id = "342623",
                    Phone = "18550110512",
                    Address = m.OfficeAddress
                },
                ResponsiblePerson = new
                {
                    Name = OpenAgent?.Name,
                    IdType = OpenAgent?.Identity.Type switch { IDType.IdentityCard or IDType.Unknown or null => "身份证", var x => EnumDescriptionTypeConverter.GetEnumDescription(x) },
                    Id = OpenAgent?.Identity.Id,
                    Phone = OpenAgent?.Phone,
                    Address = OpenAgent?.Address,
                },
                //OpenAgent = pe,
                //OrderPlacer = pe,
                //FundTransferor = pe,
                //ConfirmationPerson = pe
            };

            MiniWord.SaveAsByTemplate(Path.Combine(TargetFolder!, "开户材料2.docx"), TemplatePath, obj);

        }
        catch (Exception e)
        {
             
        }
    }
}
