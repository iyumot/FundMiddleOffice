using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee;

public partial class CMS
{
    public class TransferRecordJson
    {

        [JsonPropertyName("custName")]
        public required string CustName { get; set; }

        [JsonPropertyName("custType")]
        public required string CustType { get; set; }

        [JsonPropertyName("certificateType")]
        public required string CertificateType { get; set; }

        [JsonPropertyName("certificateNo")]
        public required string CertificateNo { get; set; }

        [JsonPropertyName("taAccountId")]
        public required string TaAccountId { get; set; }

        [JsonPropertyName("transactionAccountId")]
        public required string TransactionAccountId { get; set; }

        [JsonPropertyName("fundName")]
        public required string FundName { get; set; }

        [JsonPropertyName("fundCode")]
        public required string FundCode { get; set; }

        [JsonPropertyName("businessCode")]
        public required string BusinessCode { get; set; }

        [JsonPropertyName("applicationAmount")]
        public required string ApplicationAmount { get; set; } // 保留两位小数

        [JsonPropertyName("applicationVol")]
        public required string ApplicationVol { get; set; } // 保留两位小数

        [JsonPropertyName("transactionDate")]
        public required string TransactionDate { get; set; } // 格式：yyyymmdd

        [JsonPropertyName("nav")]
        public required string Nav { get; set; } // 清盘日可保留8位小数

        [JsonPropertyName("transactionCfmDate")]
        public required string TransactionCfmDate { get; set; } // 格式：yyyymmdd

        [JsonPropertyName("confirmedVol")]
        public required string ConfirmedVol { get; set; } // 保留两位小数

        [JsonPropertyName("confirmedAmount")]
        public required string ConfirmedAmount { get; set; } // 保留两位小数

        [JsonPropertyName("confirmedNavVol")]
        public required string ConfirmedNavVol { get; set; } // 保留两位小数

        [JsonPropertyName("charge")]
        public required string Charge { get; set; } // 保留两位小数

        [JsonPropertyName("performance")]
        public required string Performance { get; set; } // 保留两位小数

        [JsonPropertyName("distributorCode")]
        public required string DistributorCode { get; set; }

        [JsonPropertyName("distributorName")]
        public required string DistributorName { get; set; }

        [JsonPropertyName("remark1")]
        public required string Remark1 { get; set; }

        [JsonPropertyName("remark2")]
        public required string Remark2 { get; set; }

        [JsonPropertyName("note")]
        public required string Note { get; set; }

        [JsonPropertyName("origDefNo")]
        public required string OrigDefNo { get; set; }

        [JsonPropertyName("shareBonusType")]
        public required string ShareBonusType { get; set; }

        [JsonPropertyName("attributionManagerFee")]
        public required string AttributionManagerFee { get; set; } // 保留两位小数

        [JsonPropertyName("attributionFundAssetFee")]
        public required string AttributionFundAssetFee { get; set; } // 保留两位小数

        [JsonPropertyName("interest")]
        public required string Interest { get; set; } // 保留两位小数

        [JsonPropertyName("attributionSellAgencyFee")]
        public required string AttributionSellAgencyFee { get; set; } // 保留两位小数

        [JsonPropertyName("applyNo")]
        public required string ApplyNo { get; set; }

        public TransferRecord ToObject()
        {
            return new TransferRecord
            {
                CustomerIdentity = CertificateNo,
                CustomerName = CustName,
                Agency = DistributorName,
                RequestDate = DateOnly.ParseExact(TransactionDate, "yyyyMMdd"),
                RequestAmount = decimal.Parse(ApplicationAmount),
                RequestShare = decimal.Parse(ApplicationVol),
                ConfirmedDate = DateOnly.ParseExact(TransactionCfmDate, "yyyyMMdd"),
                ConfirmedAmount = decimal.Parse(ConfirmedAmount),
                ConfirmedShare = decimal.Parse(ConfirmedVol),
                ConfirmedNetAmount = decimal.Parse(ConfirmedNavVol),
                CreateDate = DateOnly.FromDateTime(DateTime.Today),
                ExternalId = Remark1,
                Type = Translate(BusinessCode),
                Fee = decimal.Parse(Charge),
                PerformanceFee = decimal.Parse(Performance),
                ExternalRequestId = ApplyNo,
                FundCode = FundCode,
                FundName = FundName,
                Source = "api",
            };
        }
    }
}
