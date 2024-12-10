using Castle.DynamicProxy;
using Common.Utility.CustomerAttribute;
using Common.Utility.CustomerModel;
using Common.Utility.Helper;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using RestSharp.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utility.AutoFacIoc
{
    public class LogAop : IInterceptor
    {
        LoggerHelper<LogAop> _logger;
        public LogAop(LoggerHelper<LogAop> logger) { _logger = logger; }
        public void Intercept(IInvocation invocation)
        {
            var method = invocation.MethodInvocationTarget ?? invocation.Method;
            var isEnableLog = method.GetCustomAttributes(typeof(LogRecordAttribute), true).Length != 0;
            if (!isEnableLog)
            {
                // 调用实际的方法
                invocation.Proceed();
                return;
            }
            string json;
            try
            {
                json = JsonConvert.SerializeObject(invocation.Arguments);
            }
            catch (Exception ex)
            {
                json = "无法序列化，可能是兰姆达表达式等原因造成，按照框架优化代码" + ex.ToString();
            }
            DateTime startTime = DateTime.Now;
            AopLogInfo apiLogAopInfo = new AopLogInfo
            {
                RequestTime = startTime.ToString("yyyy-MM-dd hh:mm:ss fff"),              
                RequestMethodName = invocation.Method.Name,
                RequestParamsName = string.Join(", ", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray()),
                ResponseJsonData = json
            };
            try
            {
                _logger.Debug($"Begin Method  ：{invocation.Method.Name}() -> ");
                //MiniProfiler.Current.Step($"执行Service方法：{invocation.Method.Name}() -> ");
                //在被拦截的方法执行完毕后 继续执行当前方法，注意是被拦截的是异步的
                invocation.Proceed();
                // 异步获取异常，先执行
                if (IsAsyncMethod(invocation.Method))
                {
                    var type = invocation.Method.ReturnType;
                    var resultProperty = type.GetProperty("Result");
                    DateTime endTime = DateTime.Now;
                    string ResponseTime = (endTime - startTime).Milliseconds.ToString();
                    apiLogAopInfo.ResponseTime = endTime.ToString("yyyy-MM-dd hh:mm:ss fff");
                    apiLogAopInfo.ResponseIntervalTime = ResponseTime + "ms";
                    apiLogAopInfo.ResponseJsonData = JsonConvert.SerializeObject(resultProperty.GetValue(invocation.ReturnValue));
                    _logger.Debug(JsonConvert.SerializeObject(apiLogAopInfo));
                }
                else
                {
                    string jsonResult;
                    try
                    {
                        jsonResult = JsonConvert.SerializeObject(invocation.ReturnValue);
                    }
                    catch (Exception ex)
                    {
                        jsonResult = "无法序列化，可能是兰姆达表达式等原因造成，按照框架优化代码" + ex.ToString();
                    }

                    var type = invocation.Method.ReturnType;
                    var resultProperty = type.GetProperty("Result");
                    DateTime endTime = DateTime.Now;
                    string ResponseTime = (endTime - startTime).Milliseconds.ToString();
                    apiLogAopInfo.ResponseTime = endTime.ToString("yyyy-MM-dd hh:mm:ss fff");
                    apiLogAopInfo.ResponseIntervalTime = ResponseTime + "ms";
                    //apiLogAopInfo.ResponseJsonData = JsonConvert.SerializeObject(resultProperty.GetValue(invocation.ReturnValue));
                    apiLogAopInfo.ResponseJsonData = jsonResult;
                    //dataIntercept += ($"【执行完成结果】：{jsonResult}");
                    Parallel.For(0, 1, e =>
                    {
                        _logger.Debug($"LogInfo -> {JsonConvert.SerializeObject(apiLogAopInfo)}");
                        _logger.Debug($"End Method  ：{invocation.Method.Name}()");
                    });
                }
            }
            catch (Exception ex)
            {
                LogEx(ex, apiLogAopInfo);
                throw;
            }

        }
        private void LogEx(Exception ex, AopLogInfo dataIntercept)
        {
            if (ex != null)
            {
                //执行的 service 中，收录异常
                //  MiniProfiler.Current.CustomTiming("Errors：", ex.Message);
                //执行的 service 中，捕获异常
                //dataIntercept += ($"【执行完成结果】：方法中出现异常：{ex.Message + ex.InnerException}\r\n");
                AopLogExInfo apiLogAopExInfo = new AopLogExInfo
                {
                    ExMessage = ex.Message,
                    InnerException = "InnerException-内部异常:\r\n" + (ex.InnerException == null ? "" : ex.InnerException.InnerException.ToString()) +
                                     ("\r\nStackTrace-堆栈跟踪:\r\n") + (ex.StackTrace == null ? "" : ex.StackTrace.ToString()),
                    ApiLogAopInfo = dataIntercept
                };
                // 异常日志里有详细的堆栈信息
                Parallel.For(0, 1, e =>
                {
                    _logger.Debug(JsonConvert.SerializeObject(apiLogAopExInfo));
                    //LogLock.OutLogAOP("AOPLogEx", new string[] { dataIntercept });
                    // LogLock.OutLogAOP("AOPLogEx", _accessor.HttpContext?.TraceIdentifier,
                    //    new string[] { apiLogAopExInfo.GetType().ToString(), JsonConvert.SerializeObject(apiLogAopExInfo) });
                });
            }
        }


        public static bool IsAsyncMethod(MethodInfo method)
        {
            return (
                method.ReturnType == typeof(Task) ||
                (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            );
        }
    }
}
