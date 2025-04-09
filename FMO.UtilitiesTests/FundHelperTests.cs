using Microsoft.VisualStudio.TestTools.UnitTesting;
using FMO.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMO.Utilities.Tests
{
    [TestClass()]
    public class FundHelperTests
    {
        [TestMethod()]
        public void GenerateShareSheetTest()
        {
            Directory.SetCurrentDirectory("E:\\funds");
            var data = FundHelper.GenerateShareSheet(6, new DateOnly(2025, 1, 1), new DateOnly(2025, 3, 31));
            Assert.Fail();
        }
    }
}