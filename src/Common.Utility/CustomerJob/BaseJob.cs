using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Common.Utility.CustomerEnum;

using Common.Utility.Helper;
using Quartz;


namespace Common.Utility.CustomerJob
{
    public class BaseJob : IJob
    {
        public static readonly ConcurrentDictionary<string, Func<Task>> Delegates = new ConcurrentDictionary<string, Func<Task>>();
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();

        public async Task Execute(IJobExecutionContext context)
        {
            JobLogger logger = new JobLogger("JobExecutionLog");

            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var key = dataMap.GetString("delegateKey");

            if (key != null && Delegates.TryGetValue(key, out var action))
            {
                // 获取或创建一个 SemaphoreSlim 实例
                var semaphore = _semaphores.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

                // 尝试获取锁
                if (await semaphore.WaitAsync(0)) // 立即尝试获取锁
                {
                    try
                    {
                        try
                        {
                            foreach (var jobInfo in QuartzHelper.JobDic.Values)
                            {
                                if (jobInfo.Status == JobStatus.已结束)
                                {
                                    Delegates.TryRemove(jobInfo.JobName, out var a);
                                }
                            }

                            var job = QuartzHelper.JobDic[key];
                            await logger.Information($"{key}任务说明：{job.Description}");
                            await logger.Information($"{key}上一次执行时间：{job.LastExecutionTime}");
                            await logger.Information($"开始执行{key}任务");

                            // 使用 await 执行异步方法
                            await action.Invoke();

                            await logger.Information($"{key}任务执行完成 " + DateTime.Now);
                            QuartzHelper.JobDic[key].LastExecutionTime = DateTime.Now;
                        }
                        catch (Exception ex)
                        {
                            await logger.Error($"{key}执行任务时发生错误: ", ex);
                        }
                    }
                    finally
                    {
                        semaphore.Release(); // 释放锁
                    }
                }
                else
                {
                    await logger.Information($"{key}任务正在执行，跳过本次调度。");
                }
            }
            else
            {
                await logger.Information($"未找到对应的{key}任务");
            }

            await Task.CompletedTask;
        }
    }





    public class JobLogger
    {
        private readonly string _logFilePath;
        private readonly string _logDirectory;
        private readonly Channel<string> _logChannel;

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

            // 创建异步通道
            _logChannel = Channel.CreateUnbounded<string>();

            // 启动日志处理任务
            _ = ProcessLogQueue();
        }

        private async Task ProcessLogQueue()
        {
            // 持续读取通道中的日志消息
            while (await _logChannel.Reader.WaitToReadAsync())
            {
                while (_logChannel.Reader.TryRead(out var logMessage))
                {
                    Log(logMessage); // 直接调用同步的 Log 方法
                }
            }
        }

        public async Task Information(string message)
        {
            await _logChannel.Writer.WriteAsync($"INFO: {message}");
        }

        public async Task Error(string message, Exception ex)
        {
            await _logChannel.Writer.WriteAsync($"ERROR: {message} | Exception: {ex.Message}");
        }

        public async Task Warning(string message)
        {
            await _logChannel.Writer.WriteAsync($"WARNING: {message}");
        }

        public async Task Debug(string message)
        {
            await _logChannel.Writer.WriteAsync($"DEBUG: {message}");
        }

        private void Log(string message)
        {
            // 记录日志
            var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}";

            File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
            Console.WriteLine(logMessage);
        }
    }
}