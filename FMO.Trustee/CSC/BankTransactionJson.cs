using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee;


#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��
public partial class CSC
{
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

}

#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��