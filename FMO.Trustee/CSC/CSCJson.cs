using System.Text.Json.Serialization;

namespace FMO.Trustee;


#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��

internal class RetJson
{
    [JsonPropertyName("retCode")]
    public string Code { get; set; }


    [JsonPropertyName("retMsg")]
    public string Msg { get; set; }

}




internal class RetJson<T>
{
    [JsonPropertyName("retCode")]
    public string Code { get; set; }


    [JsonPropertyName("retMsg")]
    public string Msg { get; set; }

    [JsonPropertyName("data")]
    public required DataJsonWrap<T> Data { get; set; }
}

internal class DataJsonWrap<T>
{

    [JsonPropertyName("result")]
    public required List<T> Data { get; set; }

    [JsonPropertyName("rowCount")]
    public int RowCount { get; set; }


    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }
}


public interface IJson<T>
{
    T ToObject();
}







#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��