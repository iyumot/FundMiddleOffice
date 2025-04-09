using Microsoft.VisualStudio.TestTools.UnitTesting;
using FMO.IO.Trustee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMO.IO.Trustee.Tests
{
    [TestClass()]
    public class CSTISCAssistTests
    {
        [TestMethod()]
        public async Task SynchronizeTransferRequestAsyncTest()
        {
            Directory.SetCurrentDirectory("e:\\funds");


            CSTISCAssist assist = new CSTISCAssist();
            await assist.SynchronizeTransferRequestAsync();
            //await assist.SynchronizeTransferRecordAsync();
            //await assist.SynchronizeDistributionAsync();
           // Assert.Fail();
        }
    }
}