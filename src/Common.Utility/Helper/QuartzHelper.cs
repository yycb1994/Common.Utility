using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Common.Utility.CustomerEnum;
using Common.Utility.CustomerJob;
using Common.Utility.CustomerModel;
using Quartz;
using Quartz.Impl;


namespace Common.Utility.Helper
{
    /// <summary>
    /// 提供对 Quartz 定时任务框架的管理和操作。
    /// </summary>
    public static class QuartzHelper
    {
        public static readonly ConcurrentDictionary<string, JobInfoVo> JobDic = new ConcurrentDictionary<string, JobInfoVo>();
        private static readonly IScheduler Scheduler;

        /// <summary>
        /// 初始化 Quartz 定时任务管理器。
        /// </summary>
        static QuartzHelper()
        {
            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
            Scheduler = schedulerFactory.GetScheduler().Result;
            Scheduler.Start().Wait();
        }

        /// <summary>
        /// 添加一个定时任务。
        /// </summary>
        /// <param name="jobName">任务名称。</param>
        /// <param name="groupName">任务组名称。</param>
        /// <param name="cronExpression">Cron 表达式。</param>
        /// <param name="action">要执行的动作。</param>
        /// <param name="description">任务描述。</param>
        /// <returns>任务名称。</returns>
        public static async Task<string> AddJob(string jobName, string groupName, string cronExpression, Action action, string description = "")
        {
            // 创建委托的唯一键
            var delegateKey = Guid.NewGuid().ToString();
            // 将委托存储在静态字典中
            BaseJob.Delegates[jobName] = action;
            // 创建作业信息并保存到列表
            var jobInfo = new JobInfoVo { JobName = jobName, GroupName = groupName, CronExpression = cronExpression, Status = JobStatus.正常运行, Description = description, JobCreateTime = DateTime.Now };
            JobDic.TryAdd(jobName, jobInfo);

            // 创建Quartz作业和触发器
            IJobDetail job = JobBuilder.Create<BaseJob>()
                .WithIdentity(jobName, groupName)
                .UsingJobData("delegateKey", jobName) // 将委托的键添加到JobDataMap
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(jobName + "Trigger", groupName)
                .StartNow()
                .WithCronSchedule(cronExpression).WithDescription(description)
                .Build();

            await Scheduler.ScheduleJob(job, trigger);
            return jobName;
        }

        /// <summary>
        /// 暂停指定的定时任务。
        /// </summary>
        /// <param name="jobkey">任务键。</param>
        public static async Task PauseJob(string jobkey)
        {
            JobDic.TryGetValue(jobkey, out var jobInfo);
            if (jobInfo == null)
            {
                throw new Exception("job 内容不存在！");
            }

            jobInfo.Status = JobStatus.暂停;
            var jobKey = new JobKey(jobInfo.JobName, jobInfo.GroupName);
            await Scheduler.PauseJob(jobKey);
            Console.WriteLine($"任务 {jobInfo.JobName} 已暂停");
        }

        /// <summary>
        /// 恢复指定的定时任务。
        /// </summary>
        /// <param name="jobkey">任务键。</param>
        public static async Task ResumeJob(string jobkey)
        {
            JobDic.TryGetValue(jobkey, out var jobInfo);
            if (jobInfo == null)
            {
                throw new Exception("job 内容不存在！");
            }

            jobInfo.Status = JobStatus.正常运行;
            var jobKey = new JobKey(jobInfo.JobName, jobInfo.GroupName);
            await Scheduler.ResumeJob(jobKey);
            Console.WriteLine($"任务 {jobInfo.JobName} 已继续");
        }

        /// <summary>
        /// 删除指定的定时任务。
        /// </summary>
        /// <param name="jobkey">任务键。</param>
        public static async Task DeleteJob(string jobkey)
        {
            JobDic.TryGetValue(jobkey, out var jobInfo);
            if (jobInfo == null)
            {
                throw new Exception("job 内容不存在！");
            }

            jobInfo.Status = JobStatus.已结束;
            var jobKey = new JobKey(jobInfo.JobName, jobInfo.GroupName);
            await Scheduler.DeleteJob(jobKey);
            Console.WriteLine($"任务 {jobInfo.JobName} 已删除");
        }
    }
}