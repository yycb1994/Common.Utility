using log4net;
using log4net.Config;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Common.Repositoy.SqlSugar.Core
{
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Ado 操作
        /// </summary>
        IAdo Ado { get; }
        AopProvider Aop { get; }
        bool IsTransactionActive { get; }
        /// <summary>
        /// 开始事务
        /// </summary>
        void BeginTran();


        /// <summary>
        /// 提交事务
        /// </summary>
        void Commit();
        void UnitOfWorkCommit();

        /// <summary>
        /// 回滚事务
        /// </summary>
        void Rollback();

    }


    /// <summary>
    /// 泛型接口定义工作单元模式，用于管理仓储和事务
    /// </summary>
    /// <typeparam name="TContext">工作单元所需的DbContext类型</typeparam>
    public interface IUnitOfWork<TContext> : IUnitOfWork where TContext : BaseDbClient
    {
        /// <summary>
        /// 获取特定实体类型的仓储
        /// </summary>
        ISimpleClient<TEntity> Repository<TEntity>() where TEntity : class, new();
        /// <summary>
        /// 执行SQL命令
        /// </summary>
        /// <param name="sql">SQL命令</param>
        /// <param name="parameters">参数</param>
        /// <returns>命令执行成功与否</returns>
        bool ExecuteCommand(string sql, params SugarParameter[] parameters);
        /// <summary>
        /// 查询数据表
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>数据表</returns>
        DataTable QueryDataTable(string sql, params SugarParameter[] parameters);
        /// <summary>
        /// 获取列表数据
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>数据列表</returns>
        List<T> GetList<T>(string sql, params SugarParameter[] parameters);

        ISugarQueryable<T> QueryableQuery<T>() where T : class, new();
        /// <summary>
        /// 获取单体数据
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>数据列表</returns>
        T GetSingle<T>(string sql, params SugarParameter[] parameters);
        /// <summary>
        /// 执行count 返回总数
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        int GetCount(string sql, params SugarParameter[] parameters);
    }

    /// <summary>
    /// 实现工作单元模式的基本功能
    /// </summary>
    /// <typeparam name="TContext">工作单元所需的DbContext类型</typeparam>
    public class UnitOfWork<TContext> : IUnitOfWork<TContext> where TContext : BaseDbClient
    {
        // 使用ThreadLocal来管理事务状态，确保每个线程都有独立的事务状态
        private static readonly ThreadLocal<bool> _isTransactionActive = new ThreadLocal<bool>(() => false);
        private readonly object _lock = new object();
        private bool _disposed;
        private readonly TContext _context;
        private readonly ILog _loggerHelper;

        /// <summary>
        /// 构造函数，初始化工作单元
        /// </summary>
        /// <param name="context">工作单元所需的DbContext实例</param>
        /// <param name="unitOfWorkConfig">基础参数配置</param>
        /// <exception cref="Exception">如果配置为空则抛出异常</exception>
        public UnitOfWork(TContext context, UnitOfWorkConfig unitOfWorkConfig)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            if (unitOfWorkConfig == null)
            {
                throw new ArgumentNullException(nameof(unitOfWorkConfig), "使用UnitOfWork前需要注入UnitOfWorkConfig");
            }

            _loggerHelper = LogManager.GetLogger(typeof(TContext));
            ConfigureLogging(unitOfWorkConfig);
        }

        private void ConfigureLogging(UnitOfWorkConfig unitOfWorkConfig)
        {
            XmlConfigurator.Configure(new FileInfo(unitOfWorkConfig.Log4NetConfigPath));
            _context.Aop.OnLogExecuting = (sql, pars) =>
            {
                if (unitOfWorkConfig.IsEnableLog)
                {
                    var logContent = $"{sql}\r\n{_context.Ado.Context.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value))}";
                    _loggerHelper.Debug(logContent);
                }
            };
        }

        public IAdo Ado => _context.Ado; // 获取Ado实例
        public AopProvider Aop => _context.Aop; // 获取Aop实例

        public bool IsTransactionActive => _isTransactionActive.Value; // 获取当前事务状态

        /// <summary>
        /// 获取指定实体类型的仓储实例
        /// </summary>
        /// <typeparam name="TEntity">实体类型</typeparam>
        /// <returns>仓储实例</returns>
        public ISimpleClient<TEntity> Repository<TEntity>() where TEntity : class, new()
        {
            return _context.UnitOfWorkCilent.GetRepository<TEntity>();
        }
        public ISugarQueryable<T> QueryableQuery<T>() where T : class, new()
        {
            return Repository<T>().AsQueryable();
        }
        /// <summary>
        /// 执行SQL命令
        /// </summary>
        /// <param name="sql">SQL命令</param>
        /// <param name="parameters">参数</param>
        /// <returns>命令执行成功与否</returns>
        public bool ExecuteCommand(string sql, params SugarParameter[] parameters)
        {
            return Ado.ExecuteCommand(sql, parameters) > 0;
        }

        /// <summary>
        /// 查询数据表
        /// </summary>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>数据表</returns>
        public DataTable QueryDataTable(string sql, params SugarParameter[] parameters)
        {
            return Ado.GetDataTable(sql, parameters);
        }

        /// <summary>
        /// 获取列表数据
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>数据列表</returns>
        public List<T> GetList<T>(string sql, params SugarParameter[] parameters)
        {
            return Ado.SqlQuery<T>(sql, parameters);
        }

        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <typeparam name="T">返回的数据类型</typeparam>
        /// <param name="sql">SQL查询语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>单个数据</returns>
        public T GetSingle<T>(string sql, params SugarParameter[] parameters)
        {
            return Ado.SqlQuerySingle<T>(sql, parameters);
        }
        public int GetCount(string sql, params SugarParameter[] parameters)
        {
            return Ado.GetInt(sql, parameters);
        }

        /// <summary>
        /// 开始事务
        /// </summary>
        public void BeginTran()
        {
            lock (_lock)
            {
                if (!_isTransactionActive.Value)
                {
                    try
                    {
                        Ado.BeginTran();
                        _isTransactionActive.Value = true;
                    }
                    catch (Exception ex)
                    {
                        _loggerHelper.Error("启动事务时出错", ex);
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public void Commit()
        {
            lock (_lock)
            {
                if (_isTransactionActive.Value)
                {
                    try
                    {
                        Ado.CommitTran();
                        _isTransactionActive.Value = false;
                    }
                    catch (Exception ex)
                    {
                        _loggerHelper.Error("提交事务时出错", ex);
                        Rollback(); // 回滚事务
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 提交工作单元
        /// </summary>
        public void UnitOfWorkCommit()
        {
            lock (_lock)
            {
                if (_isTransactionActive.Value)
                {
                    try
                    {
                        _context.UnitOfWorkCilent.Commit();
                        _isTransactionActive.Value = false;
                    }
                    catch (Exception ex)
                    {
                        _loggerHelper.Error("提交工作单元时出错", ex);
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public void Rollback()
        {
            lock (_lock)
            {
                if (_isTransactionActive.Value)
                {
                    try
                    {
                        Ado.RollbackTran();
                        _isTransactionActive.Value = false;
                    }
                    catch (Exception ex)
                    {
                        _loggerHelper.Error("回滚事务时出错", ex);
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                Ado.Dispose(); // 释放Ado资源
                if (_context is IDisposable disposableContext)
                {
                    disposableContext.Dispose(); // 释放DbContext资源
                }
                _disposed = true;
            }
            _loggerHelper.Debug(" Ado.Dispose");
            GC.SuppressFinalize(this); // 防止调用对象的终结器
        }


    }
}
