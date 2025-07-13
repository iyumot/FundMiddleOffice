using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMO.Models;



/// <summary>
/// 交易订单
/// </summary>
public class TransferOrder
{
    public int Id { get; set; }

    /// <summary>
    /// 对应的申请
    /// </summary>
    public int RequestId { get; set; }

    /// <summary>
    /// 对应的记录
    /// </summary>
    public int RecordId { get; set; }


    public int InvestorId { get; set; }

    public int FundId { get; set; }

    public DateOnly Date { get; set; }

    /// <summary>
    /// 合同
    /// </summary>
    public FileStorageInfo? Contact { get; set; }

    /// <summary>
    /// 风险揭示
    /// </summary>
    public FileStorageInfo? RiskDiscloure { get; set; }
                                                                                                                                 
    /// <summary>
    /// 认申购、赎回单
    /// </summary>
    public FileStorageInfo? OrderSheet { get; set; }

    /// <summary>
    /// 风险匹配
    /// </summary>
    public FileStorageInfo? RiskPair { get; set; }

    /// <summary>
    /// 双录
    /// </summary>
    public FileStorageInfo? Videotape { get; set; }

}
