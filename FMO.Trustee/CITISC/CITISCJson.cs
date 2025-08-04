using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace FMO.Trustee.JsonCITICS;

#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��

public class RootJson
{
    [JsonPropertyName("data")]
    public JsonNode? Data { get; set; }

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

    [JsonPropertyName("total")]
    public int Total { get; set; }

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


public class InvestorAccountMapping
{
    //FundAccount
    public string Id { get; set; }

    public string Name { get; set; }

    public string Indentity { get; set; }
}

#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��