using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Utility.CustomerModel
{
    public class SysFileJsonVo
    {
        /// <summary>
        /// 文件id
        /// </summary>
        public string fileId { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public int? size { get; set; }
        /// <summary>
        /// 文件名称
        /// </summary>
        public string fileName { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string createUser { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? createTime { get; set; }
    }
    
}
