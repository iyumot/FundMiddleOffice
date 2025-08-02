using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee;


#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��
public partial class CMS
{
    public class RaisingBankTransactionJson
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

        internal RaisingBankTransaction ToObject()
        {
            // �ж� OurAccountName ���Ƿ��л����� 
            if (OurAccountName == "����֤ȯ�ɷ����޹�˾������Ӫ�������ļ��ר��")
                OurAccountName = $"{ProductName}ļ��ר��";

            return new RaisingBankTransaction
            {
                Id = TransactionId,
                FundCode = ProductCode,
                AccountNo = OurAccountNumber,
                AccountBank = CollectionBank,
                AccountName = OurAccountName,
                Serial = TransactionId,
                CounterBank = CounterpartyBankName,
                CounterName = CounterpartyAccountName,
                CounterNo = CounterpartyAccountNumber,
                Amount = ParseDecimal(TransactionAmount),
                Direction = TransactionType == "��" ? TransctionDirection.Pay : TransctionDirection.Receive,
                Remark = BankMemo,
                Time = DateTime.ParseExact(TransactionDate + TransactionTime, "yyyyMMddHH:mm:ss", null)
            };
        }
    }


    
     
}



#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��