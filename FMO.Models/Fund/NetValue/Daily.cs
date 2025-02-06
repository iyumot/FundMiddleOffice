using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMO.Models;


public enum DailySource
{
    [Description("手工")]
    Manual,

    [Description("托管")]
    Custodian,

    [Description("估值表")]
    Sheet
}


public class DailyValue
{
    /// <summary>
    /// 默认-1，避免被赋给第一个
    /// </summary>
    public int FundId { get; set; } 

    public int Id => Date.DayNumber;

    public DateOnly Date { get; set; }

    public decimal NetValue { get; set; }

    public decimal CumNetValue { get; set; }

    public decimal Asset { get; set; }

    public decimal NetAsset { get; set; }

    /// <summary>
    /// 负债
    /// </summary>
    public decimal Liability => Asset - NetAsset;

    public decimal Share { get; set; }

    public DailySource Source { get; set; }

    /// <summary>
    /// 估值表路径
    /// </summary>
    public string?  SheetPath { get; set; }

    public bool IsAvailiable() => NetValue > 0;
}
