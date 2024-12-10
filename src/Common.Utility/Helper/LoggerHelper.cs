using log4net;
using log4net.Config;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Common.Utility.Helper
{
    public class LoggerHelper<T>
    {
        private readonly ILog _log;
        public LoggerHelper()
        {
            string xmlConfiguratorPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config");
            if (!File.Exists(xmlConfiguratorPath))
            {
                xmlConfiguratorPath = "log4net.config";
            }
            Console.WriteLine(xmlConfiguratorPath);
            _log = LogManager.GetLogger(typeof(T));
            XmlConfigurator.Configure(new FileInfo(xmlConfiguratorPath));
        }


        public void Debug(string message)
        {
            _log.Debug(message);
        }
        public void Debug(string title, string message)
        {
            _log.Debug($"{title} -> {message}");
        }
        public void Info(string title, string message)
        {
            _log.Info($"{title} -> {message}");
        }
        public void Info(string message)
        {
            _log.Info(message);
        }

        public void Warn(string message)
        {
            _log.Warn(message);
        }

        public void Error(string message, Exception ex)
        {
            _log.Error(message, ex);
        }
        public void Error(string title, string message, Exception ex)
        {
            _log.Error($"{title} -> {message}", ex);
        }
        public void Fatal(string message)
        {
            _log.Fatal(message);
        }
    }
}
