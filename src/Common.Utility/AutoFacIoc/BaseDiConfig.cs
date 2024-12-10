using Autofac;
using AutoMapper;
using Common.Utility.Helper;
using log4net.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Utility.AutoFacIoc
{
    public static class BaseDiConfig
    {
        public static void LogSetup(this ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(LoggerHelper<>)).SingleInstance();
            builder.RegisterType<LogAop>();
        }
        public static void AutoMapperSetup(this ContainerBuilder builder, IMapper mapper)
        {
            builder.RegisterInstance(mapper);
        }
    }
}
