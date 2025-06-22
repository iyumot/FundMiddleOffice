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
        public string? CertiNo { get; set; } // ֤����

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
            throw new NotImplementedException();
            return new Investor
            {
                Name = CustName
            };
        }

    }

    public class TransferRecordJson
    {
        [JsonPropertyName("ackNo")]
        public string? AckNo { get; set; }

        [JsonPropertyName("exRequestNo")]
        public string? ExRequestNo { get; set; }

        [JsonPropertyName("fundAcco")]
        public string? FundAcco { get; set; }

        [JsonPropertyName("tradeAcco")]
        public string? TradeAcco { get; set; }

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
        public string? Apkind { get; set; } // ҵ�����ʹ���

        [JsonPropertyName("taFlag")]
        public string? TaFlag { get; set; } // TA�����־��0-��1-��

        [JsonPropertyName("fundCode")]
        public string? FundCode { get; set; }

        [JsonPropertyName("shareLevel")]
        public string? ShareLevel { get; set; } // �ݶ����A-ǰ�շѣ�B-���շ�

        [JsonPropertyName("currency")]
        public string? Currency { get; set; } // ���֣�����ҡ���Ԫ �� null

        [JsonPropertyName("largeRedemptionFlag")]
        public string? LargeRedemptionFlag { get; set; } // �޶���ش����־��0-ȡ����1-˳��

        [JsonPropertyName("subAmt")]
        public string? SubAmt { get; set; } // ������

        [JsonPropertyName("subQuty")]
        public string? SubQuty { get; set; } // ����ݶ�

        [JsonPropertyName("bonusType")]
        public string? BonusType { get; set; } // �ֺ췽ʽ��0-������Ͷ�ʣ�1-�ֽ����

        [JsonPropertyName("nav")]
        public string? Nav { get; set; } // ��λ��ֵ

        [JsonPropertyName("ackAmt")]
        public string? AckAmt { get; set; } // ȷ�Ͻ��

        [JsonPropertyName("ackQuty")]
        public string? AckQuty { get; set; } // ȷ�Ϸݶ�

        [JsonPropertyName("tradeFee")]
        public string? TradeFee { get; set; } // ���׷�

        [JsonPropertyName("taFee")]
        public string? TaFee { get; set; } // ������

        [JsonPropertyName("backFee")]
        public string? BackFee { get; set; } // ���շ�

        [JsonPropertyName("realBalance")]
        public string? RealBalance { get; set; } // Ӧ�������

        [JsonPropertyName("profitBalance")]
        public string? ProfitBalance { get; set; } // ҵ������

        [JsonPropertyName("profitBalanceForAgency")]
        public string? ProfitBalanceForAgency { get; set; } // ҵ�������������

        [JsonPropertyName("totalNav")]
        public string? TotalNav { get; set; } // �ۼƾ�ֵ

        [JsonPropertyName("totalFee")]
        public string? TotalFee { get; set; } // �ܷ���

        [JsonPropertyName("agencyFee")]
        public string? AgencyFee { get; set; } // �����ۻ�������

        [JsonPropertyName("fundFee")]
        public string? FundFee { get; set; } // ������ʲ�����

        [JsonPropertyName("registFee")]
        public string? RegistFee { get; set; } // ������˷���

        [JsonPropertyName("interest")]
        public string? Interest { get; set; } // ��Ϣ

        [JsonPropertyName("interestTax")]
        public string? InterestTax { get; set; } // ��Ϣ˰

        [JsonPropertyName("interestShare")]
        public string? InterestShare { get; set; } // ��Ϣת�ݶ�

        [JsonPropertyName("frozenBalance")]
        public string? FrozenBalance { get; set; } // ȷ�϶���ݶ�

        [JsonPropertyName("unfrozenBalance")]
        public string? UnfrozenBalance { get; set; } // ȷ�Ͻⶳ�ݶ�

        [JsonPropertyName("unShares")]
        public string? UnShares { get; set; } // �޶����˳�ӷݶ�

        [JsonPropertyName("applyDate")]
        public string? ApplyDate { get; set; } // ���빤����

        [JsonPropertyName("ackDate")]
        public string? AckDate { get; set; } // ȷ������

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
        public string? AckStatus { get; set; } // ȷ��״̬

        [JsonPropertyName("agencyNo")]
        public string? AgencyNo { get; set; } // �����̴���

        [JsonPropertyName("agencyName")]
        public string? AgencyName { get; set; } // ����������

        [JsonPropertyName("retCod")]
        public string? RetCod { get; set; } // ������

        [JsonPropertyName("retMsg")]
        public string? RetMsg { get; set; } // ʧ��ԭ��

        [JsonPropertyName("adjustCause")]
        public string? AdjustCause { get; set; } // �ݶ����ԭ��

        [JsonPropertyName("navDate")]
        public string? NavDate { get; set; } // ��ֵ���� (YYYYMMDD)

        [JsonPropertyName("oriCserialNo")]
        public string? OriCserialNo { get; set; } // ԭȷ�ϵ���

        internal TransferRecord ToObject()
        {
            throw new NotImplementedException();
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
}
