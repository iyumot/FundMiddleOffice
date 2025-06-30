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