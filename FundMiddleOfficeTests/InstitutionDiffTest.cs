using FMO.Models;
using System.Diagnostics;
using System.Reflection;

namespace FMO.Tests;



[TestClass()]
public class InstitutionDiffTest
{
    [TestMethod]
    public void MyTestMethod()
    {
        var ins = typeof(Institution);
        var org = typeof(Organization);

        var pro1 = ins.GetProperties();
        var pro2 = org.GetProperties();

        List<PropertyInfo> pi = new();
        List<PropertyInfo> po = new();

        var same = pro1.IntersectBy(pro2.Select(x => x.Name), x => x.Name).OrderBy(x=>x.Name);
        var dif = pro1.ExceptBy(pro2.Select(x => x.Name), x => x.Name).OrderBy(x => x.Name);
        pi.AddRange(same);
        pi.AddRange(dif);

        same = pro2.IntersectBy(pro1.Select(x => x.Name), x => x.Name).OrderBy(x => x.Name);
        dif = pro2.ExceptBy(pro1.Select(x => x.Name), x => x.Name).OrderBy(x => x.Name);
        po.AddRange(same);
        po.AddRange(dif);

        for (int i = 0; i < Math.Max(pi.Count, po.Count); i++)
        {
            if (pi.Count > i)
                Debug.Write($"{pi[i].PropertyType.Name} {pi[i].Name}");


            Debug.Write("   ");

            if (po.Count > i)
                Debug.WriteLine($"{po[i].PropertyType.Name} {po[i].Name}");
        }
    }
}