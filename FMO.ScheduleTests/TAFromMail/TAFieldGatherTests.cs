using Microsoft.VisualStudio.TestTools.UnitTesting;
using FMO.Schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FMO.Models;

namespace FMO.Schedule.Tests;

[TestClass()]
public class TAFieldGatherTests
{
    [TestMethod()]
    public void CheckFieldTest()
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);


        string[] head = ["产品代码",   "产品名称", "客户名称",    "业务类型", "申请日期",    "确认日期", "确认金额",    "确认净额", "确认份额",    "确认结果", "单位净值",    "累计净值", "手续费", "业绩报酬",
         "归管理人费用", "归基金资产费用", "归销售机构费用", "手续费折扣率", "确认总金额（含费）",	"申请金额", "申请份额",    "剩余份额", "销售机构代码",  "销售机构名称", "直销渠道名称",
         "基金账号",   "交易账号", "证件类型",    "证件号码", "客户类型",    "分红方式", "返回信息",    "账户利息", "主产品名称",   "主产品代码", "申请单号",    "确认流水号",];

        using var fs = new FileStream(@"C:\Users\lenovo\Downloads\历史交易确认明细.xlsx", FileMode.Open);
       var reader =  ExcelDataReader.ExcelReaderFactory.CreateReader(fs);

        reader.Read();
        object[] values = new object[reader.FieldCount];
        reader.GetValues(values);


        TAFieldGather g = new();
        g.CheckField(values.Select(x=>x.ToString()).ToArray());

        List<TransferRecord> records = new List<TransferRecord>();
        for (int i = 0; i < reader.RowCount - 1; i++)
        {
            reader.Read();
            values = new object[reader.FieldCount];
            reader.GetValues(values);

            var r = new TransferRecord { CustomerIdentity = null, CustomerName = null};

            g.Fill(r, values);
            records.Add(r);
        }


    }
}