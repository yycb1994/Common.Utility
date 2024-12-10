using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Utility.CustomerAttribute.Validation
{
    /// <summary>
    /// 基础字段校验特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class FieldValidationAttribute : Attribute
    {
        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage { get; }

        public FieldValidationAttribute(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
    }
}
