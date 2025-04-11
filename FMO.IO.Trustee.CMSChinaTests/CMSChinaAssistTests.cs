using Microsoft.VisualStudio.TestTools.UnitTesting;
using FMO.IO.Trustee.CMSChina;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace FMO.IO.Trustee.CMSChina.Tests
{
    [TestClass()]
    public class CMSChinaAssistTests
    {
        [TestMethod()]
        public void SynchronizeTransferRequestAsyncTest()
        {

            using var sr = new StreamReader("test.json");


            var obj = JsonSerializer.Deserialize<CMSChina.Json.TransferRequestJson.Root>(sr.ReadToEnd());
            
            Assert.Fail();
        }
    }
}