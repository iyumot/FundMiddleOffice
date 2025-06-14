using FMO.Models;
using System.Diagnostics;

namespace FMO.Tests;

[TestClass()]
public class OpenRuleTests
{
    [TestMethod()]
    public void FilterByWeekTest()
    {
        var dates = Days.DayInfosByYear(2024);

        var rule = new OpenRule();
        rule.Type = FundOpenType.Monthly;
        rule.Dates = [1]; //每周一
        rule.Weeks = [2, 3];
        rule.DayOrder = SequenceOrder.Descend;
        rule.TradeOrNatural = true;



    }


    [TestMethod()]
    public void FilterByWeekTest2()
    {
        var dates = Days.DayInfosByYear(2024);

        var rule = new OpenRule();
        rule.Type = FundOpenType.Monthly;
        rule.Dates = [1, 3]; //每周一
        rule.Weeks = [1];
        //rule.DayOrder = SequenceOrder.Descend;
        rule.WeekOrder = SequenceOrder.Descend;
        rule.TradeOrNatural = true;



    }

    [TestMethod()]
    public void IsPairTest()
    {
        var rule = new OpenRule();
        rule.Type = FundOpenType.Quarterly;
        rule.Months = [2];
        rule.Dates = [1, 3]; //每周一
        rule.Weeks = [1];
        //rule.DayOrder = SequenceOrder.Descend;
        rule.WeekOrder = SequenceOrder.Descend;
        rule.TradeOrNatural = true;
        rule.Postpone = true;

    }

    [TestMethod]
    public void MyTestMethod()
    {
        for (int i = 1; i < 13; i++)
        {
            Debug.WriteLine($"{i} {(i - 1) % 3 + 1}");
        }
    }

}
