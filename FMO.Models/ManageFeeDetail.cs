using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMO.Models;

public record class ManageFeeDetail(DateOnly Date, decimal Fee, decimal Share);
 