using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;


namespace Common.Utility.Helper
{

    /// <summary>
    /// 提供系统相关的辅助方法。
    /// </summary>
    public class SystemHelper
    {
        /// <summary>
        /// 检查当前是否在 .NET Framework 环境中。
        /// </summary>
        /// <returns>如果在 .NET Framework 环境中，则为 true；否则为 false。</returns>
        public static bool IsFramework()
        {
            if (RuntimeInformation.FrameworkDescription.StartsWith(".NET Core") || RuntimeInformation.FrameworkDescription.StartsWith(".NET 9")
               || RuntimeInformation.FrameworkDescription.StartsWith(".NET 8")
               || RuntimeInformation.FrameworkDescription.StartsWith(".NET 7")
               || RuntimeInformation.FrameworkDescription.StartsWith(".NET 6")
               || RuntimeInformation.FrameworkDescription.StartsWith(".NET 5"))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取配置文件中指定键的值。
        /// </summary>
        /// <param name="key">要获取值的键。</param>
        /// <returns>键对应的值。</returns>
        public static string GetConfigValue(string key)
        {
            if (!IsFramework())
            {
                var appsettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
                IConfiguration config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile(appsettingsPath, optional: true, reloadOnChange: false)
                    .Build();
                return config.GetValue<string>(key);
            }
            return System.Configuration.ConfigurationManager.AppSettings[key];

        }


        /// <summary>
        /// 测量有返回值的方法的执行时间。
        /// </summary>
        /// <typeparam name="T">方法的返回类型。</typeparam>
        /// <param name="method">要测量的方法。</param>
        /// <returns>包含方法结果和执行时间的元组。</returns>
        public static (T obj, TimeSpan duration) MeasureExecutionTime<T>(Func<T> method)
        {
            var stopwatch = Stopwatch.StartNew();
            T result = method.Invoke();
            stopwatch.Stop();
            TimeSpan duration = stopwatch.Elapsed;
            return (result, duration);
        }

        /// <summary>
        /// 测量无返回值的方法的执行时间。
        /// </summary>
        /// <param name="method">要测量的方法。</param>
        /// <returns>执行时间。</returns>
        public static TimeSpan MeasureExecutionTime(Action method)
        {
            var stopwatch = Stopwatch.StartNew();
            method.Invoke();
            stopwatch.Stop();
            var duration = stopwatch.Elapsed;
            return duration;
        }
    }
}
