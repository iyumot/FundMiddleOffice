using FMO.IO.AMAC;
using FMO.Models;
using FMO.Utilities;
using Serilog;
using System.Diagnostics;

namespace TestAmac
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
            Log.Logger = new LoggerConfiguration().WriteTo.File("log.txt").CreateLogger();


            Directory.SetCurrentDirectory("E:\funds");

        }

        [Test]
        public async Task Test_GetInstitutionInfoFromAmac()
        {
            var managers = await AmacAssist.GetInstitutionInfoFromAmac("鸿");

            if (managers is [..])
                foreach (var item in managers)
                    Debug.WriteLine(item.PrintProperties());
            // Assert.Pass();
        }

        [Test]
        public async Task Test_ExtractFund()
        {
            string name = "上海国投先导私募基金管理有限公司";
            string id = "2405171029105819";

            Manager manager = new Manager { AmacId = id, Id = "unset", Name = name, RegisterNo = "unset" };

            await AmacAssist.CrawleManagerInfo(manager);


        }






    }
}
