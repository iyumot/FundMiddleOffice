namespace FMO.Disclosure;

internal class MeiShiDisclosureChannel : IDisclosureChannel
{
    public string Code => "meishi";

    public Task<DisclosureResult> Disclosre(IDisclosureReport report, IDisclosureChannelConfig config)
    {
        throw new NotImplementedException();
    }

    public DisclosureResult VerifyReport(IDisclosureReport report)
    {
        throw new NotImplementedException();
    }



    public IWorkConfig? Build(DisclosureType disclosureType)
    {
        switch (disclosureType)
        {
            case DisclosureType.Monthly: 
            case DisclosureType.Quarterly: 
            case DisclosureType.SemiAnnually: 
            case DisclosureType.Annually:
                return new MeiShiWorkConfig(); 
            default:
                return null;
        }
    }


}

internal class MeiShiWorkConfig : IWorkConfig
{
    /// <summary>
    /// 通知
    /// </summary>
    public bool Notify { get; set; }

    /// <summary>
    /// 用印
    /// </summary>
    public bool Seal { get; set; }

}