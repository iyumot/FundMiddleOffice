using FMO.Models;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace FMO.Trustee;

public partial class CITICS
{

    public class RootJson
    {
        [JsonPropertyName("data")]
        public JsonObject? Data { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; } = -1;

        [JsonPropertyName("message")]
        public string? Msg { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

    }



    public class ReturnJsonRoot<T>
    {
        [JsonPropertyName("data")]
        public T? Data { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string? Msg { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

    }



    public class QueryRoot<T>
    {
        /// <summary>
        /// ��ǰҳ
        /// </summary>
        [JsonPropertyName("pageNum")]
        public int PageNum { get; set; }

        /// <summary>
        /// ��ǰҳ������
        /// </summary>
        [JsonPropertyName("size")]
        public int Size { get; set; }

        /// <summary>
        /// ���
        /// </summary>
        [JsonPropertyName("list")]
        public List<T>? List { get; set; }

        /// <summary>
        /// ��һҳ
        /// </summary>
        [JsonPropertyName("hasNextPage")]
        public bool HasNextPage { get; set; }


        [JsonPropertyName("pages")]
        public int Pages { get; set; }

        [JsonPropertyName("navigatePages")]
        public int NavPages { get; set; }


        public int PageCount => Pages == 0 ? NavPages : Pages;

    }


    public class TokenJson
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }
    }


    public class InvestorJson
    {
        [JsonPropertyName("custName")]
        public required string CustName { get; set; } // Ͷ��������

        [JsonPropertyName("fundAcco")]
        public required string FundAcco { get; set; } // �����˺�

        [JsonPropertyName("tradeAcco")]
        public required string TradeAcco { get; set; } // �����˺�

        [JsonPropertyName("custType")]
        public required string CustType { get; set; } // �ͻ����ͣ��μ���¼4��

        [JsonPropertyName("certiType")]
        public required string CertiType { get; set; } // ֤�����ͣ��μ���¼4��

        [JsonPropertyName("certiNo")]
        public required string CertiNo { get; set; } // ֤����

        [JsonPropertyName("bankNo")]
        public string? BankNo { get; set; } // ���б��

        [JsonPropertyName("bankAccount")]
        public string? BankAccount { get; set; } // �����˺�

        [JsonPropertyName("bankOpenName")]
        public string? BankOpenName { get; set; } // ����������

        [JsonPropertyName("bankAccountName")]
        public string? BankAccountName { get; set; } // ���л���

        [JsonPropertyName("address")]
        public string? Address { get; set; } // ͨѶ��ַ

        [JsonPropertyName("tel")]
        public string? Tel { get; set; } // ��ϵ�绰

        [JsonPropertyName("zipCode")]
        public string? ZipCode { get; set; } // �ʱ�

        [JsonPropertyName("agencyNo")]
        public string? AgencyNo { get; set; } // �����̴��룬ZX6��ʾֱ��

        [JsonPropertyName("email")]
        public string? Email { get; set; } // ����

        public Investor ToObject()
        {
            return new Investor
            {
                Name = CustName,
                Identity = new Identity { Id = CertiNo, Type = ParseIdType(CustType, CertiType) },
                Email = Email,
                Phone = Tel,
            };
        }

    }

    public static IDType ParseIdType(string custType, string certiType)
    {
        if (string.IsNullOrEmpty(custType) || string.IsNullOrEmpty(certiType))
            return IDType.Unknown;

        // ����ͻ����ͣ�0=������1=���ˣ�2=��Ʒ
        switch (custType)
        {
            case "0": // ����
                return certiType.ToUpper() switch
                {
                    "0" => IDType.OrganizationCode, // ��֯��������֤
                    "1" => IDType.BusinessLicenseNumber,      // Ӫҵִ��
                    "2" => IDType.RegistrationNumber,         // ��������
                    "3" => IDType.OrganizationCode,           // �������
                    "4" => IDType.Other,                     // ����
                    "5" => IDType.Other,                     // �侯
                    "6" => IDType.Other,                     // ��������
                    "7" => IDType.Other,                     // �����
                    "8" => IDType.Other,                     // ��������
                    "9" => IDType.ProductFilingCode,         // �Ǽ�֤��
                    "A" => IDType.ManagerRegistrationCode,   // ����
                    _ => IDType.Unknown
                };

            case "1": // ����
                return certiType.ToUpper() switch
                {
                    "0" => IDType.IdentityCard,                    // ���֤
                    "1" => IDType.PassportChina,                   // �й�����
                    "2" => IDType.OfficerID,                       // ����֤
                    "3" => IDType.SoldierID,                       // ʿ��֤
                    "4" => IDType.HongKongMacauPass,               // �۰ľ��������ڵ�ͨ��֤
                    "5" => IDType.HouseholdRegister,               // ���ڱ�
                    "6" => IDType.PassportForeign,                 // �⼮����
                    "7" => IDType.Other,                           // ����
                    "8" => IDType.CivilianID,                      // ��ְ֤
                    "9" => IDType.PoliceID,                        // ����֤
                    "A" => IDType.TaiwanCompatriotsID,             // ̨��֤
                    "B" => IDType.ForeignPermanentResidentID,      // ��������þ������֤
                    "C" => IDType.ResidencePermitForHongKongMacaoAndTaiwanResidents, // �۰�̨�����ס֤
                    _ => IDType.Unknown
                };

            case "2": // ��Ʒ
                return certiType.ToUpper() switch
                {
                    "1" => IDType.BusinessLicenseNumber, // Ӫҵִ�գ�ֱ���ӿڲ�����ʹ�ã�
                    "8" => IDType.Other,                 // ����
                    "9" => IDType.ProductFilingCode,     // �Ǽ�֤�飨ֱ���ӿڲ�����ʹ�ã�
                    "A" => IDType.ManagerRegistrationCode, // ����
                    _ => IDType.Unknown
                };

            default:
                return IDType.Unknown;
        }
    }

    public class TransferRecordJson
    {
        [JsonPropertyName("ackNo")]
        public required string AckNo { get; set; }

        [JsonPropertyName("exRequestNo")]
        public required string ExRequestNo { get; set; }

        [JsonPropertyName("fundAcco")]
        public required string FundAcco { get; set; }

        [JsonPropertyName("tradeAcco")]
        public required string TradeAcco { get; set; }

        /// <summary>
        /// ҵ�����ͣ�
        ///01-�Ϲ�	50-�������
        ///02-�깺	51-������ֹ
        ///03-���	52-��������
        ///04-ת�й�	54-����ʧ��
        ///05-�й�ת��	70-ǿ�Ƶ���
        ///06-�й�ת��	71-ǿ�Ƶ���
        ///07-�޸ķֺ췽ʽ	74-��������
        ///10-�ݶ��	77-�����깺�޸�
        ///11-�ݶ�ⶳ	78-��������޸�
        ///12-�ǽ��׹���	81-���𿪻�
        ///13-����ת��(��)  82-��������
        ///14-�ǽ��׹�����	83-�˻��޸�
        ///15-�ǽ��׹�����	84-�˻�����
        ///16-����ת����	85-�˻��ⶳ
        ///17-����	88-�˻��Ǽ�
        ///20-�ڲ�ת�й�	89-ȡ���Ǽ�
        ///21-�ڲ�ת�й���	90-�����깺Э��
        ///22-�ڲ�ת�йܳ�	91-�������Э��
        ///31-���������˺�	92-��ŵ�Ż�Э��
        ///32-��������˺�	93-�����깺ȡ��
        ///41-����	94-�������ȡ��
        ///42-����	96-�������˺�
        ///46-��������ѯ	97-�ڲ�ת�й�
        ///47-��������ͨ	98-�ڲ��й�ת��
        ///48-������ȡ��	99-�ڲ��й�ת��
        ///49-����������
        /// </summary>
        [JsonPropertyName("apkind")]
        public required string Apkind { get; set; } // ҵ�����ʹ���

        [JsonPropertyName("taFlag")]
        public required string TaFlag { get; set; } // TA�����־��0-��1-��

        [JsonPropertyName("fundCode")]
        public required string FundCode { get; set; }

        [JsonPropertyName("shareLevel")]
        public required string ShareLevel { get; set; } // �ݶ����A-ǰ�շѣ�B-���շ�

        [JsonPropertyName("currency")]
        public required string Currency { get; set; } // ���֣�����ҡ���Ԫ �� null

        [JsonPropertyName("largeRedemptionFlag")]
        public required string LargeRedemptionFlag { get; set; } // �޶���ش����־��0-ȡ����1-˳��

        [JsonPropertyName("subAmt")]
        public required string SubAmt { get; set; } // ������

        [JsonPropertyName("subQuty")]
        public required string SubQuty { get; set; } // ����ݶ�

        [JsonPropertyName("bonusType")]
        public required string BonusType { get; set; } // �ֺ췽ʽ��0-������Ͷ�ʣ�1-�ֽ����

        [JsonPropertyName("nav")]
        public required string Nav { get; set; } // ��λ��ֵ

        [JsonPropertyName("ackAmt")]
        public required string AckAmt { get; set; } // ȷ�Ͻ��

        [JsonPropertyName("ackQuty")]
        public required string AckQuty { get; set; } // ȷ�Ϸݶ�

        [JsonPropertyName("tradeFee")]
        public required string TradeFee { get; set; } // ���׷�

        [JsonPropertyName("taFee")]
        public required string TaFee { get; set; } // ������

        [JsonPropertyName("backFee")]
        public required string BackFee { get; set; } // ���շ�

        [JsonPropertyName("realBalance")]
        public required string RealBalance { get; set; } // Ӧ�������

        [JsonPropertyName("profitBalance")]
        public required string ProfitBalance { get; set; } // ҵ������

        [JsonPropertyName("profitBalanceForAgency")]
        public required string ProfitBalanceForAgency { get; set; } // ҵ�������������

        [JsonPropertyName("totalNav")]
        public required string TotalNav { get; set; } // �ۼƾ�ֵ

        [JsonPropertyName("totalFee")]
        public required string TotalFee { get; set; } // �ܷ���

        [JsonPropertyName("agencyFee")]
        public required string AgencyFee { get; set; } // �����ۻ�������

        [JsonPropertyName("fundFee")]
        public required string FundFee { get; set; } // ������ʲ�����

        [JsonPropertyName("registFee")]
        public required string RegistFee { get; set; } // ������˷���

        [JsonPropertyName("interest")]
        public required string Interest { get; set; } // ��Ϣ

        [JsonPropertyName("interestTax")]
        public required string InterestTax { get; set; } // ��Ϣ˰

        [JsonPropertyName("interestShare")]
        public required string InterestShare { get; set; } // ��Ϣת�ݶ�

        [JsonPropertyName("frozenBalance")]
        public required string FrozenBalance { get; set; } // ȷ�϶���ݶ�

        [JsonPropertyName("unfrozenBalance")]
        public required string UnfrozenBalance { get; set; } // ȷ�Ͻⶳ�ݶ�

        [JsonPropertyName("unShares")]
        public required string UnShares { get; set; } // �޶����˳�ӷݶ�

        [JsonPropertyName("applyDate")]
        public required string ApplyDate { get; set; } // ���빤����

        [JsonPropertyName("ackDate")]
        public required string AckDate { get; set; } // ȷ������

        /// <summary>
        /// ȷ��״̬:
        ///0-δ����	9-�ӻ�����
        ///1-ȷ�ϳɹ� a-δ����
        ///2-ȷ��ʧ�� b-δ����
        ///3-���׳��� c-δ����
        ///4-���ȷ�� d-δ����
        ///5-��ʷ�� e-δ����
        ///6-������� f-δ����
        ///7-�޶�������� g-δ����
        ///8-��ʱ����
        /// </summary>
        [JsonPropertyName("ackStatus")]
        public required string AckStatus { get; set; } // ȷ��״̬

        [JsonPropertyName("agencyNo")]
        public required string AgencyNo { get; set; } // �����̴���

        [JsonPropertyName("agencyName")]
        public required string AgencyName { get; set; } // ����������

        [JsonPropertyName("retCod")]
        public required string RetCod { get; set; } // ������

        [JsonPropertyName("retMsg")]
        public required string RetMsg { get; set; } // ʧ��ԭ��

        [JsonPropertyName("adjustCause")]
        public required string AdjustCause { get; set; } // �ݶ����ԭ��

        [JsonPropertyName("navDate")]
        public required string NavDate { get; set; } // ��ֵ���� (YYYYMMDD)

        [JsonPropertyName("oriCserialNo")]
        public required string OriCserialNo { get; set; } // ԭȷ�ϵ���

        /// <summary>
        /// ȱ��Customer ��Ϣ
        /// Fund ��Ϣ
        /// </summary>
        /// <returns></returns>
        internal TransferRecord ToObject()
        {
            return new TransferRecord
            {
                ExternalId = AckNo,
                ExternalRequestId = ExRequestNo,
                Type = ParseRecordType(Apkind),
                FundCode = FundCode,
                Agency = AgencyName,
                RequestDate = DateOnly.ParseExact(ApplyDate, "yyyyMMdd"),
                RequestAmount = decimal.Parse(SubAmt),
                RequestShare = decimal.Parse(SubQuty),
                ConfirmedDate = DateOnly.ParseExact(AckDate, "yyyyMMdd"),
                ConfirmedAmount = decimal.Parse(SubAmt),
                ConfirmedShare = decimal.Parse(SubQuty),
                Fee = decimal.Parse(TotalFee),
                PerformanceFee = decimal.Parse(ProfitBalance),
                CustomerIdentity = "unset",
                CustomerName = "unset",
                CreateDate = DateOnly.FromDateTime(DateTime.Now),
                ConfirmedNetAmount = decimal.Parse(RealBalance),
            };
        }
    }


    /// <summary>
    /// ���ڲ�Ʒ��������ģ��
    /// </summary>
    public class FundDailyFeeJson
    {
        /// <summary>
        /// ��Ʒ����
        /// </summary>
        [JsonPropertyName("fcpdm")]
        public required string ProductCode { get; set; }

        /// <summary>
        /// �ּ���Ʒ����
        /// </summary>
        [JsonPropertyName("ffjdm")]
        public string? ClassificationCode { get; set; }

        /// <summary>
        /// ��Ʒ����
        /// </summary>
        [JsonPropertyName("fcpmc")]
        public required string ProductName { get; set; }

        /// <summary>
        /// ҵ�����ڣ���ʽ��YYYY - MM - DD
        /// </summary>
        [JsonPropertyName("cdate")]
        public required string BusinessDate { get; set; }

        #region ��������
        /// <summary>
        /// ��������
        /// </summary>
        [JsonPropertyName("jtglf")]
        public required string AccruedManagementFee { get; set; }

        /// <summary>
        /// ֧�������
        /// </summary>
        [JsonPropertyName("zfglf")]
        public required string PaidManagementFee { get; set; }

        /// <summary>
        /// δ֧�������
        /// </summary>
        [JsonPropertyName("wzfglf")]
        public required string UnpaidManagementFee { get; set; }
        #endregion

        #region ҵ���������
        /// <summary>
        /// ����ҵ������
        /// </summary>
        [JsonPropertyName("jtyjbc")]
        public required string AccruedPerformanceFee { get; set; }

        /// <summary>
        /// ֧��ҵ������
        /// </summary>
        [JsonPropertyName("zfyjbc")]
        public required string PaidPerformanceFee { get; set; }

        /// <summary>
        /// δ֧��ҵ������
        /// </summary>
        [JsonPropertyName("wzfyjbc")]
        public required string UnpaidPerformanceFee { get; set; }
        #endregion

        #region �йܷ����
        /// <summary>
        /// �����йܷ�
        /// </summary>
        [JsonPropertyName("jttgf")]
        public required string AccruedCustodyFee { get; set; }

        /// <summary>
        /// ֧���йܷ�
        /// </summary>
        [JsonPropertyName("zftgf")]
        public required string PaidCustodyFee { get; set; }

        /// <summary>
        /// δ֧���йܷ�
        /// </summary>
        [JsonPropertyName("wzftgf")]
        public required string UnpaidCustodyFee { get; set; }
        #endregion

        #region ������������
        /// <summary>
        /// �������������
        /// </summary>
        [JsonPropertyName("jtxzfwf")]
        public required string AccruedAdministrativeFee { get; set; }

        /// <summary>
        /// ֧�����������
        /// </summary>
        [JsonPropertyName("zfxzfwf")]
        public required string PaidAdministrativeFee { get; set; }

        /// <summary>
        /// δ֧�����������
        /// </summary>
        [JsonPropertyName("wzfxzfwf")]
        public required string UnpaidAdministrativeFee { get; set; }
        #endregion

        #region ���۷�������
        /// <summary>
        /// �������۷����
        /// </summary>
        [JsonPropertyName("jtxsfwf")]
        public required string AccruedSalesServiceFee { get; set; }

        /// <summary>
        /// ֧�����۷����
        /// </summary>
        [JsonPropertyName("zfxsfwf")]
        public required string PaidSalesServiceFee { get; set; }

        /// <summary>
        /// δ֧�����۷����
        /// </summary>
        [JsonPropertyName("wzfxsfwf")]
        public required string UnpaidSalesServiceFee { get; set; }
        #endregion

        #region Ͷ�ʹ��ʷ����
        /// <summary>
        /// ����Ͷ�ʹ��ʷ�
        /// </summary>
        [JsonPropertyName("jttzgwf")]
        public required string AccruedInvestmentAdvisoryFee { get; set; }

        /// <summary>
        /// ֧��Ͷ�ʹ��ʷ�
        /// </summary>
        [JsonPropertyName("zftzgwf")]
        public required string PaidInvestmentAdvisoryFee { get; set; }

        /// <summary>
        /// δ֧��Ͷ�ʹ��ʷ�
        /// </summary>
        [JsonPropertyName("wzftzgwf")]
        public required string UnpaidInvestmentAdvisoryFee { get; set; }
        #endregion

        #region ��ֵ˰����˰���
        /// <summary>
        /// ������ֵ˰����˰
        /// </summary>
        [JsonPropertyName("jtzzsfjs")]
        public required string AccruedVatSurcharge { get; set; }

        /// <summary>
        /// ֧����ֵ˰����˰
        /// </summary>
        [JsonPropertyName("zfzzsfjs")]
        public required string PaidVatSurcharge { get; set; }

        /// <summary>
        /// δ֧����ֵ˰����˰
        /// </summary>
        [JsonPropertyName("wzfzzsfjs")]
        public required string UnpaidVatSurcharge { get; set; }
        #endregion

        #region ��Ʒ����
        /// <summary>
        /// ������Ʒ�
        /// </summary>
        [JsonPropertyName("jtsjf")]
        public required string AccruedAuditFee { get; set; }

        /// <summary>
        /// ֧����Ʒ�
        /// </summary>
        [JsonPropertyName("zfsjf")]
        public required string PaidAuditFee { get; set; }

        /// <summary>
        /// δ֧����Ʒ�
        /// </summary>
        [JsonPropertyName("wzfsjf")]
        public required string UnpaidAuditFee { get; set; }
        #endregion


        public FundDailyFee ToObject()
        {
            var code = ClassificationCode switch { "M" => ProductCode, null => ProductCode, var n => n };



            return new FundDailyFee
            {
                FundCode = code,
                Date = DateOnly.ParseExact(BusinessDate, "yyyy-MM-dd"),
                ManagerFeeAccrued = ParseDecimal(AccruedManagementFee),
                ManagerFeePaid = ParseDecimal(PaidManagementFee),
                ManagerFeeBalance = ParseDecimal(UnpaidManagementFee),
                PerformanceFeeAccrued = ParseDecimal(AccruedPerformanceFee),
                PerformanceFeePaid = ParseDecimal(PaidPerformanceFee),
                PerformanceFeeBalance = ParseDecimal(UnpaidPerformanceFee),
                CustodianFeeAccrued = ParseDecimal(AccruedCustodyFee),
                ConsultantFeePaid = ParseDecimal(PaidCustodyFee),
                ConsultantFeeBalance = ParseDecimal(UnpaidCustodyFee),
                OutsourcingFeeAccrued = ParseDecimal(AccruedAdministrativeFee),
                OutsourcingFeePaid = ParseDecimal(PaidAdministrativeFee),
                OutsourcingFeeBalance = ParseDecimal(UnpaidAdministrativeFee),
                SalesFeeAccrued = ParseDecimal(AccruedSalesServiceFee),
                SalesFeePaid = ParseDecimal(PaidSalesServiceFee),
                SalesFeeBalance = ParseDecimal(UnpaidSalesServiceFee),
                ConsultantFeeAccrued = ParseDecimal(AccruedInvestmentAdvisoryFee),
                CustodianFeePaid = ParseDecimal(PaidInvestmentAdvisoryFee),
                CustodianFeeBalance = ParseDecimal(UnpaidInvestmentAdvisoryFee),
            };
        }

        private decimal ParseDecimal(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            if (decimal.TryParse(value, out var result))
                return result;

            throw new FormatException($"�޷��� '{value}' ����Ϊdecimal����");
        }
    }






    public class BankTransactionJson
    {

        /// <summary>
        /// ���׷���ʱ�䣬��ʽΪ YYYY-MM-DD HH:MM:SS
        /// </summary>
        [JsonPropertyName("occurTime")]
        public required string OccurTime { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        [JsonPropertyName("fundCode")]
        public required string FundCode { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        [JsonPropertyName("fundName")]
        public required string FundName { get; set; }

        /// <summary>
        /// �˻����ͣ����� "02-ļ����"
        /// </summary>
        [JsonPropertyName("accoType")]
        public required string AccoType { get; set; }

        /// <summary>
        /// �����˺�
        /// </summary>
        [JsonPropertyName("bankAcco")]
        public required string BankAcco { get; set; }

        /// <summary>
        /// �����˻�����
        /// </summary>
        [JsonPropertyName("accName")]
        public required string AccName { get; set; }

        /// <summary>
        /// ��������������
        /// </summary>
        [JsonPropertyName("openBankName")]
        public required string OpenBankName { get; set; }

        /// <summary>
        /// �������б�ţ������д�����ǰ��λΪ��׼������¼2��
        /// </summary>
        [JsonPropertyName("bankNo")]
        public required string BankNo { get; set; }

        /// <summary>
        /// �Է��˺�
        /// </summary>
        [JsonPropertyName("othBankAcco")]
        public required string OthBankAcco { get; set; }

        /// <summary>
        /// �Է��˻�����
        /// </summary>
        [JsonPropertyName("othAccName")]
        public required string OthAccName { get; set; }

        /// <summary>
        /// �Է�����������
        /// </summary>
        [JsonPropertyName("othOpenBankName")]
        public required string OthOpenBankName { get; set; }

        /// <summary>
        /// �Է����б�ţ������д�����ǰ��λΪ��׼������¼2��
        /// </summary>
        [JsonPropertyName("othBankNo")]
        public required string OthBankNo { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        [JsonPropertyName("curType")]
        public required string CurType { get; set; }

        /// <summary>
        /// �ո�����1��ʾ�տ2��ʾ����
        /// </summary>
        [JsonPropertyName("directFlag")]
        public required string DirectFlag { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        [JsonPropertyName("occurAmt")]
        public required string OccurAmt { get; set; }

        /// <summary>
        /// �˻����
        /// </summary>
        [JsonPropertyName("acctBal")]
        public required string AcctBal { get; set; }

        /// <summary>
        /// ������ˮ��
        /// </summary>
        [JsonPropertyName("bankJour")]
        public required string BankJour { get; set; }

        /// <summary>
        /// ���з��ش���
        /// </summary>
        [JsonPropertyName("bankRetCode")]
        public required string BankRetCode { get; set; }

        /// <summary>
        /// ����ժҪ
        /// </summary>
        [JsonPropertyName("bankNote")]
        public required string BankNote { get; set; }



        public BankTransaction ToObject()
        {
            return new BankTransaction
            {
                Id = $"{BankAcco}|{BankJour}",
                Time = DateTime.ParseExact(OccurTime, "yyyy-MM-dd HH:mm:ss", null),
                AccountBank = OpenBankName,
                AccountName = AccName,
                AccountNo = BankAcco,
                CounterBank = OthOpenBankName,
                CounterName = OthAccName,
                CounterNo = OthBankAcco,
                Amount = decimal.Parse(OccurAmt),
                Balance = decimal.Parse(AcctBal),
                Direction = DirectFlag == "����" ? TransctionDirection.Pay : TransctionDirection.Receive,
                Remark = BankNote,
                Serial = BankJour,
            };
        }
    }




    public class CustodialAccountJson
    {
        [JsonPropertyName("pdCode")]
        public required string ProductCode { get; set; }

        [JsonPropertyName("pdName")]
        public required string ProductName { get; set; }

        [JsonPropertyName("accName")]
        public required string AccountName { get; set; }

        [JsonPropertyName("account")]
        public required string Account { get; set; }

        [JsonPropertyName("bankName")]
        public required string BankName { get; set; }

        [JsonPropertyName("lrgAmtNo")]
        public required string LargePaymentNumber { get; set; }

        [JsonPropertyName("recentBalance")]
        public required string RecentBalance { get; set; }

        [JsonPropertyName("recentBalanceQueryTime")]
        public required string BalanceQueryTime { get; set; }

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


    public class VirtualNetValueJson
    {
        /// <summary>
        /// ҵ�����ڣ���ʽ��YYYY-MM-DD
        /// </summary>
        [JsonPropertyName("date")]
        public required string BusinessDate { get; set; }

        /// <summary>
        /// �ͻ�����
        /// </summary>
        [JsonPropertyName("custName")]
        public required string CustomerName { get; set; }

        /// <summary>
        /// �����˺�
        /// </summary>
        [JsonPropertyName("fundAcco")]
        public required string FundAccount { get; set; }

        /// <summary>
        /// ��Ʒ����
        /// </summary>
        [JsonPropertyName("fundCode")]
        public required string FundCode { get; set; }

        /// <summary>
        /// ��Ʒ����
        /// </summary>
        [JsonPropertyName("fundName")]
        public required string FundName { get; set; }

        /// <summary>
        /// �ֲַݶ�
        /// </summary>
        [JsonPropertyName("shares")]
        public decimal Shares { get; set; }

        /// <summary>
        /// ���᷽ʽ��1-��ֵ���᣻2-TA����
        /// </summary>
        [JsonPropertyName("flag")]
        public required string AccrualMethod { get; set; }

        /// <summary>
        /// ����ҵ��������
        /// </summary>
        [JsonPropertyName("virtualBalance")]
        public decimal VirtualPerformanceFee { get; set; }

        /// <summary>
        /// ���⾻ֵ
        /// </summary>
        [JsonPropertyName("virtualAssetVal")]
        public required string VirtualNetAssetValue { get; set; }

        /// <summary>
        /// ʵ�ʾ�ֵ
        /// </summary>
        [JsonPropertyName("netAssetVal")]
        public required string ActualNetAssetValue { get; set; }

        /// <summary>
        /// �ۼƾ�ֵ
        /// </summary>
        [JsonPropertyName("totalAssetVal")]
        public required string TotalNetAssetValue { get; set; }

        /// <summary>
        /// ����ۼ��ݶ�
        /// </summary>
        [JsonPropertyName("virtualDeductionShare")]
        public decimal VirtualDeductionShare { get; set; }

        /// <summary>
        /// ��ֵ�˶�״̬��
        /// 0-һ�£��йܸ���һ�£�
        /// 1-��һ�£�δ���й�ȷ�ϣ�
        /// 2-�����У�
        /// 3-���йܷ���Ʒ�����йܷ�����
        /// </summary>
        [JsonPropertyName("checkStatus")]
        public required string NetValueCheckStatus { get; set; }
    }


    public class BankBalanceJson
    {
        /// <summary>
        /// �˺�
        /// </summary>
        [JsonPropertyName("YHZH")]
        public required string AccountNumber { get; set; }

        /// <summary>
        /// �˻����
        /// </summary>
        [JsonPropertyName("ZHYE")]
        public required string AccountBalance { get; set; }

        /// <summary>
        /// �˻�����
        /// </summary>
        [JsonPropertyName("KHHM")]
        public required string AccountHolderName { get; set; }

        /// <summary>
        /// �������ࣺ
        /// HKD-�۱ң�
        /// RMB-����ң�
        /// USD-��Ԫ
        /// </summary>
        [JsonPropertyName("JSBZ")]
        public required string CurrencyType { get; set; }

        /// <summary>
        /// ����ѯʱ�䣬��ʽ��YYYY-MM-DD HH:MM:SS
        /// </summary>
        [JsonPropertyName("CXSJ")]
        public required string QueryTime { get; set; }

        /// <summary>
        /// �˻��������
        /// </summary>
        [JsonPropertyName("ZHKYYE")]
        public required string AvailableBalance { get; set; }

        /// <summary>
        /// ��������
        /// 0-�ɹ���
        /// -2-�����У�
        /// ����ֵ����ʧ��
        /// </summary>
        [JsonPropertyName("CLJG")]
        public required string ProcessingResult { get; set; }

        /// <summary>
        /// ����˵��
        /// </summary>
        [JsonPropertyName("CLSM")]
        public required string ProcessingDescription { get; set; }

        /// <summary>
        /// ������
        /// </summary>
        [JsonPropertyName("KHYH")]
        public required string OpeningBank { get; set; }

        public BankBalance ToObject()
        {
            return new BankBalance
            {
                AccountName = AccountHolderName,
                AccountNo = AccountNumber,
                Name = OpeningBank,
                Balance = decimal.Parse(AccountBalance),
                Currency = CurrencyType,
                Time = DateTime.ParseExact(QueryTime, "yyyy-MM-dd HH:mm:ss", null)
            };
        }
    }


    public class CustodialTransactionJson
    {
        /// <summary>
        /// ���д���
        /// </summary>
        [JsonPropertyName("YHDM")]
        public required string BankCode { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        [JsonPropertyName("YHMC")]
        public required string BankName { get; set; }

        /// <summary>
        /// �����˺ţ�CLJGΪ-1ʱ���ѯ�ʺţ�
        /// </summary>
        [JsonPropertyName("FKFZH")]
        public required string PayerAccountNo { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        [JsonPropertyName("FKFHM")]
        public required string PayerAccountName { get; set; }

        /// <summary>
        /// �շ��˺ţ�CLJGΪ-1ʱ���ѯ�ʺţ�
        /// </summary>
        [JsonPropertyName("SKFZH")]
        public required string PayeeAccountNo { get; set; }

        /// <summary>
        /// �շ�����
        /// </summary>
        [JsonPropertyName("SKFHM")]
        public required string PayeeAccountName { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        [JsonPropertyName("FSJE")]
        public decimal Amount { get; set; }

        /// <summary>
        /// �������
        /// HKD: �۱�, RMB: �����, USD: ��Ԫ
        /// </summary>
        [JsonPropertyName("JSBZ")]
        public required string Currency { get; set; }

        /// <summary>
        /// �����־��
        /// �衢��������=�衢�ۿ�=�衢�տ�=�����ۿ�����տ��
        /// </summary>
        [JsonPropertyName("JDBZ")]
        public required string DebitCreditFlag { get; set; }

        /// <summary>
        /// ժҪ����
        /// </summary>
        [JsonPropertyName("ZYMC")]
        public required string Summary { get; set; }

        /// <summary>
        /// ����ʱ�䣬��ʽ yyyy-MM-dd HH:mm:ss
        /// </summary>
        [JsonPropertyName("FSSJ")]
        public required string OccurTime { get; set; }

        /// <summary>
        /// ������Ϣ
        /// </summary>
        [JsonPropertyName("FHXX")]
        public required string ReturnInfo { get; set; }

        /// <summary>
        /// �����ֶ�
        /// </summary>
        [JsonPropertyName("BYZD")]
        public required string ReservedField { get; set; }

        /// <summary>
        /// �˻����
        /// </summary>
        [JsonPropertyName("ZHYE")]
        public decimal AccountBalance { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        [JsonPropertyName("KYYE")]
        public decimal AvailableBalance { get; set; }

        /// <summary>
        /// ��ˮ��
        /// </summary>
        [JsonPropertyName("LSH")]
        public required string SerialNumber { get; set; }

        /// <summary>
        /// ������
        /// 0 - �ɹ�
        /// -2 - ������
        /// ����ֵ����ʧ��
        /// </summary>
        [JsonPropertyName("CLJG")]
        public required string ProcessResult { get; set; }

        /// <summary>
        /// ����˵��
        /// </summary>
        [JsonPropertyName("CLSM")]
        public required string ProcessDescription { get; set; }


        public BankTransaction ToObject()
        {
            return new BankTransaction
            {
                AccountBank = BankName,
                AccountName = PayerAccountName,
                AccountNo = PayerAccountNo,
                CounterBank = "",
                CounterName = PayeeAccountName,
                CounterNo = PayeeAccountNo,
                Id = SerialNumber,
                Serial = SerialNumber,
                Amount = Amount,
                Balance = AccountBalance,
                Currency = Currency,
                Direction = DebitCreditFlag switch { string s when s.Contains("����") => TransctionDirection.Cancel, "��" or "���" or "�ۿ�" => TransctionDirection.Pay, _ => TransctionDirection.Receive },
                Remark = Summary,
                Time = DateTime.ParseExact(OccurTime, "yyyy-MM-dd HH:mm:ss", null)
            };
        }

    }






    public class CustodialTransactionJson2
    {
        /// <summary>
        /// �����˺�
        /// </summary>
        [JsonPropertyName("fpayerAcctCode")]
        public required string PayerAccountCode { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        [JsonPropertyName("fpayerName")]
        public required string PayerName { get; set; }

        /// <summary>
        /// ����������
        /// </summary>
        [JsonPropertyName("fpayerBank")]
        public required string PayerBank { get; set; }

        /// <summary>
        /// �տ�˺�
        /// </summary>
        [JsonPropertyName("fpayeeAcctCode")]
        public required string PayeeAccountCode { get; set; }

        /// <summary>
        /// �տ����
        /// </summary>
        [JsonPropertyName("fpayeeName")]
        public required string PayeeName { get; set; }

        /// <summary>
        /// �տ������
        /// </summary>
        [JsonPropertyName("fpayeeBank")]
        public required string PayeeBank { get; set; }

        /// <summary>
        /// �������ڣ���ʽ��yyyy-MM-dd��
        /// </summary>
        [JsonPropertyName("date")]
        public required string OccurDate { get; set; }

        /// <summary>
        /// �տ����룺
        /// SFKFX001: ����
        /// SFKFX002: ���
        /// SFKFX003: ����
        /// </summary>
        [JsonPropertyName("fway")]
        public required string DirectionCode { get; set; }

        /// <summary>
        /// �տ�����ƣ�SFKFX001�����SFKFX002����SFKFX003��������
        /// </summary>
        [JsonPropertyName("fwayName")]
        public required string DirectionName { get; set; }

        /// <summary>
        /// �������ַ������ͣ�������λС����
        /// </summary>
        [JsonPropertyName("famount")]
        public decimal Amount { get; set; }

        /// <summary>
        /// ��ע
        /// </summary>
        [JsonPropertyName("fsummary")]
        public required string Summary { get; set; }

        public BankTransaction ToObject()
        {
            var direction = DirectionCode switch { "SFKFX001" or "SFKFX003" => TransctionDirection.Pay,/* "SFKFX002"*/ _ => TransctionDirection.Receive };

            return new BankTransaction
            {
                AccountBank = direction == TransctionDirection.Pay ? PayerBank : PayeeBank,
                AccountName = direction == TransctionDirection.Pay ? PayerName : PayeeName,
                AccountNo = direction == TransctionDirection.Pay ? PayerAccountCode : PayeeAccountCode,
                CounterBank = direction != TransctionDirection.Pay ? PayerBank : PayeeBank,
                CounterName = direction != TransctionDirection.Pay ? PayerName : PayeeName,
                CounterNo = direction != TransctionDirection.Pay ? PayerAccountCode : PayeeAccountCode,
                Id = $"{PayeeAccountCode.GetHashCode()}|{PayerAccountCode.GetHashCode()}|{OccurDate}{Amount}",
                Serial = $"{PayeeAccountCode.GetHashCode()}|{PayerAccountCode.GetHashCode()}|{OccurDate}{Amount}",
                Amount = Amount,
                Direction = direction,
                Remark = Summary,
                Time = DateTime.ParseExact(OccurDate, "yyyy-MM-dd HH:mm:ss", null)
            };
        }
    }



    public class RaisingBalanceJson
    {
        /// <summary>
        /// ����ʱ�䣨��ʽ��YYYY-MM-DD HH:mm:ss��
        /// </summary>
        [JsonPropertyName("occurTime")]
        public required string OccurTime { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        [JsonPropertyName("fundCode")]
        public required string FundCode { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        [JsonPropertyName("fundName")]
        public required string FundName { get; set; }

        /// <summary>
        /// �˻�����
        /// 02 - ļ����
        /// </summary>
        [JsonPropertyName("accoType")]
        public required string AccountType { get; set; }

        /// <summary>
        /// �����˺�
        /// </summary>
        [JsonPropertyName("bankAcco")]
        public required string BankAccountNo { get; set; }

        /// <summary>
        /// �����˻�����
        /// </summary>
        [JsonPropertyName("accName")]
        public required string BankAccountName { get; set; }

        /// <summary>
        /// ���п���������
        /// </summary>
        [JsonPropertyName("openBankName")]
        public required string OpenBankName { get; set; }

        /// <summary>
        /// ���б�ţ������д�����ǰ��λΪ��׼��
        /// </summary>
        [JsonPropertyName("bankNo")]
        public required string BankNumber { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        [JsonPropertyName("curType")]
        public string Currency { get; set; }

        /// <summary>
        /// �˻����
        /// </summary>
        [JsonPropertyName("acctBal")]
        public decimal AccountBalance { get; set; }

        /// <summary>
        /// ���з��ش���
        /// </summary>
        [JsonPropertyName("bankRetCode")]
        public required string BankReturnCode { get; set; }

        /// <summary>
        /// ����ժҪ
        /// </summary>
        [JsonPropertyName("bankNote")]
        public required string BankSummary { get; set; }

        /// <summary>
        /// CNAPS ���֧����
        /// </summary>
        [JsonPropertyName("cnapsCode")]
        public required string CNAPSCode { get; set; }


        public BankBalance ToObject()
        {
            return new BankBalance
            {
                AccountName = BankAccountName,
                AccountNo = BankNumber,
                Name = OpenBankName,
                Balance = AccountBalance,
                Currency = Currency,
                Time = DateTime.ParseExact(OccurTime, "yyyy-MM-dd HH:mm:ss", null),
            };
        }
    }




    public class TransferRequestJson
    {
        /// <summary>
        /// ������
        /// </summary>
        [JsonPropertyName("requestNo")]
        public required string RequestNo { get; set; }

        /// <summary>
        /// �������ڣ���ʽ��YYYYMMDD��
        /// </summary>
        [JsonPropertyName("requestDate")]
        public required string RequestDate { get; set; }

        /// <summary>
        /// ����ʱ�䣨��ʽ��HHMMSS��
        /// </summary>
        [JsonPropertyName("requestTime")]
        public required string RequestTime { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        [JsonPropertyName("fundCode")]
        public required string FundCode { get; set; }

        /// <summary>
        /// ҵ������
        /// ���� BusinFlagEnum ö�ٶ���
        /// </summary>
        [JsonPropertyName("businFlag")]
        public required string BusinFlag { get; set; }

        /// <summary>
        /// �����˺�
        /// </summary>
        [JsonPropertyName("tradeAcco")]
        public required string TradeAccount { get; set; }

        /// <summary>
        /// �����ʺ�
        /// </summary>
        [JsonPropertyName("fundAcco")]
        public required string FundAccount { get; set; }

        /// <summary>
        /// �����̴���
        /// </summary>
        [JsonPropertyName("agencyNo")]
        public required string AgencyCode { get; set; }

        /// <summary>
        /// ����������
        /// </summary>
        [JsonPropertyName("agencyName")]
        public required string AgencyName { get; set; }

        /// <summary>
        /// ������
        /// </summary>
        [JsonPropertyName("balance")]
        public decimal ApplyAmount { get; set; }

        /// <summary>
        /// ����ݶ�
        /// </summary>
        [JsonPropertyName("shares")]
        public decimal ApplyShares { get; set; }

        /// <summary>
        /// �����˺�
        /// </summary>
        [JsonPropertyName("bankAcco")]
        public required string BankAccountNo { get; set; }

        /// <summary>
        /// ���п�����
        /// </summary>
        [JsonPropertyName("bankName")]
        public required string BankName { get; set; }

        /// <summary>
        /// ���л���
        /// </summary>
        [JsonPropertyName("nameInBank")]
        public required string BankAccountName { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        [JsonPropertyName("netNo")]
        public required string BranchCode { get; set; }

        /// <summary>
        /// ָ������
        /// </summary>
        [JsonPropertyName("definedFee")]
        public decimal DefinedFee { get; set; }

        /// <summary>
        /// �ۿ�
        /// </summary>
        [JsonPropertyName("agio")]
        public required string DiscountRate { get; set; }

        /// <summary>
        /// �޶���ط�ʽ��
        /// 0 - ȡ��
        /// 1 - ˳��
        /// null - Ĭ��˳��
        /// </summary>
        [JsonPropertyName("exceedFlag")]
        public string? ExceedFlag { get; set; }

        /// <summary>
        /// ���֣�����ң�Ĭ��ֵ��
        /// </summary>
        [JsonPropertyName("moneyTypeCn")]
        public required string Currency { get; set; } = "�����";

        /// <summary>
        /// Ͷ��������
        /// </summary>
        [JsonPropertyName("custName")]
        public required string CustomerName { get; set; }

        /// <summary>
        /// �ͻ����ͣ��ο���¼4��
        /// </summary>
        [JsonPropertyName("custType")]
        public required string CustomerType { get; set; }

        /// <summary>
        /// ֤�����ͣ��ο���¼4��
        /// </summary>
        [JsonPropertyName("certiType")]
        public required string CertificateType { get; set; }

        /// <summary>
        /// ֤����
        /// </summary>
        [JsonPropertyName("certiNo")]
        public required string CertificateNumber { get; set; }


        public TransferRequest ToObject()
        {
            return new TransferRequest
            {
                CustomerIdentity = CertificateNumber,
                CustomerName = CustomerName,
                FundCode = FundCode,
                FundName = "unset",
                RequestDate = DateOnly.ParseExact(RequestDate, "yyyyMMdd"),
                RequestType = ParseRequestType(BusinFlag),
                ExternalId = RequestNo,
                Agency = AgencyName,
                FeeDiscount = decimal.Parse(DiscountRate),
                LargeRedemptionFlag = ExceedFlag switch { "0" => LargeRedemptionFlag.CancelRemaining, _ => LargeRedemptionFlag.RollOver },
                Source = "api",
                RequestAmount = ApplyAmount,
                RequestShare = ApplyShares,
                Fee = DefinedFee,
                CreateDate = DateOnly.FromDateTime(DateTime.Now),
            };
        }

    }

    /// <summary>
    /// 01-�Ϲ�	50-�������
    ///02-�깺	51-������ֹ
    ///03-���	52-��������
    ///04-ת�й�	54-����ʧ��
    ///05-�й�ת��	70-ǿ�Ƶ���
    ///06-�й�ת��	71-ǿ�Ƶ���
    ///07-�޸ķֺ췽ʽ	74-��������
    ///10-�ݶ��	77-�����깺�޸�
    ///11-�ݶ�ⶳ	78-��������޸�
    ///12-�ǽ��׹���	81-���𿪻�
    ///13-����ת��(��)  82-��������
    ///14-�ǽ��׹�����	83-�˻��޸�
    ///15-�ǽ��׹�����	84-�˻�����
    ///16-����ת����	85-�˻��ⶳ
    ///17-����	88-�˻��Ǽ�
    ///20-�ڲ�ת�й�	89-ȡ���Ǽ�
    ///21-�ڲ�ת�й���	90-�����깺Э��
    ///22-�ڲ�ת�йܳ�	91-�������Э��
    ///31-���������˺�	92-��ŵ�Ż�Э��
    ///32-��������˺�	93-�����깺ȡ��
    ///41-����	94-�������ȡ��
    ///42-����	96-�������˺�
    ///46-��������ѯ	97-�ڲ�ת�й�
    ///47-��������ͨ	98-�ڲ��й�ת��
    ///48-������ȡ��	99-�ڲ��й�ת��
    ///49-����������
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static RequestType ParseRequestType(string str)
    {
        switch (str)
        {
            case "01": return RequestType.Subscription;
            case "02": return RequestType.Purchase;
            case "03": return RequestType.Redemption;
            case "04": return RequestType.Subscription;
            case "05":
            case "70":
            case "15":
            case "16":
            case "21":
            case "41":
                return RequestType.Increase;
            case "06":
            case "71":
            case "14":
            case "13":
            case "22":
            case "42":
                return RequestType.Decrease;
            default: // �������ͺ���
                return RequestType.UNK;
        }
    }


    private static TARecordType ParseRecordType(string businessTypeCode)
    {
        return businessTypeCode switch
        {
            "01" => TARecordType.Subscription,         // �Ϲ�
            "02" => TARecordType.Purchase,             // �깺
            "03" => TARecordType.Redemption,           // ���
            "04" => TARecordType.MoveOut,              // ת�йܣ�ת����
            "05" => TARecordType.MoveIn,               // �й�ת�루ת�룩
            "06" => TARecordType.MoveOut,              // �й�ת����ת����
            "07" => TARecordType.BonusType,            // �޸ķֺ췽ʽ
            "10" => TARecordType.Frozen,               // �ݶ��
            "11" => TARecordType.Thawed,               // �ݶ�ⶳ
            "12" => TARecordType.TransferIn,           // �ǽ��׹�����ת�룩
            "13" => TARecordType.SwitchOut,            // ����ת��(��)
            "14" => TARecordType.TransferOut,          // �ǽ��׹�������ת����
            "15" => TARecordType.TransferIn,           // �ǽ��׹����루ת�룩
            "16" => TARecordType.SwitchIn,             // ����ת����
            "17" => TARecordType.UNK,                  // �������޶�ӦTA��¼���ͣ�
            "20" => TARecordType.TransferOut,          // �ڲ�ת�йܣ�ת����
            "21" => TARecordType.TransferIn,           // �ڲ�ת�й��루ת�룩
            "22" => TARecordType.TransferOut,          // �ڲ�ת�йܳ���ת����
            "31" => TARecordType.UNK,                  // ���������˺ţ�����ȷ��Ӧ��
            "32" => TARecordType.UNK,                  // ��������˺ţ�����ȷ��Ӧ��
            "41" => TARecordType.Purchase,             // ���루��ͬ���깺��
            "42" => TARecordType.Redemption,           // ��������ͬ����أ�
            "46" => TARecordType.UNK,                  // ��������ѯ���޶�Ӧ��
            "47" => TARecordType.UNK,                  // ��������ͨ���޶�Ӧ��
            "48" => TARecordType.UNK,                  // ������ȡ�����޶�Ӧ��
            "49" => TARecordType.UNK,                  // ����������޶�Ӧ��
            "50" => TARecordType.UNK,                  // ���������ͨ�����ǵ��ʼ�¼��
            "51" => TARecordType.UNK,                  // ������ֹ��ͨ�����ǵ��ʼ�¼��
            "52" => TARecordType.Clear,                // ��������
            "54" => TARecordType.RaisingFailed,        // ����ʧ��
            "70" => TARecordType.Increase,             // ǿ�Ƶ���
            "71" => TARecordType.Decrease,             // ǿ�Ƶ���
            "74" => TARecordType.Distribution,         // ��������
            "77" => TARecordType.UNK,                  // �����깺�޸ģ�����ȷ��Ӧ��
            "78" => TARecordType.UNK,                  // ��������޸ģ�����ȷ��Ӧ��
            "81" => TARecordType.UNK,                  // ���𿪻����ǽ��׼�¼��
            "82" => TARecordType.UNK,                  // �����������ǽ��׼�¼��
            "83" => TARecordType.UNK,                  // �˻��޸ģ��ǽ��׼�¼��
            "84" => TARecordType.Frozen,               // �˻����ᣨ��ݶ�����ƣ�
            "85" => TARecordType.Thawed,               // �˻��ⶳ����ݶ�ⶳ���ƣ�
            "88" => TARecordType.UNK,                  // �˻��Ǽǣ��ǽ��׼�¼��
            "89" => TARecordType.UNK,                  // ȡ���Ǽǣ��ǽ��׼�¼��
            "90" => TARecordType.UNK,                  // �����깺Э�飨�ǽ��׼�¼��
            "91" => TARecordType.UNK,                  // �������Э�飨�ǽ��׼�¼��
            "92" => TARecordType.UNK,                  // ��ŵ�Ż�Э�飨�ǽ��׼�¼��
            "93" => TARecordType.UNK,                  // �����깺ȡ�����ǽ��׼�¼��
            "94" => TARecordType.UNK,                  // �������ȡ�����ǽ��׼�¼��
            "96" => TARecordType.UNK,                  // �������˺ţ��ǽ��׼�¼��
            "97" => TARecordType.TransferOut,          // �ڲ�ת�йܣ�ת����
            "98" => TARecordType.TransferIn,           // �ڲ��й�ת�루ת�룩
            "99" => TARecordType.TransferOut,          // �ڲ��й�ת����ת����

            _ => TARecordType.UNK
        };
    }



    public class InvestorAccountMapping
    {
        //FundAccount
        public required string Id { get; set; }

        public required string Name { get; set; }

        public required string Indentity { get; set; }
    }


    public class DistrubutionJson
    {
        /// <summary>
        /// �����˺�
        /// </summary>
        [JsonPropertyName("fundAcco")]
        public required string FundAccount { get; set; }

        /// <summary>
        /// �ͻ�����
        /// </summary>
        [JsonPropertyName("custName")]
        public required string CustomerName { get; set; }

        /// <summary>
        /// �����̴��루ZX6 ��ʾֱ����
        /// </summary>
        [JsonPropertyName("agencyNo")]
        public required string AgencyCode { get; set; }

        /// <summary>
        /// ����������
        /// </summary>
        [JsonPropertyName("agencyName")]
        public required string AgencyName { get; set; }

        /// <summary>
        /// �����˺�
        /// </summary>
        [JsonPropertyName("tradeAcco")]
        public required string TradeAccount { get; set; }

        /// <summary>
        /// ȷ�����ڣ���ʽ��YYYYMMDD��
        /// </summary>
        [JsonPropertyName("confirmDate")]
        public required string ConfirmDate { get; set; }

        /// <summary>
        /// TAȷ�Ϻ�
        /// </summary>
        [JsonPropertyName("cserialNo")]
        public required string TaConfirmNo { get; set; }

        /// <summary>
        /// �ֺ�Ǽ�����
        /// </summary>
        [JsonPropertyName("regDate")]
        public required string RegisterDate { get; set; }

        /// <summary>
        /// ������������
        /// </summary>
        [JsonPropertyName("date")]
        public required string DividendDate { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        [JsonPropertyName("fundName")]
        public required string FundName { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        [JsonPropertyName("fundCode")]
        public required string FundCode { get; set; }

        /// <summary>
        /// �ֺ�����ݶ�
        /// </summary>
        [JsonPropertyName("totalShare")]
        public required string TotalShares { get; set; }

        /// <summary>
        /// ÿ��λ�ֺ���
        /// </summary>
        [JsonPropertyName("unitProfit")]
        public required string UnitProfit { get; set; }

        /// <summary>
        /// �����ܶ�
        /// </summary>
        [JsonPropertyName("totalProfit")]
        public required string TotalProfit { get; set; }

        /// <summary>
        /// �ֺ췽ʽ��
        /// 0 - ������Ͷ��
        /// 1 - �ֽ����
        /// </summary>
        [JsonPropertyName("flag")]
        public required string DividendType { get; set; }

        /// <summary>
        /// ʵ���ֽ����
        /// </summary>
        [JsonPropertyName("realBalance")]
        public required string RealCashDividend { get; set; }

        /// <summary>
        /// ��Ͷ�ʺ������
        /// </summary>
        [JsonPropertyName("reinvestBalance")]
        public required string ReinvestAmount { get; set; }

        /// <summary>
        /// ��Ͷ�ʷݶ�
        /// </summary>
        [JsonPropertyName("realShares")]
        public required string ReinvestShares { get; set; }

        /// <summary>
        /// ��Ͷ������
        /// </summary>
        [JsonPropertyName("lastDate")]
        public required string ReinvestDate { get; set; }

        /// <summary>
        /// ��Ͷ�ʵ�λ��ֵ
        /// </summary>
        [JsonPropertyName("netValue")]
        public required string NetValue { get; set; }

        /// <summary>
        /// ʵ��ҵ����ɽ��
        /// </summary>
        [JsonPropertyName("deductBalance")]
        public required string PerformanceFee { get; set; }

        /// <summary>
        /// �ͻ����ͣ��μ���¼4��
        /// </summary>
        [JsonPropertyName("custType")]
        public required string CustomerType { get; set; }

        /// <summary>
        /// ֤�����ͣ��μ���¼4��
        /// </summary>
        [JsonPropertyName("certiType")]
        public required string CertificateType { get; set; }

        /// <summary>
        /// ֤������
        /// </summary>
        [JsonPropertyName("certiNo")]
        public required string CertificateNumber { get; set; }

        /// <summary>
        /// ��Ȩ��Ϣ��
        /// </summary>
        [JsonPropertyName("exDividendDate")]
        public required string ExDividendDate { get; set; }


        public TransferRecord ToObject()
        {
            return new TransferRecord
            {
                CustomerIdentity = CertificateNumber,
                CustomerName = CustomerName,
                ExternalId = TaConfirmNo,
                ConfirmedDate = DateOnly.ParseExact(ConfirmDate, "yyyyMMdd"),
                ConfirmedShare = decimal.Parse(ReinvestShares),
                ConfirmedAmount = decimal.Parse(RealCashDividend),
                ConfirmedNetAmount = decimal.Parse(RealCashDividend),
                RequestDate = DateOnly.ParseExact(ExDividendDate, "yyyyMMdd"),
                RequestAmount = 0,
                RequestShare = decimal.Parse(TotalShares),
                Type = TARecordType.Distribution,
                Agency = AgencyName,
                CreateDate = DateOnly.FromDateTime(DateTime.Now),
                PerformanceFee = decimal.Parse(PerformanceFee),
                FundCode = FundCode,
                FundName = FundName,
                Source = "api"
            };
        }
    }



    public class  PerformanceJson
    {
        [JsonPropertyName("fundAcco")]
        public required string FundAccount { get; set; } // �����˺�

        [JsonPropertyName("custName")]
        public required string CustomerName { get; set; } // �ͻ�����

        [JsonPropertyName("custType")]
        public required string CustomerType { get; set; } // �ͻ�����

        [JsonPropertyName("agencyNo")]
        public required string AgencyCode { get; set; } // �����̴���

        [JsonPropertyName("agencyName")]
        public required string AgencyName { get; set; } // ����������

        [JsonPropertyName("tradeAcco")]
        public required string TradeAccount { get; set; } // �����˺�

        [JsonPropertyName("businFlag")]
        public required string BusinessType { get; set; } // ҵ�����ͣ��ο�ӳ���

        [JsonPropertyName("sortFlag")]
        public required string SortFlag { get; set; } // ��������: 0-���״��� 1-ҵ�����

        [JsonPropertyName("requestDate")]
        public required string RequestDate { get; set; } // ��������

        [JsonPropertyName("confirmDate")]
        public required string ConfirmDate { get; set; } // ȷ������

        [JsonPropertyName("fundCode")]
        public required string FundCode { get; set; } // �������

        [JsonPropertyName("shareTypeCn")]
        public required string ShareTypeName { get; set; } // �ݶ����

        [JsonPropertyName("cserialNo")]
        public required string TaConfirmNo { get; set; } // TAȷ�Ϻ�

        [JsonPropertyName("registDate")]
        public required string RegisterDate { get; set; } // �ݶ�ע������

        [JsonPropertyName("shares")]
        public required string Shares { get; set; } // �����ݶ�

        [JsonPropertyName("beginDate")]
        public required string BeginDate { get; set; } // �ڳ�����

        [JsonPropertyName("oriNav")]
        public required string OriNav { get; set; } // �ڳ���λ��ֵ

        [JsonPropertyName("oriTotalNav")]
        public required string OriTotalNav { get; set; } // �ڳ��ۼƾ�ֵ

        [JsonPropertyName("nav")]
        public required string Nav { get; set; } // ��ĩ��λ��ֵ

        [JsonPropertyName("totalNav")]
        public required string TotalNav { get; set; } // ��ĩ�ۼƾ�ֵ

        [JsonPropertyName("currRatio")]
        public required string CurrentRatio { get; set; } // ��ǰ������

        [JsonPropertyName("yearRatio")]
        public required string YearRatio { get; set; } // �껯������

        [JsonPropertyName("oriBalance")]
        public required string OriBalance { get; set; } // Ӧ���/���׽��

        [JsonPropertyName("factBalance")]
        public required string FactBalance { get; set; } // ʵ�����/���׽��

        [JsonPropertyName("factShares")]
        public required string FactShares { get; set; } // ʵ�����/���׷ݶ�

        [JsonPropertyName("bonusBalance")]
        public required string BonusBalance { get; set; } // �ֺ��ܽ��

        [JsonPropertyName("oriCserialNo")]
        public required string OriginalTaConfirmNo { get; set; } // ԭȷ�ϵ���

        [JsonPropertyName("hold")]
        public required string HoldDays { get; set; } // ��������

        [JsonPropertyName("indexYearRatio")]
        public required string IndexYearRatio { get; set; } // ֤ȯָ���껯������

        [JsonPropertyName("beginIndexPrice")]
        public required string BeginIndexPrice { get; set; } // �ڳ�ָ���۸�

        [JsonPropertyName("endIndexPrice")]
        public required string EndIndexPrice { get; set; } // ��ĩָ���۸�

        [JsonPropertyName("calcFlag")]
        public required string CalcFlag { get; set; } // �����ʶ��0-���ᣬ1-����
    }
}
