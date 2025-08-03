using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee.JsonCITICS;

#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��

public class CustodialAccountJson : JsonBase
{
    [JsonPropertyName("pdCode")]
    public string ProductCode { get; set; }

    [JsonPropertyName("pdName")]
    public string ProductName { get; set; }

    [JsonPropertyName("accName")]
    public string AccountName { get; set; }

    [JsonPropertyName("account")]
    public string Account { get; set; }

    [JsonPropertyName("bankName")]
    public string BankName { get; set; }

    [JsonPropertyName("lrgAmtNo")]
    public string LargePaymentNumber { get; set; }

    [JsonPropertyName("recentBalance")]
    public string RecentBalance { get; set; }

    [JsonPropertyName("recentBalanceQueryTime")]
    public string BalanceQueryTime { get; set; }

    /// <summary>
    /// 1������ 2������
    /// </summary>
    [JsonPropertyName("status")]
    public int Status { get; set; }


    public FundBankAccount ToObject()
    {
        return new FundBankAccount
        {
            Name = AccountName,
            BankOfDeposit = BankName,
            FundCode = ProductCode,
            LargePayNo = LargePaymentNumber,
            Number = Account,
            IsCanceled = Status == 2,
        };
    }
}

#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��