using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FMO.Trustee;
using FMO.Utilities;
using System.Windows.Controls;

namespace FMO;

/// <summary>
/// TrusteeWorkerSettingView.xaml 的交互逻辑
/// </summary>
public partial class TrusteeWorkerSettingView : UserControl
{
    public TrusteeWorkerSettingView()
    {
        InitializeComponent();

        DataContext = new TrusteeWorkerSettingViewModel();
    }

    private void Init()
    {
        using var pdb = DbHelper.Platform();
        var ranges = pdb.GetCollection<TrusteeMethodShotRange>().FindAll().ToArray();



    }
}


public partial class TrusteeWorkerSettingViewModel : ObservableObject
{

    public TrusteeWorkingConfigViewModel[] Configs { get; set; }


    public TrusteeWorkerSettingViewModel()
    {
        using var pdb = DbHelper.Platform();
        var ranges = pdb.GetCollection<TrusteeMethodShotRange>().FindAll().ToArray();

        var cms = Create(CMS._Identifier, ranges);
        var citics = Create(CITICS._Identifier, ranges);
        var csc = Create(CSC._Identifier, ranges);

        Configs = [cms, citics, csc];
    }

    private TrusteeWorkingConfigViewModel Create(string idf, TrusteeMethodShotRange[] ranges)
    {
        var citics = new TrusteeWorkingConfigViewModel { Identifier = idf, };
        citics.QueryTransferRecord = Create(ranges, idf, nameof(ITrustee.QueryTransferRecords));
        citics.QueryTransferRequest = Create(ranges, idf, nameof(ITrustee.QueryTransferRequests));
        citics.QueryRaisingAccountTransction = Create(ranges, idf, nameof(ITrustee.QueryRaisingAccountTransction));
        citics.QueryRaisingBalance = Create(ranges, idf, nameof(ITrustee.QueryRaisingBalance));
        citics.QueryFundDailyFee = Create(ranges, idf, nameof(ITrustee.QueryFundDailyFee));
        return citics;
    }

    public TrusteeMethodConfigViewModel Create(TrusteeMethodShotRange[] ranges, string identifier, string method)
    {
        var range = ranges.FirstOrDefault(x => x.Id == identifier + method);
        return new TrusteeMethodConfigViewModel
        {
            Identifier = identifier,
            Method = method,
            DateBegin = range?.Begin,
            DateEnd = range?.End
        };
    }


}

public partial class TrusteeMethodConfigViewModel : ObservableObject
{
    public required string Identifier { get; set; }

    public required string Method { get; set; }

    [ObservableProperty]
    public partial DateOnly? DateBegin { get; set; }

    [ObservableProperty]
    public partial DateOnly? DateEnd { get; set; }

    [RelayCommand]
    public void SaveDataRange(DataGrid v)
    {
        using var pdb = DbHelper.Platform();

        if (DateBegin is not null && DateEnd is not null)
        {
            var ran = new TrusteeMethodShotRange(Identifier + Method, DateBegin.Value, DateEnd.Value);

            var ranges = pdb.GetCollection<TrusteeMethodShotRange>().Upsert(ran);
        }
        
        v.CommitEdit();
    }
}

public partial class TrusteeWorkingConfigViewModel : ObservableObject
{
    public required string Identifier { get; set; }

    public TrusteeMethodConfigViewModel? QueryTransferRequest { get; set; }

    public TrusteeMethodConfigViewModel? QueryTransferRecord { get; set; }
    public TrusteeMethodConfigViewModel? QueryRaisingAccountTransction { get; internal set; }
    public TrusteeMethodConfigViewModel? QueryRaisingBalance { get; internal set; }
    public TrusteeMethodConfigViewModel? QueryFundDailyFee { get; internal set; }

 
}