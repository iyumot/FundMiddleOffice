using System.Text.RegularExpressions;
using FMO.Models;
using MiniExcelLibs;
using MiniSoftware;
using Serilog;

namespace FMO.TPL;

public static class Tpl
{
    public static bool IsExists(string name) => File.Exists(@$"files\tpl\{name}");


    public static string GetPath(string name) => @$"files\tpl\{name}";



    public static bool Generate(string path, string tpl, object obj)
    {
        try
        {
            var di = new FileInfo(path).Directory;
            if (di is not null && !di.Exists) di.Create();

            if (Regex.IsMatch(tpl, @"\.doc|\.docx", RegexOptions.IgnoreCase))
            {
                MiniWord.SaveAsByTemplate(path, tpl, obj);
                return true;
            }
            if (Regex.IsMatch(tpl, @"\.xls|\.xlsx", RegexOptions.IgnoreCase))
            {
                MiniExcel.SaveAsByTemplate(path, tpl, obj);
                return true;
            }
            return false;
        }
        catch {  return false; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <param name="tplName">模板文件名</param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool GenerateByPredefined(string path, string tplName, object obj)
    {
        if (string.IsNullOrWhiteSpace(tplName)) return false;
        if (!IsExists(tplName)) return false;

        try { Generate(path, GetPath(tplName), obj); return true; } catch { return false; }
    }


    public static bool GenerateRegisterAnounce(Fund fund, string path)
    {
        // 获取模板
        var fi = new FileInfo(@"files\tpl\备案承诺函.docx");
        if (!fi.Exists)
        {
            Log.Error("生成备案承诺函失败：未找到模板");
            return false;
        }

        try
        {
            MiniWord.SaveAsByTemplate(path, fi.FullName, new Dictionary<string, object> { { "Name", fund.Name }, { "Code", fund.Code! } });
            return true;
        }
        catch (Exception e)
        {
            Log.Error($"生成备案承诺函失败：{e.Message}");
            return false;
        }
    }
}
