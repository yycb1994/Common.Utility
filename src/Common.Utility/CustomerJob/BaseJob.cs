using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Common.Utility.CustomerEnum;
using Common.Utility.CustomerModel;
using Common.Utility.Helper;
using Quartz;
using static Quartz.Logging.OperationName;

namespace Common.Utility.CustomerJob
{
    public class BaseJob : IJob
    {
        public static readonly ConcurrentDictionary<string, Action> Delegates = new ConcurrentDictionary<string, Action>();
        public async Task Execute(IJobExecutionContext context)
        {

            JobLogger logger = new JobLogger("JobExecutionLog");
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var key = dataMap.GetString("delegateKey");

            if (key != null && Delegates.TryGetValue(key, out var action))
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
                    await logger.Information($"{key}任务说明：{job.Description}");
                    await logger.Information($"{key}上一次执行时间：{job.LastExecutionTime}");
                    await logger.Information($"开始执行{key}任务");
                    action.Invoke();
                    await logger.Information($"{key}任务执行完成 " + DateTime.Now);
                    QuartzHelper.JobDic[key].LastExecutionTime = DateTime.Now;
                }
                catch (Exception ex)
                {
                    await logger.Error($"{key}执行任务时发生错误: ", ex);
                }
            }
            else
            {
                await logger.Information($"未找到对应的{key}任务");
            }

            // 可以在这里考虑移除已经执行过的委托
            //Delegates.TryRemove(key, out _);
            await Task.CompletedTask;
        }
    }
}