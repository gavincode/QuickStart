// ****************************************
// FileName:ChineseToLetter.cs
// Description:
// Tables:
// Author:Gavin
// Create Date:2015/5/25 17:45:07
// Revision History:
// ****************************************

using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;

namespace System.Text
{
    /// <summary>
    /// 中文-首字母转换
    /// </summary>
    public class ChineseToLetter
    {
        //中文编码区间
        private static Int32[] areaCodeArray = new Int32[] { 45217, 45253, 45761, 46318, 46826, 47010, 47297, 47614, 48119, 48119, 49062, 49324, 49896, 50371, 50614, 50622, 50906, 51387, 51446, 52218, 52698, 52698, 52698, 52980, 53689, 54481, 55290 };

        /// <summary>
        /// 将中文文本, 转换为中文首字母组成字符串
        /// </summary>
        /// <param name="chineseText">中文文本</param>
        /// <returns>中文首字母组成字符串</returns>
        public static String ToLetters(String chineseText)
        {
            if (String.IsNullOrEmpty(chineseText)) return chineseText;

            String letterString = String.Empty;

            foreach (var item in chineseText.ToArray())
            {
                letterString = String.Concat(letterString, ToLetter(item.ToString()));
            }

            return letterString;
        }

        /// <summary>
        /// 将单个字符字符串转换为首字母
        /// </summary>
        /// <param name="singleChar">单个字符字符串</param>
        /// <returns>首字母</returns>
        private static String ToLetter(String singleChar)
        {
            Byte[] arrCN = Encoding.Default.GetBytes(singleChar);

            //不处理单字节字符
            if (arrCN.Length < 2) return singleChar;

            Int32 area = (Int32)arrCN[0];
            Int32 pos = (Int32)arrCN[1];

            Int32 code = (area << 8) + pos;

            for (Int32 i = 0; i < 26; i++)
            {
                if (areaCodeArray[i] <= code && code < areaCodeArray[i + 1])
                {
                    return Encoding.Default.GetString(new Byte[] { (Byte)(97 + i) });
                }
            }

            return "*";
        }
    }
}
