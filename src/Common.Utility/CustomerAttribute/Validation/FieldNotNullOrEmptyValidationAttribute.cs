using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Utility.CustomerAttribute.Validation
{
    /// <summary>
    /// 字段非空或者null校验特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class FieldNotNullOrEmptyValidationAttribute : Attribute
    {
        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; } = "cannot be null";

        public FieldNotNullOrEmptyValidationAttribute(string errorMessage)
        {
            ErrorMessage = errorMessage;       
        }
    }
}
