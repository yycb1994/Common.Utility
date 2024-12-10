using Common.Utility.Extensions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Common.Utility.CustomerModel;

namespace Common.Utility.Helper
{
    public class RedXunHelper
    {
        private readonly LoggerHelper<RedXunHelper> _logger;
        public RedXunHelper(LoggerHelper<RedXunHelper> logger, string referer, string origin, string cookie, string password, string username, string tokenRequestUrl, string fileUploadUrl, string downLoadFileUrl, string getFileInfoUrl)
        {
            _logger = logger;
            Referer = referer;
            Origin = origin;
            Cookie = cookie;
            Password = password;
            Username = username;
            TokenRequestUrl = tokenRequestUrl;
            FileUploadUrl = fileUploadUrl;
            DownLoadFileUrl = downLoadFileUrl;
            GetFileInfoUrl = getFileInfoUrl;
        }
        string Referer { get; set; }
        string Origin { get; set; }
        string Cookie { get; set; }
        string Password { get; set; }
        string Username { get; set; }
        string FileUploadUrl { get; set; }
        string TokenRequestUrl { get; set; }
        string DownLoadFileUrl { get; set; }
        string GetFileInfoUrl { get; set; }

        /// <summary>
        /// 将文件上传到 RedSun 平台。
        /// </summary>

        /// <param name="filePath">要上传的文件路径。</param>
        /// <returns>上传请求的响应。</returns>
        public string UploadFile(string filePath)
        {
            string authToken = GetToken();
            var request = new RestRequest();
            request.AddHeader("Authorization", $"Bearer {authToken}");
            request.AddHeader("Referer", Referer);
            request.AddFile("files", filePath);

            var result = FileUploadUrl.SendHttpRequest(Method.Post, request);
            return result.content;
        }

        /// <summary>
        /// 获取 RedSun 平台令牌。
        /// </summary>
        /// <returns>RedSun 平台令牌。</returns>
        public string GetToken()
        {
            RestRequest restRequest = new RestRequest();
            restRequest.AddHeader("Origin", Origin);
            restRequest.AddHeader("Referer", Referer);
            restRequest.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            restRequest.AddHeader("Cookie", Cookie);
            restRequest.AddParameter("password", Password);
            restRequest.AddParameter("username", Username);
            string value = TokenRequestUrl.SendHttpRequest(Method.Post, restRequest).content;
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<JObject>(value)["data"]["access_token"].ToString();
        }


        public async Task<string> DownLoadFileAsync(string fileId, string savePath)
        {
            string token = GetToken();
            var items = GetRedXunFileInfo(fileId);
            _logger.Debug($"RedXun downloadUrl:{(DownLoadFileUrl + "/" + fileId + "?accessToken=" + token)}");
            _logger.Debug($"RedXun savePath:{savePath}");
            _logger.Debug($"RedXun fileName:{Path.GetFileNameWithoutExtension(items.fileName)}");
            _logger.Debug($"RedXun extension:.{Path.GetFileNameWithoutExtension(items.extension)}");
            var filePath = await (DownLoadFileUrl + "/" + fileId + "?accessToken=" + token).FileDownLoadAsync(savePath,
                Path.GetFileNameWithoutExtension(items.fileName), "." + items.extension);
            _logger.Debug($"RedXun filePath:{filePath}");
            return filePath;
        }

        public string DownLoadFile(string fileId, string savePath)
        {
            string token = GetToken();
            var items = GetRedXunFileInfo(fileId);
            _logger.Debug($"RedXun downloadUrl:{(DownLoadFileUrl + "/" + fileId + "?accessToken=" + token)}");
            _logger.Debug($"RedXun savePath:{savePath}");
            _logger.Debug($"RedXun fileName:{Path.GetFileNameWithoutExtension(items.fileName)}");
            _logger.Debug($"RedXun extension:.{Path.GetFileNameWithoutExtension(items.extension)}");
            var filePath = (DownLoadFileUrl + "/" + fileId + "?accessToken=" + token).FileDownLoad(savePath,
                Path.GetFileNameWithoutExtension(items.fileName), "." + items.extension);
            _logger.Debug($"RedXun filePath:{filePath}");
            return filePath;
        }

        public (string fileName, string extension) GetRedXunFileInfo(string fileId)
        {
            string token = GetToken();
            RestRequest restRequest = new RestRequest();
            restRequest.AddHeader("Authorization", "Bearer " + token);
            //restRequest.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            //restRequest.AddHeader("Referer", Referer);
            string value = (GetFileInfoUrl + "?fileId=" + fileId).SendHttpRequest(Method.Get, restRequest).content;
            if (string.IsNullOrEmpty(value))
            {
                throw new FileNotFoundException(fileId + " File Not Found!");
            }

            JObject jObject = JsonConvert.DeserializeObject<JObject>(value);
            var extension = jObject["ext"].ToString();
            var fileName = jObject["fileName"].ToString();
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(extension))
            {
                throw new Exception(fileId + " The correct information for the file was not found.");
            }
            return (fileName, extension);
        }



        (string fileJson, string fileId) UploadFileAndGetFileJson(string filePath)
        {
            string text = null;
            try
            {
                string token = GetToken();
                string value = UploadFile(filePath);
                if (string.IsNullOrEmpty(value))
                {
                    throw new Exception(filePath + " ->The interface did not respond during the file upload process");
                }

                JToken jToken = ((JArray)JsonConvert.DeserializeObject<JObject>(value)["data"])[0];
                SysFileJsonVo value2 = new SysFileJsonVo
                {
                    fileName = jToken["fileName"].ToString(),
                    fileId = jToken["fileId"].ToString(),
                    createTime = DateTime.Now,
                    size = (int)jToken["totalBytes"],
                    createUser = "RedXunService"
                };
                text = jToken["fileId"].ToString();
                return ("[" + JsonConvert.SerializeObject(value2) + "]", text);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
