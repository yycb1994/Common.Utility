using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Utility.CustomerAttribute.Validation
{

    /// <summary>
    /// 值范围校验特性
    /// </summary>
    public class RangeValidationAttribute : FieldValidationAttribute
    {
        /// <summary>
        /// 最小值
        /// </summary>
        public object MinValue { get; }

        /// <summary>
        /// 最大值
        /// </summary>
        public object MaxValue { get; }

        public RangeValidationAttribute(object minValue, object maxValue, string errorMessage) : base(errorMessage)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }
    }

}
