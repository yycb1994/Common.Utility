using Common.Utility.CustomerAttribute.Validation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Common.Utility.Extensions
{
    public static class ValidationExpand
    {
        #region 对象字段校验
        /// <summary>
        /// 对象字段校验
        /// </summary>
        /// <param name="obj">对象</param>
        public static void ValidateFields(this object obj)
        {
            var properties = obj.GetType().GetProperties();
            foreach (var property in properties)
            {
                var notNullAttribute = (FieldNotNullOrEmptyValidationAttribute)Attribute.GetCustomAttribute(property, typeof(FieldNotNullOrEmptyValidationAttribute));
                if (notNullAttribute != null)
                {
                    var value = property.GetValue(obj);
                    if (value == null || value.ToString() == string.Empty)
                    {
                        throw new ArgumentNullException($"{property.Name} {notNullAttribute.ErrorMessage}");
                    }

                    var rangeValidationAttribute = (RangeValidationAttribute)Attribute.GetCustomAttribute(property, typeof(RangeValidationAttribute));
                    if (rangeValidationAttribute != null)
                    {
                        if (value is IComparable comparableValue)
                        {
                            object minValue = rangeValidationAttribute.MinValue;
                            object maxValue = rangeValidationAttribute.MaxValue;

                            if (comparableValue.CompareTo(minValue) < 0 || comparableValue.CompareTo(maxValue) > 0)
                            {
                                throw new ArgumentOutOfRangeException(rangeValidationAttribute.ErrorMessage);
                            }
                        }
                    }

                    var regexValidationAttribute = (RegexValidationAttribute)Attribute.GetCustomAttribute(property, typeof(RegexValidationAttribute));
                    if (regexValidationAttribute != null)
                    {
                        string regexPattern = regexValidationAttribute.RegexPattern;
                        string fieldValue = value.ToString();
                        if (!Regex.IsMatch(fieldValue, regexPattern))
                        {
                            throw new ArgumentException(regexValidationAttribute.ErrorMessage);
                        }
                    }
                }
            }

        }
        #endregion
    }
}