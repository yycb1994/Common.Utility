using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Net.Mime;
using System.Net;
using System.Text;
using System.IO;

namespace Common.Utility.Helper
{
    /// <summary>
    /// 用于使用SMTP发送电子邮件的辅助类。
    /// </summary>
    public static class EmailHelper
    {
        static SmtpClient _smtp;
        static MailMessage _message;

        /// <summary>
        /// 初始化发送电子邮件的设置。
        /// </summary>
        /// <param name="smtpServer">SMTP服务器地址。</param>
        /// <param name="smtpPort">SMTP服务器端口。</param>
        /// <param name="senderName">发件人电子邮件地址。</param>
        /// <param name="senderPassword">发件人电子邮件密码。</param>
        /// <param name="recipientEmail">收件人电子邮件地址。</param>
        /// <param name="ccEmails">可选的抄送电子邮件地址。</param>
        public static void InitEmailSetting(string smtpServer, int smtpPort, string senderName, string senderPassword,
            string recipientEmail, params string[] ccEmails)
        {
            _smtp = new SmtpClient(smtpServer, smtpPort);
            _smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            //  _smtp.EnableSsl = true;
            _smtp.Credentials = new NetworkCredential(senderName, senderPassword);
            _message = new MailMessage();
            _message.From = new MailAddress(senderName);
            _message.To.Add(new MailAddress(recipientEmail));
            _message.BodyEncoding = Encoding.UTF8;
            _message.IsBodyHtml = true;

            if (ccEmails != null && ccEmails.Length > 0)
            {
                foreach (var ccEmail in ccEmails)
                {
                    _message.CC.Add(new MailAddress(ccEmail));
                }
            }
        }

        /// <summary>
        /// 发送具有指定主题、内容和可选附件的电子邮件。
        /// </summary>
        /// <param name="subjectPrefix">电子邮件的主题前缀。</param>
        /// <param name="emailContext">电子邮件内容。</param>
        /// <param name="attachmentFilePaths">附件文件路径（可选）。</param>
        /// <returns>如果成功发送电子邮件，则返回true。</returns>
        public static bool SendMail(string subjectPrefix, string emailContext, params string[] attachmentFilePaths)
        {
            try
            {
                if (_smtp == null || _message == null)
                {
                    throw new Exception("调用SendMail前，需要调用InitEmailSetting");
                }
                _message.Subject = $"{subjectPrefix}";

                if (attachmentFilePaths.Length > 0)
                {
                    foreach (var file in attachmentFilePaths)
                    {
                        if (File.Exists(file))
                        {
                            Attachment attachment = new Attachment(file, MediaTypeNames.Application.Octet);
                            attachment.ContentDisposition.CreationDate = File.GetCreationTime(file);
                            attachment.ContentDisposition.ModificationDate = File.GetLastWriteTime(file);
                            attachment.ContentDisposition.ReadDate = File.GetCreationTime(file);
                            _message.Attachments.Add(attachment);
                        }
                    }
                }

                _message.Body = $"{emailContext}";
                //在代码中临时禁用证书验证。使用SmtpClient的ServerCertificateValidationCallback属性，设置一个返回true的回调方法来忽略证书验证错误。
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
                _smtp.Send(_message);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _message.Dispose();
                _smtp.Dispose();
            }
        }
    }
}
