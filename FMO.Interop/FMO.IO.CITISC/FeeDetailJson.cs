using FMO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMO.IO.Trustee.CITISC.Json.FeeDetail;


#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
public class Data
{
    public int? startRow { get; set; }
    public List<int?> navigatepageNums { get; set; }
    public int? prePage { get; set; }
    public bool? hasNextPage { get; set; }
    public int? nextPage { get; set; }
    public int? pageSize { get; set; }
    public int? endRow { get; set; }
    public List<Item> list { get; set; }
    public int? pageNum { get; set; }
    public int? navigatePages { get; set; }
    public int? total { get; set; }
    public int? navigateFirstPage { get; set; }
    public int? pages { get; set; }
    public int? size { get; set; }
    public bool? isLastPage { get; set; }
    public bool? hasPreviousPage { get; set; }
    public int? navigateLastPage { get; set; }
    public bool? isFirstPage { get; set; }
}

public class Item
{
    public object zftgf { get; set; }
    public string zcjz { get; set; }
    public object jttzgwf { get; set; }
    public string jttgf { get; set; }
    public object endDate { get; set; }
    public object jtxsfwf { get; set; }
    public string wzfzzs { get; set; }
    public string wzfzzsfjs { get; set; }
    public string ftzh { get; set; }
    public string dwjz { get; set; }
    public object glftgfwbfhz { get; set; }
    public object jtzzsfjs { get; set; }
    public string fcpdm { get; set; }
    public object zfglf { get; set; }
    public object wzfxsfwf { get; set; }
    public object wzfsjf { get; set; }
    public string jtglf { get; set; }
    public string wzffjs { get; set; }
    public object ccpdyzcpdm { get; set; }
    public object id { get; set; }
    public object zfxzfwf { get; set; }
    public string jtyjbc { get; set; }
    public object wzftzgwf { get; set; }
    public object zfsjf { get; set; }
    public string fcpmc { get; set; }
    public object jtzzs { get; set; }
    public object zftzgwf { get; set; }
    public string wzftgf { get; set; }
    public string wzfglf { get; set; }
    public object jtsjf { get; set; }
    public object jtfjs { get; set; }
    public string zfzzsfjs { get; set; }
    public string ljdwjz { get; set; }
    public string cdate { get; set; }
    public string jtxzfwf { get; set; }
    public string wzfxzfwf { get; set; }
    public object zfxsfwf { get; set; }
    public object zfyjbc { get; set; }
    public string ffjdm { get; set; }
    public string wzfyjbc { get; set; }
    public string fe { get; set; }
    public object total3fee { get; set; }

    public ManageFeeDetail ToFeeDetail()
    {
        return new ManageFeeDetail(DateOnly.Parse(cdate), decimal.Parse(jtglf));
    }
}

public class Root
{
    public int? code { get; set; }
    public Data data { get; set; }
    public bool? success { get; set; }
    public string message { get; set; }
}


#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。