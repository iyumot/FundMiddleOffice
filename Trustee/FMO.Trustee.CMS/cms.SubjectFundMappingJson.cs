using FMO.Models;
using System.Text.Json.Serialization;


namespace FMO.Trustee;


#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��

internal class SubjectFundMappingJson : JsonBase
{
    [JsonPropertyName("gradeNo")]
    public string GradeNo { get; set; }

    [JsonPropertyName("gradeName")]
    public string GradeName { get; set; }

    [JsonPropertyName("productNo")]
    public string ProductNo { get; set; }

    [JsonPropertyName("productName")]
    public string ProductName { get; set; }

    [JsonPropertyName("isGrade")]
    public string IsGrade { get; set; }

    [JsonPropertyName("registerNo")]
    public string RegisterNo { get; set; }

    public SubjectFundMapping ToObject()
    {
        var sc = IsGrade == "1" ? GradeName.Replace(ProductName, "") : "";
        return new SubjectFundMapping { FundCode = GradeNo, FundName = GradeName, MasterCode = ProductNo, MasterName = ProductName, ShareClass = sc };
    }
}



#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��