using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass()]
    public class DebouncerTests
    {
        [TestMethod()]
        public void DebouncerTest()
        {
            var t = Environment.TickCount64;

            var h = new Debouncer(() => Debug.WriteLine(Environment.TickCount64 - t), 50);


            for (int i = 0; i < 5000; i++)
                h.Invoke();

            Thread.Sleep(5000);
             

        }

        [TestMethod()]
        public void InvokeTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void DisposeTest()
        {
            Assert.Fail();
        }
    }
}