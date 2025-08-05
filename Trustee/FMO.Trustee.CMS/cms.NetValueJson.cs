using FMO.Models;
using System.Text.Json.Serialization;

namespace FMO.Trustee;


#pragma warning disable CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��
internal class NetValueJson : JsonBase
{

    /// <summary>
    /// ��Ʒ���룬��󳤶� 6
    /// </summary>
    [JsonPropertyName("fundCode")]
    public string FundCode { get; set; } // String(6)

    /// <summary>
    /// ��Ʒ���ƣ���󳤶� 300
    /// </summary>
    [JsonPropertyName("fundName")]
    public string FundName { get; set; } // String(300)

    /// <summary>
    /// ��ֵ���ڣ���ʽ��yyyymmdd����󳤶� 8
    /// </summary>
    [JsonPropertyName("navDate")]
    public string NavDate { get; set; } // String(8)����ʽ��yyyyMMdd

    /// <summary>
    /// �ʲ��ݶ������λС��
    /// </summary>
    [JsonPropertyName("assetVol")]
    public string AssetVol { get; set; } // ������λС��

    /// <summary>
    /// �ʲ���ֵ��������λС��
    /// </summary>
    [JsonPropertyName("assetNav")]
    public string AssetNav { get; set; } // ������λС��

    /// <summary>
    /// �ʲ���ֵ��������λС��
    /// </summary>
    [JsonPropertyName("totalAsset")]
    public string TotalAsset { get; set; } // ������λС��

    /// <summary>
    /// ��λ��ֵ��������λС���������ձ���8λ��
    /// ע�⣺ʵ�ʴ���ʱ������Ƿ����̶�̬����С��λ��
    /// </summary>
    [JsonPropertyName("nav")]
    public string Nav { get; set; } // ͨ��������λ������ʱ8λ

    /// <summary>
    /// �ۼƵ�λ��ֵ��������λС���������ձ���8λ��
    /// ע�⣺ʵ�ʴ���ʱ������Ƿ����̶�̬����С��λ��
    /// </summary>
    [JsonPropertyName("accumulativeNav")]
    public string AccumulativeNav { get; set; } // ͨ��������λ������ʱ8λ

    /// <summary>
    /// Ԥ���ֶ�1����󳤶� 500
    /// </summary>
    [JsonPropertyName("remark1")]
    public string Remark1 { get; set; } // String(500)

    /// <summary>
    /// Ԥ���ֶ�2����󳤶� 500
    /// </summary>
    [JsonPropertyName("remark2")]
    public string Remark2 { get; set; } // String(500)


    
}

#pragma warning restore CS8618 // ���˳����캯��ʱ������Ϊ null ���ֶα�������� null ֵ���뿼����� "required" ���η�������Ϊ��Ϊ null��