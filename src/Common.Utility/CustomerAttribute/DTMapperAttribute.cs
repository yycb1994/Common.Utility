using System;

namespace Common.Utility.CustomerAttribute
{
    /// <summary>
    /// 表示用于指定DataTable列字段名的属性。
    /// </summary>
    public class DTMapperAttribute : Attribute
    {
        /// <summary>
        /// 获取列名称。
        /// </summary>
        public string ColumnName { get; }

        /// <summary>
        /// 使用指定的列名称初始化 DataTableFieldNameAttribute 类的新实例。
        /// </summary>
        /// <param name="columnName">列的名称。</param>
        public DTMapperAttribute(string columnName)
        {
            ColumnName = columnName;
        }
    }

}