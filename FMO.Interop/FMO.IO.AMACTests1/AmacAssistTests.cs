using FMO.Models;
using FMO.Utilities;

namespace FMO.IO.AMAC.Tests
{
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
    }
}