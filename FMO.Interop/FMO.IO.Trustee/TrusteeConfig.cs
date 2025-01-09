namespace FMO.IO.Trustee;





public class TrusteeConfig
{
    public required string Id { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 是否登陆
    /// </summary>
    public bool IsLogedIn { get; set; }












}


public class LatestSyncDate
{
    /// <summary>
    /// 募集户流水最后同步的日期
    /// </summary>
    public DateOnly FundRaisingRecord { get; set; }

    /// <summary>
    /// 托管户流水最后同步的日期
    /// </summary>
    public DateOnly BankRecord { get; set; }
}



public class SyncOperationTime
{
    /// <summary>
    /// 募集户流水最后同步的日期
    /// </summary>
    public DateTime FundRaisingRecord { get; set; }

    /// <summary>
    /// 托管户流水最后同步的日期
    /// </summary>
    public DateTime BankRecord { get; set; }

}

















