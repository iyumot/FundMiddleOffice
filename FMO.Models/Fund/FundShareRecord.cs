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
public record class FundShareRecord(int Id, int FundId, DateOnly Date, decimal Share);
