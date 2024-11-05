using Common.Utility.CustomerAttribute;
using SharpCompress;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;


namespace Common.Utility.Extensions
{
    /// <summary>
    /// 为DataTable提供扩展方法。
    /// </summary>
    public static class DataTableExpand
    {
        public enum OrderType
        {
            Asc,
            Desc
        }

        /// <summary>
        /// 判断DataTable是否为null或空。
        /// </summary>
        /// <param name="dt">要检查的DataTable。</param>
        /// <returns>如果DataTable为null或空，则为true；否则为false。</returns>
        public static bool TableIsNull(this DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
                return true;
            return false;
        }

        /// <summary>
        /// 获取DataRow在指定列索引处的值。
        /// </summary>
        /// <param name="row">要从中检索值的DataRow。</param>
        /// <param name="columnIndex">列索引。</param>
        /// <returns>列值的字符串表示，如果为空则为null。</returns>
        public static string GetValue(this DataRow row, int columnIndex)
        {
            try
            {
                if (row[columnIndex] == null)
                {
                    return null;
                }

                return row[columnIndex].ToString();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取DataRow具有指定列名称的值。
        /// </summary>
        /// <param name="row">要从中检索值的DataRow。</param>
        /// <param name="columnName">列名称。</param>
        /// <returns>列值的字符串表示，如果为空则为null。</returns>
        public static string GetValue(this DataRow row, string columnName)
        {
            try
            {
                if (row[columnName] == null)
                {
                    return null;
                }

                return row[columnName].ToString();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取DataTable中第一行第一列的值。
        /// </summary>
        /// <param name="dt">要从中检索值的DataTable。</param>
        /// <returns>第一行第一列的值的字符串表示，如果DataTable为空则为null。</returns>
        public static string FirstOrDefault(this DataTable dt)
        {
            if (dt.TableIsNull())
            {
                return string.Empty;
            }
            return dt.Rows[0].GetValue(0);
        }

        /// <summary>
        /// 获取DataTable中第一列单元格的值列表。
        /// </summary>
        /// <param name="dt">要从中检索值的DataTable。</param>
        /// <returns>第一列单元格值的列表。</returns>
        public static List<string> GetFirstCellValues(this DataTable dt)
        {
            List<string> firstCellList = new List<string>();

            if (dt.TableIsNull()) return firstCellList;
            // 遍历DataTable中的每一行
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                // 获取当前行中第一个单元格的值并添加到列表中
                firstCellList.Add(dt.Rows[i].GetValue(0));
            }

            return firstCellList;
        }

        /// <summary>
        /// 获取DataTable中第一列单元格的值列表。
        /// </summary>
        /// <param name="dt">要从中检索值的DataTable。</param>
        /// <returns>第一列单元格值的列表。</returns>
        public static (Type type, List<object> data) GetFirstCellObjValues(this DataTable dt)
        {

            Type type = dt.Columns[0].DataType;
            List<object> firstCellList = new List<object>();

            if (dt.TableIsNull()) return (type, firstCellList);

            // 遍历 DataTable 中的每一行
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                // 获取当前行中第一个单元格的值并添加到列表中
                firstCellList.Add(dt.Rows[i][0]);
            }

            return (type, firstCellList);
        }

        /// <summary>
        /// 根据单元格名称获取DataTable中的值列表。
        /// </summary>
        /// <param name="dt">要从中检索值的DataTable。</param>
        /// <param name="cellName">单元格名称。</param>
        /// <returns>指定单元格值的列表。</returns>
        public static List<string> GetValuesByCellName(this DataTable dt, string cellName)
        {
            List<string> firstCellList = new List<string>();

            if (dt.TableIsNull()) return firstCellList;
            // 遍历DataTable中的每一行
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                // 获取当前行中指定单元格的值并添加到列表中
                firstCellList.Add(dt.Rows[i].GetValue(cellName));
            }

            return firstCellList;
        }

        /// <summary>
        /// 将DataTable转换为指定类型的对象列表。
        /// </summary>
        /// <typeparam name="TModel">目标对象类型。</typeparam>
        /// <param name="dataTable">要转换的DataTable。</param>
        /// <returns>转换后的对象列表。</returns>
        public static List<TModel> ToObjectList<TModel>(this DataTable dataTable) where TModel : class, new()
        {
            var objectList = new List<TModel>();
            try
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    TModel obj = Activator.CreateInstance<TModel>();

                    foreach (var property in typeof(TModel).GetProperties())
                    {
                        var attribute = property.GetCustomAttribute<DTMapperAttribute>();
                        var columnName = attribute?.ColumnName ?? property.Name;

                        if (dataTable.Columns.Contains(columnName))
                        {
                            var value = row[columnName];
                            if (value != DBNull.Value)
                            {
                                property.SetValue(obj, Convert.ChangeType(value, property.PropertyType));
                            }
                        }
                    }

                    objectList.Add(obj);
                }

                return objectList;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to convert DataTable:", ex);
            }
        }

        /// <summary>
        /// 将泛型集合转换为DataTable。
        /// </summary>
        /// <typeparam name="T">集合中项的类型。</typeparam>
        /// <param name="list">要转换的集合。</param>
        /// <param name="tableName">表名称。</param>
        /// <returns>生成的DataTable。</returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> list, string tableName = null)
        {
            var result = new DataTable(tableName);

            var enumerable = list.ToList();
            if (!enumerable.Any())
            {
                return result;
            }

            var properties = typeof(T).GetProperties();
            result.Columns.AddRange(properties.Select(p =>
            {
                var columnType = p.PropertyType;
                if (columnType.IsGenericType && columnType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    columnType = Nullable.GetUnderlyingType(columnType);
                }

                return new DataColumn(p.GetCustomAttribute<DTMapperAttribute>()?.ColumnName ?? p.Name,
                    columnType);
            }).ToArray());

            enumerable.ToList().ForEach(item => result.Rows.Add(properties.Select(p => p.GetValue(item)).ToArray()));

            return result;
        }

        /// <summary>
        /// 根据某个字段或多个字段将DataTable分组为多个DataTable。
        /// </summary>
        /// <param name="dataTable">要分组的DataTable。</param>
        /// <param name="groupFields">用于分组的字段名称数组。</param>
        /// <returns>包含分组后的多个DataTable的列表。</returns>
        /// <exception cref="ArgumentNullException">当dataTable或groupFields为null时抛出。</exception>
        /// <exception cref="ArgumentException">当groupFields中的字段在dataTable中不存在时抛出。</exception>
        public static List<DataTable> GroupBy(this DataTable dataTable, params string[] groupFields)
        {
            List<DataTable> tables = new List<DataTable>();
            // 检查dataTable是否为null
            if (dataTable == null) throw new ArgumentNullException(nameof(dataTable));
            // 检查groupFields是否为null
            if (groupFields == null)
            {
                tables.Add(dataTable);
                return tables;
            }


            // 检查groupFields中的每一个字段是否都存在于dataTable中
            foreach (var field in groupFields)
            {
                if (!dataTable.Columns.Contains(field))
                    throw new ArgumentException($"字段 '{field}' 在DataTable中不存在。");
            }

            // 使用LINQ对dataTable的行按照groupFields进行分组
            var groups = dataTable.AsEnumerable()
                .GroupBy(r => new NTuple<object>(groupFields.Select(f => r[f])));

            // 分组后的DataTable列表

            foreach (var group in groups)
            {
                // 克隆原始DataTable的结构
                DataTable newTable = dataTable.Clone();
                // 将分组中的每一行添加到新的DataTable中
                foreach (var row in group)
                    newTable.Rows.Add(row.ItemArray);

                // 将新的DataTable添加到列表中
                tables.Add(newTable);
            }

            // 返回包含分组后的DataTable的列表
            return tables;
        }

        /// <summary>
        /// 添加新列到Datatable【会排到第一列，默认序号】
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="columnName">列名称</param>
        /// <returns></returns>
        public static DataTable AddIdentityColumn(this DataTable dt, string columnName = "identityid")
        {
            if (!dt.Columns.Contains(columnName))
            {
                DataColumn identityColumn = new DataColumn(columnName);
                dt.Columns.Add(identityColumn);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i][columnName] = (i + 1).ToString();
                }

                dt.Columns[columnName].SetOrdinal(0); // 将列排在第一位
            }

            return dt;
        }

        /// <summary>
        /// 添加新列到Datatable
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="cellData">集合数据</param>
        /// <param name="columnName">列名称</param>
        /// <returns></returns>
        public static DataTable AddColumn(this DataTable dt, List<string> cellData, string columnName = "identityid")
        {
            if (!dt.Columns.Contains(columnName))
            {
                DataColumn identityColumn = new DataColumn(columnName);
                dt.Columns.Add(identityColumn);

                if (cellData != null)
                {
                    for (int i = 0; i < cellData.Count; i++)
                    {
                        dt.Rows[i][columnName] = cellData[i];
                    }
                }
            }

            return dt;
        }

        /// <summary>
        /// 指定DataTable列的顺序
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="columnName">列名称</param>
        /// <param name="orderIndex">顺序值</param>
        public static void SetOrdinal(this DataTable dt,string columnName,int orderIndex)
        {
            dt.Columns[columnName].SetOrdinal(orderIndex);
        }


        /// <summary>
        /// 向 DataTable 中插入一行数据
        /// </summary>
        /// <param name="dt">要插入数据的 DataTable 对象</param>
        /// <param name="cellData">要插入的单元格数据</param>
        /// <returns>更新后的 DataTable 对象</returns>
        public static DataTable InsertRow(this DataTable dt, params object[] cellData)
        {
            // 检查dataTable是否为null
            if (dt == null) throw new ArgumentNullException(nameof(dt));

            if (cellData.Length != dt.Columns.Count)
            {
                throw new ArgumentException("cellData 数量与 DataTable 列数不匹配");
            }

            DataRow newRow = dt.NewRow();
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (cellData[i] == null)
                    newRow[dt.Columns[i].ColumnName] = DBNull.Value;
                else
                    newRow[dt.Columns[i].ColumnName] = Convert.ChangeType(cellData[i], dt.Columns[i].DataType);
            }

            dt.Rows.Add(newRow);
            return dt;
        }

        /// <summary>
        /// 对 DataTable 进行按指定字段降序排序
        /// </summary>
        /// <param name="dt">要排序的 DataTable 对象</param>
        /// <param name="orderType">排序类型，ASC（升序）或 DESC（降序），默认为 ASC</param>
        /// <param name="orderFields">要排序的字段名</param>
        /// <returns>排序后的 DataTable 对象</returns>
        public static DataTable OrderBy(this DataTable dt, OrderType orderType, params string[] orderFields)
        {
            // 检查dataTable是否为null
            if (dt == null) throw new ArgumentNullException(nameof(dt));

            DataView view = dt.DefaultView;
            var orderByStr = string.Join(",", orderFields);
            view.Sort = $"{orderByStr} {orderType}";

            return view.ToTable();
        }

        /// <summary>
        /// 将 List<DataTable> 中的所有 DataTable 合并成一个单独的 DataTable。
        /// </summary>
        /// <param name="tables">包含要合并的 DataTable 的 List。</param>
        /// <returns>合并后的 DataTable。</returns>
        public static DataTable MergeTables(this List<DataTable> tables)
        {
            if (tables == null || tables.Count == 0)
            {
                return null;
                //throw new ArgumentException("List<DataTable> 不能为空或为空。");
            }

            DataTable mergedTable = tables[0].Clone(); // 克隆第一个 DataTable 的结构

            foreach (DataTable table in tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    mergedTable.ImportRow(row);
                }
            }

            return mergedTable;
        }

        /// <summary>
        /// 根据筛选表达式选择 DataTable 中的行并返回一个新的 DataTable。
        /// </summary>
        /// <param name="dataTable">要筛选的 DataTable。</param>
        /// <param name="filterExpression">筛选表达式，用于选择要包含在结果中的行。</param>
        /// <returns>根据筛选表达式筛选后的新 DataTable。</returns>
        public static DataTable SelectRows(this DataTable dataTable, string filterExpression)
        {
            try
            {
                DataRow[] filteredRows = dataTable.Select(filterExpression);

                DataTable filteredTable = dataTable.Clone(); // 复制表结构

                foreach (DataRow row in filteredRows)
                {
                    filteredTable.ImportRow(row);
                }

                return filteredTable;
            }
            catch (Exception ex)
            {
                throw new Exception($"筛选表达式无效：{filterExpression}" + ex.Message);
            }
        }

        /// <summary>
        /// 将DataTable转换为字典
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static Dictionary<string, IEnumerable<string>> ConvertDictionary(this DataTable dt)
        {
            Dictionary<string, IEnumerable<string>> dic = new Dictionary<string, IEnumerable<string>>();
            if (!dt.TableIsNull())
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    var commandName = dt.Columns[i].ColumnName;
                    var commandData = dt.GetValuesByCellName(commandName);

                    dic.Add(commandName, commandData);
                }
            }

            return dic;
        }
    }



    /// <summary>
    /// 用于分组时将多个字段值作为一个整体处理的辅助类。
    /// </summary>
    /// <typeparam name="T">字段值的类型。</typeparam>
    public class NTuple<T> : IEquatable<NTuple<T>>
    {
        /// <summary>
        /// 创建一个新的NTuple实例。
        /// </summary>
        /// <param name="values">字段值的集合。</param>
        public NTuple(IEnumerable<T> values)
        {
            Values = values.ToArray();
        }

        /// <summary>
        /// 存储字段值的数组。
        /// </summary>
        public T[] Values { get; }

        /// <summary>
        /// 判断当前NTuple实例是否与指定的对象相等。
        /// </summary>
        /// <param name="obj">要比较的对象。</param>
        /// <returns>如果相等返回true，否则返回false。</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return Equals((NTuple<T>)obj);
        }

        /// <summary>
        /// 判断当前NTuple实例是否与另一个NTuple实例相等。
        /// </summary>
        /// <param name="other">要比较的NTuple实例。</param>
        /// <returns>如果相等返回true，否则返回false。</returns>
        public bool Equals(NTuple<T> other)
        {
            return Values.SequenceEqual(other.Values);
        }

        /// <summary>
        /// 获取当前NTuple实例的哈希码。
        /// </summary>
        /// <returns>哈希码。</returns>
        public override int GetHashCode()
        {
            // 对Values中的每个值进行哈希计算，并通过异或运算合并起来
            return Values.Aggregate(0, (hash, value) => hash ^ value?.GetHashCode() ?? 0);
        }
    }


}