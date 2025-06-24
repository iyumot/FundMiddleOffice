using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee;


#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��
public partial class CMS
{
    public class JsonRoot
    {
        [JsonPropertyName("resultcode")]
        public string Code { get; set; }


        [JsonPropertyName("msg")]
        public string Msg { get; set; }


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
        public string Code { get; set; }


        [JsonPropertyName("msg")]
        public string Msg { get; set; }


        [JsonPropertyName("page")]
        public required PaginationInfo Page { get; set; }

        [JsonPropertyName("data")]
        public required T[] Data { get; set; }
    }




    public class SubjectFundMappingJson
    {
        [JsonPropertyName("gradeNo")]
        public string GradeNo { get; set; }

        [JsonPropertyName("gradeName")]
        public string GradeName { get; set; }

        [JsonPropertyName("productNo")]
        public string ProductNo { get; set; }

        [JsonPropertyName("productName")]
        public string ProductName { get; set; }

        [JsonPropertyName("isGrade")]
        public bool IsGrade { get; set; }

        [JsonPropertyName("registerNo")]
        public string RegisterNo { get; set; }

        public SubjectFundMapping ToObject()
        {
            var sc = IsGrade ? GradeName.Replace(ProductName, "") : "";
            return new SubjectFundMapping { FundCode = GradeNo, FundName = GradeName, MasterCode = ProductNo, MasterName = ProductName, ShareClass = sc };
        }
    }


    public class TransferRequestJson
    {
        /// <summary>
        /// �ͻ����ƣ���󳤶ȣ�200��
        /// </summary>
        [JsonPropertyName("custName")]
        public string CustomerName { get; set; }

        /// <summary>
        /// �ͻ����ͣ���󳤶ȣ�30��
        /// </summary>
        [JsonPropertyName("custType")]
        public string CustomerType { get; set; }

        /// <summary>
        /// ֤�����ͣ���󳤶ȣ�50��
        /// </summary>
        [JsonPropertyName("certificateType")]
        public string CertificateType { get; set; }

        /// <summary>
        /// ֤�����루��󳤶ȣ�30��
        /// </summary>
        [JsonPropertyName("certificateNo")]
        public string CertificateNumber { get; set; }

        /// <summary>
        /// �����˺ţ���󳤶ȣ�20��
        /// </summary>
        [JsonPropertyName("taAccountId")]
        public string TaAccountId { get; set; }

        /// <summary>
        /// �����˺ţ���󳤶ȣ�30��
        /// </summary>
        [JsonPropertyName("transactionAccountId")]
        public string TransactionAccountId { get; set; }

        /// <summary>
        /// ��Ʒ���ƣ���󳤶ȣ�300��
        /// </summary>
        [JsonPropertyName("fundName")]
        public string FundName { get; set; }

        /// <summary>
        /// ��Ʒ���루��󳤶ȣ�6��
        /// </summary>
        [JsonPropertyName("fundCode")]
        public string FundCode { get; set; }

        /// <summary>
        /// ҵ�����ͣ���󳤶ȣ�6��
        /// </summary>
        [JsonPropertyName("businessCode")]
        public string BusinessCode { get; set; }

        /// <summary>
        /// �����������λС��
        /// </summary>
        [JsonPropertyName("applicationAmount")]
        public string ApplicationAmount { get; set; }

        /// <summary>
        /// ����ݶ������λС��
        /// </summary>
        [JsonPropertyName("applicationVol")]
        public string ApplicationVol { get; set; }

        /// <summary>
        /// �������ڣ���ʽ��yyyymmdd
        /// </summary>
        [JsonPropertyName("transactionDate")]
        public string TransactionDate { get; set; }

        /// <summary>
        /// �������ۿ��ʣ�������λС��
        /// </summary>
        [JsonPropertyName("discountRateOfCommission")]
        public string DiscountRateOfCommission { get; set; }

        /// <summary>
        /// �����������루��󳤶ȣ�3��
        /// </summary>
        [JsonPropertyName("distributorCode")]
        public string DistributorCode { get; set; }

        /// <summary>
        /// �����������ƣ���󳤶ȣ�300��
        /// </summary>
        [JsonPropertyName("distributorName")]
        public string DistributorName { get; set; }

        /// <summary>
        /// ������ˮ�ţ���󳤶ȣ�500��
        /// </summary>
        [JsonPropertyName("remark1")]
        public string Remark1 { get; set; }

        /// <summary>
        /// Ԥ���ֶ�2����󳤶ȣ�500��
        /// </summary>
        [JsonPropertyName("remark2")]
        public string Remark2 { get; set; }

        /// <summary>
        /// ԤԼ�깺���ڣ���ʽ��yyyymmdd����Ϊ�գ�
        /// </summary>
        [JsonPropertyName("futureBuyDate")]
        public string FutureBuyDate { get; set; }

        /// <summary>
        /// ԤԼ������ڣ���ʽ��yyyymmdd����Ϊ�գ�
        /// </summary>
        [JsonPropertyName("redemptionDateInAdvance")]
        public string RedemptionDateInAdvance { get; set; }


        public TransferRequest ToObject()
        {
            return new TransferRequest
            {
                CustomerIdentity = CertificateNumber,
                CustomerName = CustomerName,
                FundName = FundName,
                FundCode = FundCode,
                RequestDate = DateOnly.ParseExact(TransactionDate, "yyyyMMdd"),
                RequestType = TranslateRequest(BusinessCode),
                RequestAmount = ParseDecimal(ApplicationAmount),
                RequestShare = ParseDecimal(ApplicationVol),
                Agency = DistributorName,
                FeeDiscount = ParseDecimal(DiscountRateOfCommission),
                ExternalId = Remark1,
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
        public string FundName { get; set; }

        /// <summary>
        /// ��Ʒ���루�����󳤶�6��
        /// </summary>
        [JsonPropertyName("fundCode")]
        public string FundCode { get; set; }

        /// <summary>
        /// �������ڣ���ʽ��yyyyMMdd�����
        /// </summary>
        [JsonPropertyName("busiDate")]
        public string BusiDate { get; set; }

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
        public string OurAccountNumber { get; set; }

        /// <summary>
        /// �����˻���
        /// </summary>
        [JsonPropertyName("bfzhmc")]
        public string OurAccountName { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        [JsonPropertyName("bz")]
        public string Currency { get; set; }

        /// <summary>
        /// ��Ʒ����
        /// </summary>
        [JsonPropertyName("cpdm")]
        public string ProductCode { get; set; }

        /// <summary>
        /// ��Ʒ����
        /// </summary>
        [JsonPropertyName("cpmc")]
        public string ProductName { get; set; }

        /// <summary>
        /// ���ַ�����������
        /// </summary>
        [JsonPropertyName("dsfkhhmc")]
        public string CounterpartyBankName { get; set; }

        /// <summary>
        /// �Է��˻���
        /// </summary>
        [JsonPropertyName("dsfzhh")]
        public string CounterpartyAccountNumber { get; set; }

        /// <summary>
        /// �Է��˻���
        /// </summary>
        [JsonPropertyName("dsfzhmc")]
        public string CounterpartyAccountName { get; set; }

        /// <summary>
        /// ���׽��
        /// </summary>
        [JsonPropertyName("jyje")]
        public string TransactionAmount { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        [JsonPropertyName("jyrq")]
        public string TransactionDate { get; set; }

        /// <summary>
        /// ����ʱ��
        /// </summary>
        [JsonPropertyName("jysj")]
        public string TransactionTime { get; set; }

        /// <summary>
        /// ��ˮ��
        /// </summary>
        [JsonPropertyName("lsh")]
        public string TransactionId { get; set; }

        /// <summary>
        /// ļ����������
        /// </summary>
        [JsonPropertyName("mjhyh")]
        public string CollectionBank { get; set; }

        /// <summary>
        /// �ո������ա�����
        /// </summary>
        [JsonPropertyName("sffx")]
        public string TransactionType { get; set; }

        /// <summary>
        /// ����ժҪ
        /// </summary>
        [JsonPropertyName("yhbz")]
        public string BankMemo { get; set; }

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

    public class BankBalanceJson
    {
        /// <summary>
        /// ����
        /// </summary>
        [JsonPropertyName("bz")]
        public string Currency { get; set; }

        /// <summary>
        /// ˵��
        /// </summary>
        [JsonPropertyName("clsm")]
        public string Description { get; set; }

        /// <summary>
        /// ��Ʒ����
        /// </summary>
        [JsonPropertyName("cpdm")]
        public string ProductCode { get; set; }

        /// <summary>
        /// ��Ʒ����
        /// </summary>
        [JsonPropertyName("cpmc")]
        public string ProductName { get; set; }

        /// <summary>
        /// ����ʱ�䣨��ʽ���飺yyyyMMddHHmmss��
        /// </summary>
        [JsonPropertyName("gxsj")]
        public string UpdateTime { get; set; }

        /// <summary>
        /// ������
        /// </summary>
        [JsonPropertyName("khzh")]
        public string BankName { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        [JsonPropertyName("kyye")]
        public string AvailableBalance { get; set; }

        /// <summary>
        /// �˺�
        /// </summary>
        [JsonPropertyName("zhh")]
        public string AccountNo { get; set; }

        /// <summary>
        /// �˺�����
        /// </summary>
        [JsonPropertyName("zhmc")]
        public string AccountName { get; set; }

        /// <summary>
        /// �˺����
        /// </summary>
        [JsonPropertyName("zhye")]
        public string AccountBalance { get; set; }

        public FundBankBalance ToObject()
        {
            return new FundBankBalance
            {
                FundCode = ProductCode,
                FundName = ProductName,
                Currency = Currency,
                AccountName = AccountName,
                AccountNo = AccountNo,
                Name = BankName,
                Balance = decimal.Parse(AccountBalance),
                Time = DateTime.ParseExact(UpdateTime, "yyyy-MM-dd HH:mm:ss", null)
            };
        }
    }



    public class InvestorJson
    {
        /// <summary>
        /// �ͻ����ƣ���󳤶�200
        /// </summary>
        [JsonPropertyName("custName")]
        public string CustName { get; set; }

        /// <summary>
        /// �ͻ����ͣ���󳤶�30
        /// </summary>
        [JsonPropertyName("custType")]
        public string CustType { get; set; }

        /// <summary>
        /// ֤�����ͣ���󳤶�50
        /// </summary>
        [JsonPropertyName("certificateType")]
        public string CertificateType { get; set; }

        /// <summary>
        /// ֤�����룬��󳤶�30
        /// </summary>
        [JsonPropertyName("certificateNo")]
        public string CertificateNo { get; set; }

        /// <summary>
        /// �����˺ţ���󳤶�20
        /// </summary>
        [JsonPropertyName("taAccountId")]
        public string TaAccountId { get; set; }

        /// <summary>
        /// �����˺ţ���󳤶�30
        /// </summary>
        [JsonPropertyName("transactionAccountId")]
        public string TransactionAccountId { get; set; }

        /// <summary>
        /// �����˻����ƣ���󳤶�300
        /// </summary>
        [JsonPropertyName("acctName")]
        public string AcctName { get; set; }

        /// <summary>
        /// �����˺ţ���󳤶�20
        /// </summary>
        [JsonPropertyName("acctNo")]
        public string AcctNo { get; set; }

        /// <summary>
        /// ���������ƣ���󳤶�50
        /// </summary>
        [JsonPropertyName("clearingAgency")]
        public string ClearingAgency { get; set; }

        /// <summary>
        /// ������ʡ�ݣ���󳤶�50
        /// </summary>
        [JsonPropertyName("provinces")]
        public string Provinces { get; set; }

        /// <summary>
        /// �����г��У���󳤶�50
        /// </summary>
        [JsonPropertyName("city")]
        public string City { get; set; }

        /// <summary>
        /// �����˺ſ������ڣ���ʽ��yyyymmdd
        /// </summary>
        [JsonPropertyName("openDate")]
        public string OpenDate { get; set; }

        /// <summary>
        /// �ͻ�����������������󳤶�50
        /// </summary>
        [JsonPropertyName("distributorName")]
        public string DistributorName { get; set; }

        /// <summary>
        /// �����������룬��󳤶�3
        /// </summary>
        [JsonPropertyName("distributorCode")]
        public string DistributorCode { get; set; }

        /// <summary>
        /// �Ƿ�רҵͶ�ʻ�������󳤶�10
        /// </summary>
        [JsonPropertyName("individualOrInstitution")]
        public string IndividualOrInstitution { get; set; }


        public Investor ToObject()
        {
            return new Investor
            {
                Name = CustName,
                Identity = new Identity { Id = CertificateNo, Type = ParseIdType(CertificateType) },
                EntityType = ParseCustomerType(CustType),

            };
        }

        private EntityType ParseCustomerType(string custType)
        {
            return custType switch
            {
                "����"=> EntityType.Natural,
                "����" => EntityType.Institution,
                "��Ʒ"=> EntityType.Product,
                _ => EntityType.Unk
            };
        }

        private IDType ParseIdType(string certificateType)
        {
            return certificateType switch
            {
                "0" or "δ֪֤������" or "δ֪" => IDType.Unknown,
                "1" or "���֤" or "�������֤" => IDType.IdentityCard,
                "2" or "�籣��" => IDType.Unknown, // �ɸ���ʵ�����ӳ��
                "3" or "�й�����" => IDType.PassportChina,
                "4" or "����֤" => IDType.OfficerID,
                "5" or "ʿ��֤" => IDType.SoldierID,
                "6" or "�۰ľ��������ڵ�ͨ��֤" => IDType.HongKongMacauPass,
                "7" or "���ڱ�" => IDType.HouseholdRegister,
                "8" or "�������" => IDType.PassportForeign,
                "9" or "����" or "����֤��" => IDType.Other,
                "10" or "��ְ֤" => IDType.CivilianID,
                "11" or "����֤" => IDType.PoliceID,
                "12" or "̨��֤" => IDType.TaiwanCompatriotsID,
                "13" or "��������þ������֤" => IDType.ForeignPermanentResidentID,
                "20" => IDType.Unknown,
                "21" or "����֤��" => IDType.ProductFilingCode, // �ɸ���ʵ�����ӳ��
                "22" or "��֯��������" or "��֯��������֤" => IDType.OrganizationCode,
                "23" or "���ͳһ���ô���" or "ͳһ������ô���" => IDType.UnifiedSocialCreditCode,
                "24" or "����ע���" or "ע���" => IDType.RegistrationNumber,
                "25" or "Ӫҵִ��" or "Ӫҵִ�պ�" => IDType.BusinessLicenseNumber,
                "26" or "��������" => IDType.Other, // �ɸ���ʵ�����ӳ��
                "27" or "�������" => IDType.Other, // �ɸ���ʵ�����ӳ��
                "28" or "����" => IDType.Other, // �ɸ���ʵ�����ӳ��
                "29" or "�侯" => IDType.Other, // �ɸ���ʵ�����ӳ��
                "30" or "�����������������ܵ�λ���ĺţ�" => IDType.Other, // �ɸ���ʵ�����ӳ��
                "31" or "�����" => IDType.Other, // �ɸ���ʵ�����ӳ��
                "32" or "�Ǽ�֤��" => IDType.Other, // �ɸ���ʵ�����ӳ��
                "33" or "����" => IDType.Other, // �ɸ���ʵ�����ӳ��
                "34" or "����" => IDType.Other,
                "40" => IDType.Other, // �ɸ���ʵ�����ӳ��
                "41" => IDType.BusinessLicenseNumber,
                "42" => IDType.Other, // �ɸ���ʵ�����ӳ��
                "43" => IDType.Other,
                "�����˵ǼǱ���" => IDType.ManagerRegistrationCode,
                "��Ʒ��������" => IDType.ProductFilingCode,
                "֤ȯҵ�����֤" => IDType.SecuritiesBusinessLicense,
                "��Ʒ�ǼǱ���" => IDType.ProductRegistrationCode,
                "�۰�̨�����ס֤" => IDType.ResidencePermitForHongKongMacaoAndTaiwanResidents,
                "���еǼ�ϵͳ��Ʒ����" => IDType.TrustRegistrationSystemProductCode,
                _ => IDType.Unknown,
            };
        }
    }


    public static TransferRecordType Translate(string c)
    {
        return c switch
        {
            //"120" => TARecordType.Subscription, //"�Ϲ�ȷ��" ����û�зݶ�����, ���Ϲ����
            "122" => TransferRecordType.Purchase,     //"�깺ȷ��",
            "124" => TransferRecordType.Redemption,// "���ȷ��",
            "126" => TransferRecordType.MoveIn,   //"ת��ȷ��",
            "127" => TransferRecordType.MoveIn,   //"ת������/����ת��",
            "128" => TransferRecordType.MoveOut,  //"ת������/����ת��",
            "129" => TransferRecordType.BonusType,//"�ֺ췽ʽ",
            "130" => TransferRecordType.Subscription, // "�Ϲ����",
            "131" => TransferRecordType.Frozen,       //"�����������",
            "132" => TransferRecordType.Thawed,       //"��������ⶳ",
            //"133" => TARecordType.TransferIn,   //"�ǽ��׹���",
            "134" => TransferRecordType.TransferIn,   //"�ǽ��׹���ת��",
            "135" => TransferRecordType.TransferOut,  //"�ǽ��׹���ת��",
            //"136" => TARecordType.SwitchIn,     //"����ת��",
            "137" => TransferRecordType.SwitchIn,     //"����ת��ת��",
            "138" => TransferRecordType.SwitchOut,    //"����ת��ת��",


            "139" => TransferRecordType.Purchase,     //"��ʱ�����깺",
            "142" => TransferRecordType.ForceRedemption,//"ǿ�����",
            "143" => TransferRecordType.Distribution,     //"�ֺ�ȷ��",
            "144" => TransferRecordType.Increase,     //"ǿ�е���",
            "145" => TransferRecordType.Decrease,     //"ǿ�е���",
            _ => TransferRecordType.UNK,              //"δ֪ҵ������"
        };
    }


    public static TransferRequestType TranslateRequest(string c)
    {
        return c switch
        {
            "122" => TransferRequestType.Purchase,
            "124" => TransferRequestType.Redemption,
            "130" => TransferRequestType.Subscription,
            "142" => TransferRequestType.ForceRedemption,
            "144" => TransferRequestType.Increase,     //"ǿ�е���", �ݶ����͵���
            "145" => TransferRequestType.Decrease,     //"ǿ�е���",
            _ => TransferRequestType.UNK
        };
    }
}



#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��