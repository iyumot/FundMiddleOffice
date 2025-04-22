using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FMO.Utilities;

public class NumberHelper
{
    public static string NumberToChinese(decimal number)
    {
        string[] numChinese = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
        string[] unitChinese = { "", "十", "百", "千" };
        string[] bigUnitChinese = { "", "万", "亿", "兆" };

        string integerPart = Math.Truncate(number).ToString();
        string decimalPart = ((number - Math.Truncate(number)) * 100).ToString("00");

        string result = "";
        int groupCount = (integerPart.Length + 3) / 4;
        for (int i = 0; i < groupCount; i++)
        {
            int startIndex = Math.Max(0, integerPart.Length - (i + 1) * 4);
            int length = Math.Min(4, integerPart.Length - startIndex);
            string group = integerPart.Substring(startIndex, length);

            string groupResult = "";
            bool zeroFlag = false;
            for (int j = 0; j < group.Length; j++)
            {
                int digit = int.Parse(group[j].ToString());
                if (digit == 0)
                {
                    zeroFlag = true;
                }
                else
                {
                    if (zeroFlag)
                    {
                        groupResult += "零";
                        zeroFlag = false;
                    }
                    groupResult += numChinese[digit] + unitChinese[group.Length - j - 1];
                }
            }
            if (groupResult.EndsWith("零"))
            {
                groupResult = groupResult.TrimEnd('零');
            }
            if (!string.IsNullOrEmpty(groupResult))
            {
                groupResult += bigUnitChinese[i];
            }
            result = groupResult + result;
        }
        if (string.IsNullOrEmpty(result))
        {
            result = "零";
        }
        if (decimalPart != "00")
        {
            result += "点";
            for (int i = 0; i < decimalPart.Length; i++)
            {
                int digit = int.Parse(decimalPart[i].ToString());
                result += numChinese[digit];
            }
        }
        return result;
    }
}