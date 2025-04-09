using FMO.Models;
using System.Data;

namespace FMO.Utilities;

public static class FundHelper
{

    private static Dictionary<int, string> FundStorageMap = new();

    public static DirectoryInfo Folder(this Fund fund)
    {
        return new DirectoryInfo(FundStorageMap[fund.Id]);
    }

    public static DirectoryInfo GetFolder(int fundId)
    {
        return new DirectoryInfo(FundStorageMap[fundId]);
    }

    public static string GetFolder(int fundId, string sub)
    {
        return Path.Combine(FundStorageMap[fundId], sub);
    }


    public static void Map(Fund fund, string folder)
    {
        FundStorageMap[fund.Id] = folder;
    }


    /// <summary>
    /// 生成每日份额表
    /// </summary>
    /// <param name="fundId"></param>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    public static Dictionary<DateOnly, Dictionary<int, decimal>> GenerateShareSheet(int fundId, DateOnly begin, DateOnly end)
    {
        using var db = DbHelper.Base();

        if (db.GetCollection<TransferRecord>().Find(x => x.FundId == 0).Count() > 0)
        {
            var all = db.GetCollection<TransferRecord>().FindAll().ToArray();
            var funds = db.GetCollection<Fund>().FindAll().Select(x => new { x.Id, x.Code, x.Name }).ToArray();
            foreach (var item in all)
            {
                item.FundId = (funds.FirstOrDefault(x => x.Code == item.FundCode) ?? funds.FirstOrDefault(x => x.Name == item.FundName))!.Id;
            }
            db.GetCollection<TransferRecord>().Update(all);
        }

        var data = db.GetCollection<TransferRecord>().Find(x => x.FundId == fundId).OrderBy(x => x.ConfirmedDate).ToList();

        Dictionary<DateOnly, Dictionary<int, decimal>> result = new();
        var tab = new DataTable();
        tab.Columns.Add("Date", typeof(DateOnly));
        var date = begin;
        while (date <= end)
        {
            var row = data.Where(x => x.ConfirmedDate < date).GroupBy(x => x.CustomerId).ToDictionary(x => x.Key, x => x.ToArray());

            result.Add(date, row.ToDictionary(x => x.Key, x => x.Value.Sum(y => y.ShareChange())));

            foreach (var item in row)
            {
                if(!tab.Columns.Contains(item.Key.ToString()))
                    tab.Columns.Add(item.Key.ToString(), typeof(decimal)); 
            }


            var dar = tab.NewRow();
            dar["Date"] = date;
            foreach (var item in row)
            {
                dar[item.Key.ToString()] = item.Value.Sum(x=>x.ShareChange());
            }
            tab.Rows.Add(dar);

            date = date.AddDays(1);
        }

        for (var i = tab.Columns.Count - 1;i>= 0;i--)
        {
            var col = tab.Columns[i];

            if (col.DataType != typeof(decimal)) continue;

            var sum = tab.AsEnumerable().Select(r => r[col]).Sum(x => x switch { decimal d=>d, _=>0 });//.OfType<decimal>.Sum()

            if(sum == 0) 
                tab.Columns.RemoveAt(i);
        }
         

        return result;
    }



}
