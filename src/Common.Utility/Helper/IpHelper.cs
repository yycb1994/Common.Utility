using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Common.Utility.Helper
{
    public static class IpHelper
    {
        /// <summary>
        /// 获取系统ip地址
        /// </summary>
        /// <returns></returns>
        public static List<string> GetHost()
        {
            var resultHost = new List<string>();
            string hostName = Dns.GetHostName();
            IPAddress[] localIPs = Dns.GetHostAddresses(hostName);
            foreach (var localIp in localIPs)
            {
                resultHost.Add(localIp.ToString());               
            }
            return resultHost;
        }

       
    }
}
