using Common.Utility.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Common.Utility.Extensions
{
    /// <summary>
    /// XML 扩展方法类，提供 XML 序列化和反序列化功能。
    /// </summary>
    public static class XmlExpand
    {
        /// <summary>
        /// 将实体列表序列化并保存到指定 XML 文件。
        /// </summary>
        /// <typeparam name="TSource">要序列化的实体类型。</typeparam>
        /// <param name="list">要保存的实体列表。</param>
        /// <param name="filePath">XML 文件的路径。</param>
        /// <exception cref="Exception">如果保存 XML 时发生错误。</exception>
        public static void SaveXml<TSource>(this IList<TSource> list, string filePath)
        {
            if (Path.GetExtension(filePath).ToLower() != ".xml")
                throw new Exception($"{filePath} 格式不正确！");

            FileHelper.CreateFolderByFilePath(filePath);
            FileHelper.CreateFile(filePath);

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<TSource>));
                using (Stream writer = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    xmlSerializer.Serialize(writer, list);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"保存 XML 时出错: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 从指定 XML 文件中读取并反序列化为单个实体。
        /// </summary>
        /// <typeparam name="TSource">要反序列化的实体类型。</typeparam>
        /// <param name="filePath">XML 文件的路径。</param>
        /// <returns>反序列化的实体，如果未找到则返回 null。</returns>
        /// <exception cref="Exception">如果读取 XML 时发生错误。</exception>
        public static TSource GetObject<TSource>(this string filePath)
        {
            if (Path.GetExtension(filePath).ToLower() != ".xml")
                throw new Exception($"{filePath} 格式不正确！");

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"{filePath} 不存在！");

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<TSource>));
                using (Stream reader = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var listdata = xmlSerializer.Deserialize(reader) as List<TSource>;
                    return listdata.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"读取 XML 时出错: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 从指定 XML 文件中读取并反序列化为实体列表。
        /// </summary>
        /// <typeparam name="TSource">要反序列化的实体类型。</typeparam>
        /// <param name="filePath">XML 文件的路径。</param>
        /// <returns>反序列化的实体列表。</returns>
        /// <exception cref="Exception">如果读取 XML 时发生错误。</exception>
        public static List<TSource> GetList<TSource>(this string filePath)
        {
            if (Path.GetExtension(filePath).ToLower() != ".xml")
                throw new Exception($"{filePath} 格式不正确！");

            if (!File.Exists(filePath))
                throw new FileNotFoundException($"{filePath} 不存在！");

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<TSource>));
                using (Stream reader = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var listdata = xmlSerializer.Deserialize(reader) as List<TSource>;
                    return listdata ?? new List<TSource>(); // 返回空列表而不是 null
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"读取 XML 时出错: {ex.Message}", ex);
            }
        }
    }
}
