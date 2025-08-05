using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee.JsonCSC;


#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��

public class SubjectFundMappingJson : JsonBase
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



#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��