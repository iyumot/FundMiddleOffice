namespace FMO.Disclosure;

public class EmailDisclosureChannel : IDisclosureChannel
{
    public string Code => DisclosureChannelCode.Email;

    public Task<DisclosureResult> Disclosure(IDisclosureNotice Notice, IDisclosureChannelConfig config)
    {
        throw new NotImplementedException();
    }

    public DisclosureResult VerifyNotice(IDisclosureNotice Notice)
    {
        throw new NotImplementedException();
    }
}