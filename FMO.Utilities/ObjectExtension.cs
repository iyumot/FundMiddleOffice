using System.Text.RegularExpressions;

namespace FMO.Utilities;

public static class ObjectExtension
{
    public static string PrintProperties(this object obj)
    {
        if (obj is null) return "Null Object";

        var type = obj.GetType();
        string info = string.Join(",\n\t", type.GetProperties().Select(x => $"{x.Name}:{x.GetValue(obj)?.ToString() ?? "null"}"));
        return $"{type.Name} \n\t {info} \n\n";
    }


    public static void UpdateFrom(this object des, object src)
    {
        if (src is null) return;

        var psd = des.GetType().GetProperties();
        var pss = src.GetType().GetProperties();

        foreach (var p in psd)
        {
            if (!p.CanWrite) continue;

            //是否有同名属性
            var ps = pss.FirstOrDefault(x => x.Name == p.Name && x.CanRead);
            if (ps is null) continue;

            p.SetValue(des, ps.GetValue(src));
        }
    }

    public static IEnumerable<T> Gather<T>(this IEnumerable<T> obj, IEnumerable<int> idx)
    {
        var sort = idx.Order().ToArray();
        foreach (var t in obj.Index())
            if (Array.BinarySearch(sort, t.Index) >= 0)
                yield return t.Item;
    }

    public static IEnumerable<T> TakeRevese<T>(this IEnumerable<T> obj, int[] idx)
    {
        var sort = idx.Order().ToArray();
        foreach (var t in obj.Reverse().Index())
            if (Array.BinarySearch(sort, t.Index) >= 0)
                yield return t.Item;
    }
}



public static class StringHelper
{
    private static readonly Regex BasicEmailRegex = new Regex(
        @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);


    public static bool IsMail(this string? mail )
    {
        if (mail is null) return false;
        return BasicEmailRegex.IsMatch(mail);
    }
}
