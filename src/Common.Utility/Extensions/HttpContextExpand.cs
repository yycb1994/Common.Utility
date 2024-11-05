using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Utility.Extensions
{
    public static class HttpContextExpand
    {
        public static string GetRequestIp(this IHttpContextAccessor httpContextAccessor)
        {
            string result = "";

            var httpContext = httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                result = "127.0.0.1";
            }
            else
            {
                result = httpContext.Request.Headers["X-Forwarded-For"];
                if (string.IsNullOrEmpty(result))
                {
                    result = httpContext.Connection.RemoteIpAddress.ToString();
                }
                if (string.IsNullOrEmpty(result))
                {
                    result = httpContext.Connection.LocalIpAddress.ToString();
                }
                if (result == "::1")
                {
                    result = "127.0.0.1";
                }
                if (string.IsNullOrEmpty(result))
                {
                    return "0.0.0.0";
                }
            }
            return result;
        }


    }
}
