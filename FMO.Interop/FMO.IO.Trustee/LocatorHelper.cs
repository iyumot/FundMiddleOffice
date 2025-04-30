using Microsoft.Playwright;

namespace FMO.IO.Trustee;

public static class LocatorHelper
{
    /// <summary>
    /// 返回唯一可见项，多个返回null
    /// </summary>
    /// <param name="locator"></param>
    /// <returns></returns>
    public static async Task<ILocator?> SingleVisible(this ILocator locator)
    {
        ILocator? l = null;
        foreach (var item in await locator.AllAsync())
        {
            if (!await item.IsVisibleAsync())
                continue;

            if (l is not null) return null;
            l = item;
        }
        return l;
    }
}