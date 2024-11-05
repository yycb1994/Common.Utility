using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Utility.Extensions
{
    public  static class ConvertExpand
    {
        /// <summary>
        /// 将包含整数的字符串按指定字符分隔并转换为整数列表。
        /// </summary>
        /// <param name="ids">包含整数的字符串</param>
        /// <param name="splitStr">分隔字符</param>
        /// <returns>整数列表</returns>
        public static List<int> SplitAndConvertToIntList(this string ids, char splitStr)
        {
            try
            {
                return ids.Split(splitStr).Select(ToInt).ToList();
            }
            catch (Exception ex)
            {

                throw new Exception($"输入的{ids}中存在无法转换int的值，参照：{ex.Message}");
            }
        }

        /// <summary>
        /// 将包含字符串的字符串按指定字符分隔并转换为字符串列表。
        /// </summary>
        /// <param name="ids">包含字符串的字符串</param>
        /// <param name="splitStr">分隔字符</param>
        /// <returns>字符串列表</returns>
        public static List<string> SplitAndConvertToStringList(this string ids, char splitStr)
        {
            return ids.Split(splitStr).ToList();
        }

        /// <summary>
        /// 将 List<string> 转换为 List<T>。
        /// </summary>
        /// <typeparam name="T">目标类型，必须实现 IConvertible。</typeparam>
        /// <param name="stringList">要转换的字符串列表。</param>
        /// <returns>转换后的 List<T>。</returns>
        /// <exception cref="FormatException">当字符串无法转换为目标类型时抛出。</exception>
        /// <exception cref="InvalidCastException">当目标类型不支持转换时抛出。</exception>
        public static List<T> ConvertList<T>(this IList<string> stringList) where T : IConvertible
        {
            var resultList = new List<T>();
            foreach (var str in stringList)
            {
                try
                {
                    T value = (T)Convert.ChangeType(str, typeof(T));
                    resultList.Add(value);
                }
                catch (Exception ex) when (ex is FormatException || ex is InvalidCastException)
                {
                    Console.WriteLine($"无法转换 '{str}' 为类型 {typeof(T).Name}: {ex.Message}");
                }
            }
            return resultList;
        }

        public static DateTime ToDateTime(this object obj)
        {
            if (obj != null && DateTime.TryParse(obj.ToString(), out var dt))
            {
                return dt;
            }
            throw new ArgumentException($"对象{obj}不是有效的时间。");

        }

        /// <summary>
        /// 将字符串转换为双精度浮点数的扩展方法。
        /// </summary>
        public static double ToDouble(this object obj)
        {
            if (obj != null)
            {
                return obj.ToString().ToDouble();
            }
            throw new ArgumentException($"对象{obj}不是有效的数字。");
        }
        /// <summary>
        /// 将字符串转换为整数的扩展方法。
        /// </summary>
        public static int ToInt(this string input)
        {
            if (int.TryParse(input, out var result))
            {
                return result;
            }
            throw new ArgumentException($"输入字符串{input}不是有效的数字。");
        }

        /// <summary>
        /// 将字符串转换为双精度浮点数的扩展方法。
        /// </summary>
        public static double ToDouble(this string input)
        {
            if (double.TryParse(input, out var result))
            {
                return result;
            }
            throw new ArgumentException($"输入字符串{input}不是有效的数字。");
        }



        /// <summary>
        /// 将字符串转换为double类型，并四舍五入到指定的小数位数，返回带有百分号的值。
        /// </summary>
        /// <param name="input">要转换的字符串。</param>
        /// <param name="decimalPlaces">要保留的小数位数。</param>
        /// <returns>带有百分号的四舍五入后的值。</returns>
        /// <exception cref="ArgumentException">如果输入字符串无法转换为有效的数字，则抛出异常。</exception>
        public static string ToRoundedPercentage(this string input, int decimalPlaces)
        {
            if (double.TryParse(input, out var number))
            {
                double roundedNumber = Math.Round(number, decimalPlaces);
                string formattedPercentage = (roundedNumber * 100).ToString($"F{decimalPlaces}") + "%";
                return formattedPercentage;
            }
            throw new ArgumentException($"输入字符串{input}不是有效的数字。");
        }

        /// <summary>
        /// 将字符串转换为单精度浮点数的扩展方法。
        /// </summary>
        public static float ToFloat(this string input)
        {
            if (float.TryParse(input, out var result))
            {
                return result;
            }
            throw new ArgumentException($"输入字符串{input}不是有效的数字。");
        }

        /// <summary>
        /// 将字符串转换为长整型的扩展方法。
        /// </summary>
        public static long ToLong(this string input)
        {
            if (long.TryParse(input, out var result))
            {
                return result;
            }
            throw new ArgumentException($"输入字符串{input}不是有效的数字。");
        }

        /// <summary>
        /// 将字符串转换为十进制数的扩展方法。
        /// </summary>
        public static decimal ToDecimal(this string input)
        {
            if (decimal.TryParse(input, out var result))
            {
                return result;
            }
            throw new ArgumentException($"输入字符串{input}不是有效的数字。");
        }


        /// <summary>
        /// 将字符串转换为double类型，并四舍五入到指定的小数位数。
        /// </summary>
        /// <param name="input">需要转换的字符串。</param>
        /// <param name="digits">四舍五入到的小数位数。</param>
        /// <param name="calculatePercentage">是否计算百分比。</param>
        /// <returns>转换并四舍五入后的double值。</returns>
        /// <exception cref="ArgumentException">如果输入字符串不是一个有效的double值，则抛出此异常。</exception>
        public static object ToDoubleAndRound(this string input, int digits, bool calculatePercentage = false)
        {
            if (double.TryParse(input, out double number))
            {
                if (calculatePercentage)
                {
                    return Math.Round(number, digits) * 100 + "%";
                }
                return Math.Round(number, digits);
            }
            else
            {
                throw new ArgumentException($"输入字符串{input}不是有效的数字。");
            }
        }

        /// <summary>
        /// 将字符串转换为指定日期时间格式的字符串。
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <param name="dataTimeFormat">日期时间格式字符串</param>
        /// <returns>转换后的日期时间字符串，如果转换失败则返回 null</returns>
        public static string ConvertDataTime(this string str, string dataTimeFormat)
        {
            return DateTime.TryParse(str, out var dt) ? dt.ToString(dataTimeFormat) : null;
        }
        /// <summary>
        /// 将 DateTime 对象转换为指定格式的字符串。
        /// </summary>
        /// <param name="dt">DateTime 对象</param>
        /// <param name="format">日期时间格式字符串</param>
        /// <returns>转换后的日期时间字符串</returns>
        public static string DataTimeConvertString(this DateTime dt, string format)
        {
            return dt.ToString(format);
        }

        /// <summary>
        /// 将字符串转换为日期时间。
        /// </summary>
        /// <param name="str">要转换的字符串</param>
        /// <returns>转换后的日期时间，如果转换失败则返回 null</returns>
        public static DateTime? ConvertDataTime(this string str)
        {
            if (DateTime.TryParse(str, out var dt))
            {
                return dt;
            }
            return null;
        }
    }
}
