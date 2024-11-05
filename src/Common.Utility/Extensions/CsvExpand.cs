using CsvHelper;
using CsvHelper.Configuration;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Formats.Asn1;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Common.Utility.Extensions
{
    public abstract class CsvBaseModel
    {
        //public int Id { get; set; } // 添加一个新的Id属性作为行号
    }

    public static class CsvExpand
    {
        /// <summary>
        /// 将List<T>导出到 CSV 文件
        /// </summary>
        /// <typeparam name="T">列表项的类型。</typeparam>
        /// <param name="list">要转换的List<T>。</param>
        /// <param name="filePath">要保存CSV文件的路径。</param>
        public static void ExPortToCsv<T>(this List<T> list, string filePath)
        {
            try
            {
                using (var writer = new StreamWriter(filePath, false, new UTF8Encoding(true)))
                {
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(list);
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"没有足够的权限访问文件: {ex.Message}");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"找不到指定的文件: {ex.Message}");
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine($"找不到指定的目录: {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IO 错误: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 读取CSV文件并转换为List<T>。
        /// </summary>
        /* 调用示例：以通过在类型属性上添加特性来进行列映射：
         * using CsvHelper.Configuration.Attributes;

            public class ModelType
            {
                [Name("Header Name In CSV")] // CSV文件中的列标题
                public string Property1 { get; set; }

                [Index(0)] // CSV文件中的列索引（从0开始）
                public int Property2 { get; set; }

                [Ignore] // 忽略这个属性，不会写入也不会读取
                public string Property3 { get; set; }

                // 更多特性可以用来格式化，例如日期格式或自定义类型转换器等
                [Format("yyyy-MM-dd")]
                public DateTime Property4 { get; set; }
            }
         * 
        */
        /// <typeparam name="T">列表项的类型。</typeparam>
        /// <param name="filePath">要读取的CSV文件的路径。</param>
        /// <returns>转换得到的List<T>。</returns>
        public static List<T> ReadCsvToList<T, MapT>(this string filePath, bool isReadHeader = false)
      where T : CsvBaseModel
      where MapT : ClassMap<T>
        {
            try
            {
                using (StreamReader streamReader1 = new StreamReader(filePath, (Encoding)new UTF8Encoding(true)))
                {
                    StreamReader streamReader2 = streamReader1;
                    using (CsvReader csvReader = new CsvReader((TextReader)streamReader2, (IReaderConfiguration)new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = isReadHeader
                    }))
                    {
                        csvReader.Context.RegisterClassMap<MapT>();
                        //int num = 1;
                        List<T> list = csvReader.GetRecords<T>().ToList<T>();
                        //foreach (T obj in list)
                        //    obj.Id = num++;
                        return list;
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("没有足够的权限访问文件: " + ex.Message);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("找不到指定的文件: " + ex.Message);
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine("找不到指定的目录: " + ex.Message);
            }
            catch (IOException ex)
            {
                Console.WriteLine("IO 错误: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("发生异常: " + ex.Message);
            }
            return new List<T>();
        }


        /// <summary>
        /// 将 DataTable 导出到 CSV 文件
        /// </summary>
        /// <param name="dataTable">要导出的 DataTable</param>
        /// <param name="filePath">保存文件的完整路径</param>
        public static void ExportToCsv(this DataTable dataTable, string filePath)
        {
            try
            {
                using (var writer = new StreamWriter(filePath, false, new UTF8Encoding(true)))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        csv.WriteField(column.ColumnName);
                    }
                    csv.NextRecord();

                    foreach (DataRow row in dataTable.Rows)
                    {
                        for (var i = 0; i < dataTable.Columns.Count; i++)
                        {
                            csv.WriteField(row[i]);
                        }
                        csv.NextRecord();
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"没有足够的权限访问文件: {ex.Message}");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"找不到指定的文件: {ex.Message}");
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine($"找不到指定的目录: {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IO 错误: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生异常: {ex.Message}");
            }
        }

        /// <summary>
        /// 读取CSV文件并转换为DataTable。
        /// </summary>
        /// <param name="filePath">要读取的CSV文件的路径。</param>
        /// <returns>转换得到的DataTable。</returns>
        public static DataTable ReadCsvToDataTable(this string filePath, bool isReadHeader = false)
        {
            try
            {
                using (var reader = new StreamReader(filePath, new UTF8Encoding(true)))
                {

                    // 配置 CsvReader 在初始化时不将第一行视为标题行
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = isReadHeader,
                    };
                    using (var csv = new CsvReader(reader, config))
                    {
                        using (var dr = new CsvDataReader(csv))
                        {
                            var dt = new DataTable();
                            dt.Load(dr);
                            return dt;
                        }
                    }
                }

            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"没有足够的权限访问文件: {ex.Message}");
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"找不到指定的文件: {ex.Message}");
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine($"找不到指定的目录: {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IO 错误: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生异常: {ex.Message}");
            }
            return null;
        }


        /// <summary>
        /// CSV转换成DataTable（OleDb数据库访问方式）
        /// </summary>
        /// <param name="csvPath">csv文件路径</param>
        /// <returns></returns>
        public static DataTable CSVToDataTableByOledb(this string csvPath)
        {
            DataTable csvdt = new DataTable("csv");
            if (!File.Exists(csvPath))
            {
                throw new FileNotFoundException("csv文件路径不存在!");
            }
            if (Path.GetExtension(csvPath).ToLower()!=".csv")
            {
                throw new Exception($"{csvPath} 格式不正确！");
            }

            FileInfo fileInfo = new FileInfo(csvPath);
            using (OleDbConnection conn = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileInfo.DirectoryName + ";Extended Properties='Text;'"))
            {
                OleDbDataAdapter adapter = new OleDbDataAdapter("SELECT * FROM [" + fileInfo.Name + "]", conn);
                adapter.Fill(csvdt);
            }
            return csvdt;
        }

        /// <summary>
        /// CSV转换成DataTable（文件流方式）
        /// </summary>
        /// <param name="csvPath">csv文件路径</param>
        /// <returns></returns>
        public static DataTable CSVToDataTableByStreamReader(this string csvPath)
        {
            if (Path.GetExtension(csvPath).ToLower() != ".csv")
            {
                throw new Exception($"{csvPath} 格式不正确！");
            }
            DataTable csvdt = new DataTable("csv");

            int intColCount = 0;
            bool blnFlag = true;
            DataColumn column;
            DataRow row;
            string strline = null;
            string[] aryline;
            Encoding encoding = null;

            using (StreamReader reader = new StreamReader(csvPath, encoding))
            {
                while (!string.IsNullOrEmpty((strline = reader.ReadLine())))
                {
                    aryline = strline.Split(new char[] { ',' });

                    if (blnFlag)
                    {
                        blnFlag = false;
                        intColCount = aryline.Length;
                        for (int i = 0; i < aryline.Length; i++)
                        {
                            column = new DataColumn(aryline[i]);
                            csvdt.Columns.Add(column);
                        }
                        continue;
                    }

                    row = csvdt.NewRow();
                    for (int i = 0; i < intColCount; i++)
                    {
                        row[i] = aryline[i];
                    }
                    csvdt.Rows.Add(row);
                }
            }

            return csvdt;
        }

        /// <summary>
        /// DataTable 生成 CSV
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <param name="csvPath">csv文件路径</param>
        public static void DataTableToCSV(this DataTable dt, string csvPath)
        {
            if (Path.GetExtension(csvPath).ToLower() != ".csv")
            {
                throw new Exception($"{csvPath} 格式不正确！");
            }
            if (null == dt)
                return;

            StringBuilder csvText = new StringBuilder();
            StringBuilder csvrowText = new StringBuilder();
            foreach (DataColumn dc in dt.Columns)
            {
                csvrowText.Append(",");
                csvrowText.Append(dc.ColumnName);
            }
            csvText.AppendLine(csvrowText.ToString().Substring(1));

            foreach (DataRow dr in dt.Rows)
            {
                csvrowText = new StringBuilder();
                foreach (DataColumn dc in dt.Columns)
                {
                    csvrowText.Append(",");
                    csvrowText.Append(dr[dc.ColumnName].ToString().Replace(',', ' '));
                }
                csvText.AppendLine(csvrowText.ToString().Substring(1));
            }

            File.WriteAllText(csvPath, csvText.ToString(), Encoding.Default);
        }
    }
}
