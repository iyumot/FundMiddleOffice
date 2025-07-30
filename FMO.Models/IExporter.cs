namespace FMO.TPL;


public class ExportInfo
{
    public ExportInfo()
    {
    }

    public ExportInfo(string v)
    {
        FailedReason = v;
    }



    public string? FileName { get; set; }

    public string Filter { get; set; } = "文件|*.*";

    public object? Data { get; set; }


    public string? FailedReason { get; set; }
}

[Flags]
public enum ExportTypeFlag
{
    None,

    /// <summary>
    /// 单一基金净值列表
    /// 需要支持以下属性 
    /// 集合名为dy
    /// - Date: 日期
    /// - NetValue: 单位净值
    /// - CumNetValue: 累计净值
    /// - Asset: 资产净值
    /// - NetAsset: 净资产
    /// </summary>
    SingleFundNetValueList,








    Custom = 99999
}

public interface IExporter
{

    string Id { get; }

    string Name { get;  }

    string Description { get; }

    ExportTypeFlag Suit { get; }

    ExportInfo Generate(object? parameter = null);
}

public record TemplateInfo(string Id, string Name, string Description, string Type,ExportTypeFlag Suit, string Path);
