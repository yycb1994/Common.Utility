using Common.Utility.CustomerModel;
using System;
using System.Collections.Generic;
using Common.Utility.Extensions;
using System.Linq;

namespace Common.Utility.Helper
{
    public static class XmlEntityProcess<T> where T : XmlBaseEntity
    {
        public static List<T> GetAll(string xmlPath)
        {
            var list = xmlPath.GetList<T>();
            return list;
        }
        public static Guid Update(T entity, string xmlPath)
        {
            List<T> list = xmlPath.GetList<T>();
            list.Remove(entity);
            list.Add(entity);
            list.SaveXml(xmlPath);
            return entity.ID;
        }
        public static Guid Insert(T entity, string xmlPath)
        {
            List<T> list = xmlPath.GetList<T>();
            list.Add(entity);
            list.SaveXml(xmlPath);
            return entity.ID;
        }
        public static T GetById(Guid id, string xmlPath)
        {
            return GetAll(xmlPath).FirstOrDefault(c => c.ID == id);
        }
       
        public static bool DeleteById(Guid id,string xmlPath)
        {
            var list = xmlPath.GetList<T>();
            list.Where(c => c.ID != id).ToList().SaveXml(xmlPath);
            return true;
        }

    }
}
