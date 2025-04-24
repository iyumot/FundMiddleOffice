using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMO.Models;

public record FundDailyUpdateMessage(int FundId, DailyValue Daily);

public record FundStatusChangedMessage(int FundId, FundStatus Status);

public record OpenFundMessage(int Id);