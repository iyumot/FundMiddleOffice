using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee;

public partial class CMS
{
    public class JsonRoot
    {
        [JsonPropertyName("resultcode")]
        public required string Code { get; set; }


        [JsonPropertyName("msg")]
        public required string Msg { get; set; }


        [JsonPropertyName("page")]
        public string? Page { get; set; }

        [JsonPropertyName("data")]
        public string? Data { get; set; }
    }
    public class PaginationInfo
    {
        [JsonPropertyName("pageNumber")]
        public int PageNumber { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("pageCount")]
        public int PageCount { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    public class JsonRoot<T>
    {
        [JsonPropertyName("resultcode")]
        public required string Code { get; set; }


        [JsonPropertyName("msg")]
        public required string Msg { get; set; }


        [JsonPropertyName("page")]
        public required PaginationInfo Page { get; set; }

        [JsonPropertyName("data")]
        public required T[] Data { get; set; }
    }




    public class SubjectFundMappingJson
    {
        [JsonPropertyName("gradeNo")]
        public required string GradeNo { get; set; }

        [JsonPropertyName("gradeName")]
        public required string GradeName { get; set; }

        [JsonPropertyName("productNo")]
        public required string ProductNo { get; set; }

        [JsonPropertyName("productName")]
        public required string ProductName { get; set; }

        [JsonPropertyName("isGrade")]
        public bool IsGrade { get; set; }

        [JsonPropertyName("registerNo")]
        public required string RegisterNo { get; set; }

        public SubjectFundMapping ToObject()
        {
            var sc = IsGrade ? GradeName.Replace(ProductName, "") : "";
            return new SubjectFundMapping { FundCode = GradeNo, FundName = GradeName, MasterCode = ProductNo, MasterName = ProductName, ShareClass = sc };
        }
    }


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
        public required string ApplicationAmount { get; set; } // ������λС��

        [JsonPropertyName("applicationVol")]
        public required string ApplicationVol { get; set; } // ������λС��

        [JsonPropertyName("transactionDate")]
        public required string TransactionDate { get; set; } // ��ʽ��yyyymmdd

        [JsonPropertyName("nav")]
        public required string Nav { get; set; } // �����տɱ���8λС��

        [JsonPropertyName("transactionCfmDate")]
        public required string TransactionCfmDate { get; set; } // ��ʽ��yyyymmdd

        [JsonPropertyName("confirmedVol")]
        public required string ConfirmedVol { get; set; } // ������λС��

        [JsonPropertyName("confirmedAmount")]
        public required string ConfirmedAmount { get; set; } // ������λС��

        [JsonPropertyName("confirmedNavVol")]
        public required string ConfirmedNavVol { get; set; } // ������λС��

        [JsonPropertyName("charge")]
        public required string Charge { get; set; } // ������λС��

        [JsonPropertyName("performance")]
        public required string Performance { get; set; } // ������λС��

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
        public required string AttributionManagerFee { get; set; } // ������λС��

        [JsonPropertyName("attributionFundAssetFee")]
        public required string AttributionFundAssetFee { get; set; } // ������λС��

        [JsonPropertyName("interest")]
        public required string Interest { get; set; } // ������λС��

        [JsonPropertyName("attributionSellAgencyFee")]
        public required string AttributionSellAgencyFee { get; set; } // ������λС��

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


    /// <summary>
    /// ���������Ϣʵ����
    /// </summary>
    public class FundDailyFeeJson
    {
        /// <summary>
        /// ��Ʒ���ƣ������󳤶�300��
        /// </summary>
        [JsonPropertyName("fundName")]
        public required string FundName { get; set; }

        /// <summary>
        /// ��Ʒ���루�����󳤶�6��
        /// </summary>
        [JsonPropertyName("fundCode")]
        public required string FundCode { get; set; }

        /// <summary>
        /// �������ڣ���ʽ��yyyyMMdd�����
        /// </summary>
        [JsonPropertyName("busiDate")]
        public required string BusiDate { get; set; }

        // �йܷ�
        [JsonPropertyName("custodianFeeJt")]
        public decimal CustodianFeeJt { get; set; } // ����

        [JsonPropertyName("custodianFeeZf")]
        public decimal CustodianFeeZf { get; set; } // ֧��

        [JsonPropertyName("custodianFeeYe")]
        public decimal CustodianFeeYe { get; set; } // ���

        // ��Ӫ�����
        [JsonPropertyName("operationServiceFeeJt")]
        public decimal OperationServiceFeeJt { get; set; }

        [JsonPropertyName("operationServiceFeeZf")]
        public decimal OperationServiceFeeZf { get; set; }

        [JsonPropertyName("operationServiceFeeYe")]
        public decimal OperationServiceFeeYe { get; set; }

        // �����
        [JsonPropertyName("managementFeeJt")]
        public decimal ManagementFeeJt { get; set; }

        [JsonPropertyName("managementFeeZf")]
        public decimal ManagementFeeZf { get; set; }

        [JsonPropertyName("managementFeeYe")]
        public decimal ManagementFeeYe { get; set; }

        // ҵ�������
        [JsonPropertyName("performanceFeeJt")]
        public decimal PerformanceFeeJt { get; set; }

        [JsonPropertyName("performanceFeeZf")]
        public decimal PerformanceFeeZf { get; set; }

        [JsonPropertyName("performanceFeeYe")]
        public decimal PerformanceFeeYe { get; set; }

        // ���۷����
        [JsonPropertyName("salesandServiceFeesJt")]
        public decimal SalesAndServiceFeesJt { get; set; }

        [JsonPropertyName("salesandServiceFeesZf")]
        public decimal SalesAndServiceFeesZf { get; set; }

        [JsonPropertyName("salesandServiceFeesYe")]
        public decimal SalesAndServiceFeesYe { get; set; }

        // Ͷ�ʹ��ʷ�
        [JsonPropertyName("investmentConsultantFeeJt")]
        public decimal InvestmentConsultantFeeJt { get; set; }

        [JsonPropertyName("investmentConsultantFeeZf")]
        public decimal InvestmentConsultantFeeZf { get; set; }

        [JsonPropertyName("investmentConsultantFeeYe")]
        public decimal InvestmentConsultantFeeYe { get; set; }

        // �ͻ������
        [JsonPropertyName("customerServiceFeeJt")]
        public decimal CustomerServiceFeeJt { get; set; }

        [JsonPropertyName("customerServiceFeeZf")]
        public decimal CustomerServiceFeeZf { get; set; }

        [JsonPropertyName("customerServiceFeeYe")]
        public decimal CustomerServiceFeeYe { get; set; }

        public FundDailyFee ToObject()
        {
            return new FundDailyFee
            {
                FundCode = FundCode,
                Date = DateOnly.ParseExact(BusiDate, "yyyyMMdd"),
                // �����
                ManagerFeeAccrued = ManagementFeeJt,
                ManagerFeePaid = ManagementFeeZf,
                ManagerFeeBalance = ManagementFeeYe,

                // �йܷ�
                CustodianFeeAccrued = CustodianFeeJt,
                CustodianFeePaid = CustodianFeeZf,
                CustodianFeeBalance = CustodianFeeYe,

                // �����Ӫ����ѣ�OperationServiceFee��
                OutsourcingFeeAccrued = OperationServiceFeeJt,
                OutsourcingFeePaid = OperationServiceFeeZf,
                OutsourcingFeeBalance = OperationServiceFeeYe,

                // ҵ�������
                PerformanceFeeAccrued = PerformanceFeeJt,
                PerformanceFeePaid = PerformanceFeeZf,
                PerformanceFeeBalance = PerformanceFeeYe,

                // ���۷����
                SalesFeeAccrued = SalesAndServiceFeesJt,
                SalesFeePaid = SalesAndServiceFeesZf,
                SalesFeeBalance = SalesAndServiceFeesYe,

                // Ͷ�ʹ��ʷ�
                ConsultantFeeAccrued = InvestmentConsultantFeeJt,
                ConsultantFeePaid = InvestmentConsultantFeeZf,
                ConsultantFeeBalance = InvestmentConsultantFeeYe

            };
        }

    }


    public class BankTransactionJson
    {
        /// <summary>
        /// �����˻���
        /// </summary>
        [JsonPropertyName("bfzhh")]
        public required string OurAccountNumber { get; set; }

        /// <summary>
        /// �����˻���
        /// </summary>
        [JsonPropertyName("bfzhmc")]
        public required string OurAccountName { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        [JsonPropertyName("bz")]
        public required string Currency { get; set; }

        /// <summary>
        /// ��Ʒ����
        /// </summary>
        [JsonPropertyName("cpdm")]
        public required string ProductCode { get; set; }

        /// <summary>
        /// ��Ʒ����
        /// </summary>
        [JsonPropertyName("cpmc")]
        public required string ProductName { get; set; }

        /// <summary>
        /// ���ַ�����������
        /// </summary>
        [JsonPropertyName("dsfkhhmc")]
        public required string CounterpartyBankName { get; set; }

        /// <summary>
        /// �Է��˻���
        /// </summary>
        [JsonPropertyName("dsfzhh")]
        public required string CounterpartyAccountNumber { get; set; }

        /// <summary>
        /// �Է��˻���
        /// </summary>
        [JsonPropertyName("dsfzhmc")]
        public required string CounterpartyAccountName { get; set; }

        /// <summary>
        /// ���׽��
        /// </summary>
        [JsonPropertyName("jyje")]
        public required string TransactionAmount { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        [JsonPropertyName("jyrq")]
        public required string TransactionDate { get; set; }

        /// <summary>
        /// ����ʱ��
        /// </summary>
        [JsonPropertyName("jysj")]
        public required string TransactionTime { get; set; }

        /// <summary>
        /// ��ˮ��
        /// </summary>
        [JsonPropertyName("lsh")]
        public required string TransactionId { get; set; }

        /// <summary>
        /// ļ����������
        /// </summary>
        [JsonPropertyName("mjhyh")]
        public required string CollectionBank { get; set; }

        /// <summary>
        /// �ո������ա�����
        /// </summary>
        [JsonPropertyName("sffx")]
        public required string TransactionType { get; set; }

        /// <summary>
        /// ����ժҪ
        /// </summary>
        [JsonPropertyName("yhbz")]
        public required string BankMemo { get; set; }

        internal BankTransaction ToObject()
        {
            return new BankTransaction
            {
                Id = TransactionId,
                AccountNo = OurAccountNumber,
                AccountBank = CollectionBank,
                AccountName = OurAccountName,
                Serial = TransactionId,
                CounterBank = CounterpartyBankName,
                CounterName = CounterpartyAccountName,
                CounterNo = CounterpartyAccountNumber,
                Amount = decimal.Parse(TransactionAmount),
                Direction = TransactionType == "��" ? TransctionDirection.Pay : TransctionDirection.Receive,
                Remark = BankMemo,
                Time = DateTime.ParseExact(TransactionDate + TransactionTime, "yyyyMMddHH:mm:ss", null)
            };
        }
    }



    public static TARecordType Translate(string c)
    {
        return c switch
        {
            //"120" => TARecordType.Subscription, //"�Ϲ�ȷ��" ����û�зݶ�����, ���Ϲ����
            "122" => TARecordType.Purchase,     //"�깺ȷ��",
            "124" => TARecordType.Redemption,// "���ȷ��",
            "126" => TARecordType.MoveIn,   //"ת��ȷ��",
            "127" => TARecordType.MoveIn,   //"ת������/����ת��",
            "128" => TARecordType.MoveOut,  //"ת������/����ת��",
            "129" => TARecordType.BonusType,//"�ֺ췽ʽ",
            "130" => TARecordType.Subscription, // "�Ϲ����",
            "131" => TARecordType.Frozen,       //"�����������",
            "132" => TARecordType.Thawed,       //"��������ⶳ",
            //"133" => TARecordType.TransferIn,   //"�ǽ��׹���",
            "134" => TARecordType.TransferIn,   //"�ǽ��׹���ת��",
            "135" => TARecordType.TransferOut,  //"�ǽ��׹���ת��",
            //"136" => TARecordType.SwitchIn,     //"����ת��",
            "137" => TARecordType.SwitchIn,     //"����ת��ת��",
            "138" => TARecordType.SwitchOut,    //"����ת��ת��",


            "139" => TARecordType.Purchase,     //"��ʱ�����깺",
            "142" => TARecordType.ForceRedemption,//"ǿ�����",
            "143" => TARecordType.Distribution,     //"�ֺ�ȷ��",
            "144" => TARecordType.Increase,     //"ǿ�е���",
            "145" => TARecordType.Decrease,     //"ǿ�е���",
            _ => TARecordType.UNK,              //"δ֪ҵ������"
        };
    }
}
