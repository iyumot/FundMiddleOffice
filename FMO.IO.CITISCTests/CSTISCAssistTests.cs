
using FMO.IO.Trustee.CITISC;
using FMO.Models;
using FMO.Utilities;
using Serilog;
using System.Reflection;

namespace FMO.IO.Trustee.Tests;

[TestClass()]
public class CSTISCAssistTests
{
    [TestMethod()]
    public async Task LoginTest()
    {
        Directory.SetCurrentDirectory(@"E:\funds");

        Log.Logger = new LoggerConfiguration().WriteTo.File("log.txt").CreateLogger();

        using ITrusteeAssist assist = new CSTISCAssist();

        await assist.LoginAsync();


    }




    [TestMethod()]
    public async Task SyncTest()
    {
        Directory.SetCurrentDirectory(@"E:\funds");


        Log.Logger = new LoggerConfiguration().WriteTo.File("log.txt").CreateLogger();



        using ITrusteeAssist assist = new CSTISCAssist();

        await assist.SynchronizeFundRaisingRecord();
    }



    [TestMethod()]
    public async Task SyncCustomerTest()
    {
        Directory.SetCurrentDirectory(@"E:\funds");


        Log.Logger = new LoggerConfiguration().WriteTo.File("log.txt").CreateLogger();



        using ITrusteeAssist assist = new CSTISCAssist();

        await assist.SynchronizeCustomer();
    }







    [TestMethod]
    public void TestId()
    {
        Directory.CreateDirectory("data");

        using var db = new BaseDatabase();
        var c = db.GetCollection<BankTransaction>();


        // c.Insert(new BankTransaction { Id = "1", PayBank = "", PayName = "", PayNo = "", ReceiveBank = "", ReceiveName = "", ReceiveNo = "", TransactionId = "1" });
        c.Insert(new BankTransaction { Id = "2", PayBank = "", PayName = "", PayNo = "", ReceiveBank = "", ReceiveName = "", ReceiveNo = "", TransactionId = "1" });

    }

    [TestMethod]
    public void TestId2()
    {
        Directory.CreateDirectory("data");

        using var db = new BaseDatabase();
        var c = db.GetCollection<TestObj>();

        var a = new TestObj();
        var b = new TestObj();

        c.Insert(a);
        c.Insert(b);

    }

    [TestMethod]
    public void TestDynamicLoad()
    {

        Directory.SetCurrentDirectory(@"E:\funds");

        var files = new DirectoryInfo("plugins").GetFiles("*.dll");


        foreach (var file in files)
        {
            var assembly = Assembly.LoadFile(file.FullName);

            var types = assembly.GetTypes().Where(x=> x.GetInterface(typeof(ITrusteeAssist).FullName!) is not null);

            ITrusteeAssist trusteeAssist = Activator.CreateInstance(types.First()) as ITrusteeAssist;

            //var info = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{trusteeAssist.Icon.OriginalString}");

            var res = assembly.GetManifestResourceNames();
            var name = res.FirstOrDefault(x => x.Contains(".logo."));
            var stream = assembly.GetManifestResourceStream(name);

        }


    }


}



public class TestObj
{
    public int Id { get; set; }

}