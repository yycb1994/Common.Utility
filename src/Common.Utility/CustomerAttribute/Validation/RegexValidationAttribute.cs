using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Utility.CustomerAttribute.Validation
{
    /// <summary>
    /// 正则表达式校验特性
    /// </summary>
    public class RegexValidationAttribute : FieldValidationAttribute
    {
        /// <summary>
        /// 正则表达式模式
        /// </summary>
        public string RegexPattern { get; }

        public RegexValidationAttribute(string regexPattern, string errorMessage) : base(errorMessage)
        {
            RegexPattern = regexPattern;
        }
    }
}
