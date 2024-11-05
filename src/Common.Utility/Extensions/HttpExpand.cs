using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using ZstdSharp.Unsafe;

namespace Common.Utility.Extensions
{
    public static class HttpExpand
    {

        /// <summary>
        /// 发起HTTP请求并返回响应状态码和内容。
        /// </summary>
        /// <param name="baseUrl">请求的URL。</param>
        /// <param name="method">请求的方法类型。</param>
        /// <param name="requestBody">请求体内容。</param>
        /// <returns>包含响应状态码和内容的元组。</returns>
        /// <exception cref="Exception">HTTP请求失败时引发异常。</exception>
        public static (HttpStatusCode? httpStatusCode, string content) SendHttpRequest(this string baseUrl, Method method, RestRequest requestBody)
        {
            try
            {
                using (var client = new RestClient(baseUrl))
                {
                    requestBody.Method = method;
                    var response = client.Execute(requestBody);
                    return (response?.StatusCode, response?.Content);

                }
            }
            catch (Exception ex)
            {
                throw new Exception($"HTTP请求失败：请求地址 {baseUrl},requestBody:{JsonConvert.SerializeObject(requestBody)}，错误信息：{ex.Message}");
            }
        }

    }
}
