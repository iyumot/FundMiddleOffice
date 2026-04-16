namespace FMO.Disclosure;


/// <summary>
/// 信批配置
/// </summary>
public class DisclosureWorkflow
{
    public int Id { get; set; }

    // 信批类型
    public DisclosureType Type { get; set; }

    /// <summary>
    /// 适用全部产品
    /// </summary>
    public bool ForAllFunds { get; set; }

    /// <summary>
    /// 适用的基金ID列表，仅当ForAllFunds为false时有效
    /// </summary>
    public int[] TargetFunds { get; set; } = [];


    public required string Channel { get; set; }

    // public required IDisclosureChannelConfig Config { get; set; }

    public IWorkConfig? Config { get; set; }
}

public enum DisclosureType
{
    Monthly,
    Quarterly,
    SemiAnnually,
    Annually,

}



/// <summary>
/// 信批通道
/// </summary>
public interface IDisclosureChannel
{
    public string Code { get; }

    public DisclosureResult VerifyReport(IDisclosureReport report);

    public Task<DisclosureResult> Disclosre(IDisclosureReport report, IDisclosureChannelConfig config);


}

public interface IDisclosureChannelConfig
{
    public string Channel { get; set; }
}

public class DisclosureResult
{
    public bool Successed { get; set; }

    public string? Error { get; set; }
}


public static class DisclosureChannelGalley
{
    private static readonly Dictionary<string, IDisclosureChannel> _channels = new();

    public static bool Register(IDisclosureChannel channelInstance)
    {
        // 注册通道实例，可以存储在一个字典中
        // 这里可以使用反射或者工厂模式来实现
        if (_channels.ContainsKey(channelInstance.Code))
            return false; // 已经注册过了 

        _channels[channelInstance.Code] = channelInstance;
        return true;
    }

    public static bool Unregister(string channel) => _channels.Remove(channel);

    public static IEnumerable<string> GetRegisteredChannels() => _channels.Keys;

    public static bool IsChannelRegistered(string channel) => _channels.ContainsKey(channel);

    public static void Initialize()
    {
        // 初始化默认的通道实例
        Register(new EmailDisclosureChannel());
        Register(new PFIDDisclosureChannel());
        Register(new MeiShiDisclosureChannel());
    }


    public static IDisclosureChannel? GetChannel(string channel) => _channels.TryGetValue(channel, out var channelInstance) ? channelInstance : null;




}



public class DisclosureInstanse
{
    public int WorkflowId { get; set; }

    public int ReportId { get; set; }

    public int FundId { get; set; }

    public DisclosureType Type { get; set; }

    public DisclosureResult? VerifyResult { get; set; }

    public DisclosureResult? WorkResult { get; set; }


}



//帮我完善一下，我要做一下信批配置
// 1. 信批类型：月报、季报、半年报、年报，月报、季报、半年报都有ecxcel和pdf两种格式，年报另外还有用印pdf格式
// 2. 适用范围：全部产品，还是指定基金列表
// 3. 信批通道：平台A、平台B等，每个平台的配置项可能不同
// 4. 做一个配置UI，用户可以选择信批类型、适用范围和信批通道，并填写相应的配置项
// 5. 通道例子：
//    - 邮件通道：需要配置SMTP服务器地址、端口、发件人邮箱
//    - pfid：
//    - 其他通道：根据实际情况设计配置项