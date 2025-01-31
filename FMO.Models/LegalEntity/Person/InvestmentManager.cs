namespace FMO.Models;

/// <summary>
/// 投资经理
/// </summary>
public class InvestmentManager : Person
{

    public string? Description { get; set; }

}




public class FundInvestmentManager 
{
    public int Id { get; set; }

    /// <summary>
    /// 姓名
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// 简介
    /// </summary>
    public string? Info { get; set; }  

}