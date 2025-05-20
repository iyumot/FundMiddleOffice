using PDFiumSharp;

namespace FMO.PDF.Tests
{
    [TestClass()]
    public class PdfHelperTests
    {
        [TestMethod()]
        public void GetSecurityAccountsTest()
        {
            using var fs = new FileStream(@"C:\Users\lenovo\Downloads\5555\a.pdf", FileMode.Open);

            var res = PdfHelper.GetSecurityAccounts(fs);

            Assert.Fail();
        }


        [TestMethod]
        public void GetAccountInfo()
        {
            using var fs = new FileStream(@"C:\Users\lenovo\Downloads\b.pdf", FileMode.Open);
            PdfHelper.GetAccountInfo(fs);
        }

        [TestMethod]
        public void MyTestMethod()
        {
            var d = new DirectoryInfo("-");
            d.Create();
        }


        [TestMethod]
        public void TaPdf()
        {
            var d = PDFium.FPDF_LoadDocument(@"C:\Users\lenovo\Downloads\x.pdf", null);
            var cnt = PDFium.FPDF_GetPageCount(d);
            if (cnt == 0) return;

            var page = PDFium.FPDF_LoadPage(d, 0);
            var textpage = PDFium.FPDFText_LoadPage(page);
            var charcnt = PDFium.FPDFText_CountChars(textpage);
            if (charcnt == 0) return;

            var rotate = PDFium.FPDFPage_GetRotation(page);

            var str = PDFium.FPDFText_GetText(textpage, 0, charcnt);
             

            PDFium.FPDFText_ClosePage(textpage);
            PDFium.FPDF_ClosePage(page);

        }
    }
}