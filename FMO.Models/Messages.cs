using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMO.Models;

public class FundDailyUpdateMessage
{

    public int FundId { get; set; }

    public required DailyValue Daily { get; set; }
}

public class FundStatusChangedMessage
{
    public int FundId { get; set; }

    public FundStatus Status { get; set; }
}
