using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Utility.Extensions
{
    public static class ListExpand
    {
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
