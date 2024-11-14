using Common.Utility.CustomerModel;
using System;
using System.Collections.Generic;
using Common.Utility.Extensions;
using System.Linq;

namespace Common.Utility.Helper
{
    /// <summary>
    /// 处理 XML 文件的通用类，用于对 XmlBaseEntity 类型的实体进行基本操作。
    /// </summary>
    /// <typeparam name="T">必须是 XmlBaseEntity 的类型。</typeparam>
    public static class XmlEntityProcess<T> where T : XmlBaseEntity
    {
        private static readonly object _lock = new object(); // 用于线程安全的锁

        /// <summary>
        /// 获取指定 XML 文件中的所有实体。
        /// </summary>
        /// <param name="xmlPath">XML 文件的路径。</param>
        /// <returns>所有实体的列表。</returns>
        /// <exception cref="Exception">如果读取 XML 时发生错误。</exception>
        public static List<T> GetAll(string xmlPath)
        {
            try
            {
                return xmlPath.GetList<T>();
            }
            catch (Exception ex)
            {
                throw new Exception($"获取所有实体时出错: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 更新指定实体并保存到 XML 文件。
        /// </summary>
        /// <param name="entity">要更新的实体。</param>
        /// <param name="xmlPath">XML 文件的路径。</param>
        /// <returns>更新实体的 ID。</returns>
        /// <exception cref="Exception">如果更新实体时发生错误。</exception>
        public static Guid Update(T entity, string xmlPath)
        {
            lock (_lock) // 确保线程安全
            {
                try
                {
                    List<T> list = xmlPath.GetList<T>();
                    list.Remove(entity);
                    list.Add(entity);
                    list.SaveXml(xmlPath);
                    return entity.ID;
                }
                catch (Exception ex)
                {
                    throw new Exception($"更新实体时出错: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// 插入新实体并保存到 XML 文件。
        /// </summary>
        /// <param name="entity">要插入的实体。</param>
        /// <param name="xmlPath">XML 文件的路径。</param>
        /// <returns>插入实体的 ID。</returns>
        /// <exception cref="Exception">如果插入实体时发生错误。</exception>
        public static Guid Insert(T entity, string xmlPath)
        {
            lock (_lock) // 确保线程安全
            {
                try
                {
                    List<T> list = xmlPath.GetList<T>();
                    list.Add(entity);
                    list.SaveXml(xmlPath);
                    return entity.ID;
                }
                catch (Exception ex)
                {
                    throw new Exception($"插入实体时出错: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// 根据 ID 获取指定 XML 文件中的实体。
        /// </summary>
        /// <param name="id">要查找的实体 ID。</param>
        /// <param name="xmlPath">XML 文件的路径。</param>
        /// <returns>找到的实体，如果未找到则返回 null。</returns>
        /// <exception cref="Exception">如果获取实体时发生错误。</exception>
        public static T GetById(Guid id, string xmlPath)
        {
            try
            {
                return GetAll(xmlPath).FirstOrDefault(c => c.ID == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"根据 ID 获取实体时出错: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 根据 ID 删除指定 XML 文件中的实体。
        /// </summary>
        /// <param name="id">要删除的实体 ID。</param>
        /// <param name="xmlPath">XML 文件的路径。</param>
        /// <returns>如果删除成功则返回 true，否则返回 false。</returns>
        /// <exception cref="Exception">如果删除实体时发生错误。</exception>
        public static bool DeleteById(Guid id, string xmlPath)
        {
            lock (_lock) // 确保线程安全
            {
                try
                {
                    var list = xmlPath.GetList<T>();
                    var entityToRemove = list.FirstOrDefault(c => c.ID == id);
                    if (entityToRemove != null)
                    {
                        list.Remove(entityToRemove);
                        list.SaveXml(xmlPath);
                        return true;
                    }
                    return false; // ID 不存在
                }
                catch (Exception ex)
                {
                    throw new Exception($"删除实体时出错: {ex.Message}", ex);
                }
            }
        }
    }
}
