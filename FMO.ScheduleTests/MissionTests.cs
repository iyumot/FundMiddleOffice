using Microsoft.VisualStudio.TestTools.UnitTesting;
using FMO.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExcelDataReader;
using System.IO;
using System.Diagnostics;

namespace FMO.Schedule.Tests
{
    [TestClass()]
    public class MissionTests
    {
        [TestMethod()]
        public void TaTest()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using var stream = new FileStream(@"C:\Users\lenovo\Downloads\历史交易确认明细(含当日已确认)_20250520_97b59d10256e42e38423939e9b855130.xlsx", FileMode.Open);


            if (stream is null || stream.Length == 0) return;

            if (stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);

            var reader = ExcelReaderFactory.CreateReader(stream);


            for (int i = 0; i < 10; i++)
            {
                reader.Read();
                for (int j = 0; j < reader.FieldCount; j++)
                {
                    var d = reader.GetValue(j);

                    Debug.Write($"{d},");
                    
                }
                Debug.WriteLine("");
            }





        }
    }
}