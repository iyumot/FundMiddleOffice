using Microsoft.VisualStudio.TestTools.UnitTesting;
using FMO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FMO.Models;

namespace FMO.Tests
{
    [TestClass()]
    public class OpenRuleTests
    {
        [TestMethod()]
        public void FilterByWeekTest()
        {
            Days.DayInfosByYear(2024);
        }
    }
}