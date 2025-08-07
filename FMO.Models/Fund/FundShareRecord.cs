using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMO.Models;

/// <summary>
/// 基金份额记录，在有ta日，记录剩余份额
/// </summary>
/// <param name="Id"></param>
/// <param name="FundId"></param>
/// <param name="Date"></param>
/// <param name="Share"></param>
public record class FundShareRecord(int FundId, DateOnly Date, decimal Share)
{
    public long Id => ((long)FundId << 32) | (long)Date.DayNumber;
    public int FundId { get; } = FundId;
    public DateOnly Date { get; } = Date;
    public decimal Share { get; } = Share;
}
