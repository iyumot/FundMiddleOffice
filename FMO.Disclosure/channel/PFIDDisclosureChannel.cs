namespace FMO.Disclosure;

internal class PFIDDisclosureChannel : IDisclosureChannel
{
    public string Code => "pfid";

    public Task<DisclosureResult> Disclosre(IDisclosureReport report, IDisclosureChannelConfig config)
    {
        throw new NotImplementedException();
    }

    public DisclosureResult VerifyReport(IDisclosureReport report)
    {
        throw new NotImplementedException();
    }
}