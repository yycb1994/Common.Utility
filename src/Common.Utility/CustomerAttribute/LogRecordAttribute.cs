using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Utility.CustomerAttribute
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class LogRecordAttribute : Attribute
    {
    }
}
