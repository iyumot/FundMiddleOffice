using Microsoft.VisualStudio.TestTools.UnitTesting;
using FMO.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDataReader;
using System.IO;

namespace FMO.Schedule.Tests;

[TestClass()]
public class TAFromMailMissionTests
{
    [TestMethod()]
    public void WorkOnSheetTest()
    {
        Directory.SetCurrentDirectory("D:\\fmo");
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        using var fs = new FileStream(@"E:\share\【交易确认数据】20250520.xlsx", FileMode.Open);


        var sp = SheetParser.Create("cstisc.com");
        var reader = ExcelReaderFactory.CreateReader(fs);
        sp.ParseTASheet(reader);


       // TAFromMailMission.WorkOnSheet(fs, "cstisc.com");


    }
}