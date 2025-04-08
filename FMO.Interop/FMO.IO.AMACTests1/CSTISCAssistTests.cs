﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using FMO.IO.Trustee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using FMO.Models;

namespace FMO.IO.Trustee.Tests
{
    [TestClass()]
    public class CSTISCAssistTests
    {
        [TestMethod()]
        public void LoginValidationOverrideAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SynchronizeFundRaisingRecordTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SynchronizeCustomerAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SynchronizeTransferRequestAsyncTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void SynchronizeTransferRecordAsyncTest()
        {
            using var fs = new StreamReader("ta.json");

             var ss = JsonSerializer.Deserialize<FMO.IO.Trustee.CITISC.Json.TransferRecord.Root>(fs.ReadToEnd());
        }
    }
}