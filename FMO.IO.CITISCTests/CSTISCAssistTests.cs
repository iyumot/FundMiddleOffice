namespace FMO.IO.Trustee.Tests
{
    [TestClass()]
    public class CSTISCAssistTests
    {
        [TestMethod()]
        public async Task LoginTest()
        {
          //  Directory.SetCurrentDirectory(@"E:\funds");


            using ITrusteeAssist assist = new CSTISCAssist();
            assist.SetCredential("18550110512", "sj@123478");

            await assist.LoginAsync();


        }
    }
}