using Microsoft.Playwright;




namespace FMO.IO.Trustee;

public class BrowserContextInfo : IDisposable
{
    IPlaywright? playwright;
    IBrowser? browser;

    public IBrowserContext Context { get; private set; }
    public bool HasCache { get; private set; }

    public BrowserContextInfo(IPlaywright playwright, IBrowser browser, IBrowserContext context, bool hascache)
    {
        this.playwright = playwright;
        this.browser = browser;
        this.Context = context;
        this.HasCache = hascache;
    }

    public async void Dispose()
    {
        if (browser is not null)
            await browser.CloseAsync();
        playwright?.Dispose();
    }
}
