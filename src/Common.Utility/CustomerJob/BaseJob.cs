using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Common.Utility.CustomerEnum;
using Common.Utility.Helper;
using Quartz;

namespace Common.Utility.CustomerJob
{
    public class BaseJob : IJob
    {
        public static readonly ConcurrentDictionary<string, Action> Delegates = new ConcurrentDictionary<string, Action>();
        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("执行BaseJob任务");

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
                    action.Invoke();
                    Console.WriteLine("任务执行完成 " + DateTime.Now);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("执行任务时发生错误: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("未找到对应的委托");
            }

            // 可以在这里考虑移除已经执行过的委托
            //Delegates.TryRemove(key, out _);
            await Task.CompletedTask;
        }
    }
}