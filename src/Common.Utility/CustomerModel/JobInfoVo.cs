using Common.Utility.CustomerEnum;
using System;

namespace Common.Utility.CustomerModel
{
    /// <summary>
    /// job 基础信息的值对象
    /// </summary>
    public class JobInfoVo
    {
        /// <summary>
        /// 任务名称
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// 任务组名称
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Cron 表达式
        /// </summary>
        public string CronExpression { get; set; }

        /// <summary>
        /// 上一次执行时间
        /// </summary>
        public DateTime LastExecutionTime { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        public JobStatus Status { get; set; }

        /// <summary>
        /// 任务说明
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 任务创建时间
        /// </summary>
        public DateTime JobCreateTime { get; set; }

        public Action TaskAction { get; set; }
        public Func<string> TaskFunc { get; set; }
    }
}