using MiniExcelLibs;
using MiniSoftware;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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



public static class ExcelTpl
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path">目标路径</param>
    /// <param name="tpl">模板文件名</param>
    /// <param name="values"></param>
    /// <returns></returns>
    public static bool GenerateFromTemplate(string path, string tpl, object values)
    {
        // 获取模板
        var fi = new FileInfo(@$"files\tpl\{tpl}");
        if (!fi.Exists)
        {
            Log.Error("生成失败：未找到模板");
            return false;
        }

        try
        {
            MiniExcel.SaveAsByTemplate(path, fi.FullName, values);

            return true;
        }
        catch (Exception e)
        {
            Log.Error($"生成失败：{e.Message}");
            return false;
        }
    }




}
