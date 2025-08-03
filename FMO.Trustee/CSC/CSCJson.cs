using System.Text.Json.Serialization;

namespace FMO.Trustee.JsonCSC;


#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��

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







#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��