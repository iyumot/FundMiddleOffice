using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee;


#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��
public partial class CMS
{
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
                Balance = ParseDecimal(AccountBalance),
                Time = DateTime.ParseExact(UpdateTime, "yyyy-MM-dd HH:mm:ss", null)
            };
        }
    }


    
     
}



#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��