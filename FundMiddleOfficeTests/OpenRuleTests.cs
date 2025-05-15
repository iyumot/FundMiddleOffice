using FMO.Models;

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
       
         rule.Filter(dates) ;


    }


    [TestMethod()]
    public void FilterByWeekTest2()
    {
        var dates = Days.DayInfosByYear(2024);

        var rule = new OpenRule();
        rule.Type = FundOpenType.Monthly;
        rule.Dates = [1,3]; //每周一
        rule.Weeks = [1];
        //rule.DayOrder = SequenceOrder.Descend;
        rule.WeekOrder = SequenceOrder.Descend;
        rule.TradeOrNatural = true;

        rule.Filter(dates);


    }
}