using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Utility.Extensions
{
    public static class StringExpand
    {
        /// <summary>
        /// 将指定的字符串追加到当前字符串中，不添加任何分隔符。
        /// </summary>
        /// <param name="str">当前字符串。</param>
        /// <param name="parms">要追加的字符串。</param>
        /// <returns>将指定的字符串追加到当前字符串后的新字符串。</returns>
        public static string AppendString(this object str, params string[] parms)
        {
            StringBuilder builder = new StringBuilder(str.ToString());

            foreach (string s in parms)
            {
                builder.Append(s);
            }

            return builder.ToString();
        }

        /// <summary>
        /// 当字符串为空时，设置字符串的默认值。
        /// </summary>
        /// <param name="str">要检查的字符串。</param>
        /// <param name="value">默认值。</param>
        /// <returns>如果字符串为空，则返回默认值；否则返回原字符串。</returns>
        public static string SetDefaultValue(this string str, string value)
        {
            return string.IsNullOrWhiteSpace(str) ? value : str;
        }

        #region 字符串截取
        /// <summary>
        /// 从当前字符串中提取指定起始和结束子字符串之间的内容，包括起始和结束子字符串。
        /// </summary>
        /// <param name="input">要处理的字符串。</param>
        /// <param name="start">起始子字符串。</param>
        /// <param name="end">结束子字符串。</param>
        /// <returns>包含起始和结束子字符串之间的内容，如果未找到则返回 null。</returns>
        /// <exception cref="ArgumentNullException">如果输入字符串、起始子字符串或结束子字符串为 null。</exception>
        public static string ExtractBetween(this string input, string start, string end)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (start == null) throw new ArgumentNullException(nameof(start));
            if (end == null) throw new ArgumentNullException(nameof(end));

            int startIndex = input.IndexOf(start);
            int endIndex = input.IndexOf(end);

            // 确保找到的索引有效
            if (startIndex != -1 && endIndex != -1 && startIndex < endIndex)
            {
                // 计算包含 start 和 end 的子字符串
                return input.Substring(startIndex, endIndex + end.Length - startIndex);
            }

            return null; // 未找到合适的子字符串
        }


        #endregion

        /// <summary>
        /// 将中文全名分割为姓氏和名字。
        /// </summary>
        /// <param name="fullname">中文全名。</param>
        /// <param name="doubleSurnames">常见的双字姓氏。</param>
        /// <returns>返回一个元组，包含姓氏和名字。</returns>
        /// <exception cref="ArgumentException">如果输入的姓名为空或格式不正确，则抛出异常。</exception>
        /// <remarks>
        /// 此方法首先检查姓名是否为空或长度不足，然后尝试识别并分离出双字姓氏。
        /// 如果姓名不符合预期格式，会抛出一个ArgumentException。
        /// 注意：这个方法假设所有的双字姓氏都已经包含在内部HashSet中。
        /// </remarks>
        public static (string Surname, string Name) SplitName(this string fullname, HashSet<string> doubleSurnames = null)
        {
            if (doubleSurnames == null || doubleSurnames.Count == 0)
            {
                // 定义一个HashSet，包含一些常见的双字姓氏
                doubleSurnames = new HashSet<string>
             {
                 "司马", "欧阳", "上官", "端木", "诸葛", "东方", "独孤", "南宫", "万俟", "闻人", "夏侯"
             };
            }

            // 检查输入的全名是否为空或者长度小于2，如果是，则抛出一个异常
            if (string.IsNullOrWhiteSpace(fullname) || fullname.Length < 2)
            {
                throw new ArgumentException("姓名格式不正确。");
            }

            // 检查全名的前两个字符是否在双字姓氏的HashSet中，如果是，则认为是双字姓氏
            if (fullname.Length >= 2 && doubleSurnames.Contains(fullname.Substring(0, 2)))
            {
                // 如果是双字姓氏，返回姓氏和名字
                return (fullname.Substring(0, 2), fullname.Substring(2));
            }
            else
            {
                // 如果不是双字姓氏，将第一个字符作为姓氏，其余的作为名字
                return (fullname.Substring(0, 1), fullname.Substring(1));
            }
        }


        /// <summary>
        /// 将字符串集合拆分为 SQL IN 条件字符串。
        /// </summary>
        /// <param name="list">要拆分的字符串集合。</param>
        /// <param name="appendStr">拆分单元后追加的字符。</param>
        /// <returns>SQL IN 条件字符串。</returns>
        public static string ToSqlInCondition(this IEnumerable<string> list, string appendStr = "")
        {
            var sqlInCondition = "";
            list.ToList().ForEach(item =>
            {
                sqlInCondition += "'" + (string.IsNullOrEmpty(appendStr) ? item : appendStr + item) + "',";
            });
            return string.IsNullOrEmpty(sqlInCondition) ? null : sqlInCondition.Substring(0, sqlInCondition.Length - 1);
        }

        /// <summary>
        /// 将字符串集合按照分隔符拼接为字符串。推荐使用 string.Join()。
        /// </summary>
        /// <param name="list">要拼接的字符串集合。</param>
        /// <param name="delimiter">拼接的分隔符。</param>
        /// <returns>拼接后的字符串，或在集合为空时返回 null。</returns>
        public static string JoinWithDelimiter(this IEnumerable<string> list, string delimiter = "")
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (string.IsNullOrEmpty(delimiter))
                return string.Join("", list); // 使用内置的 string.Join()

            var stringBuilder = new StringBuilder();
            foreach (var item in list)
            {
                if (item != null) // 处理可能的 null 值
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append(delimiter);
                    }
                    stringBuilder.Append(item);
                }
            }

            return stringBuilder.Length > 0 ? stringBuilder.ToString() : null;
        }
    }
}
