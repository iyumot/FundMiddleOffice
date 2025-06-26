using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee;


#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��
public partial class CSC
{
    public class RetJson
    {
        [JsonPropertyName("retCode")]
        public string Code { get; set; }


        [JsonPropertyName("retMsg")]
        public string Msg { get; set; }

    }




    public class RetJson<T>
    {
        [JsonPropertyName("retCode")]
        public string Code { get; set; }


        [JsonPropertyName("retMsg")]
        public string Msg { get; set; }

        [JsonPropertyName("data")]
        public required DataJsonWrap<T> Data { get; set; }
    }

    public class DataJsonWrap<T>
    {

        [JsonPropertyName("result")]
        public required T[] Data { get; set; }

        [JsonPropertyName("rowCount")]
        public int RowCount { get; set; }


        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }
    }


    public interface IJson<T>
    {
        T ToObject();
    }

    internal class TransferRecordJson : IJson<TransferRecord>
    {
        /// <summary>
        /// ����ȷ������ (��ʽ: YYYYMMDD, 8λ)
        /// </summary>
        [JsonPropertyName("confDate")]
        public string ConfDate { get; set; }

        /// <summary>
        /// �������� (��ʽ: YYYYMMDD, 8λ)
        /// </summary>
        [JsonPropertyName("applyDate")]
        public string ApplyDate { get; set; }

        /// <summary>
        /// ������� (32λ)
        /// </summary>
        [JsonPropertyName("fundCode")]
        public string FundCode { get; set; }

        /// <summary>
        /// �������� (250�ַ�)
        /// </summary>
        [JsonPropertyName("fundName")]
        public string FundName { get; set; }

        /// <summary>
        /// �����̴��� (6λ)
        /// </summary>
        [JsonPropertyName("agencyNo")]
        public string AgencyNo { get; set; }

        /// <summary>
        /// ���������� (64�ַ�)
        /// </summary>
        [JsonPropertyName("agencyName")]
        public string AgencyName { get; set; }

        /// <summary>
        /// Ͷ�������� (64�ַ�)
        /// </summary>
        [JsonPropertyName("custName")]
        public string CustName { get; set; }

        /// <summary>
        /// �����˺� (20�ַ�)
        /// </summary>
        [JsonPropertyName("fundAcco")]
        public string FundAcco { get; set; }

        /// <summary>
        /// �����˺� (16�ַ�)
        /// </summary>
        [JsonPropertyName("tradeAcco")]
        public string TradeAcco { get; set; }

        /// <summary>
        /// �����˺� (32�ַ�)
        /// </summary>
        [JsonPropertyName("bankAcco")]
        public string BankAcco { get; set; }

        /// <summary>
        /// ���б�� (6�ַ�)
        /// </summary>
        [JsonPropertyName("bankNo")]
        public string BankNo { get; set; }

        /// <summary>
        /// ���������� (250�ַ�)
        /// </summary>
        [JsonPropertyName("openBankName")]
        public string OpenBankName { get; set; }

        /// <summary>
        /// ���л��� (64�ַ�)
        /// </summary>
        [JsonPropertyName("nameInBank")]
        public string NameInBank { get; set; }

        /// <summary>
        /// �ͻ����� (6�ַ�)
        /// </summary>
        [JsonPropertyName("custType")]
        public string CustType { get; set; }

        /// <summary>
        /// ֤������ (3�ַ�)
        /// </summary>
        [JsonPropertyName("certiType")]
        public string CertiType { get; set; }

        /// <summary>
        /// ֤���� (32�ַ�)
        /// </summary>
        [JsonPropertyName("certiNo")]
        public string CertiNo { get; set; }

        /// <summary>
        /// TA�����־ (2�ַ�)
        /// </summary>
        [JsonPropertyName("taFlag")]
        public string TaFlag { get; set; }

        /// <summary>
        /// ȷ��״̬ (2�ַ�)
        /// </summary>
        [JsonPropertyName("confStatus")]
        public string ConfStatus { get; set; }

        /// <summary>
        /// ȷ�Ͻ������ (250�ַ�)
        /// </summary>
        [JsonPropertyName("describe")]
        public string Describe { get; set; }

        /// <summary>
        /// �ֺ췽ʽ (2�ַ�)
        /// </summary>
        [JsonPropertyName("bonusType")]
        public string BonusType { get; set; }

        /// <summary>
        /// ������ (20�ַ�)
        /// </summary>
        [JsonPropertyName("balance")]
        public string Balance { get; set; }

        /// <summary>
        /// ����ݶ� (20�ַ�)
        /// </summary>
        [JsonPropertyName("shares")]
        public string Shares { get; set; }

        /// <summary>
        /// ��λ��ֵ (10�ַ�)
        /// </summary>
        [JsonPropertyName("netValue")]
        public string NetValue { get; set; }

        /// <summary>
        /// ȷ�Ͻ�� (20�ַ�)
        /// </summary>
        [JsonPropertyName("confBalance")]
        public string ConfBalance { get; set; }

        /// <summary>
        /// ȷ�Ϸݶ� (20�ַ�)
        /// </summary>
        [JsonPropertyName("confShares")]
        public string ConfShares { get; set; }

        /// <summary>
        /// ������ (20�ַ�)
        /// </summary>
        [JsonPropertyName("charge")]
        public string Charge { get; set; }

        /// <summary>
        /// ������������� (20�ַ�)
        /// </summary>
        [JsonPropertyName("managerCharge")]
        public string ManagerCharge { get; set; }

        /// <summary>
        /// �������������� (20�ַ�)
        /// </summary>
        [JsonPropertyName("distributorCharge")]
        public string DistributorCharge { get; set; }

        /// <summary>
        /// ���Ʒ������ (20�ַ�)
        /// </summary>
        [JsonPropertyName("fundcharge")]
        public string Fundcharge { get; set; }

        /// <summary>
        /// ҵ������ (20�ַ�)
        /// </summary>
        [JsonPropertyName("achievementPay")]
        public string AchievementPay { get; set; }

        /// <summary>
        /// TAȷ�ϱ�� (32�ַ�)
        /// </summary>
        [JsonPropertyName("cserialNo")]
        public string CserialNo { get; set; }

        /// <summary>
        /// ȷ��ҵ������ (6�ַ�)
        /// </summary>
        [JsonPropertyName("busiFlag")]
        public string BusiFlag { get; set; }

        /// <summary>
        /// ԭ�ⲿϵͳ��������ˮ�� (32�ַ�)
        /// </summary>
        [JsonPropertyName("originalNo")]
        public string OriginalNo { get; set; }

        /// <summary>
        /// ������ʽ (1�ַ�)
        /// </summary>
        [JsonPropertyName("operWayNew")]
        public string OperWayNew { get; set; }

        /// <summary>
        /// 120	�Ϲ�
        //122	�깺
        //124	���
        //126	ת���ۻ���
        //127	ת���ۻ�����
        //129	���÷ֺ췽ʽ
        //131	�ݶ��
        //132	�ݶ�ⶳ
        //133	ת��
        //134	����
        //136	�ݶ�ת��
        //137	�ݶ�ת����
        //142	ǿ�����
        //143	��������
        //144	ǿ�е���
        //145	ǿ�е���
        //149	ļ��ʧ��
        //150	��������
        /// </summary>
        /// <returns></returns>
        public TransferRecord ToObject()
        {
            var r = new TransferRecord
            {
                FundCode = FundCode,
                ConfirmedDate = DateOnly.ParseExact(ConfDate, "yyyyMMdd"),
                RequestDate = DateOnly.ParseExact(ApplyDate, "yyyyMMdd"),
                FundName = FundName,
                Agency = AgencyName,
                CustomerName = CustName,
                CustomerIdentity = CertiNo,
                RequestAmount = ParseDecimal(Balance),
                RequestShare = ParseDecimal(Shares),
                ConfirmedAmount = ParseDecimal(ConfBalance),
                ConfirmedShare = ParseDecimal(ConfShares),
                CreateDate = DateOnly.FromDateTime(DateTime.Today),
                ExternalId = CserialNo,
                PerformanceFee = ParseDecimal(AchievementPay),
                Fee = ParseDecimal(Charge),
                ExternalRequestId = OriginalNo,
                Type = ParseType(BusiFlag),
                Source = "api",
            };
            // ����
            r.ConfirmedNetAmount = r.ConfirmedAmount - r.Fee;

            return r;
        }


        private TransferRecordType ParseType(string str)
        {
            switch (str)
            {
                case "120": // �Ϲ� 
                    return TransferRecordType.Subscription;
                case "122": // �깺 
                    return TransferRecordType.Purchase;
                case "124": // ��� 
                    return TransferRecordType.Redemption;
                case "126": // ת���ۻ��� 
                    return TransferRecordType.MoveIn;
                case "127": // ת���ۻ����� 
                    return TransferRecordType.MoveIn;
                case "129": // ���÷ֺ췽ʽ 
                    return TransferRecordType.BonusType;
                case "131": // �ݶ�� 
                    return TransferRecordType.Frozen;
                case "132": // �ݶ�ⶳ 
                    return TransferRecordType.Thawed;
                case "133": // ת�� 
                    return TransferRecordType.TransferOut;
                case "134": // ���� 
                    return TransferRecordType.TransferIn;
                case "136": // �ݶ�ת�� 
                    return TransferRecordType.SwitchOut;
                case "137": // �ݶ�ת���� 
                    return TransferRecordType.SwitchIn;

                case "142": // ǿ����� 
                    return TransferRecordType.ForceRedemption;
                case "143": // �������� 
                    return TransferRecordType.Distribution;
                case "144": // ǿ�е��� 
                    return TransferRecordType.Increase;
                case "145": // ǿ�е��� 
                    return TransferRecordType.Decrease;
                case "149": // ļ��ʧ�� 
                    return TransferRecordType.RaisingFailed;
                case "150": // �������� 
                    return TransferRecordType.Clear;

                default:
                    return TransferRecordType.UNK;
            }
        }

    }



    public class FundDailyFeeJson
    {

        /// <summary>
        /// ���ü�������
        /// </summary>
        [JsonPropertyName("confDate")]
        public string ConfDate { get; set; }

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

        #region ���ü���
        /// <summary>
        /// �����
        /// </summary>
        [JsonPropertyName("managementFee")]
        public string ManagementFee { get; set; }

        /// <summary>
        /// Ͷ�˷�
        /// </summary>
        [JsonPropertyName("investmentFee")]
        public string InvestmentFee { get; set; }

        /// <summary>
        /// ���۷����
        /// </summary>
        [JsonPropertyName("salesFee")]
        public string SalesFee { get; set; }

        /// <summary>
        /// �йܷ�
        /// </summary>
        [JsonPropertyName("custodianFee")]
        public string CustodianFee { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        [JsonPropertyName("outsourcingFee")]
        public string OutsourcingFee { get; set; }

        /// <summary>
        /// ҵ���������
        /// </summary>
        [JsonPropertyName("reward")]
        public string Reward { get; set; }

        /// <summary>
        /// ��ֵ˰����
        /// </summary>
        [JsonPropertyName("addedTax")]
        public string AddedTax { get; set; }

        /// <summary>
        /// ����˰����
        /// </summary>
        [JsonPropertyName("surTax")]
        public string SurTax { get; set; }

        /// <summary>
        /// ��ֵ˰������˰����
        /// </summary>
        [JsonPropertyName("addedSurTax")]
        public string AddedSurTax { get; set; }
        #endregion

        #region ����֧��
        /// <summary>
        /// �����֧��
        /// </summary>
        [JsonPropertyName("managementFeePay")]
        public string ManagementFeePay { get; set; }

        /// <summary>
        /// Ͷ�˷�֧��
        /// </summary>
        [JsonPropertyName("investmentFeePay")]
        public string InvestmentFeePay { get; set; }

        /// <summary>
        /// ���۷����֧��
        /// </summary>
        [JsonPropertyName("salesFeePay")]
        public string SalesFeePay { get; set; }

        /// <summary>
        /// �йܷ�֧��
        /// </summary>
        [JsonPropertyName("custodianFeePay")]
        public string CustodianFeePay { get; set; }

        /// <summary>
        /// ��������֧��
        /// </summary>
        [JsonPropertyName("outsourcingFeePay")]
        public string OutsourcingFeePay { get; set; }

        /// <summary>
        /// ҵ������֧��
        /// </summary>
        [JsonPropertyName("rewardPay")]
        public string RewardPay { get; set; }

        /// <summary>
        /// ��ֵ˰������˰֧��
        /// </summary>
        [JsonPropertyName("addedSurTaxPay")]
        public string AddedSurTaxPay { get; set; }
        #endregion

        #region �������
        /// <summary>
        /// ��������
        /// </summary>
        [JsonPropertyName("managementFeeBalance")]
        public string ManagementFeeBalance { get; set; }

        /// <summary>
        /// Ͷ�˷����
        /// </summary>
        [JsonPropertyName("investmentFeeBalance")]
        public string InvestmentFeeBalance { get; set; }

        /// <summary>
        /// ���۷�������
        /// </summary>
        [JsonPropertyName("salesFeeBalance")]
        public string SalesFeeBalance { get; set; }

        /// <summary>
        /// �йܷ����
        /// </summary>
        [JsonPropertyName("custodianFeeBalance")]
        public string CustodianFeeBalance { get; set; }

        /// <summary>
        /// �����������
        /// </summary>
        [JsonPropertyName("outsourcingFeeBalance")]
        public string OutsourcingFeeBalance { get; set; }

        /// <summary>
        /// ҵ���������
        /// </summary>
        [JsonPropertyName("rewardBalance")]
        public string RewardBalance { get; set; }

        /// <summary>
        /// ��ֵ˰������˰���
        /// </summary>
        [JsonPropertyName("addedSurTaxBalance")]
        public string AddedSurTaxBalance { get; set; }
        #endregion


        public FundDailyFee ToObject()
        {


            return new FundDailyFee
            {
                FundCode = FundCode,
                Date = DateOnly.ParseExact(ConfDate, "yyyyMMdd"),

                ManagerFeeAccrued = ParseDecimal(ManagementFee),
                ManagerFeePaid = ParseDecimal(ManagementFeePay),
                ManagerFeeBalance = ParseDecimal(ManagementFeeBalance),
                PerformanceFeeAccrued = ParseDecimal(Reward),
                PerformanceFeePaid = ParseDecimal(RewardPay),
                PerformanceFeeBalance = ParseDecimal(RewardBalance),
                CustodianFeeAccrued = ParseDecimal(CustodianFee),
                CustodianFeePaid = ParseDecimal(CustodianFeePay),
                CustodianFeeBalance = ParseDecimal(CustodianFeeBalance),
                ConsultantFeeAccrued = ParseDecimal(InvestmentFee),
                ConsultantFeePaid = ParseDecimal(InvestmentFeePay),
                ConsultantFeeBalance = ParseDecimal(InvestmentFeeBalance),
                SalesFeeAccrued = ParseDecimal(SalesFee),
                SalesFeePaid = ParseDecimal(SalesFeePay),
                SalesFeeBalance = ParseDecimal(SalesFeeBalance),
                OutsourcingFeeAccrued = ParseDecimal(OutsourcingFee),
                OutsourcingFeePaid = ParseDecimal(OutsourcingFeePay),
                OutsourcingFeeBalance = ParseDecimal(OutsourcingFeeBalance),
            };
        }

    }



    public class BankTransactionJson
    {
        /// <summary>
        /// �������ڣ���ʽ��YYYY-MM-DD
        /// </summary>
        [JsonPropertyName("tradeDate")]

        public string TradeDate { get; set; }

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
        /// ����ʱ�䣬��ʽ��HHMMSS
        /// </summary>
        [JsonPropertyName("tradeTime")]

        public string TradeTime { get; set; }

        /// <summary>
        /// ���׽���ʽ��С�������λ
        /// </summary>
        [JsonPropertyName("amt")]

        public string Amount { get; set; }

        /// <summary>
        /// �����ʶ��1-�裬2-��
        /// </summary>
        [JsonPropertyName("transferInOut")]
        public string TransferType { get; set; }

        /// <summary>
        /// �˻�����ʽ��С�������λ
        /// </summary>
        [JsonPropertyName("balance")]

        public string Balance { get; set; }

        /// <summary>
        /// �Է��˺�
        /// </summary>
        [JsonPropertyName("optBankAcco")]

        public string CounterpartyAccount { get; set; }

        /// <summary>
        /// �Է�����
        /// </summary>
        [JsonPropertyName("optAccName")]

        public string CounterpartyName { get; set; }

        /// <summary>
        /// �Է�����������
        /// </summary>
        [JsonPropertyName("optOpenBankName")]

        public string CounterpartyBank { get; set; }

        /// <summary>
        /// �����˺�
        /// </summary>
        [JsonPropertyName("bankAcco")]
        public string OurAccount { get; set; }

        /// <summary>
        /// ����ժҪ
        /// </summary>
        [JsonPropertyName("digest")]

        public string Summary { get; set; }

        /// <summary>
        /// ���¼ID
        /// </summary>
        [JsonPropertyName("id")]
        public string RecordId { get; set; }

        /// <summary>
        /// ������ˮ��
        /// </summary>
        [JsonPropertyName("serialNo")]

        public string TransactionNo { get; set; }

        /// <summary>
        /// ��ˮ��ϸ״̬
        /// 0-�޹���������1-���µ�����2-���µ������˿3-�˿��У�5-���µ���������˿��У���-����״̬
        /// </summary>
        [JsonPropertyName("detailStatus")]

        public string Status { get; set; }


        public BankTransaction ToObject()
        {
            return new BankTransaction
            {
                Id = $"{OurAccount}|{TransactionNo}",
                Time = DateTime.ParseExact(TradeDate + TradeTime, "yyyyMMddHHmmss", null),
                Amount = ParseDecimal(Amount),
                AccountNo = OurAccount,
                // ������ȱʧ
                AccountBank = "unset",
                AccountName = "unset",
                Direction = TransferType == "1" ? TransctionDirection.Pay : TransctionDirection.Receive,
                CounterBank = CounterpartyBank,
                CounterName = CounterpartyName,
                CounterNo = CounterpartyAccount,
                Balance = ParseDecimal(Balance),
                Serial = TransactionNo,
                Remark = Summary,
            };
        }
    }



    public class BankBalanceJson
    {  /// <summary>
       /// ����
       /// </summary>
        [JsonPropertyName("curType")]
        public string Currency { get; set; }

        /// <summary>
        /// ������루��󳤶�32��
        /// </summary>
        [JsonPropertyName("fundCode")]
        public string FundCode { get; set; }

        /// <summary>
        /// �������ƣ���󳤶�250��
        /// </summary>
        [JsonPropertyName("fundName")]
        public string FundName { get; set; }

        /// <summary>
        /// �˻����ͣ���󳤶�2��
        /// </summary>
        [JsonPropertyName("accType")]
        public string AccountType { get; set; }

        /// <summary>
        /// �����˺ţ���󳤶�32��
        /// </summary>
        [JsonPropertyName("bankAcco")]
        public string BankAccountNo { get; set; }

        /// <summary>
        /// �˻����ƣ���󳤶�64��
        /// </summary>
        [JsonPropertyName("accName")]
        public string AccountName { get; set; }

        /// <summary>
        /// ���б�ţ���󳤶�6��ͨ��Ϊ���д��֧����ǰ��λ��
        /// </summary>
        [JsonPropertyName("bankNo")]
        public string BankNumber { get; set; }

        /// <summary>
        /// ���������ƣ���󳤶�250��
        /// </summary>
        [JsonPropertyName("openBankName")]
        public string OpenBankName { get; set; }

        /// <summary>
        /// ��������󳤶�20��������λС����
        /// </summary>
        [JsonPropertyName("acctBal")]
        public string AccountBalance { get; set; }

        /// <summary>
        /// �йܻ��˻�״̬�����йܻ���Ч��ļ���������壩
        /// �μ� 3.16 �й��˻�״̬
        /// </summary>
        [JsonPropertyName("acctStatus")]
        public string CustodialStatus { get; set; }

        /// <summary>
        /// ļ�����˻�״̬����ļ������Ч���йܻ������壩
        /// �μ� 3.23 ļ�����˻�״̬
        /// </summary>
        [JsonPropertyName("raiseStatus")]
        public string RaiseStatus { get; set; }

        /// <summary>
        /// ������ʱ�䣨��ʽ��YYYY-MM-dd HH:mm:ss��
        /// </summary>
        [JsonPropertyName("retTime")]
        public string LastUpdateTime { get; set; }



        public FundBankBalance ToObject()
        {
            return new FundBankBalance
            {
                AccountName = AccountName,
                AccountNo = BankAccountNo,
                Name = OpenBankName,
                FundName = FundName,
                FundCode = FundCode,
                Balance = string.IsNullOrWhiteSpace(AccountBalance) ? 0 : ParseDecimal(AccountBalance),
                Currency = ParseCurrency(Currency),
                Time = string.IsNullOrWhiteSpace(LastUpdateTime) ? default : DateTime.ParseExact(LastUpdateTime, "yyyy-MM-dd HH:mm:ss", null)
            };
        }

    }


    public class SubjectFundMappingJson
    {
        /// <summary>
        /// ��Ʒ���루�����󳤶�32��
        /// </summary>
        [JsonPropertyName("productNo")]
        public string ProductNo { get; set; }

        /// <summary>
        /// ��Ʒ���ƣ������󳤶�250��
        /// </summary>
        [JsonPropertyName("productName")]
        public string ProductName { get; set; }

        /// <summary>
        /// ��˾���ƣ������󳤶�128��
        /// </summary>
        [JsonPropertyName("companyName")]
        public string CompanyName { get; set; }

        /// <summary>
        /// ��Ʒ״̬������ֵ����¼3.4����󳤶�2��
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }

        /// <summary>
        /// ����ʱ�䣨�����ʽyyyyMMdd��
        /// </summary>
        [JsonPropertyName("publishDate")]
        public string PublishDate { get; set; }

        /// <summary>
        /// �йܻ������Ǳ����󳤶�128��
        /// </summary>
        [JsonPropertyName("custodianOrg")]
        public string CustodianOrg { get; set; }

        /// <summary>
        /// ����������Ǳ����󳤶�128��
        /// </summary>
        [JsonPropertyName("outsourcingOrg")]
        public string OutsourcingOrg { get; set; }

        /// <summary>
        /// ĸ��Ʒ���루�Ǳ�����Ӳ�Ʒ���أ���󳤶�32��
        /// </summary>
        [JsonPropertyName("parentProductNo")]
        public string ParentProductNo { get; set; }

        /// <summary>
        /// ĸ��Ʒ���ƣ��Ǳ�����Ӳ�Ʒ���أ���󳤶�250��
        /// </summary>
        [JsonPropertyName("parentProductName")]
        public string ParentProductName { get; set; }


        public SubjectFundMapping ToObject()
        {
            var sc = !string.IsNullOrWhiteSpace(ParentProductName) ? ProductName.Replace(ProductName, "") : "";
            return new SubjectFundMapping
            {
                FundCode = ProductNo,
                FundName = ProductName,
                MasterCode = ParentProductNo,
                MasterName = ParentProductName,
                ShareClass = sc,
                Status = GetStatus(Status),
            };
        }


        public FundStatus GetStatus(string s)
        {
            return s switch
            {
                "0" => FundStatus.Initiate,
                "1" => FundStatus.Normal,
                "2" => FundStatus.StartLiquidation,
                "3" => FundStatus.Liquidation,
                _ => FundStatus.Unk,
            };
        }
    }




    private static string ParseCurrency(string currencyCode)
    {
        switch (currencyCode)
        {
            case "156":
                return "CNY"; // �����
            case "250":
                return "CHF"; // ��ʿ����
            case "280":
                return "DEM"; // �¹���ˣ���ͣ�ã�
            case "344":
                return "HKD"; // ��Ԫ
            case "392":
                return "JPY"; // ��Ԫ
            case "826":
                return "GBP"; // Ӣ��
            case "840":
                return "USD"; // ��Ԫ
            case "954":
                return "EUR"; // ŷԪ
            default:
                return "";  // �����׳��쳣��������Ҫ������Ч����
        }
    }

}

#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��