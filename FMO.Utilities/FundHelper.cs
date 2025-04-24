using System.Data;
using FMO.Models;

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

    public static void InitNew(Fund f)
    {
        var name = $"{f.Code}.{f.Name}";
        string folder = $"files\\funds\\{name}";
        Directory.CreateDirectory(folder);

        using var db = DbHelper.Base();
        db.GetCollection<Fund>().Insert(f);
        db.GetCollection<FundElements>().Insert(new FundElements { Id = f.Id });


        Map(f, folder);
    }

    /// <summary>
    /// 生成每日份额表
    /// </summary>
    /// <param name="fundId"></param>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    public static (IList<DateOnly> Dates, IList<int> CustomerIds, IList<string> Names, decimal[,] Data) GenerateShareSheet(int fundId, DateOnly begin, DateOnly end)
    {
        using var db = DbHelper.Base();

        IEnumerable<TransferRecord> uncheck = db.GetCollection<TransferRecord>().Find(x => x.FundId == 0);
        if (uncheck.Count() > 0)
        {
            var funds = db.GetCollection<Fund>().FindAll().Select(x => new { x.Id, x.Code, x.Name }).ToArray();
            foreach (var item in uncheck)
            {
                item.FundId = (funds.FirstOrDefault(x => x.Code == item.FundCode) ?? funds.FirstOrDefault(x => x.Name == item.FundName))!.Id;
            }
            db.GetCollection<TransferRecord>().Update(uncheck);
        }

        var data = db.GetCollection<TransferRecord>().Find(x => x.FundId == fundId).OrderBy(x => x.ConfirmedDate).ToList();


        /// 生成行、列头
        List<DateOnly> dates = new List<DateOnly>();
        var idname = data.Select(x => (x.CustomerId, x.CustomerName)).DistinctBy(x => x.CustomerId);
        var ids = idname.Select(x => x.CustomerId).ToList();
        var names = idname.Select(x => x.CustomerName).ToList();

        var date = begin;
        while (date <= end)
        {
            dates.Add(date);
            date = date.AddDays(1);
        }

        var array = new decimal[dates.Count, ids.Count];

        Dictionary<DateOnly, Dictionary<int, decimal>> result = new();


        for (int i = 0; i < dates.Count; i++)
        {
            foreach (var d in data)
            {
                if (d.ConfirmedDate >= dates[i]) continue;

                var cid = ids.IndexOf(d.CustomerId);
                array[i, cid] += d.ShareChange();
            }
        }


        return (dates, ids, names, array);
    }



}
