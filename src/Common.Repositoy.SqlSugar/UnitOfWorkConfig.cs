using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Common.Repositoy.SqlSugar
{
    public class UnitOfWorkConfig
    {
        public string Log4NetConfigPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "log4net.config");
        public bool IsEnableLog { get; set; } = true;
    }
}
