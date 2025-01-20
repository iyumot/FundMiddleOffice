using CommunityToolkit.Mvvm.ComponentModel;
using FMO.Models;
using FMO.Utilities;

namespace FMO.IO.AMAC.Tests;

[TestClass()]
public class AmacAssistTests
{
    [TestMethod()]
    public async Task CrawlFundInfoTest()
    {
        Directory.SetCurrentDirectory(@"E:\funds");


        var db = new BaseDatabase();
        var c = db.GetCollection<Fund>().FindAll().ToArray();
        db.Dispose();


        var client = new HttpClient();
        await AmacAssist.SyncFundInfoAsync(c.First(), client);


    }





    [TestMethod()]
public void test()
    {
        DataItem<string> item = new DataItem<string>() { NewValue = "11" };

        obj o = new obj();

        item.Upd(o.name);
    }
}


public   class DataItem<T> 
{ 
    public   T? OldValue { get; set; }
     
    public   T? NewValue { get; set; }

    //public required string PropertyName { get; set; }

    public bool IsChanged => NewValue is not null && !NewValue.Equals(OldValue);

    //public required Action<T, TEntity> Update { get; set; }

    public void Upd(T pro) { pro = NewValue; }
}

public class obj
{
    public string name { get; set; }
}