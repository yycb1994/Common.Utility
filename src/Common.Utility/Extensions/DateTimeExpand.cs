using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Utility.Extensions
{
    public static class DateTimeExpand
    {
        /// <summary>
        /// 判断指定时间与当前时间的差是否超过给定的秒数。
        /// </summary>
        /// <param name="targetTime">要比较的目标时间。</param>
        /// <param name="totalSeconds">要比较的秒数。</param>
        /// <returns>如果时间差超过指定的秒数，则返回 true；否则返回 false。</returns>
        public static bool IsTimeDifferenceExceeds(this DateTime targetTime, int totalSeconds)
        {
            // 计算时间差
            TimeSpan difference = DateTime.Now - targetTime;

            // 判断时间差是否超过指定的秒数
            return Math.Abs(difference.TotalSeconds) > totalSeconds;
        }

        /// <summary>
        /// 判断指定时间是否在工作日（周一至周五）。
        /// </summary>
        /// <param name="dateTime">要检查的时间。</param>
        /// <returns>如果是工作日返回 true；否则返回 false。</returns>
        public static bool IsWeekday(this DateTime dateTime)
        {
            return dateTime.DayOfWeek != DayOfWeek.Saturday && dateTime.DayOfWeek != DayOfWeek.Sunday;
        }
    }
}
