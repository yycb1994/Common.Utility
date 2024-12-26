using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utility.Helper
{
    public class JobLogger
    {
        private readonly string _logFilePath;
        private readonly string _logDirectory;

        public JobLogger(string logDirectory)
        {
            _logDirectory = logDirectory;
            // 确保日志目录存在
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }

            // 设置日志文件路径，使用当前日期
            _logFilePath = Path.Combine(_logDirectory, $"job-{DateTime.Now:yyyy-MM-dd}.log");
        }

        public async Task Information(string message)
        {
            await LogAsync("INFO", message);
            await Task.CompletedTask;
        }

        public async Task Error(string message, Exception ex)
        {
            await LogAsync("ERROR", $"{message} | Exception: {ex.Message}");
            await Task.CompletedTask;
        }

        public async Task Warning(string message)
        {
            await LogAsync("WARNING", message);
            await Task.CompletedTask;
        }

        public async Task Debug(string message)
        {
            await LogAsync("DEBUG", message);
            await Task.CompletedTask;
        }     

        public async Task LogAsync(string level, string message)
        {
            var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";
            int retryCount = 3;

            while (retryCount > 0)
            {
                try
                {
                    using (var stream = new FileStream(_logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                    using (var writer = new StreamWriter(stream))
                    {
                        await writer.WriteLineAsync(logMessage);
                        await Task.CompletedTask;
                    }
                    break; // 如果成功写入，退出循环
                }
                catch (IOException)
                {
                    retryCount--;
                    await Task.Delay(100); // 等待100毫秒后重试
                    if (retryCount == 0)
                    {
                        Console.WriteLine($"无法写入日志: {logMessage}");
                    }
                }
            }

        }
    }
}
