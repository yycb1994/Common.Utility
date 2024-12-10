using SqlSugar;
using System;

namespace Common.Repositoy.SqlSugar
{
    /// <summary>
    /// 数据库实例基类
    /// </summary>
    public abstract class BaseDbClient : IDisposable
    {
        private readonly SqlSugarClient _db;
        private bool _disposed = false; // 用于跟踪是否已经释放资源

        /// <summary>
        /// 获取数据库客户端实例。
        /// </summary>
        public virtual SugarUnitOfWork UnitOfWorkCilent { get; }
        public virtual IAdo Ado => _db.Ado; // 直接返回 Ado 实例
        public virtual AopProvider Aop => _db.Aop; // 直接返回 Aop 实例

        /// <summary>
        /// 基类构造函数，初始化数据库客户端实例。
        /// </summary>
        /// <param name="connectionString">数据库连接字符串。</param>
        /// <param name="dbType">数据库类型。</param>
        /// <param name="isTran">是否默认开启事务。</param>
        protected BaseDbClient(string connectionString, DbType dbType, bool isTran = false)
        {
            _db = new SqlSugarClient(new ConnectionConfig
            {
                ConnectionString = connectionString,
                DbType = dbType,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });

            UnitOfWorkCilent = _db.CreateContext(isTran);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放托管和非托管资源
        /// </summary>
        /// <param name="disposing">指示是否释放托管资源</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // 释放托管资源
                    _db?.Dispose();
                }
                _disposed = true;
            }
        }


        ~BaseDbClient()
        {
            Dispose(false);
        }
    }
}
