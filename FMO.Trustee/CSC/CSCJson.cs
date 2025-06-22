using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee;

public partial class CSC
{
    public class RetJson
    {
        [JsonPropertyName("retCode")]
        public required string Code { get; set; }


        [JsonPropertyName("retMsg")]
        public required string Msg { get; set; }

    }




    public class RetJson<T>
    {
        [JsonPropertyName("retCode")]
        public required string Code { get; set; }


        [JsonPropertyName("retMsg")]
        public required string Msg { get; set; }

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
        public required string ConfDate { get; set; }

        /// <summary>
        /// �������� (��ʽ: YYYYMMDD, 8λ)
        /// </summary>
        [JsonPropertyName("applyDate")]
        public required string ApplyDate { get; set; }

        /// <summary>
        /// ������� (32λ)
        /// </summary>
        [JsonPropertyName("fundCode")]
        public required string FundCode { get; set; }

        /// <summary>
        /// �������� (250�ַ�)
        /// </summary>
        [JsonPropertyName("fundName")]
        public required string FundName { get; set; }

        /// <summary>
        /// �����̴��� (6λ)
        /// </summary>
        [JsonPropertyName("agencyNo")]
        public required string AgencyNo { get; set; }

        /// <summary>
        /// ���������� (64�ַ�)
        /// </summary>
        [JsonPropertyName("agencyName")]
        public required string AgencyName { get; set; }

        /// <summary>
        /// Ͷ�������� (64�ַ�)
        /// </summary>
        [JsonPropertyName("custName")]
        public required string CustName { get; set; }

        /// <summary>
        /// �����˺� (20�ַ�)
        /// </summary>
        [JsonPropertyName("fundAcco")]
        public required string FundAcco { get; set; }

        /// <summary>
        /// �����˺� (16�ַ�)
        /// </summary>
        [JsonPropertyName("tradeAcco")]
        public required string TradeAcco { get; set; }

        /// <summary>
        /// �����˺� (32�ַ�)
        /// </summary>
        [JsonPropertyName("bankAcco")]
        public required string BankAcco { get; set; }

        /// <summary>
        /// ���б�� (6�ַ�)
        /// </summary>
        [JsonPropertyName("bankNo")]
        public required string BankNo { get; set; }

        /// <summary>
        /// ���������� (250�ַ�)
        /// </summary>
        [JsonPropertyName("openBankName")]
        public required string OpenBankName { get; set; }

        /// <summary>
        /// ���л��� (64�ַ�)
        /// </summary>
        [JsonPropertyName("nameInBank")]
        public required string NameInBank { get; set; }

        /// <summary>
        /// �ͻ����� (6�ַ�)
        /// </summary>
        [JsonPropertyName("custType")]
        public required string CustType { get; set; }

        /// <summary>
        /// ֤������ (3�ַ�)
        /// </summary>
        [JsonPropertyName("certiType")]
        public required string CertiType { get; set; }

        /// <summary>
        /// ֤���� (32�ַ�)
        /// </summary>
        [JsonPropertyName("certiNo")]
        public required string CertiNo { get; set; }

        /// <summary>
        /// TA�����־ (2�ַ�)
        /// </summary>
        [JsonPropertyName("taFlag")]
        public required string TaFlag { get; set; }

        /// <summary>
        /// ȷ��״̬ (2�ַ�)
        /// </summary>
        [JsonPropertyName("confStatus")]
        public required string ConfStatus { get; set; }

        /// <summary>
        /// ȷ�Ͻ������ (250�ַ�)
        /// </summary>
        [JsonPropertyName("describe")]
        public required string Describe { get; set; }

        /// <summary>
        /// �ֺ췽ʽ (2�ַ�)
        /// </summary>
        [JsonPropertyName("bonusType")]
        public required string BonusType { get; set; }

        /// <summary>
        /// ������ (20�ַ�)
        /// </summary>
        [JsonPropertyName("balance")]
        public required string Balance { get; set; }

        /// <summary>
        /// ����ݶ� (20�ַ�)
        /// </summary>
        [JsonPropertyName("shares")]
        public required string Shares { get; set; }

        /// <summary>
        /// ��λ��ֵ (10�ַ�)
        /// </summary>
        [JsonPropertyName("netValue")]
        public required string NetValue { get; set; }

        /// <summary>
        /// ȷ�Ͻ�� (20�ַ�)
        /// </summary>
        [JsonPropertyName("confBalance")]
        public required string ConfBalance { get; set; }

        /// <summary>
        /// ȷ�Ϸݶ� (20�ַ�)
        /// </summary>
        [JsonPropertyName("confShares")]
        public required string ConfShares { get; set; }

        /// <summary>
        /// ������ (20�ַ�)
        /// </summary>
        [JsonPropertyName("charge")]
        public required string Charge { get; set; }

        /// <summary>
        /// ������������� (20�ַ�)
        /// </summary>
        [JsonPropertyName("managerCharge")]
        public required string ManagerCharge { get; set; }

        /// <summary>
        /// �������������� (20�ַ�)
        /// </summary>
        [JsonPropertyName("distributorCharge")]
        public required string DistributorCharge { get; set; }

        /// <summary>
        /// ���Ʒ������ (20�ַ�)
        /// </summary>
        [JsonPropertyName("fundcharge")]
        public required string Fundcharge { get; set; }

        /// <summary>
        /// ҵ������ (20�ַ�)
        /// </summary>
        [JsonPropertyName("achievementPay")]
        public required string AchievementPay { get; set; }

        /// <summary>
        /// TAȷ�ϱ�� (32�ַ�)
        /// </summary>
        [JsonPropertyName("cserialNo")]
        public required string CserialNo { get; set; }

        /// <summary>
        /// ȷ��ҵ������ (6�ַ�)
        /// </summary>
        [JsonPropertyName("busiFlag")]
        public required string BusiFlag { get; set; }

        /// <summary>
        /// ԭ�ⲿϵͳ��������ˮ�� (32�ַ�)
        /// </summary>
        [JsonPropertyName("originalNo")]
        public required string OriginalNo { get; set; }

        /// <summary>
        /// ������ʽ (1�ַ�)
        /// </summary>
        [JsonPropertyName("operWayNew")]
        public required string OperWayNew { get; set; }

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
                RequestAmount = decimal.Parse(Balance),
                RequestShare = decimal.Parse(Shares),
                ConfirmedAmount = decimal.Parse(ConfBalance),
                ConfirmedShare = decimal.Parse(ConfShares),
                CreateDate = DateOnly.FromDateTime(DateTime.Today),
                ExternalId = CserialNo,
                PerformanceFee = decimal.Parse(AchievementPay),
                Fee = decimal.Parse(Charge),
                ExternalRequestId = OriginalNo,
                Type = ParseType(BusiFlag),
                Source = "api",
            };
            // ����
            r.ConfirmedNetAmount = r.ConfirmedAmount - r.Fee;

            return r;
        }


        private TARecordType ParseType(string str)
        {
            switch (str)
            {
                case "120": // �Ϲ� 
                    return TARecordType.Subscription;
                case "122": // �깺 
                    return TARecordType.Purchase;
                case "124": // ��� 
                    return TARecordType.Redemption;
                case "126": // ת���ۻ��� 
                    return TARecordType.MoveIn;
                case "127": // ת���ۻ����� 
                    return TARecordType.MoveIn;
                case "129": // ���÷ֺ췽ʽ 
                    return TARecordType.BonusType;
                case "131": // �ݶ�� 
                    return TARecordType.Frozen;
                case "132": // �ݶ�ⶳ 
                    return TARecordType.Thawed;
                case "133": // ת�� 
                    return TARecordType.TransferOut;
                case "134": // ���� 
                    return TARecordType.TransferIn;
                case "136": // �ݶ�ת�� 
                    return TARecordType.SwitchOut;
                case "137": // �ݶ�ת���� 
                    return TARecordType.SwitchIn;

                case "142": // ǿ����� 
                    return TARecordType.ForceRedemption;
                case "143": // �������� 
                    return TARecordType.Distribution;
                case "144": // ǿ�е��� 
                    return TARecordType.Increase;
                case "145": // ǿ�е��� 
                    return TARecordType.Decrease;
                case "149": // ļ��ʧ�� 
                    return TARecordType.RaisingFailed;
                case "150": // �������� 
                    return TARecordType.Clear;

                default:
                    return TARecordType.UNK;
            }
        }

    }



    public class FundDailyFeeJson
    {

        /// <summary>
        /// ���ü�������
        /// </summary>
        [JsonPropertyName("confDate")]
        public required string ConfDate { get; set; }

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

        #region ���ü���
        /// <summary>
        /// �����
        /// </summary>
        [JsonPropertyName("managementFee")]
        public required string ManagementFee { get; set; }

        /// <summary>
        /// Ͷ�˷�
        /// </summary>
        [JsonPropertyName("investmentFee")]
        public required string InvestmentFee { get; set; }

        /// <summary>
        /// ���۷����
        /// </summary>
        [JsonPropertyName("salesFee")]
        public required string SalesFee { get; set; }

        /// <summary>
        /// �йܷ�
        /// </summary>
        [JsonPropertyName("custodianFee")]
        public required string CustodianFee { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        [JsonPropertyName("outsourcingFee")]
        public required string OutsourcingFee { get; set; }

        /// <summary>
        /// ҵ���������
        /// </summary>
        [JsonPropertyName("reward")]
        public required string Reward { get; set; }

        /// <summary>
        /// ��ֵ˰����
        /// </summary>
        [JsonPropertyName("addedTax")]
        public required string AddedTax { get; set; }

        /// <summary>
        /// ����˰����
        /// </summary>
        [JsonPropertyName("surTax")]
        public required string SurTax { get; set; }

        /// <summary>
        /// ��ֵ˰������˰����
        /// </summary>
        [JsonPropertyName("addedSurTax")]
        public required string AddedSurTax { get; set; }
        #endregion

        #region ����֧��
        /// <summary>
        /// �����֧��
        /// </summary>
        [JsonPropertyName("managementFeePay")]
        public required string ManagementFeePay { get; set; }

        /// <summary>
        /// Ͷ�˷�֧��
        /// </summary>
        [JsonPropertyName("investmentFeePay")]
        public required string InvestmentFeePay { get; set; }

        /// <summary>
        /// ���۷����֧��
        /// </summary>
        [JsonPropertyName("salesFeePay")]
        public required string SalesFeePay { get; set; }

        /// <summary>
        /// �йܷ�֧��
        /// </summary>
        [JsonPropertyName("custodianFeePay")]
        public required string CustodianFeePay { get; set; }

        /// <summary>
        /// ��������֧��
        /// </summary>
        [JsonPropertyName("outsourcingFeePay")]
        public required string OutsourcingFeePay { get; set; }

        /// <summary>
        /// ҵ������֧��
        /// </summary>
        [JsonPropertyName("rewardPay")]
        public required string RewardPay { get; set; }

        /// <summary>
        /// ��ֵ˰������˰֧��
        /// </summary>
        [JsonPropertyName("addedSurTaxPay")]
        public required string AddedSurTaxPay { get; set; }
        #endregion

        #region �������
        /// <summary>
        /// ��������
        /// </summary>
        [JsonPropertyName("managementFeeBalance")]
        public required string ManagementFeeBalance { get; set; }

        /// <summary>
        /// Ͷ�˷����
        /// </summary>
        [JsonPropertyName("investmentFeeBalance")]
        public required string InvestmentFeeBalance { get; set; }

        /// <summary>
        /// ���۷�������
        /// </summary>
        [JsonPropertyName("salesFeeBalance")]
        public required string SalesFeeBalance { get; set; }

        /// <summary>
        /// �йܷ����
        /// </summary>
        [JsonPropertyName("custodianFeeBalance")]
        public required string CustodianFeeBalance { get; set; }

        /// <summary>
        /// �����������
        /// </summary>
        [JsonPropertyName("outsourcingFeeBalance")]
        public required string OutsourcingFeeBalance { get; set; }

        /// <summary>
        /// ҵ���������
        /// </summary>
        [JsonPropertyName("rewardBalance")]
        public required string RewardBalance { get; set; }

        /// <summary>
        /// ��ֵ˰������˰���
        /// </summary>
        [JsonPropertyName("addedSurTaxBalance")]
        public required string AddedSurTaxBalance { get; set; }
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
        /// �������ڣ���ʽ��YYYY-MM-DD
        /// </summary>
        [JsonPropertyName("tradeDate")]

        public required string TradeDate { get; set; }

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
        /// ����ʱ�䣬��ʽ��HHMMSS
        /// </summary>
        [JsonPropertyName("tradeTime")]

        public required string TradeTime { get; set; }

        /// <summary>
        /// ���׽���ʽ��С�������λ
        /// </summary>
        [JsonPropertyName("amt")]

        public required string Amount { get; set; }

        /// <summary>
        /// �����ʶ��1-�裬2-��
        /// </summary>
        [JsonPropertyName("transferInOut")]
        public required string TransferType { get; set; }

        /// <summary>
        /// �˻�����ʽ��С�������λ
        /// </summary>
        [JsonPropertyName("balance")]

        public required string Balance { get; set; }

        /// <summary>
        /// �Է��˺�
        /// </summary>
        [JsonPropertyName("optBankAcco")]

        public required string CounterpartyAccount { get; set; }

        /// <summary>
        /// �Է�����
        /// </summary>
        [JsonPropertyName("optAccName")]

        public required string CounterpartyName { get; set; }

        /// <summary>
        /// �Է�����������
        /// </summary>
        [JsonPropertyName("optOpenBankName")]

        public required string CounterpartyBank { get; set; }

        /// <summary>
        /// �����˺�
        /// </summary>
        [JsonPropertyName("bankAcco")]
        public required string OurAccount { get; set; }

        /// <summary>
        /// ����ժҪ
        /// </summary>
        [JsonPropertyName("digest")]

        public required string Summary { get; set; }

        /// <summary>
        /// ���¼ID
        /// </summary>
        [JsonPropertyName("id")]
        public required string RecordId { get; set; }

        /// <summary>
        /// ������ˮ��
        /// </summary>
        [JsonPropertyName("serialNo")]

        public required string TransactionNo { get; set; }

        /// <summary>
        /// ��ˮ��ϸ״̬
        /// 0-�޹���������1-���µ�����2-���µ������˿3-�˿��У�5-���µ���������˿��У���-����״̬
        /// </summary>
        [JsonPropertyName("detailStatus")]

        public required string Status { get; set; }


        public BankTransaction ToObject()
        {
            return new BankTransaction
            {
                Id = $"{OurAccount}|{TransactionNo}",
                Time = DateTime.ParseExact(TradeDate + TradeTime, "yyyyMMddHHmmss", null),
                Amount = decimal.Parse(Amount),
                AccountNo = OurAccount,
                // ������ȱʧ
                AccountBank = "unset",
                AccountName = "unset",
                Direction = TransferType == "1" ? TransctionDirection.Pay : TransctionDirection.Receive,
                CounterBank = CounterpartyBank,
                CounterName = CounterpartyName,
                CounterNo = CounterpartyAccount,
                Balance = decimal.Parse(Balance),
                Serial = TransactionNo,
                Remark = Summary,
            };
        }
    }












}
