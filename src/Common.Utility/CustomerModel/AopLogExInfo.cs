using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Utility.CustomerModel
{
    public class AopLogExInfo
    {
        public AopLogInfo ApiLogAopInfo { get; set; }
        /// <summary>
        /// 异常
        /// </summary>
        public string InnerException { get; set; } = string.Empty;
        /// <summary>
        /// 异常信息
        /// </summary>
        public string ExMessage { get; set; } = string.Empty;
    }
}
