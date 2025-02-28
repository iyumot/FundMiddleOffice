using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Utilities;
using Org.BouncyCastle.Asn1.Esf;
using Serilog;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Xml.Linq;

namespace FMO;

/// <summary>
/// QualificationView.xaml 的交互逻辑
/// </summary>
public partial class QualificationView : UserControl
{
    public QualificationView()
    {
        InitializeComponent();

        DataContext = new QualificationViewModel();
    }
}


public partial class QualificationViewModel : ObservableObject
{
    public int Id { get; private set; }


    [ObservableProperty]
    public partial bool IsReadOnly { get; set; } = true;

    //[ObservableProperty]
    //public partial DateTime? Date { get; set; }

    public EntityValueViewModel<InvestorQualification, DateOnly> Date { get; } = new() { InitFunc = x => x.Date == default ? null : x.Date, UpdateFunc = (x, y) => x.Date = y??default, ClearFunc = x => x.Date = default };


    [ObservableProperty]
    public partial bool IsProfessional { get; set; }

    [ObservableProperty]
    public partial bool NeedExperience { get; set; }

    [ObservableProperty]
    public partial AmacInvestorType InvestorType { get; set; }

    /// <summary>
    /// 金融资产（万） （个人）
    /// </summary>
    [ObservableProperty]
    public partial decimal? FinancialAssets { get; set; }

    /// <summary>
    /// 近三年年均收入
    /// </summary>
    [ObservableProperty]
    public partial decimal? Income { get; set; }

    public FileViewModel AssetsFile { get; }

    public QualificationViewModel()
    {

        AssetsFile = new FileViewModel { Id = nameof(AssetsFile), SaveFunc = x => Save(x) };

    }

    public QualificationViewModel(int id)
    {
        Id = id;
        AssetsFile = new FileViewModel { Id = nameof(AssetsFile), SaveFunc = x => Save(x) };
    }

    private void Save(FileViewModel v)
    {
        var fi = v.File;
        if (fi is null) return;

        string hash = fi.ComputeHash()!;

        // 保存副本
        var dir = Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "qualification", Id.ToString()));

        var tar = FileHelper.CopyFile2(fi, dir.FullName);
        if (tar is null)
        {
            Log.Error($"保存合投文件出错，{fi.Name}");
            HandyControl.Controls.Growl.Error($"无法保存{fi.Name}，文件名异常或者存在过多重名文件");
            return;
        }

        var path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tar);

        using var db = new BaseDatabase();
        var obj = db.GetCollection<InvestorQualification>().FindById(Id);




        if (obj!.GetType().GetProperty(v.Id) is PropertyInfo property && property.PropertyType == typeof(FileStorageInfo))
            property.SetValue(obj, new FileStorageInfo(tar, hash, fi.LastWriteTime));
        db.GetCollection<InvestorQualification>().Update(obj);

    }


    public static QualificationViewModel From(InvestorQualification x, Investor investor)
    {
        var obj = new QualificationViewModel
        {
            Id = x.Id,
            //Date = x.Date == default ? null : new DateTime(x.Date, default),
            IsProfessional = x.Result == QualifiedInvestorType.Professional,
            InvestorType = investor.Type,
        };

        obj.Date.Init(x);

        return obj;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        Debug.WriteLine($"{Id} {IsProfessional}");
        switch (e.PropertyName)
        {
            //case nameof(Date):

            //using var db = new BaseDatabase();
            //var obj = db.GetCollection<InvestorQualification>().FindById(Id);
            //obj.Date = Date;
            //   break;
        }
    }


    [RelayCommand]
    public void Delete(UnitViewModel unit)
    {
        if (unit is IEntityViewModel<InvestorQualification> entity)
        {
            using var db = new BaseDatabase();
            var v = db.GetCollection<InvestorQualification>().FindById(Id);

            if (v is not null)
            {
                entity.RemoveValue(v);
                db.GetCollection<InvestorQualification>().Upsert(v);

                WeakReferenceMessenger.Default.Send(v);
            }
        }
    }

    [RelayCommand]
    public void Reset(UnitViewModel unit)
    {
        var ps = unit.GetType().GetProperties().Where(x => x.PropertyType.IsGenericType && (x.PropertyType.GetGenericTypeDefinition() == typeof(ValueViewModel<>)|| x.PropertyType.GetGenericTypeDefinition() == typeof(RefrenceViewModel<>)));
        foreach (var item in ps)
        {
            var ty = item.PropertyType!;
            object? obj = item.GetValue(unit);
            ty.GetProperty("New")!.SetValue(obj, ty.GetProperty("Old")!.GetValue(obj));
        }
    }

    [RelayCommand]
    public void Modify(UnitViewModel unit)
    {
        if (unit is IEntityViewModel<InvestorQualification> property)
        {
            using var db = new BaseDatabase();
            var v = db.GetCollection<InvestorQualification>().FindById(Id);

            if (v is not null)
                property.UpdateEntity(v); 

            if (v is not null)
            {
                db.GetCollection<InvestorQualification>().Upsert(v);
                if (Id == 0) Id = v.Id;

                //WeakReferenceMessenger.Default.Send(v);
            }
        }
        unit.Apply();
    }
}


