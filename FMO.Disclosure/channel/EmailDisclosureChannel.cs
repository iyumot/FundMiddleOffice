namespace FMO.Disclosure;

public class EmailDisclosureChannel : IDisclosureChannel
{
    public string Code => "email";

    public Task<DisclosureResult> Disclosre(IDisclosureReport report, IDisclosureChannelConfig config)
    {
        throw new NotImplementedException();
    }

    public DisclosureResult VerifyReport(IDisclosureReport report)
    {
        throw new NotImplementedException();
    }
}