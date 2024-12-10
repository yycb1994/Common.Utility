using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Utility.Extensions
{
    public static class ListExpand
    {

        public static void ShowContext<T>(this IEnumerable<T> list)
        {
            list.ToList().ForEach(item =>
            {
                Console.WriteLine(JsonConvert.SerializeObject(item));
            });
        }
        /// <summary>
        /// 检查字符串数组 b 中是否包含字符串数组 a 中的任何一个元素
        /// </summary>
        /// <param name="a">要检查的字符串数组</param>
        /// <param name="b">包含的字符串数组</param>
        /// <returns>如果 b 中包含 a 中的任何一个元素，则返回 true；否则返回 false</returns>
        ///
        /// <remarks>
        ///         string[] a = { "#", "*", "P", "L", "S" };
        ///         string[] b = { "A", "B", "C", "#", "D" };
        ///
        ///         if (IsContains(a, b))
        ///               Console.WriteLine("b 中包含 a 中的至少一个元素。");
        ///         else
        ///               Console.WriteLine("b 中不包含 a 中的任何一个元素。");
        ///
        /// </remarks>
        public static bool IsContains(this string[] a, string[] b)
        {
            foreach (string defaultChar in a)
            {
                if (b.Contains(defaultChar))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 判断索引是否在数组的有效范围内
        /// </summary>
        /// <typeparam name="T">数组元素的类型</typeparam>
        /// <param name="array">要检查的数组</param>
        /// <param name="index">要检查的索引</param>
        /// <returns>如果索引在数组的有效范围内，则返回 true；否则返回 false</returns>
        public static bool IsIndexWithinBounds<T>(this T[] array, int index)
        {
            return index >= 0 && index < array.Length;
        }

        /// <summary>
        /// 判断索引是否在集合的有效范围内
        /// </summary>
        /// <typeparam name="T">集合元素的类型</typeparam>
        /// <param name="collection">要检查的集合</param>
        /// <param name="index">要检查的索引</param>
        /// <returns>如果索引在集合的有效范围内，则返回 true；否则返回 false</returns>
        public static bool IsIndexWithinBounds<T>(this IList<T> collection, int index)
        {
            return index >= 0 && index < collection.Count;
        }
    }
}
