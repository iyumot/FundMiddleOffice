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

        var cms = new TrusteeWorkingConfigViewModel { Identifier = CMS._Identifier, };
        cms.QueryTransferRecord = Create(ranges, CMS._Identifier, nameof(ITrustee.QueryTransferRecords));
        cms.QueryTransferRequest = Create(ranges, CMS._Identifier, nameof(ITrustee.QueryTransferRequests));
        cms.QueryRaisingAccountTransction = Create(ranges, CMS._Identifier, nameof(ITrustee.QueryRaisingAccountTransction));
        cms.QueryRaisingBalance = Create(ranges, CMS._Identifier, nameof(ITrustee.QueryRaisingBalance));
        cms.QueryFundDailyFee = Create(ranges, CMS._Identifier, nameof(ITrustee.QueryFundDailyFee));



        var citics = new TrusteeWorkingConfigViewModel { Identifier = CITICS._Identifier, };
        citics.QueryTransferRecord = Create(ranges, CITICS._Identifier, nameof(ITrustee.QueryTransferRecords)); 
        citics.QueryTransferRequest = Create(ranges, CITICS._Identifier, nameof(ITrustee.QueryTransferRequests));
        citics.QueryRaisingAccountTransction = Create(ranges, CITICS._Identifier, nameof(ITrustee.QueryRaisingAccountTransction));
        citics.QueryRaisingBalance = Create(ranges, CITICS._Identifier, nameof(ITrustee.QueryRaisingBalance));
        citics.QueryFundDailyFee = Create(ranges, CITICS._Identifier, nameof(ITrustee.QueryFundDailyFee));


        var csc = new TrusteeWorkingConfigViewModel { Identifier = CSC._Identifier, };
        csc.QueryTransferRecord = Create(ranges, CSC._Identifier, nameof(ITrustee.QueryTransferRecords));
        csc.QueryTransferRequest = Create(ranges, CSC._Identifier, nameof(ITrustee.QueryTransferRequests));
        csc.QueryRaisingAccountTransction = Create(ranges, CSC._Identifier, nameof(ITrustee.QueryRaisingAccountTransction));
        csc.QueryRaisingBalance = Create(ranges, CSC._Identifier, nameof(ITrustee.QueryRaisingBalance));
        csc.QueryFundDailyFee = Create(ranges, CSC._Identifier, nameof(ITrustee.QueryFundDailyFee));

        Configs = [cms, citics, csc];
    }


    public TrusteeMethodConfigViewModel Create(TrusteeMethodShotRange[] ranges, string identifier, string method)
    {
        var range = ranges.FirstOrDefault(x => x.Id == identifier + method);
        return new TrusteeMethodConfigViewModel
        {
            Method = method,
            DateBegin = range?.Begin,
            DateEnd = range?.End
        };
    }


}

public partial class TrusteeMethodConfigViewModel : ObservableObject
{
    public required string Method { get; set; }

    [ObservableProperty]
    public partial DateOnly? DateBegin { get; set; }

    [ObservableProperty]
    public partial DateOnly? DateEnd { get; set; }


}

public partial class TrusteeWorkingConfigViewModel : ObservableObject
{
    public required string Identifier { get; set; }

    public TrusteeMethodConfigViewModel? QueryTransferRequest { get; set; }

    public TrusteeMethodConfigViewModel? QueryTransferRecord { get; set; }
    public TrusteeMethodConfigViewModel? QueryRaisingAccountTransction { get; internal set; }
    public TrusteeMethodConfigViewModel? QueryRaisingBalance { get; internal set; }
    public TrusteeMethodConfigViewModel? QueryFundDailyFee { get; internal set; }

    [RelayCommand]
    public void Save(TrusteeMethodConfigViewModel v)
    {
        using var pdb = DbHelper.Platform();

        if (v.DateBegin is not null && v.DateEnd is not null)
        {
            var ran = new TrusteeMethodShotRange(Identifier + v.Method, v.DateBegin.Value, v.DateEnd.Value);

            var ranges = pdb.GetCollection<TrusteeMethodShotRange>().Upsert(ran);
        }
    }
}