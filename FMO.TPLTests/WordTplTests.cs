using Microsoft.VisualStudio.TestTools.UnitTesting;
using FMO.TPL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMO.TPL.Tests
{
    [TestClass()]
    public class WordTplTests
    {
        [TestMethod()]
        public void GenerateRegisterAnounceTest()
        {
            Directory.SetCurrentDirectory("E:\\funds");

            WordTpl.GenerateRegisterAnounce(new Models.Fund { Name = "ffff", Code = "abcdes" }, "承诺.docx");
             
        }
    }
}