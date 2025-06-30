using System.Text.Json.Serialization;

namespace FMO.Trustee;


#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
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
                return "CNY"; // 人民币
            case "250":
                return "CHF"; // 瑞士法郎
            case "280":
                return "DEM"; // 德国马克（已停用）
            case "344":
                return "HKD"; // 港元
            case "392":
                return "JPY"; // 日元
            case "826":
                return "GBP"; // 英镑
            case "840":
                return "USD"; // 美元
            case "954":
                return "EUR"; // 欧元
            default:
                return "";  // 或者抛出异常，根据需要处理无效编码
        }
    }

}

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。