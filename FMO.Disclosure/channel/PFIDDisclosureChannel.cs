namespace FMO.Disclosure;

internal class PFIDDisclosureChannel : IDisclosureChannel
{
    public string Code => DisclosureChannelCode.Pfid;

    public Task<DisclosureResult> Disclosure(IDisclosureNotice Notice, IDisclosureChannelConfig config)
    {
        throw new NotImplementedException();
    }

    public DisclosureResult VerifyNotice(IDisclosureNotice Notice)
    {
        throw new NotImplementedException();
    }
}