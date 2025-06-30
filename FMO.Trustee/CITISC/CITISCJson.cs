using FMO.Models;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace FMO.Trustee;

#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��
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
        public string CustName { get; set; } // Ͷ��������

        [JsonPropertyName("fundAcco")]
        public string FundAcco { get; set; } // �����˺�

        [JsonPropertyName("tradeAcco")]
        public string TradeAcco { get; set; } // �����˺�

        [JsonPropertyName("custType")]
        public string CustType { get; set; } // �ͻ����ͣ��μ���¼4��

        [JsonPropertyName("certiType")]
        public string CertiType { get; set; } // ֤�����ͣ��μ���¼4��

        [JsonPropertyName("certiNo")]
        public string CertiNo { get; set; } // ֤����

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


    /// <summary>
    /// ���ڲ�Ʒ��������ģ��
    /// </summary>
    public class FundDailyFeeJson
    {
        /// <summary>
        /// ��Ʒ����
        /// </summary>
        [JsonPropertyName("fcpdm")]
        public string ProductCode { get; set; }

        /// <summary>
        /// �ּ���Ʒ����
        /// </summary>
        [JsonPropertyName("ffjdm")]
        public string? ClassificationCode { get; set; }

        /// <summary>
        /// ��Ʒ����
        /// </summary>
        [JsonPropertyName("fcpmc")]
        public string ProductName { get; set; }

        /// <summary>
        /// ҵ�����ڣ���ʽ��YYYY - MM - DD
        /// </summary>
        [JsonPropertyName("cdate")]
        public string BusinessDate { get; set; }

        #region ��������
        /// <summary>
        /// ��������
        /// </summary>
        [JsonPropertyName("jtglf")]
        public string AccruedManagementFee { get; set; }

        /// <summary>
        /// ֧�������
        /// </summary>
        [JsonPropertyName("zfglf")]
        public string PaidManagementFee { get; set; }

        /// <summary>
        /// δ֧�������
        /// </summary>
        [JsonPropertyName("wzfglf")]
        public string UnpaidManagementFee { get; set; }
        #endregion

        #region ҵ���������
        /// <summary>
        /// ����ҵ������
        /// </summary>
        [JsonPropertyName("jtyjbc")]
        public string AccruedPerformanceFee { get; set; }

        /// <summary>
        /// ֧��ҵ������
        /// </summary>
        [JsonPropertyName("zfyjbc")]
        public string PaidPerformanceFee { get; set; }

        /// <summary>
        /// δ֧��ҵ������
        /// </summary>
        [JsonPropertyName("wzfyjbc")]
        public string UnpaidPerformanceFee { get; set; }
        #endregion

        #region �йܷ����
        /// <summary>
        /// �����йܷ�
        /// </summary>
        [JsonPropertyName("jttgf")]
        public string AccruedCustodyFee { get; set; }

        /// <summary>
        /// ֧���йܷ�
        /// </summary>
        [JsonPropertyName("zftgf")]
        public string PaidCustodyFee { get; set; }

        /// <summary>
        /// δ֧���йܷ�
        /// </summary>
        [JsonPropertyName("wzftgf")]
        public string UnpaidCustodyFee { get; set; }
        #endregion

        #region ������������
        /// <summary>
        /// �������������
        /// </summary>
        [JsonPropertyName("jtxzfwf")]
        public string AccruedAdministrativeFee { get; set; }

        /// <summary>
        /// ֧�����������
        /// </summary>
        [JsonPropertyName("zfxzfwf")]
        public string PaidAdministrativeFee { get; set; }

        /// <summary>
        /// δ֧�����������
        /// </summary>
        [JsonPropertyName("wzfxzfwf")]
        public string UnpaidAdministrativeFee { get; set; }
        #endregion

        #region ���۷�������
        /// <summary>
        /// �������۷����
        /// </summary>
        [JsonPropertyName("jtxsfwf")]
        public string AccruedSalesServiceFee { get; set; }

        /// <summary>
        /// ֧�����۷����
        /// </summary>
        [JsonPropertyName("zfxsfwf")]
        public string PaidSalesServiceFee { get; set; }

        /// <summary>
        /// δ֧�����۷����
        /// </summary>
        [JsonPropertyName("wzfxsfwf")]
        public string UnpaidSalesServiceFee { get; set; }
        #endregion

        #region Ͷ�ʹ��ʷ����
        /// <summary>
        /// ����Ͷ�ʹ��ʷ�
        /// </summary>
        [JsonPropertyName("jttzgwf")]
        public string AccruedInvestmentAdvisoryFee { get; set; }

        /// <summary>
        /// ֧��Ͷ�ʹ��ʷ�
        /// </summary>
        [JsonPropertyName("zftzgwf")]
        public string PaidInvestmentAdvisoryFee { get; set; }

        /// <summary>
        /// δ֧��Ͷ�ʹ��ʷ�
        /// </summary>
        [JsonPropertyName("wzftzgwf")]
        public string UnpaidInvestmentAdvisoryFee { get; set; }
        #endregion

        #region ��ֵ˰����˰���
        /// <summary>
        /// ������ֵ˰����˰
        /// </summary>
        [JsonPropertyName("jtzzsfjs")]
        public string AccruedVatSurcharge { get; set; }

        /// <summary>
        /// ֧����ֵ˰����˰
        /// </summary>
        [JsonPropertyName("zfzzsfjs")]
        public string PaidVatSurcharge { get; set; }

        /// <summary>
        /// δ֧����ֵ˰����˰
        /// </summary>
        [JsonPropertyName("wzfzzsfjs")]
        public string UnpaidVatSurcharge { get; set; }
        #endregion

        #region ��Ʒ����
        /// <summary>
        /// ������Ʒ�
        /// </summary>
        [JsonPropertyName("jtsjf")]
        public string AccruedAuditFee { get; set; }

        /// <summary>
        /// ֧����Ʒ�
        /// </summary>
        [JsonPropertyName("zfsjf")]
        public string PaidAuditFee { get; set; }

        /// <summary>
        /// δ֧����Ʒ�
        /// </summary>
        [JsonPropertyName("wzfsjf")]
        public string UnpaidAuditFee { get; set; }
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
        public string OccurTime { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        [JsonPropertyName("fundCode")]
        public string FundCode { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        [JsonPropertyName("fundName")]
        public string FundName { get; set; }

        /// <summary>
        /// �˻����ͣ����� "02-ļ����"
        /// </summary>
        [JsonPropertyName("accoType")]
        public string AccoType { get; set; }

        /// <summary>
        /// �����˺�
        /// </summary>
        [JsonPropertyName("bankAcco")]
        public string BankAcco { get; set; }

        /// <summary>
        /// �����˻�����
        /// </summary>
        [JsonPropertyName("accName")]
        public string AccName { get; set; }

        /// <summary>
        /// ��������������
        /// </summary>
        [JsonPropertyName("openBankName")]
        public string OpenBankName { get; set; }

        /// <summary>
        /// �������б�ţ������д�����ǰ��λΪ��׼������¼2��
        /// </summary>
        [JsonPropertyName("bankNo")]
        public string BankNo { get; set; }

        /// <summary>
        /// �Է��˺�
        /// </summary>
        [JsonPropertyName("othBankAcco")]
        public string OthBankAcco { get; set; }

        /// <summary>
        /// �Է��˻�����
        /// </summary>
        [JsonPropertyName("othAccName")]
        public string OthAccName { get; set; }

        /// <summary>
        /// �Է�����������
        /// </summary>
        [JsonPropertyName("othOpenBankName")]
        public string OthOpenBankName { get; set; }

        /// <summary>
        /// �Է����б�ţ������д�����ǰ��λΪ��׼������¼2��
        /// </summary>
        [JsonPropertyName("othBankNo")]
        public string OthBankNo { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        [JsonPropertyName("curType")]
        public string CurType { get; set; }

        /// <summary>
        /// �ո�����1��ʾ�տ2��ʾ����
        /// </summary>
        [JsonPropertyName("directFlag")]
        public string DirectFlag { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        [JsonPropertyName("occurAmt")]
        public string OccurAmt { get; set; }

        /// <summary>
        /// �˻����
        /// </summary>
        [JsonPropertyName("acctBal")]
        public string AcctBal { get; set; }

        /// <summary>
        /// ������ˮ��
        /// </summary>
        [JsonPropertyName("bankJour")]
        public string BankJour { get; set; }

        /// <summary>
        /// ���з��ش���
        /// </summary>
        [JsonPropertyName("bankRetCode")]
        public string BankRetCode { get; set; }

        /// <summary>
        /// ����ժҪ
        /// </summary>
        [JsonPropertyName("bankNote")]
        public string BankNote { get; set; }



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
                Amount = ParseDecimal(OccurAmt),
                Balance = ParseDecimal(AcctBal),
                Direction = DirectFlag == "����" ? TransctionDirection.Pay : TransctionDirection.Receive,
                Remark = BankNote,
                Serial = BankJour,
            };
        }
    }




    public class CustodialAccountJson
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


    public class VirtualNetValueJson
    {
        /// <summary>
        /// ҵ�����ڣ���ʽ��YYYY-MM-DD
        /// </summary>
        [JsonPropertyName("date")]
        public string BusinessDate { get; set; }

        /// <summary>
        /// �ͻ�����
        /// </summary>
        [JsonPropertyName("custName")]
        public string CustomerName { get; set; }

        /// <summary>
        /// �����˺�
        /// </summary>
        [JsonPropertyName("fundAcco")]
        public string FundAccount { get; set; }

        /// <summary>
        /// ��Ʒ����
        /// </summary>
        [JsonPropertyName("fundCode")]
        public string FundCode { get; set; }

        /// <summary>
        /// ��Ʒ����
        /// </summary>
        [JsonPropertyName("fundName")]
        public string FundName { get; set; }

        /// <summary>
        /// �ֲַݶ�
        /// </summary>
        [JsonPropertyName("shares")]
        public decimal Shares { get; set; }

        /// <summary>
        /// ���᷽ʽ��1-��ֵ���᣻2-TA����
        /// </summary>
        [JsonPropertyName("flag")]
        public string AccrualMethod { get; set; }

        /// <summary>
        /// ����ҵ��������
        /// </summary>
        [JsonPropertyName("virtualBalance")]
        public decimal VirtualPerformanceFee { get; set; }

        /// <summary>
        /// ���⾻ֵ
        /// </summary>
        [JsonPropertyName("virtualAssetVal")]
        public string VirtualNetAssetValue { get; set; }

        /// <summary>
        /// ʵ�ʾ�ֵ
        /// </summary>
        [JsonPropertyName("netAssetVal")]
        public string ActualNetAssetValue { get; set; }

        /// <summary>
        /// �ۼƾ�ֵ
        /// </summary>
        [JsonPropertyName("totalAssetVal")]
        public string TotalNetAssetValue { get; set; }

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
        public string NetValueCheckStatus { get; set; }
    }


    public class BankBalanceJson
    {
        /// <summary>
        /// �˺�
        /// </summary>
        [JsonPropertyName("YHZH")]
        public string AccountNumber { get; set; }

        /// <summary>
        /// �˻����
        /// </summary>
        [JsonPropertyName("ZHYE")]
        public string AccountBalance { get; set; }

        /// <summary>
        /// �˻�����
        /// </summary>
        [JsonPropertyName("KHHM")]
        public string AccountHolderName { get; set; }

        /// <summary>
        /// �������ࣺ
        /// HKD-�۱ң�
        /// RMB-����ң�
        /// USD-��Ԫ
        /// </summary>
        [JsonPropertyName("JSBZ")]
        public string CurrencyType { get; set; }

        /// <summary>
        /// ����ѯʱ�䣬��ʽ��YYYY-MM-DD HH:MM:SS
        /// </summary>
        [JsonPropertyName("CXSJ")]
        public string QueryTime { get; set; }

        /// <summary>
        /// �˻��������
        /// </summary>
        [JsonPropertyName("ZHKYYE")]
        public string AvailableBalance { get; set; }

        /// <summary>
        /// ��������
        /// 0-�ɹ���
        /// -2-�����У�
        /// ����ֵ����ʧ��
        /// </summary>
        [JsonPropertyName("CLJG")]
        public string ProcessingResult { get; set; }

        /// <summary>
        /// ����˵��
        /// </summary>
        [JsonPropertyName("CLSM")]
        public string ProcessingDescription { get; set; }

        /// <summary>
        /// ������
        /// </summary>
        [JsonPropertyName("KHYH")]
        public string OpeningBank { get; set; }

        public BankBalance ToObject()
        {
            return new BankBalance
            {
                AccountName = AccountHolderName,
                AccountNo = AccountNumber,
                Name = OpeningBank,
                Balance = ParseDecimal(AccountBalance),
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
        public string BankCode { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        [JsonPropertyName("YHMC")]
        public string BankName { get; set; }

        /// <summary>
        /// �����˺ţ�CLJGΪ-1ʱ���ѯ�ʺţ�
        /// </summary>
        [JsonPropertyName("FKFZH")]
        public string PayerAccountNo { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        [JsonPropertyName("FKFHM")]
        public string PayerAccountName { get; set; }

        /// <summary>
        /// �շ��˺ţ�CLJGΪ-1ʱ���ѯ�ʺţ�
        /// </summary>
        [JsonPropertyName("SKFZH")]
        public string PayeeAccountNo { get; set; }

        /// <summary>
        /// �շ�����
        /// </summary>
        [JsonPropertyName("SKFHM")]
        public string PayeeAccountName { get; set; }

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
        public string Currency { get; set; }

        /// <summary>
        /// �����־��
        /// �衢��������=�衢�ۿ�=�衢�տ�=�����ۿ�����տ��
        /// </summary>
        [JsonPropertyName("JDBZ")]
        public string DebitCreditFlag { get; set; }

        /// <summary>
        /// ժҪ����
        /// </summary>
        [JsonPropertyName("ZYMC")]
        public string Summary { get; set; }

        /// <summary>
        /// ����ʱ�䣬��ʽ yyyy-MM-dd HH:mm:ss
        /// </summary>
        [JsonPropertyName("FSSJ")]
        public string OccurTime { get; set; }

        /// <summary>
        /// ������Ϣ
        /// </summary>
        [JsonPropertyName("FHXX")]
        public string ReturnInfo { get; set; }

        /// <summary>
        /// �����ֶ�
        /// </summary>
        [JsonPropertyName("BYZD")]
        public string ReservedField { get; set; }

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
        public string SerialNumber { get; set; }

        /// <summary>
        /// ������
        /// 0 - �ɹ�
        /// -2 - ������
        /// ����ֵ����ʧ��
        /// </summary>
        [JsonPropertyName("CLJG")]
        public string ProcessResult { get; set; }

        /// <summary>
        /// ����˵��
        /// </summary>
        [JsonPropertyName("CLSM")]
        public string ProcessDescription { get; set; }


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
        public string PayerAccountCode { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        [JsonPropertyName("fpayerName")]
        public string PayerName { get; set; }

        /// <summary>
        /// ����������
        /// </summary>
        [JsonPropertyName("fpayerBank")]
        public string PayerBank { get; set; }

        /// <summary>
        /// �տ�˺�
        /// </summary>
        [JsonPropertyName("fpayeeAcctCode")]
        public string PayeeAccountCode { get; set; }

        /// <summary>
        /// �տ����
        /// </summary>
        [JsonPropertyName("fpayeeName")]
        public string PayeeName { get; set; }

        /// <summary>
        /// �տ������
        /// </summary>
        [JsonPropertyName("fpayeeBank")]
        public string PayeeBank { get; set; }

        /// <summary>
        /// �������ڣ���ʽ��yyyy-MM-dd��
        /// </summary>
        [JsonPropertyName("date")]
        public string OccurDate { get; set; }

        /// <summary>
        /// �տ����룺
        /// SFKFX001: ����
        /// SFKFX002: ���
        /// SFKFX003: ����
        /// </summary>
        [JsonPropertyName("fway")]
        public string DirectionCode { get; set; }

        /// <summary>
        /// �տ�����ƣ�SFKFX001�����SFKFX002����SFKFX003��������
        /// </summary>
        [JsonPropertyName("fwayName")]
        public string DirectionName { get; set; }

        /// <summary>
        /// �������ַ������ͣ�������λС����
        /// </summary>
        [JsonPropertyName("famount")]
        public decimal Amount { get; set; }

        /// <summary>
        /// ��ע
        /// </summary>
        [JsonPropertyName("fsummary")]
        public string Summary { get; set; }

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
        public string OccurTime { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        [JsonPropertyName("fundCode")]
        public string FundCode { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        [JsonPropertyName("fundName")]
        public string FundName { get; set; }

        /// <summary>
        /// �˻�����
        /// 02 - ļ����
        /// </summary>
        [JsonPropertyName("accoType")]
        public string AccountType { get; set; }

        /// <summary>
        /// �����˺�
        /// </summary>
        [JsonPropertyName("bankAcco")]
        public string BankAccountNo { get; set; }

        /// <summary>
        /// �����˻�����
        /// </summary>
        [JsonPropertyName("accName")]
        public string BankAccountName { get; set; }

        /// <summary>
        /// ���п���������
        /// </summary>
        [JsonPropertyName("openBankName")]
        public string OpenBankName { get; set; }

        /// <summary>
        /// ���б�ţ������д�����ǰ��λΪ��׼��
        /// </summary>
        [JsonPropertyName("bankNo")]
        public string BankNumber { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        [JsonPropertyName("curType")]
        public string Currency { get; set; }

        /// <summary>
        /// �˻����
        /// </summary>
        [JsonPropertyName("acctBal")]
        public string AccountBalance { get; set; }

        /// <summary>
        /// ���з��ش���
        /// </summary>
        [JsonPropertyName("bankRetCode")]
        public string BankReturnCode { get; set; }

        /// <summary>
        /// ����ժҪ
        /// </summary>
        [JsonPropertyName("bankNote")]
        public string BankSummary { get; set; }

        /// <summary>
        /// CNAPS ���֧����
        /// </summary>
        [JsonPropertyName("cnapsCode")]
        public string CNAPSCode { get; set; }


        public FundBankBalance ToObject()
        {
            return new FundBankBalance
            {
                FundCode = FundCode,
                FundName = FundName,
                AccountName = BankAccountName,
                AccountNo = BankNumber,
                Name = OpenBankName,
                Balance = ParseDecimal(AccountBalance),
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
        public string RequestNo { get; set; }

        /// <summary>
        /// �������ڣ���ʽ��YYYYMMDD��
        /// </summary>
        [JsonPropertyName("requestDate")]
        public string RequestDate { get; set; }

        /// <summary>
        /// ����ʱ�䣨��ʽ��HHMMSS��
        /// </summary>
        [JsonPropertyName("requestTime")]
        public string RequestTime { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        [JsonPropertyName("fundCode")]
        public string FundCode { get; set; }

        /// <summary>
        /// ҵ������
        /// ���� BusinFlagEnum ö�ٶ���
        /// </summary>
        [JsonPropertyName("businFlag")]
        public string BusinFlag { get; set; }

        /// <summary>
        /// �����˺�
        /// </summary>
        [JsonPropertyName("tradeAcco")]
        public string TradeAccount { get; set; }

        /// <summary>
        /// �����ʺ�
        /// </summary>
        [JsonPropertyName("fundAcco")]
        public string FundAccount { get; set; }

        /// <summary>
        /// �����̴���
        /// </summary>
        [JsonPropertyName("agencyNo")]
        public string AgencyCode { get; set; }

        /// <summary>
        /// ����������
        /// </summary>
        [JsonPropertyName("agencyName")]
        public string AgencyName { get; set; }

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
        public string BankAccountNo { get; set; }

        /// <summary>
        /// ���п�����
        /// </summary>
        [JsonPropertyName("bankName")]
        public string BankName { get; set; }

        /// <summary>
        /// ���л���
        /// </summary>
        [JsonPropertyName("nameInBank")]
        public string BankAccountName { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        [JsonPropertyName("netNo")]
        public string BranchCode { get; set; }

        /// <summary>
        /// ָ������
        /// </summary>
        [JsonPropertyName("definedFee")]
        public decimal DefinedFee { get; set; }

        /// <summary>
        /// �ۿ�
        /// </summary>
        [JsonPropertyName("agio")]
        public string DiscountRate { get; set; }

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
        public string Currency { get; set; } = "�����";

        /// <summary>
        /// Ͷ��������
        /// </summary>
        [JsonPropertyName("custName")]
        public string CustomerName { get; set; }

        /// <summary>
        /// �ͻ����ͣ��ο���¼4��
        /// </summary>
        [JsonPropertyName("custType")]
        public string CustomerType { get; set; }

        /// <summary>
        /// ֤�����ͣ��ο���¼4��
        /// </summary>
        [JsonPropertyName("certiType")]
        public string CertificateType { get; set; }

        /// <summary>
        /// ֤����
        /// </summary>
        [JsonPropertyName("certiNo")]
        public string CertificateNumber { get; set; }


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
                FeeDiscount = ParseDecimal(DiscountRate),
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
    private static TransferRequestType ParseRequestType(string str)
    {
        switch (str)
        {
            case "01": return TransferRequestType.Subscription;
            case "02": return TransferRequestType.Purchase;
            case "03": return TransferRequestType.Redemption;
            case "04": return TransferRequestType.Subscription;
            case "05":
            case "70":
            case "15":
            case "16":
            case "21":
            case "41":
                return TransferRequestType.Increase;
            case "06":
            case "71":
            case "14":
            case "13":
            case "22":
            case "42":
                return TransferRequestType.Decrease;
            default: // �������ͺ���
                return TransferRequestType.UNK;
        }
    }


    private static TransferRecordType ParseRecordType(string businessTypeCode)
    {
        return businessTypeCode switch
        {
            "01" => TransferRecordType.Subscription,         // �Ϲ�
            "02" => TransferRecordType.Purchase,             // �깺
            "03" => TransferRecordType.Redemption,           // ���
            "04" => TransferRecordType.MoveOut,              // ת�йܣ�ת����
            "05" => TransferRecordType.MoveIn,               // �й�ת�루ת�룩
            "06" => TransferRecordType.MoveOut,              // �й�ת����ת����
            "07" => TransferRecordType.BonusType,            // �޸ķֺ췽ʽ
            "10" => TransferRecordType.Frozen,               // �ݶ��
            "11" => TransferRecordType.Thawed,               // �ݶ�ⶳ
            "12" => TransferRecordType.TransferIn,           // �ǽ��׹�����ת�룩
            "13" => TransferRecordType.SwitchOut,            // ����ת��(��)
            "14" => TransferRecordType.TransferOut,          // �ǽ��׹�������ת����
            "15" => TransferRecordType.TransferIn,           // �ǽ��׹����루ת�룩
            "16" => TransferRecordType.SwitchIn,             // ����ת����
            "17" => TransferRecordType.UNK,                  // �������޶�ӦTA��¼���ͣ�
            "20" => TransferRecordType.TransferOut,          // �ڲ�ת�йܣ�ת����
            "21" => TransferRecordType.TransferIn,           // �ڲ�ת�й��루ת�룩
            "22" => TransferRecordType.TransferOut,          // �ڲ�ת�йܳ���ת����
            "31" => TransferRecordType.UNK,                  // ���������˺ţ�����ȷ��Ӧ��
            "32" => TransferRecordType.UNK,                  // ��������˺ţ�����ȷ��Ӧ��
            "41" => TransferRecordType.Purchase,             // ���루��ͬ���깺��
            "42" => TransferRecordType.Redemption,           // ��������ͬ����أ�
            "46" => TransferRecordType.UNK,                  // ��������ѯ���޶�Ӧ��
            "47" => TransferRecordType.UNK,                  // ��������ͨ���޶�Ӧ��
            "48" => TransferRecordType.UNK,                  // ������ȡ�����޶�Ӧ��
            "49" => TransferRecordType.UNK,                  // ����������޶�Ӧ��
            "50" => TransferRecordType.UNK,                  // ���������ͨ�����ǵ��ʼ�¼��
            "51" => TransferRecordType.UNK,                  // ������ֹ��ͨ�����ǵ��ʼ�¼��
            "52" => TransferRecordType.Clear,                // ��������
            "54" => TransferRecordType.RaisingFailed,        // ����ʧ��
            "70" => TransferRecordType.Increase,             // ǿ�Ƶ���
            "71" => TransferRecordType.Decrease,             // ǿ�Ƶ���
            "74" => TransferRecordType.Distribution,         // ��������
            "77" => TransferRecordType.UNK,                  // �����깺�޸ģ�����ȷ��Ӧ��
            "78" => TransferRecordType.UNK,                  // ��������޸ģ�����ȷ��Ӧ��
            "81" => TransferRecordType.UNK,                  // ���𿪻����ǽ��׼�¼��
            "82" => TransferRecordType.UNK,                  // �����������ǽ��׼�¼��
            "83" => TransferRecordType.UNK,                  // �˻��޸ģ��ǽ��׼�¼��
            "84" => TransferRecordType.Frozen,               // �˻����ᣨ��ݶ�����ƣ�
            "85" => TransferRecordType.Thawed,               // �˻��ⶳ����ݶ�ⶳ���ƣ�
            "88" => TransferRecordType.UNK,                  // �˻��Ǽǣ��ǽ��׼�¼��
            "89" => TransferRecordType.UNK,                  // ȡ���Ǽǣ��ǽ��׼�¼��
            "90" => TransferRecordType.UNK,                  // �����깺Э�飨�ǽ��׼�¼��
            "91" => TransferRecordType.UNK,                  // �������Э�飨�ǽ��׼�¼��
            "92" => TransferRecordType.UNK,                  // ��ŵ�Ż�Э�飨�ǽ��׼�¼��
            "93" => TransferRecordType.UNK,                  // �����깺ȡ�����ǽ��׼�¼��
            "94" => TransferRecordType.UNK,                  // �������ȡ�����ǽ��׼�¼��
            "96" => TransferRecordType.UNK,                  // �������˺ţ��ǽ��׼�¼��
            "97" => TransferRecordType.TransferOut,          // �ڲ�ת�йܣ�ת����
            "98" => TransferRecordType.TransferIn,           // �ڲ��й�ת�루ת�룩
            "99" => TransferRecordType.TransferOut,          // �ڲ��й�ת����ת����

            _ => TransferRecordType.UNK
        };
    }



    public class InvestorAccountMapping
    {
        //FundAccount
        public string Id { get; set; }

        public string Name { get; set; }

        public string Indentity { get; set; }
    }


    public class DistrubutionJson
    {
        /// <summary>
        /// �����˺�
        /// </summary>
        [JsonPropertyName("fundAcco")]
        public string FundAccount { get; set; }

        /// <summary>
        /// �ͻ�����
        /// </summary>
        [JsonPropertyName("custName")]
        public string CustomerName { get; set; }

        /// <summary>
        /// �����̴��루ZX6 ��ʾֱ����
        /// </summary>
        [JsonPropertyName("agencyNo")]
        public string AgencyCode { get; set; }

        /// <summary>
        /// ����������
        /// </summary>
        [JsonPropertyName("agencyName")]
        public string AgencyName { get; set; }

        /// <summary>
        /// �����˺�
        /// </summary>
        [JsonPropertyName("tradeAcco")]
        public string TradeAccount { get; set; }

        /// <summary>
        /// ȷ�����ڣ���ʽ��YYYYMMDD��
        /// </summary>
        [JsonPropertyName("confirmDate")]
        public string ConfirmDate { get; set; }

        /// <summary>
        /// TAȷ�Ϻ�
        /// </summary>
        [JsonPropertyName("cserialNo")]
        public string TaConfirmNo { get; set; }

        /// <summary>
        /// �ֺ�Ǽ�����
        /// </summary>
        [JsonPropertyName("regDate")]
        public string RegisterDate { get; set; }

        /// <summary>
        /// ������������
        /// </summary>
        [JsonPropertyName("date")]
        public string DividendDate { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        [JsonPropertyName("fundName")]
        public string FundName { get; set; }

        /// <summary>
        /// �������
        /// </summary>
        [JsonPropertyName("fundCode")]
        public string FundCode { get; set; }

        /// <summary>
        /// �ֺ�����ݶ�
        /// </summary>
        [JsonPropertyName("totalShare")]
        public string TotalShares { get; set; }

        /// <summary>
        /// ÿ��λ�ֺ���
        /// </summary>
        [JsonPropertyName("unitProfit")]
        public string UnitProfit { get; set; }

        /// <summary>
        /// �����ܶ�
        /// </summary>
        [JsonPropertyName("totalProfit")]
        public string TotalProfit { get; set; }

        /// <summary>
        /// �ֺ췽ʽ��
        /// 0 - ������Ͷ��
        /// 1 - �ֽ����
        /// </summary>
        [JsonPropertyName("flag")]
        public string DividendType { get; set; }

        /// <summary>
        /// ʵ���ֽ����
        /// </summary>
        [JsonPropertyName("realBalance")]
        public string RealCashDividend { get; set; }

        /// <summary>
        /// ��Ͷ�ʺ������
        /// </summary>
        [JsonPropertyName("reinvestBalance")]
        public string ReinvestAmount { get; set; }

        /// <summary>
        /// ��Ͷ�ʷݶ�
        /// </summary>
        [JsonPropertyName("realShares")]
        public string ReinvestShares { get; set; }

        /// <summary>
        /// ��Ͷ������
        /// </summary>
        [JsonPropertyName("lastDate")]
        public string ReinvestDate { get; set; }

        /// <summary>
        /// ��Ͷ�ʵ�λ��ֵ
        /// </summary>
        [JsonPropertyName("netValue")]
        public string NetValue { get; set; }

        /// <summary>
        /// ʵ��ҵ����ɽ��
        /// </summary>
        [JsonPropertyName("deductBalance")]
        public string PerformanceFee { get; set; }

        /// <summary>
        /// �ͻ����ͣ��μ���¼4��
        /// </summary>
        [JsonPropertyName("custType")]
        public string CustomerType { get; set; }

        /// <summary>
        /// ֤�����ͣ��μ���¼4��
        /// </summary>
        [JsonPropertyName("certiType")]
        public string CertificateType { get; set; }

        /// <summary>
        /// ֤������
        /// </summary>
        [JsonPropertyName("certiNo")]
        public string CertificateNumber { get; set; }

        /// <summary>
        /// ��Ȩ��Ϣ��
        /// </summary>
        [JsonPropertyName("exDividendDate")]
        public string ExDividendDate { get; set; }


        public TransferRecord ToObject()
        {
            return new TransferRecord
            {
                CustomerIdentity = CertificateNumber,
                CustomerName = CustomerName,
                ExternalId = TaConfirmNo,
                ConfirmedDate = DateOnly.ParseExact(ConfirmDate, "yyyyMMdd"),
                ConfirmedShare = ParseDecimal(ReinvestShares),
                ConfirmedAmount = ParseDecimal(RealCashDividend),
                ConfirmedNetAmount = ParseDecimal(RealCashDividend),
                RequestDate = DateOnly.ParseExact(ExDividendDate, "yyyyMMdd"),
                RequestAmount = 0,
                RequestShare = ParseDecimal(TotalShares),
                Type = TransferRecordType.Distribution,
                Agency = AgencyName,
                CreateDate = DateOnly.FromDateTime(DateTime.Now),
                PerformanceFee = ParseDecimal(PerformanceFee),
                FundCode = FundCode,
                FundName = FundName,
                Source = "api"
            };
        }
    }
}

#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��