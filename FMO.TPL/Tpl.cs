using MiniExcelLibs;
using MiniSoftware;
using System.Text.RegularExpressions;

namespace FMO.TPL;

public static class Tpl
{
    public static bool IsExists(string name) => File.Exists(@$"files\tpl\{name}");


    public static string GetPath(string name) => @$"files\tpl\{name}";



    public static void Generate(string path, string tpl, object obj)
    {
        if(Regex.IsMatch(tpl, @"\.doc|\.docx", RegexOptions.IgnoreCase)) 
            MiniWord.SaveAsByTemplate(path, tpl, obj);
        if (Regex.IsMatch(tpl, @"\.xls|\.xlsx", RegexOptions.IgnoreCase))
            MiniExcel.SaveAsByTemplate(path, tpl, obj);
    }

}
