using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FMO.Models;
using FMO.Utilities;
using LiteDB;
using System.Diagnostics.CodeAnalysis;

namespace FMO.Trustee;

internal record FundTrusteePair(int FundId, ITrustee trustee);


public record TrusteeWorkResult(string Method, IList<TrusteeWorker.WorkReturn> Returns);

public partial class TrusteeWorker : ObservableObject
{
    public class WorkConfig
    {
        public WorkConfig()
        {
        }

        [SetsRequiredMembers]
        public WorkConfig(string id) => Id = id;

        public required string Id { get; set; }

        /// <summary>
        /// ���ʱ�� ��
        /// </summary>
        public int Interval { get; set; } = 15;

        /// <summary>
        /// �ϴ�ִ��ʱ��
        /// </summary>
        public DateTime Last { get; set; }

    }

    public record WorkReturn(string Name, ReturnCode Code, object? Data = null);

    public const string TableRaisingBalance = "api_raising_balance";


    internal List<FundTrusteePair> Maps { get; } = new();

    private Timer timer;

    ITrustee[] Trustees { get; }

    private WorkConfig RaisingBalance { get; set; }


    public TrusteeWorker(ITrustee[] trustees)
    {
        WeakReferenceMessenger.Default.RegisterAll(this);

        timer = new Timer(OnTimer, null, Timeout.Infinite, 1000);

        WorkConfig[] cfg;
        using (var db = DbHelper.Platform())
            cfg = db.GetCollection<WorkConfig>().FindAll().ToArray();

        RaisingBalance = cfg.FirstOrDefault(x => x.Id == nameof(RaisingBalance)) ?? new(nameof(RaisingBalance));
        Trustees = trustees;
        foreach (var t in trustees)
            t.Prepare();


        // ���������api��ӳ��
        //Fund[] funds;
        //using (var db = DbHelper.Base())
        //    funds = db.GetCollection<Fund>().FindAll().ToArray();

        //foreach (var f in funds)
        //{
        //    f.Trustee
        //}


    }



    private async void OnTimer(object? state)
    {
        var t = DateTime.Now;

        // ����λ
        var minute = t.Hour * 60 + t.Minute; //  new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, 0);
        if (minute % RaisingBalance.Interval == 0)
        {
            await QueryRaisingBalanceOnce();
        }
    }

    [RelayCommand]
    public async Task QueryRaisingBalanceOnce()
    {
        List<WorkReturn> ret = new();
        // �������ݿ�
        using var db = DbHelper.Base();

        foreach (var tr in Trustees)
        {
            if (!tr.IsValid)
            {
                ret.Add(new(tr.Title, ReturnCode.ConfigInvalid));
                continue;
            }

            try
            {
                var rc = await tr.QueryRaisingBalance();

                ///
                // �������ݿ� 
                if (rc.Data is not null)
                    db.GetCollection<FundBankBalance>().Upsert(rc.Data);

                ret.Add(new(tr.Title, rc.Code, rc.Data));
            }
            catch (Exception e)
            {
                ret.Add(new(tr.Title, ReturnCode.Unknown));
            }
        }

        // ����ret���������ʱ�ָ�����������Ϣ
        db.DropCollection(TableRaisingBalance);
        db.GetCollection<WorkReturn>(TableRaisingBalance).Insert(ret);

        WeakReferenceMessenger.Default.Send(new TrusteeWorkResult(nameof(ITrustee.QueryRaisingBalance), ret));
        RaisingBalance.Last = DateTime.Now;
        Save(RaisingBalance);
    }

    internal void Start() => timer.Change(0, 1000);

    private void Save(WorkConfig workConfig)
    {
        using var db = DbHelper.Platform();
        db.GetCollection<WorkConfig>().Upsert(workConfig);
    }

}
