using Microsoft.VisualStudio.TestTools.UnitTesting;
using FMO.FeeCalc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FMO.Utilities;

namespace FMO.FeeCalc.Tests
{
    [TestClass()]
    public class MainWindowViewModelTests
    {
        [TestMethod()]
        public void CalcTest()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Sample Sheet");
                worksheet.Cell("A1").Value = "Hello World!";
                worksheet.Cell("A2").FormulaA1 = "=MID(A1, 7, 5)";
                workbook.SaveAs($"{f.Fund.ShortName}_{dates[0]}-{dates[^1]}.xlsx");
            }
        }
    }
}