using Common.Utility.Helper;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace Common.Utility.Extensions
{
    public static class NpoiExpand
    {
        #region Open File


        /// <summary>
        /// 打开一个 Excel 文件并返回工作簿对象。
        /// </summary>
        /// <param name="filePath">Excel 文件的路径。</param>
        /// <param name="password">Excel密码。</param>
        /// <param name="isReadOnly">是否只读。</param>
        /// <returns>已打开的 Excel 工作簿对象。</returns>
        public static IWorkbook OpenExcel(this string filePath, string password = "", bool isReadOnly = false)
        {
            //filePath = filePath.ConvertXlsToXlsx();
            if (!File.Exists(filePath))
                throw new FileNotFoundException("文件不存在！", filePath);
            var workbook = string.IsNullOrWhiteSpace(password) ? WorkbookFactory.Create(filePath, password, isReadOnly) : WorkbookFactory.Create(filePath);

            return workbook;
        }

        /// <summary>
        /// 打开一个 Excel 文件并返回工作簿对象。
        /// </summary>
        /// <param name="fs">文件流</param>
        /// <param name="isReadOnly">是否只读。</param>
        /// <returns>已打开的 Excel 工作簿对象。</returns>
        public static IWorkbook OpenExcel(this FileStream fs, bool isReadOnly = false)
        {
            if (fs == null)
                throw new FileNotFoundException("文件不存在！");
            IWorkbook workbook = WorkbookFactory.Create(fs, isReadOnly);
            return workbook;
        }


        #endregion

        #region Excel保存前，确保每个公式都被计算
        /// <summary>
        /// 强制所有工作表中的公式在打开时重新计算。
        /// </summary>
        /// <param name="workbook">要处理的工作簿。</param>
        public static void ForceRecalculateAllFormulas(this IWorkbook workbook)
        {
            for (int i = 0; i < workbook.NumberOfSheets; i++)
            {
                ISheet sheet = workbook.GetSheetAt(i);
                sheet.ForceRecalculateFormulas();
            }
        }

        /// <summary>
        /// 强制工作表中的公式在打开时重新计算。
        /// </summary>
        /// <param name="sheet">要处理的工作表。</param>
        public static void ForceRecalculateFormulas(this ISheet sheet)
        {
            sheet.ForceFormulaRecalculation = true;
        }
        #endregion

        #region Save File
        /// <summary>
        /// 将工作簿保存为字节数组。
        /// </summary>
        /// <param name="workBook">要保存的工作簿对象。</param>
        /// <returns>保存的工作簿的字节数组。</returns>
        public static byte[] SaveExcelToByte(this IWorkbook workBook)
        {
            workBook.ForceRecalculateAllFormulas();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                workBook.Write(memoryStream); // 将工作簿写入内存流
                var fileBytes = memoryStream.ToArray(); // 将内存流转换为字节数组
                return fileBytes;
            }
        }

        /// <summary>
        /// 将工作簿保存为文件。
        /// </summary>
        /// <param name="workBook">要保存的工作簿对象。</param>
        /// <param name="saveFileFullPath">要保存的文件的完整路径。</param>
        /// <returns>保存的文件的完整路径。</returns>
        public static string SaveExcelToFile(this IWorkbook workBook, string saveFileFullPath)
        {
            workBook.ForceRecalculateAllFormulas();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                workBook.Write(memoryStream); // 将工作簿写入内存流
                                              //memoryStream.Position = 0;// 重置内存流的位置到起始位置
                var fileBytes = memoryStream.ToArray(); // 将内存流转换为字节数组
                fileBytes.SaveFile(saveFileFullPath);
                return saveFileFullPath;
            }
        }
        /// <summary>
        /// 将工作簿保存为文件 方法2
        /// </summary>
        /// <param name="workBook">要保存的工作簿对象。</param>
        /// <param name="saveFileFullPath">要保存的文件的完整路径。</param>
        /// <returns>保存的文件的完整路径。</returns>
        public static string SaveExcelToFile2(this IWorkbook workBook, string saveFileFullPath)
        {
            workBook.ForceRecalculateAllFormulas();
            FileHelper.CreateFolderByFilePath(saveFileFullPath);
            using (FileStream outputStrean = new FileStream(saveFileFullPath, FileMode.Create))
            {
                workBook.Write(outputStrean);
            }

            return saveFileFullPath;
        }
        #endregion

        #region Get Value
        /// <summary>
        /// 获取指定行和列索引处的单元格，如果单元格不存在则创建
        /// </summary>
        /// <param name="sheet">工作表对象</param>
        /// <param name="rowIndex">行索引</param>
        /// <param name="cellIndex">列索引</param>
        /// <returns>存在或新创建的单元格对象</returns>
        public static ICell GetCell(this ISheet sheet, int rowIndex, int cellIndex)
        {
            return sheet.RowAndCellExist(rowIndex, cellIndex);
        }

        /// <summary>
        /// 获取单元格的值。
        /// </summary>
        /// <param name="cell">要获取值的单元格</param>
        /// <returns>单元格的值，如果单元格为空则返回 DBNull.Value</returns>
        public static object GetCellValue(this ICell cell)
        {
            if (cell == null)
                return DBNull.Value;

            switch (cell.CellType)
            {
                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(cell))
                        return cell.DateCellValue;
                    else
                        return cell.NumericCellValue;

                case CellType.String:
                    return cell.StringCellValue;

                case CellType.Boolean:
                    return cell.BooleanCellValue;

                case CellType.Formula:
                    return cell.CellFormula;

                default:
                    return DBNull.Value;
            }
        }


        /// <summary>
        /// 获取指定单元格的值。
        /// </summary>
        /// <param name="sheet">工作表对象。</param>
        /// <param name="rowIndex">行索引。</param>
        /// <param name="cellIndex">列索引。</param>
        /// <param name="replaceStrs">需要将字符替换为空的内容</param>
        /// <returns>单元格的值。</returns>
        public static string GetCellValue(this ISheet sheet, int rowIndex, int cellIndex, params string[] replaceStrs)
        {
            IRow row = sheet.GetRow(rowIndex);
            if (row == null) return null;

            ICell cell = row.GetCell(cellIndex);
            if (cell == null) return null;
            string cellValue;
            if (cell.CellType == CellType.Formula)
            {
                XSSFFormulaEvaluator evaluator = new XSSFFormulaEvaluator(sheet.Workbook);
                CellValue cellFormulaValue = evaluator.Evaluate(cell);
                cellValue = cellFormulaValue.FormatAsString();
            }
            else
            {
                cellValue = cell.GetCellValue()?.ToString();
            }


            if (cellValue != null && replaceStrs != null && replaceStrs.Length > 0)
            {
                foreach (string replaceStr in replaceStrs)
                {
                    cellValue = cellValue.Replace(replaceStr, string.Empty);
                }
            }

            return cellValue?.Trim();
        }
        /// <summary>
        /// 获取指定单元格的值。
        /// </summary>
        /// <param name="sheet">工作表对象。</param>
        /// <param name="rowIndex">行索引。</param>
        /// <param name="cellIndex">列索引。</param>
        /// <returns>单元格的值。</returns>
        public static object GetCellValueObj(this ISheet sheet, int rowIndex, int cellIndex)
        {
            IRow row = sheet.GetRow(rowIndex);
            if (row == null) return null;
            ICell cell = row.GetCell(cellIndex);
            if (cell == null) return null;
            return cell.GetCellValue();
        }
        /// <summary>
        /// 从 Excel 文件中读取 sheetIndex 页的内容,保留单元格样式，表格样式颜色等
        /// </summary>
        /// <param name="filePath">Excel 文件路径</param>
        /// <param name="sheetIndex">第几个sheet页 文件路径</param>
        /// <param name="htmlFileSavePath">sheet页转换html文件后的保存地址</param>
        /// <returns>第一个 sheet 页的内容</returns>
        public static void SheetToStyledHtml(this string filePath, int sheetIndex, string htmlFileSavePath)
        {
            string htmlFilePath = htmlFileSavePath;
            StringBuilder htmlBuilder = new StringBuilder();
            htmlBuilder.Append("<table border='1' style='border-collapse: collapse;border: 1px solid black;width: 66%;'>");
            using (IWorkbook workbook = filePath.OpenExcel())
            {
                ISheet sheet = workbook.GetSheetAt(sheetIndex);
                IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
                for (int rowIndex = 0; rowIndex <= sheet.LastRowNum; rowIndex++)
                {
                    IRow row = sheet.GetRow(rowIndex);
                    if (row == null) continue;

                    htmlBuilder.Append("<tr>");
                    for (int colIndex = 0; colIndex < row.LastCellNum; colIndex++)
                    {
                        ICell cell = row.GetCell(colIndex);
                        if (cell == null)
                        {
                            htmlBuilder.Append("<td></td>");
                        }
                        else
                        {
                            Console.WriteLine(cell.CellType);
                            if (cell.CellType == CellType.Numeric)
                            {
                                var cellValue1 = cell.NumericCellValue.ToString();
                            }

                            short formatIndex = cell.CellStyle.DataFormat;
                            var style = cell.CellStyle;
                            var font = workbook.GetFontAt(style.FontIndex);

                            // 背景颜色处理
                            string bgColor = style.FillForegroundColorColor == null ? "#FFFFFF" : ((XSSFColor)style.FillForegroundColorColor).ARGBHex == null ? "" : ((XSSFColor)style.FillForegroundColorColor).ARGBHex.Substring(2);
                            // 字体样式处理
                            string fontColor = font.Color == 0 ? "#000000" : "#" + font.Color.ToString("X");

                            object cellValue = string.Empty;
                            switch (cell.CellType)
                            {
                                case CellType.Blank:
                                    cell.SetCellValue(string.Empty);
                                    break;
                                case CellType.Boolean:
                                    cell.SetCellValue(cell.BooleanCellValue);
                                    break;
                                case CellType.Error:
                                    cell.SetCellErrorValue(cell.ErrorCellValue);
                                    break;
                                case CellType.Formula:
                                    try
                                    {
                                        if (cell.ToString().Contains("IF"))
                                        {
                                            cellValue = evaluator.Evaluate(cell).FormatAsString().ToDoubleAndRound(2, true);
                                        }
                                        else
                                        {
                                            cellValue = evaluator.Evaluate(cell).FormatAsString().ToDoubleAndRound(0, false);
                                        }
                                    }
                                    catch
                                    {

                                        cellValue = string.Empty;
                                    }

                                    break;
                                case CellType.Numeric:
                                    if (DateUtil.IsCellDateFormatted(cell))
                                    {
                                        cellValue = cell.DateCellValue.ToString();
                                    }
                                    else
                                    {
                                        if (cell.CellStyle.GetDataFormatString().Contains("%"))
                                        {
                                            cellValue = cell.NumericCellValue.ToString().ToDoubleAndRound(2, true);
                                        }
                                        else
                                        {
                                            cellValue = cell.NumericCellValue.ToString();
                                        }

                                    }
                                    break;
                                case CellType.String:
                                    cellValue = cell.StringCellValue;
                                    break;
                            }
                            htmlBuilder.Append($"<td style='background-color:#{bgColor};color:{fontColor};'>{cellValue}</td>");


                        }
                    }
                    htmlBuilder.Append("</tr>");
                }

                htmlBuilder.Append("</table></body></html>");
            }

            File.WriteAllText(htmlFilePath, htmlBuilder.ToString());
        }
        #endregion

        #region 确保指定的行和单元格在工作表中存在

        /// <summary>
        /// 确保指定的行和单元格在工作表中存在。
        /// 如果行或单元格不存在，则创建它们。
        /// </summary>
        /// <param name="sheet">操作的工作表。</param>
        /// <param name="rowIndex">要检查或创建的行索引。</param>
        /// <param name="cellIndex">要检查或创建的单元格索引。</param>
        public static ICell RowAndCellExist(this ISheet sheet, int rowIndex, int cellIndex)
        {
            if (sheet.GetRow(rowIndex) == null)
                sheet.CreateRow(rowIndex);

            var cell = sheet.GetRow(rowIndex).GetCell(cellIndex) ?? sheet.GetRow(rowIndex).CreateCell(cellIndex);

            return cell;
        }
        /// <summary>
        /// 检查指定索引的行是否存在，如果不存在则创建该行并返回
        /// </summary>
        /// <param name="sheet">工作表对象</param>
        /// <param name="rowIndex">行索引</param>
        /// <returns>存在或新创建的行对象</returns>
        public static IRow RowExist(this ISheet sheet, int rowIndex)
        {
            if (sheet.GetRow(rowIndex) == null)
                sheet.CreateRow(rowIndex);
            return sheet.GetRow(rowIndex);
        }

        /// <summary>
        /// 检查工作簿中是否存在指定名称的工作表，如果存在则返回该工作表，否则抛出异常
        /// </summary>
        /// <param name="workbook">工作簿对象</param>
        /// <param name="sheetName">工作表名称</param>
        /// <returns>存在的工作表对象</returns>
        public static ISheet SheetExists(this IWorkbook workbook, string sheetName)
        {
            for (int i = 0; i < workbook.NumberOfSheets; i++)
            {
                if (workbook.GetSheetName(i).Equals(sheetName, StringComparison.OrdinalIgnoreCase))
                {
                    return workbook.GetSheet(sheetName);
                }
            }
            throw new Exception($"工作表 {sheetName} 不存在！");
        }
        #endregion

        #region Set Value

        /// <summary>
        /// 将指定值写入指定的单元格，并根据值的类型设置适当的格式和样式。
        /// </summary>
        /// <param name="sheet">要写入的工作表。</param>
        /// <param name="rowIndex">单元格的行索引（从 0 开始）。</param>
        /// <param name="columnIndex">单元格的列索引（从 0 开始）。</param>
        /// <param name="value">要写入的值，可以是字符串、整数、浮点数、布尔值或日期。</param>
        /// <param name="type">值的类型，用于确定如何处理和格式化该值。</param>
        /// <param name="doubleFormat">浮点数的格式，默认为 "0.00"。</param>
        /// <param name="dateFormat">日期的格式，默认为 "yyyy/MM/dd"。</param>
        /// <exception cref="NotSupportedException">当提供的类型不被支持时抛出。</exception>
        public static void SetCellValue(this ISheet sheet, int rowIndex, int columnIndex, object value, Type type, string doubleFormat = "0.00", string dateFormat = "yyyy/MM/dd")
        {
            var cell = sheet.RowAndCellExist(rowIndex, columnIndex);

            var cellStyle = cell.CellStyle;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.DBNull:
                    cell.SetBlank();
                    break;
                case TypeCode.String:
                    cell.SetCellValue(value.ToString());
                    // 设置字符串样式（例如，字体样式）
                    cellStyle.WrapText = true; // 自动换行
                    break;

                case TypeCode.Int32:
                    cell.SetCellValue(Convert.ToInt32(value));
                    // 设置整数样式
                    cellStyle.DataFormat = sheet.Workbook.CreateDataFormat().GetFormat("0"); // 整数格式
                    break;

                case TypeCode.Double:
                    cell.SetCellValue(Convert.ToDouble(value));
                    // 设置浮点数样式
                    cellStyle.DataFormat = sheet.Workbook.CreateDataFormat().GetFormat(doubleFormat);
                    break;

                case TypeCode.Boolean:
                    cell.SetCellValue(Convert.ToBoolean(value));
                    break;

                case TypeCode.DateTime:
                    cell.SetCellValue(Convert.ToDateTime(value));
                    // 设置日期样式
                    cellStyle.DataFormat = sheet.Workbook.CreateDataFormat().GetFormat(dateFormat);
                    break;

                default:
                    // 处理其他类型或抛出异常
                    throw new NotSupportedException($"不支持的类型: {type}");
            }

            // 应用样式到单元格
            cell.CellStyle = cellStyle;
        }

        /// <summary>
        /// 将指定值写入指定的单元格，并根据值的类型设置适当的格式和样式。
        /// </summary>
        /// <param name="sheet">要写入的单元格。</param>       
        /// <param name="value">要写入的值，可以是字符串、整数、浮点数、布尔值或日期。</param>
        /// <param name="type">值的类型，用于确定如何处理和格式化该值。</param>
        /// <param name="doubleFormat">浮点数的格式，默认为 "0.00"。</param>
        /// <param name="dateFormat">日期的格式，默认为 "yyyy/MM/dd"。</param>
        /// <exception cref="NotSupportedException">当提供的类型不被支持时抛出。</exception>
        public static void SetCellValue(this ICell cell, object value, Type type, string doubleFormat = "0.00", string dateFormat = "yyyy/MM/dd")
        {

            var cellStyle = cell.CellStyle;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.DBNull:
                    cell.SetBlank();
                    break;
                case TypeCode.String:
                    cell.SetCellValue(value.ToString());
                    // 设置字符串样式（例如，字体样式）
                    cellStyle.WrapText = true; // 自动换行
                    break;

                case TypeCode.Int32:
                    cell.SetCellValue(Convert.ToInt32(value));
                    // 设置整数样式
                    cellStyle.DataFormat = cell.Sheet.Workbook.CreateDataFormat().GetFormat("0"); // 整数格式
                    break;

                case TypeCode.Double:
                    cell.SetCellValue(Convert.ToDouble(value));
                    // 设置浮点数样式
                    cellStyle.DataFormat = cell.Sheet.Workbook.CreateDataFormat().GetFormat(doubleFormat);
                    break;

                case TypeCode.Boolean:
                    cell.SetCellValue(Convert.ToBoolean(value));
                    break;

                case TypeCode.DateTime:
                    cell.SetCellValue(Convert.ToDateTime(value));
                    // 设置日期样式
                    cellStyle.DataFormat = cell.Sheet.Workbook.CreateDataFormat().GetFormat(dateFormat);
                    break;

                default:
                    // 处理其他类型或抛出异常
                    throw new NotSupportedException($"不支持的类型: {type}");
            }

            // 应用样式到单元格
            cell.CellStyle = cellStyle;
        }

        /// <summary>
        /// 将指定文本写入指定的单元格
        /// </summary>
        /// <param name="sheet">工作表</param>
        /// <param name="rowIndex">单元格行索引</param>
        /// <param name="columnIndex">单元格列索引</param>
        /// <param name="text">要写入的数字</param>
        public static ICell SetCellTextValue(this ISheet sheet, int rowIndex, int columnIndex, object text)
        {
            if (sheet == null)
            {
                throw new Exception("sheet 不存在");
            }
            ICell cell = sheet.RowAndCellExist(rowIndex, columnIndex);

            cell.SetCellValue(Convert.ToString(text));
            return cell;
        }
        /// <summary>
        /// 将指定数字写入指定的单元格
        /// </summary>
        /// <param name="sheet">工作表</param>
        /// <param name="rowIndex">单元格行索引</param>
        /// <param name="columnIndex">单元格列索引</param>
        /// <param name="text">要写入的数字</param>
        public static ICell SetCellIntValue(this ISheet sheet, int rowIndex, int columnIndex, object text)
        {
            if (sheet == null)
            {
                throw new Exception("sheet 不存在");
            }
            ICell cell = sheet.RowAndCellExist(rowIndex, columnIndex);
            cell.SetCellValue(Convert.ToInt32(text));
            return cell;
        }
        /// <summary>
        /// 将指定小数写入指定的单元格
        /// </summary>
        /// <param name="sheet">工作表</param>
        /// <param name="rowIndex">单元格行索引</param>
        /// <param name="columnIndex">单元格列索引</param>
        /// <param name="text">要写入的数字</param>
        public static ICell SetCellDoubleValue(this ISheet sheet, int rowIndex, int columnIndex, object text)
        {
            if (sheet == null)
            {
                throw new Exception("sheet 不存在");
            }
            ICell cell = sheet.RowAndCellExist(rowIndex, columnIndex);
            cell.SetCellValue(Convert.ToDouble(text));
            return cell;
        }

        /// <summary>
        /// 将指定bool写入指定的单元格
        /// </summary>
        /// <param name="sheet">工作表</param>
        /// <param name="rowIndex">单元格行索引</param>
        /// <param name="columnIndex">单元格列索引</param>
        /// <param name="text">要写入的bool值</param>
        public static ICell SetCellBoolValue(this ISheet sheet, int rowIndex, int columnIndex, object text)
        {
            if (sheet == null)
            {
                throw new Exception("sheet 不存在");
            }
            ICell cell = sheet.RowAndCellExist(rowIndex, columnIndex);
            cell.SetCellValue(Convert.ToBoolean(text));
            return cell;
        }
        /// <summary>
        /// 将指定时间写入指定的单元格
        /// </summary>
        /// <param name="sheet">工作表</param>
        /// <param name="rowIndex">单元格行索引</param>
        /// <param name="columnIndex">单元格列索引</param>
        /// <param name="text">要写入的数字</param>
        public static ICell SetCellDateTimeValue(this ISheet sheet, int rowIndex, int columnIndex, object text)
        {
            if (sheet == null)
            {
                throw new Exception("sheet 不存在");
            }
            ICell cell = sheet.RowAndCellExist(rowIndex, columnIndex);
            cell.SetCellValue(text.ToDateTime());
            return cell;
        }
        /// <summary>
        /// 清除单元格内容
        /// </summary>
        /// <param name="sheet">工作表</param>
        /// <param name="rowIndex">单元格行索引</param>
        /// <param name="columnIndex">单元格列索引</param>   
        public static void CleanCellValue(this ISheet sheet, int rowIndex, int columnIndex)
        {
            if (sheet == null)
            {
                throw new Exception("sheet 不存在");
            }
            ICell cell = sheet.RowAndCellExist(rowIndex, columnIndex);
            cell.SetBlank();
        }


      

        /// <summary>
        /// 将指定值写入指定的单元格范围，并合并这些单元格。
        /// </summary>
        /// <param name="sheet">要写入的工作表。</param>
        /// <param name="startRow">合并区域的起始行索引（从 0 开始）。</param>
        /// <param name="endRow">合并区域的结束行索引（从 0 开始）。</param>
        /// <param name="startColumn">合并区域的起始列索引（从 0 开始）。</param>
        /// <param name="endColumn">合并区域的结束列索引（从 0 开始）。</param>
        /// <param name="text">要写入的值，可以是字符串、整数、浮点数、布尔值或日期。</param>
        /// <param name="type">值的类型，用于确定如何处理和格式化该值。</param>
        /// <param name="doubleFormat">浮点数的格式，默认为 "0.00"。</param>
        /// <param name="dateFormat">日期的格式，默认为 "yyyy/MM/dd"。</param>
        /// <returns>合并单元格的地址，类型为 <see cref="CellRangeAddress"/>。</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="sheet"/> 为 null 时抛出。</exception>
        /// <exception cref="NotSupportedException">当提供的 <paramref name="type"/> 不被支持时抛出。</exception>
        /// <remarks>
        /// 此方法会首先检查指定的单元格范围是否存在，如果不存在则会创建相应的行和单元格。
        /// 然后，它会取消合并该范围内的单元格（如果已经合并），并将指定的值写入合并后的单元格。
        /// 根据值的类型，方法会自动设置相应的格式和样式。
        /// </remarks>
        public static CellRangeAddress SetCellRangeTextValue(this ISheet sheet, int startRow, int endRow, int startColumn, int endColumn, object text, Type type, string doubleFormat = "0.00", string dateFormat = "yyyy/MM/dd")
        {
            if (sheet == null)
            {
                throw new Exception("sheet 不存在");
            }
            // 如果行不存在，则创建一个新行
            for (int i = startRow; i <= endRow; i++)
            {
                for (int j = startColumn; j <= endColumn; j++)
                {
                    sheet.RowAndCellExist(startRow, startColumn);
                }
            }
            sheet.UnmergeCells(startRow, endRow, startColumn, endColumn);
            CellRangeAddress cellRange = new CellRangeAddress(startRow, endRow, startColumn, endColumn);
            sheet.AddMergedRegion(cellRange);

            sheet.RowAndCellExist(startRow, startColumn).SetCellValue(text, type);

            return cellRange;
        }



        /// <summary>
        /// 将指定图片的路径写入指定范围的合并单元格
        /// </summary>
        /// <param name="sheet">工作表</param>
        /// <param name="startRow">合并单元格的起始行索引</param>
        /// <param name="endRow">合并单元格的结束行索引</param>
        /// <param name="startColumn">合并单元格的起始列索引</param>
        /// <param name="endColumn">合并单元格的结束列索引</param>
        /// <param name="imagePath">图片的路径</param>
        /// <param name="pictureType">图片的写入格式</param>
        /// <param name="scaleX">图片水平缩放比例（可选）</param>
        /// <param name="scaleY">图片垂直缩放比例（可选）</param>
        public static CellRangeAddress SetCellRangeImage(this ISheet sheet, int startRow, int endRow, int startColumn, int endColumn, string imagePath, PictureType pictureType = PictureType.JPEG, double scaleX = 0, double scaleY = 0)
        {
            if (sheet == null)
            {
                throw new Exception("sheet 不存在");
            }
            // 如果行不存在，则创建一个新行
            for (int i = startRow; i <= endRow; i++)
            {
                for (int j = startColumn; j <= endColumn; j++)
                {
                    sheet.RowAndCellExist(startRow, startColumn);
                }
            }
            sheet.UnmergeCells(startRow, endRow, startColumn, endColumn);
            CellRangeAddress cellRange = new CellRangeAddress(startRow, endRow, startColumn, endColumn);
            sheet.AddMergedRegion(cellRange);

            // 添加图片
            byte[] bytes = File.ReadAllBytes(imagePath);
            int pictureIdx = sheet.Workbook.AddPicture(bytes, pictureType);
            IDrawing drawing = sheet.CreateDrawingPatriarch();
            IClientAnchor anchor = drawing.CreateAnchor(0, 0, 0, 0, startColumn, startRow, endColumn + 1, endRow + 1);
            IPicture picture = drawing.CreatePicture(anchor, pictureIdx);
            // 调整图片大小（可选）
            if (scaleX != 0 && scaleY != 0)
            {
                picture.Resize(scaleX, scaleY);
            }

            return cellRange;
        }

        #endregion

        #region 行复制/单元格复制

        /// <summary>
        /// 在指定行的下方插入新行
        /// </summary>
        /// <param name="sheet">要插入新行的工作表</param>
        /// <param name="rowIndex">要插入新行的位置索引</param>
        /// <param name="n">要插入的新行数量</param>
        /// <exception cref="Exception">当 n 小于等于 0 时抛出异常</exception>

        public static void InsertRow(this ISheet sheet, int rowIndex, int n)
        {
            rowIndex = rowIndex + 1;
            if (n <= 0)
            {
                throw new ArgumentException("要插入的新行数量必须大于0");
            }
            // 确保 rowIndex 在有效范围内
            if (rowIndex < 0 || rowIndex > sheet.LastRowNum + 1)
            {
                throw new ArgumentOutOfRangeException("rowIndex", "行索引超出范围");
            }

            sheet.ShiftRows(rowIndex, rowIndex + 1, n, true, false);

            // 根据需要插入新行
            for (int i = 0; i < n; i++)
            {
                sheet.CreateRow(rowIndex + i);
            }
        }

        /// <summary>
        /// 将源行的内容复制到目标工作表的目标行中。
        /// </summary>
        /// <param name="targetSheet">目标工作表</param>
        /// <param name="sourceRowNum">源行号</param>
        /// <param name="targetRowNum">目标行号</param>
        /// <param name="isCopyCellValue">是否复制单元格的值</param>
        public static void CopyRow(this ISheet targetSheet, int sourceRowNum, int targetRowNum, bool isCopyCellValue = false)
        {
           var f= targetSheet.RowExist(targetRowNum).FirstCellNum;
            var l =targetSheet.RowExist(targetRowNum).LastCellNum;
           
            if (targetRowNum < 0 || targetRowNum > targetSheet.LastRowNum)
            {
                throw new ArgumentOutOfRangeException("targetRowIndex", "目标行索引超出范围");
            }
            targetSheet.CopyRow(sourceRowNum, targetRowNum);
            if (!isCopyCellValue)
            {
                for (int i = 0; i < targetSheet.GetRow(targetRowNum).LastCellNum; i++)
                {
                    targetSheet.GetCell(targetRowNum, i).SetBlank();
                }
            }

        }
        /// <summary>
        /// 复制源单元格的所有数据、格式和样式到目标单元格。
        /// </summary>
        /// <param name="sourceCell">源单元格。</param>
        /// <param name="destinationCell">目标单元格。</param>
        /// <exception cref="ArgumentNullException">当源或目标单元格为空时抛出。</exception>
        public static ICell CopyCell(ICell sourceCell, ICell destinationCell)
        {
            if (sourceCell == null || destinationCell == null)
                throw new ArgumentNullException("源或目标单元格不能为空。");

            // 复制单元格的值
            switch (sourceCell.CellType)
            {
                case CellType.String:
                    destinationCell.SetCellValue(sourceCell.StringCellValue);
                    break;
                case CellType.Numeric:
                    destinationCell.SetCellValue(sourceCell.NumericCellValue);
                    break;
                case CellType.Boolean:
                    destinationCell.SetCellValue(sourceCell.BooleanCellValue);
                    break;
                case CellType.Formula:
                    destinationCell.CellFormula = sourceCell.CellFormula;
                    break;
                case CellType.Blank:
                    destinationCell.SetCellType(CellType.Blank);
                    break;
                case CellType.Error:
                    destinationCell.SetCellErrorValue(sourceCell.ErrorCellValue);
                    break;
            }

            // 复制单元格的样式
            ICellStyle newCellStyle = destinationCell.Sheet.Workbook.CreateCellStyle();
            newCellStyle.CloneStyleFrom(sourceCell.CellStyle);
            destinationCell.CellStyle = newCellStyle;

            // 复制单元格的注释
            if (sourceCell.CellComment != null)
            {
                destinationCell.CellComment = sourceCell.CellComment;
            }

            // 复制单元格的超链接
            if (sourceCell.Hyperlink != null)
            {
                destinationCell.Hyperlink = sourceCell.Hyperlink;
            }

            // 复制单元格的富文本字符串（如果有）
            if (sourceCell.RichStringCellValue != null)
            {
                destinationCell.SetCellValue(sourceCell.RichStringCellValue);
            }

            return destinationCell;
        }

        /// <summary>
        /// 将当前工作表复制到指定名称的工作表中
        /// </summary>
        /// <param name="originalSheet">要复制的原始工作表对象</param>
        /// <param name="sheetName">新工作表的名称</param>
        /// <param name="isCopyStyle">是否复制样式，默认为 true</param>
        public static void CopyToSheet(this ISheet originalSheet, string sheetName, bool isCopyStyle = true)
        {
            originalSheet.CopySheet(sheetName, isCopyStyle);
        }
        #endregion

        #region AutoPage
        /// <summary>
        /// 根据指定的每页行数和页数，将数据复制到工作表中，并设置分页符。
        /// </summary>
        /// <param name="sheet">要操作的工作表。</param>
        /// <param name="pageSize">每页的行数，决定了数据的分页方式。</param>
        /// <param name="pageCount">总页数，决定了需要复制的页数。</param>
        /// <returns>返回工作表的最后一行索引，如果只有一页则返回最后一行索引加一。</returns>
        /// <remarks>
        /// 此方法会将指定页数的数据从第一页开始逐页复制到工作表中。
        /// 每页的行数由 <paramref name="pageSize"/> 决定，方法会在每页的最后一行设置分页符。
        /// 如果只有一页数据，则返回的值为最后一行索引加一；如果有多页，则返回最后一行索引。
        /// </remarks>
        public static int AutoPageForSheet(this ISheet sheet, int pageSize, int pageCount)
        {
            for (int p = 1; p < pageCount; p++)
            {
                for (int i = 0; i < pageSize; i++)
                {
                    sheet.CopyRow(i, p * pageSize + i);
                }
                if (p == 1)
                {
                    sheet.SetRowBreak(p * (pageSize - 1));

                }
                else
                {
                    sheet.SetRowBreak(p * pageSize - 1);
                }

            }

            return pageCount > 1 ? sheet.LastRowNum : sheet.LastRowNum + 1;
        }
        /// <summary>
        /// 根据给定的每页数据大小和数据总数，自动创建多个工作表，并返回创建的工作表名称列表。
        /// </summary>
        /// <param name="sheetTemplate">用于复制的工作表模板。</param>
        /// <param name="pageSize">每个工作表允许的最大数据行数。</param>
        /// <param name="dataCount">数据的总行数，用于计算需要创建的工作表数量。</param>
        /// <param name="sheetPageIndexName">新工作表的名称前缀，用于标识不同页面。</param>
        /// <returns>包含创建的工作表名称的列表。</returns>
        /// <remarks>
        /// 此方法首先计算需要创建的工作表数量，然后根据模板工作表复制相应数量的工作表。
        /// 每个新工作表的名称将由 <paramref name="sheetPageIndexName"/> 和页码组成，格式为 "sheetPageIndexName-页码"。
        /// </remarks>
        public static List<string> AutoPageCreateSheet(this ISheet sheetTemplate, int pageSize, int dataCount, string sheetPageIndexName)
        {
            var sheetNameList = new List<string>();
            var pageCount = dataCount.CalculatePageCount(pageSize);
            for (int i = 0; i < pageCount; i++)
            {
                sheetTemplate.CopyToSheet($"{sheetPageIndexName}-{i + 1}");
                sheetNameList.Add($"{sheetPageIndexName}-{i + 1}");
            }
            return sheetNameList;
        }




        /// <summary>
        /// 删除指定的行，将后续行上移。
        /// </summary>
        /// <param name="sheet">要删除行的工作表。</param>
        /// <param name="rowIndex">要删除的行的索引（从零开始）。</param>
        public static void DeleteRow(this ISheet sheet, int rowIndex)
        {
            // 获取要删除的行
            IRow row = sheet.GetRow(rowIndex);
            if (row != null)
            {
                // 处理与要删除的行相关的合并区域
                for (int i = 0; i < sheet.NumMergedRegions; i++)
                {
                    CellRangeAddress mergedRegion = sheet.GetMergedRegion(i);
                    if (mergedRegion.FirstRow <= rowIndex && mergedRegion.LastRow >= rowIndex)
                    {
                        // 删除与要删除的行相关的合并区域
                        sheet.RemoveMergedRegion(i);
                        i--; // 因为删除了一个合并区域，需要调整索引
                    }
                }

                sheet.RemoveRow(row);

                // 将后续行上移
                int lastRowNum = sheet.LastRowNum;
                if (rowIndex >= 0 && rowIndex < sheet.LastRowNum)
                {
                    sheet.ShiftRows(rowIndex + 1, sheet.LastRowNum, -1);
                }
            }
        }
        #endregion

        #region 合并/拆分 单元格


        /// <summary>
        /// 取消重叠的合并单元格区域
        /// </summary>
        /// <param name="sheet">要取消合并单元格的工作表</param>
        /// <param name="firstRow">要取消合并单元格的起始行索引</param>
        /// <param name="lastRow">要取消合并单元格的结束行索引</param>
        /// <param name="firstCol">要取消合并单元格的起始列索引</param>
        /// <param name="lastCol">要取消合并单元格的结束列索引</param>
        public static void UnmergeCells(this ISheet sheet, int firstRow, int lastRow, int firstCol, int lastCol)
        {
            for (int i = sheet.NumMergedRegions - 1; i >= 0; i--)
            {
                CellRangeAddress mergedRegion = sheet.GetMergedRegion(i);

                // 判断合并区域是否与指定区域重叠
                if (mergedRegion.FirstRow <= lastRow && mergedRegion.LastRow >= firstRow &&
                    mergedRegion.FirstColumn <= lastCol && mergedRegion.LastColumn >= firstCol)
                {
                    sheet.RemoveMergedRegion(i);
                }
            }
        }

        /// <summary>
        /// 指定sheet范围合并单元格
        /// </summary>
        /// <param name="sheet">工作表</param>
        /// <param name="startRow">合并单元格的起始行索引</param>
        /// <param name="endRow">合并单元格的结束行索引</param>
        /// <param name="startColumn">合并单元格的起始列索引</param>
        /// <param name="endColumn">合并单元格的结束列索引</param>
        public static void AddMergedRegion(this ISheet sheet, int startRow, int endRow, int startColumn,
            int endColumn)
        {
            sheet.UnmergeCells(startRow, endRow, startColumn, endColumn);
            CellRangeAddress cellRange = new CellRangeAddress(startRow, endRow, startColumn, endColumn);
            sheet.AddMergedRegion(cellRange);
        }
        #endregion

        #region 设置单元格样式

        /// <summary>
        /// 创建单元格边框样式
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="color">边框颜色</param>
        /// <param name="topBorder">是否让单元格的上方有边框颜色</param>
        /// <param name="rightBorder">是否让单元格的右边有边框颜色</param>
        /// <param name="font">字体样式</param>
        /// <param name="leftBorder">是否让单元格的左边有边框颜色</param>
        /// <param name="downBorder">是否让单元格的下方有边框颜色</param>
        /// <param name="horizontal">文本对齐方式</param>
        /// <param name="vertical">对齐方式</param>
        /// <param name="backgroundcolor">设置背景颜色</param>
        /// <param name="fillPattern">设置填充模式</param>
        /// <returns></returns>
        public static ICellStyle CreateBoderCellStyle(this ISheet sheet, IndexedColors color, bool topBorder, bool leftBorder, bool downBorder, bool rightBorder, IFont font = null, HorizontalAlignment horizontal = HorizontalAlignment.Center, VerticalAlignment vertical = VerticalAlignment.Center, IndexedColors backgroundcolor = null, FillPattern fillPattern = FillPattern.NoFill)
        {
            ICellStyle sharedStyle = sheet.Workbook.CreateCellStyle();
            sharedStyle.Alignment = horizontal; // 设置水平居中对齐
            sharedStyle.VerticalAlignment = vertical; // 设置垂直居中对齐

            if (!rightBorder)
            {
                sharedStyle.BorderRight = BorderStyle.None; // 如果不需要底部边框，则设置为无边框
            }
            else
            {
                sharedStyle.BorderRight = BorderStyle.Medium; // 设置右边框为中等粗细
                sharedStyle.RightBorderColor = color.Index; // 设置右边框颜色
            }

            if (!leftBorder)
            {
                sharedStyle.BorderLeft = BorderStyle.None; // 如果不需要底部边框，则设置为无边框
            }
            else
            {
                sharedStyle.BorderLeft = BorderStyle.Medium; // 设置左边框为中等粗细
                sharedStyle.LeftBorderColor = color.Index; // 设置左边框颜色
            }

            if (!downBorder)
            {
                sharedStyle.BorderBottom = BorderStyle.None; // 如果不需要底部边框，则设置为无边框
            }
            else
            {
                sharedStyle.BorderBottom = BorderStyle.Medium; // 设置底部边框为中等粗细
                sharedStyle.BottomBorderColor = color.Index; // 设置底部边框颜色
            }

            if (!topBorder)
            {
                sharedStyle.BorderTop = BorderStyle.None; // 如果不需要顶部边框，则设置为无边框
            }
            else
            {
                sharedStyle.BorderTop = BorderStyle.Medium; // 设置顶部边框为中等粗细
                sharedStyle.TopBorderColor = color.Index; // 设置顶部边框颜色
            }

            if (font == null)
            {
                IFont font1 = sheet.Workbook.CreateFont();
                font1.FontName = "宋体"; // 设置字体为宋体
                font1.FontHeightInPoints = 9; // 设置字体大小为9
                font = font1;
            }
            sharedStyle.SetFont(font); // 设置单元格样式的字体
            if (backgroundcolor != null)
            {
                // 设置背景颜色
                sharedStyle.FillForegroundColor = backgroundcolor.Index; // 设置背景颜色
                sharedStyle.FillPattern = fillPattern; // 设置填充模式
                /*
                 *  NoFill：无填充，即不设置任何背景填充。
                    SolidForeground：纯色填充，使用单一颜色填充单元格背景。
                    FineDots：细小点状填充。
                    AltBars：交替条纹填充。
                    SparseDots：稀疏点状填充。
                    ThickHorizontalBands：粗水平条纹填充。
                    ThickVerticalBands：粗垂直条纹填充。
                    ThickBackwardDiagonals：粗逆对角线填充。
                    ThickForwardDiagonals：粗顺对角线填充。
                    BigSpots：大点状填充。
                    Bricks：砖块填充。
                    ThinHorizontalBands：细水平条纹填充。
                    ThinVerticalBands：细垂直条纹填充。
                    ThinBackwardDiagonals：细逆对角线填充。
                    ThinForwardDiagonals：细顺对角线填充。
                 */
            }

            return sharedStyle;
        }
        /// <summary>
        /// 设置指定范围内的单元格边框颜色。
        /// </summary>
        /// <param name="sheet">要设置边框的工作表</param>
        /// <param name="startRow">起始行号</param>
        /// <param name="endRow">结束行号</param>
        /// <param name="startColumn">起始列号</param>
        /// <param name="endColumn">结束列号</param>
        /// <param name="color">边框颜色</param>
        /// <param name="topBorder">是否让单元格的上方有边框颜色</param>
        /// <param name="rightBorder">是否让单元格的右边有边框颜色</param>
        /// <param name="font">字体样式</param>
        /// <param name="leftBorder">是否让单元格的左边有边框颜色</param>
        /// <param name="downBorder">是否让单元格的下方有边框颜色</param>
        /// <param name="horizontal">文本对齐方式</param>
        /// <param name="vertical">对齐方式</param>
        /// <param name="backgroundcolor">设置背景颜色</param>
        /// <param name="fillPattern">设置填充模式</param>
        public static void SetCellBordersColor(this ISheet sheet, int startRow, int endRow, int startColumn, int endColumn, IndexedColors color, bool topBorder, bool leftBorder, bool downBorder, bool rightBorder, IFont font = null, HorizontalAlignment horizontal = HorizontalAlignment.Center, VerticalAlignment vertical = VerticalAlignment.Center, IndexedColors backgroundcolor = null, FillPattern fillPattern = FillPattern.NoFill)
        {
            var style = sheet.CreateBoderCellStyle(color, topBorder, leftBorder, downBorder, rightBorder, font, horizontal, vertical, backgroundcolor, fillPattern);
            for (int rowNum = startRow; rowNum <= endRow; rowNum++)
            {
                for (int colNum = startColumn; colNum <= endColumn; colNum++)
                {
                    ICell cell = sheet.RowAndCellExist(rowNum, colNum);
                    cell.CellStyle = null; // 清除单元格样式
                    cell.CellStyle = style;
                }
            }

        }

        public static void SetFontStyle(this CellRangeAddress cellRange, IFont font, ISheet sheet)
        {
            // 获取单元格范围内的所有行和列
            for (int row = cellRange.FirstRow; row <= cellRange.LastRow; row++)
            {
                for (int column = cellRange.FirstColumn; column <= cellRange.LastColumn; column++)
                {
                    ICell cell = sheet.RowAndCellExist(row, column);
                    cell.CellStyle.SetFont(font);
                }
            }
        }


        #endregion

        #region DataTable 的导入导出

        /// <summary>
        /// 从 Excel 文件中导入数据到 DataTable 列表中。
        /// </summary>
        /// <param name="filePath">Excel 文件的路径。</param>
        /// <param name="titleRowIndex">头部起始行。</param>
        /// <param name="dataSkip">数据 起始行。</param>
        /// <param name="columnValidators">用于验证特定列值的列验证器字典。</param>
        /// <returns>包含导入数据的 DataTable 列表。</returns>
        public static List<DataTable> ExcelImportDataTable(this string filePath, int titleRowIndex = 0, int dataSkip = 0,
            Dictionary<string, Func<object, bool>> columnValidators = null)
        {
            List<DataTable> dataTables = new List<DataTable>();

            using (var workBook = filePath.OpenExcel())
            {
                var sheetCount = workBook.NumberOfSheets;
                for (int i = 0; i < sheetCount; i++)
                {
                    var worksheet = workBook.GetSheetAt(i);
                    var dataTable = new DataTable(worksheet.SheetName);

                    // 读取表头行
                    var headerRow = worksheet.GetRow(titleRowIndex);
                    for (int col = 0; col < headerRow.LastCellNum; col++)
                    {
                        var columnHeader = headerRow.GetCell(col)?.ToString();
                        dataTable.Columns.Add(columnHeader);
                    }

                    // 读取数据行
                    for (int row = 1; row <= worksheet.LastRowNum; row++)
                    {
                        if (row <= dataSkip)
                        {
                            continue;
                        }
                        var dataRow = dataTable.NewRow();
                        var currentRow = worksheet.GetRow(row);

                        for (int col = 0; col < currentRow.LastCellNum; col++)
                        {
                            var cell = currentRow.GetCell(col);
                            var cellValue = cell.GetCellValue();

                            dataRow[col] = cellValue;
                        }

                        if (columnValidators != null)
                        {
                            // 验证特定列的值
                            foreach (var columnValidator in columnValidators)
                            {
                                var columnName = columnValidator.Key;
                                var validator = columnValidator.Value;

                                var columnValue = dataRow[columnName];
                                if (columnValue != null && columnValue != DBNull.Value)
                                {
                                    if (!validator(columnValue))
                                    {
                                        throw new Exception($"第 {row + 1} 行的 '{columnName}' 列的值不符合要求。");
                                    }
                                }
                            }
                        }

                        dataTable.Rows.Add(dataRow);
                    }

                    dataTables.Add(dataTable);
                }

                return dataTables;
            }

        }


        /// <summary>
        /// 从 Excel 文件中读取数据并将其转换为 DataTable
        /// </summary>
        /// <param name="filePath">Excel 文件路径</param>
        /// <param name="sheetName">工作表名称</param>
        /// <param name="skip">要跳过的行数</param>
        /// <returns>转换后的 DataTable</returns>
        public static DataTable ExcelToDataTable(this string filePath, string sheetName = "", int skip = 0)
        {
            // 打开 Excel 文件
            using (var workBook = filePath.OpenExcel())
            {
                // 获取工作表
                var worksheet = string.IsNullOrEmpty(sheetName) ? workBook.GetSheetAt(0) : workBook.GetSheet(sheetName);

                var dataTable = new DataTable(sheetName);

                // 读取表头行
                var headerRow = worksheet.GetRow(skip);
                for (int col = 0; col < headerRow.LastCellNum; col++)
                {
                    var columnHeader = headerRow.GetCell(col)?.ToString();
                    dataTable.Columns.Add(columnHeader);
                }

                // 读取数据行
                for (int row = 1; row <= worksheet.LastRowNum; row++)
                {
                    if (row <= skip) continue;

                    var dataRow = dataTable.NewRow();
                    var currentRow = worksheet.GetRow(row);


                    for (int col = 0; col < currentRow.LastCellNum; col++)
                    {
                        var cell = currentRow.GetCell(col);
                        var cellValue = cell?.GetCellValue();

                        dataRow[col] = cellValue;
                    }
                    dataTable.Rows.Add(dataRow);
                }

                return dataTable;
            }
        }

        /// <summary>
        /// 将工作表转换为 DataTable，其中工作表的第一行作为列标题。
        /// </summary>
        /// <param name="worksheet">要转换为 DataTable 的工作表</param>
        /// <param name="titleRowIndex">列标题所在行的索引，默认为 0</param>
        /// <param name="dataSkip">跳过的数据行数，默认为 0</param>
        /// <param name="columnValidators">包含列名和验证函数的字典，用于验证特定列的值</param>
        /// <returns>转换后的 DataTable</returns>
        public static DataTable SheetToDateTable(this ISheet worksheet, int titleRowIndex = 0, int dataSkip = 0, Dictionary<string, Func<object, bool>> columnValidators = null)
        {
            var dataTable = new DataTable(worksheet.SheetName);

            // 读取表头行
            var headerRow = worksheet.GetRow(titleRowIndex);
            for (int col = 0; col < headerRow.LastCellNum; col++)
            {
                var columnHeader = headerRow.GetCell(col)?.ToString();
                dataTable.Columns.Add(columnHeader);
            }

            // 读取数据行
            for (int row = titleRowIndex + 1; row <= worksheet.LastRowNum; row++)
            {
                if (row <= dataSkip) continue;
                var dataRow = dataTable.NewRow();
                var currentRow = worksheet.GetRow(row);

                for (int col = 0; col < currentRow.LastCellNum; col++)
                {
                    var cell = currentRow.GetCell(col);
                    var cellValue = cell.GetCellValue();

                    dataRow[col] = cellValue;
                }

                if (columnValidators != null)
                {
                    // 验证特定列的值
                    foreach (var columnValidator in columnValidators)
                    {
                        var columnName = columnValidator.Key;
                        var validator = columnValidator.Value;

                        var columnValue = dataRow[columnName];
                        if (columnValue != null && columnValue != DBNull.Value)
                        {
                            if (!validator(columnValue))
                            {
                                throw new Exception($"{worksheet.SheetName} 第 {row + 1} 行的 '{columnName}' 列的值不符合要求。");
                            }
                        }
                    }
                }

                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        #endregion

        #region 插入公式
        /// <summary>
        /// 在指定的 Excel 单元格中插入公式。
        /// </summary>
        /// <param name="sheet">要插入公式的工作表。</param>
        /// <param name="rowIndex">单元格的行索引（从零开始）。</param>
        /// <param name="columnIndex">单元格的列索引（从零开始）。</param>
        /// <param name="formula">要插入的公式，作为字符串。</param>
        public static void InsertFormula(this ISheet sheet, int rowIndex, int columnIndex, string formula)
        {
            // 获取行，如果不存在则创建
            IRow row = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);

            // 获取单元格，如果不存在则创建
            ICell cell = row.GetCell(columnIndex) ?? row.CreateCell(columnIndex);

            // 设置单元格类型为公式
            cell.SetCellType(CellType.Formula);

            // 设置单元格公式
            cell.SetCellFormula(formula);
        }



        #endregion

        #region 删除sheet页
        /// <summary>
        /// 根据sheet名删除sheet
        /// </summary>
        /// <param name="workbook"></param>
        /// <param name="sheetName"></param>
        public static void DeleteSheetByName(this IWorkbook workbook, string sheetName)
        {
            for (int i = 0; i < workbook.NumberOfSheets; i++)
            {
                ISheet sheet = workbook.GetSheetAt(i);
                if (sheet.SheetName == sheetName)
                {
                    workbook.RemoveSheetAt(i);
                    break; // 找到并删除后跳出循环
                }
            }
        }
        /// <summary>
        /// 删除自身sheet
        /// </summary>
        /// <param name="rSheet"></param>
        public static void DeleteSheet(this ISheet rSheet)
        {
            var workbook = rSheet.Workbook;
            for (int i = 0; i < workbook.NumberOfSheets; i++)
            {
                ISheet sheet = workbook.GetSheetAt(i);
                if (sheet.SheetName == rSheet.SheetName)
                {
                    workbook.RemoveSheetAt(i);
                    break; // 找到并删除后跳出循环
                }
            }
        }
        #endregion
    }
}
