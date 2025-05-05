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
    public class CSCAssistTests
    {
        [TestMethod()]
        public async Task SynchronizeTransferRequestAsyncTest()
        {
            CSCAssist assist = new();
            await assist.SynchronizeTransferRequestAsync();

             
        }
    }
}