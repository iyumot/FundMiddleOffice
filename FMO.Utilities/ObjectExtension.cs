using FMO.Models;
using System.Xml.Linq;

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

}
