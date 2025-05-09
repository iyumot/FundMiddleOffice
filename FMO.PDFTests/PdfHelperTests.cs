using Microsoft.VisualStudio.TestTools.UnitTesting;
using FMO.PDF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMO.PDF.Tests
{
    [TestClass()]
    public class PdfHelperTests
    {
        [TestMethod()]
        public void GetSecurityAccountsTest()
        {
            using var fs = new FileStream(@"C:\Users\iyu\Downloads\鑫享世宸量化CTA1号私募证券投资基金.pdf",FileMode.Open);

            var res = PdfHelper.GetSecurityAccounts(fs);

            Assert.Fail();
        }


        [TestMethod]
        public void GetAccountInfo()
        {
            using var fs = new FileStream(@"C:\Users\lenovo\Downloads\鑫享世宸量化CTA1号A期私募证券投资基金_募集账户信息函.pdf", FileMode.Open);
            PdfHelper.GetAccountInfo(fs);
        }
    }
}