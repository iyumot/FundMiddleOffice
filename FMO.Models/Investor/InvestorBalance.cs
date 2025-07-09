namespace FMO.Models;

public class InvestorBalance
{
    public long Id => MakeId(InvestorId, FundId);

    public int InvestorId { get; set; }

    public int FundId { get; set; }

    public decimal Deposit { get; set; }

    public decimal Withdraw { get; set; }

    public decimal Share { get; set; }

    public DateOnly Date { get; set; }



    public static long MakeId(int investorId, int fundId) => (long)investorId << 32 | (uint)fundId;

    public static (int investorId, int fundId) ParseId(long id)
    {
        int investorId = (int)(id >> 32);
        int fundId = (int)(id & 0xFFFFFFFF);
        return (investorId, fundId);
    }
}
