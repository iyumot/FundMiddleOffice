using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMO.IO.DS.MeiShi.Json.QualificationJson;



#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
public class ListItem
{
    /// <summary>
    /// 
    /// </summary>
    public int identifyFlowId { get; set; }

    /// <summary>
    /// 许志聪
    /// </summary>
    public string customerName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int customerType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string managerName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int investorType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int? riskType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int identifyWay { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int flowStatus { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int codeValue { get; set; }

    /// <summary>
    /// 认定完成
    /// </summary>
    public string codeText { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string commitTime { get; set; }

    /// <summary>
    /// 客户
    /// </summary>
    public string commitPerson { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int checkFlowStatus { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int checkStatus { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string auditTime { get; set; }

    /// <summary>
    /// 系统管理员
    /// </summary>
    public string auditPerson { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string startTime { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string identifyEndTime { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string endTime { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string cardNumber { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int isDelete { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string identifyTime { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int customerId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int customerLevel { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int identifyStatus { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string identifyLimitDate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int isVIP { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int? aptnessType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int? channelType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int? signType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string flowUpdateTime { get; set; }

}



public class Data
{
    /// <summary>
    /// 
    /// </summary>
    public List<ListItem> list { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int total { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int pageNum { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int pageSize { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int pages { get; set; }

}



public class Root
{
    /// <summary>
    /// 
    /// </summary>
    public int code { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Data data { get; set; }

    /// <summary>
    /// 请求成功
    /// </summary>
    public string message { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int total { get; set; }

}



#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。