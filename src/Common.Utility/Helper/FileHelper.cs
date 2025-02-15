﻿using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Writers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Common.Utility.Helper
{
    public static class FileHelper
    {
        /// <summary>
        /// 文件下载
        /// </summary>
        /// <param name="downLoadUrl">下载请求地址</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="fileName">文件名</param>
        /// <param name="fileExtension">文件拓展名</param>
        /// <returns>文件路径</returns>
        public static async Task<string> FileDownLoadAsync(this string downLoadUrl, string savePath, string fileName, string fileExtension)
        {
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            var filepath = Path.Combine(savePath, Path.GetFileNameWithoutExtension(fileName) + fileExtension);
            using (HttpClient client = new HttpClient())
            {
                var fileData = await client.GetByteArrayAsync(downLoadUrl);

                File.WriteAllBytes(filepath, fileData);
            }

            return filepath;
        }


        /// <summary>
        /// 异步下载文件并保存到指定路径.
        /// </summary>
        /// <param name="downLoads">一个包含下载 URL 和文件名元组的集合.</param>
        /// <param name="savePath">文件保存的目标目录.</param>
        /// <returns>一个包含已下载文件路径的列表.</returns>
        /// <exception cref="ArgumentException">如果保存路径无效.</exception>
        /// <exception cref="HttpRequestException">如果下载过程中发生错误.</exception>
        public static async Task<List<string>> FileDownLoadAsync(this IEnumerable<(string downLoadUrl, string fileName)> downLoads, string savePath)
        {
            var tasks = new List<Task<string>>();
            using (HttpClient client = new HttpClient())
            {
                foreach (var d in downLoads)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        var fileData = await client.GetByteArrayAsync(d.downLoadUrl);
                        var fileName = Path.Combine(savePath, d.fileName); // 根据提供的文件名生成完整路径
                        File.WriteAllBytes(fileName, fileData);
                        return fileName; // 返回下载的文件路径
                    }));
                }

                // 等待所有下载任务完成
                var results = await Task.WhenAll(tasks);
                return results.ToList(); // 返回所有下载文件的路径
            }
        }


        /// <summary>
        /// 文件下载
        /// </summary>
        /// <param name="downLoadUrl">下载请求地址</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="fileName">文件名</param>
        /// <param name="fileExtension">文件拓展名</param>
        /// <returns>文件路径</returns>
        public static string FileDownLoad(this string downLoadUrl, string savePath, string fileName, string fileExtension)
        {
            try
            {
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                var filepath = Path.Combine(savePath, Path.GetFileNameWithoutExtension(fileName) + fileExtension);
                using (HttpClient client = new HttpClient())
                {
                    var fileData = client.GetByteArrayAsync(downLoadUrl).Result;

                    File.WriteAllBytes(filepath, fileData);
                }
                return filepath;
            }
            catch (Exception ex)
            {
                throw new Exception($"文件下载出错！ url:{downLoadUrl}", ex);
            }


        }

        /// <summary>
        /// 根据文件夹路径获取该文件夹内所有的文件
        /// </summary>
        /// <param name="dirPath">文件夹路径</param>
        /// <returns>文件信息数组</returns>
        public static FileInfo[] GetFiles(this string dirPath)
        {
            DirectoryInfo subdirectoryFolder = new DirectoryInfo(dirPath);
            FileInfo[] files = subdirectoryFolder.GetFiles();
            return files;
        }

        /// <summary>
        /// 检查指定路径的文件是否存在.
        /// </summary>
        /// <param name="filePath">要检查的文件的完整路径.</param>
        /// <returns>如果文件存在则返回 <c>true</c>，否则返回 <c>false</c>.</returns>
        public static bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// 删除指定文件夹中的所有文件和子文件夹，但保留文件夹本身。
        /// </summary>
        /// <param name="folderPath">应删除其内容的文件夹路径。</param>
        public static void DeleteFolderContents(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                return;
            }
            // 删除文件夹中的所有文件
            foreach (string file in Directory.GetFiles(folderPath))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    // 处理删除文件时发生的异常
                    throw new Exception($"无法删除文件 '{file}': {ex.Message}");
                }
            }

            // 递归删除文件夹中所有子文件夹的内容
            foreach (string subfolder in Directory.GetDirectories(folderPath))
            {
                try
                {
                    DeleteFolderContents(subfolder);
                }
                catch (Exception ex)
                {
                    // 处理递归删除子文件夹时发生的异常
                    throw new Exception($"无法删除文件夹 '{subfolder}': {ex.Message}");
                }
            }
        }


        /// <summary>
        /// 根据输入的文件夹路径创建目录路径。
        /// </summary>
        /// <param name="directoryPath">文件夹路径。</param>
        /// <param name="isDelFile">指定是否清除文件夹中的文件（如果文件夹已存在）。</param>
        /// <returns>目录路径。</returns>
        public static string CreateFolder(string directoryPath, bool isDelFile = false)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (isDelFile)
            {
                DeleteFolderContents(directoryPath);
            }
            return directoryPath;
        }

        /// <summary>
        /// 根据输入的文件路径创建目录路径。
        /// </summary>
        /// <param name="filePath">文件路径。</param>
        /// <param name="isDelFile">指定是否清除文件夹中的文件（如果文件夹已存在）。</param>
        /// <returns>目录路径。</returns>
        public static string CreateFolderByFilePath(string filePath, bool isDelFile = false)
        {
            var directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (isDelFile)
            {
                DeleteFolderContents(directoryPath);
            }
            return Path.Combine(directoryPath, Path.GetFileName(filePath));
        }

        public static void CreateFile(string filePath)
        {
            if (File.Exists(filePath)) return;
            FileInfo fileInfo = new FileInfo(filePath);
            using (FileStream fs = fileInfo.Create())
            {

            }
        }

        /// <summary>
        /// 将文件按照自定义格式进行命名备份，并可选择备份到指定路径。
        /// </summary>
        /// <param name="filePath">要备份的文件路径。</param>
        /// <param name="appendFormat">重命名格式，默认按 文件名_bak_年月日时分秒 命名。</param>
        /// <param name="backupPath">可选参数，备份文件的路径。如果不填，则默认为原路径。</param>
        /// <returns>备份文件后的路径。</returns>
        public static string BackupFile(this string filePath, string appendFormat = "", string backupPath = null)
        {
            if (File.Exists(filePath))
            {
                string directory = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string extension = Path.GetExtension(filePath);
                string date = DateTime.Now.ToString("yyyyMMddHHmmss");
                if (string.IsNullOrEmpty(appendFormat))
                {
                    appendFormat = $"_bak_{date}";
                }


                string newFilePath;
                if (string.IsNullOrEmpty(backupPath))
                {
                    newFilePath = Path.Combine(directory, $"{fileName}{appendFormat}{extension}");
                }
                else
                {
                    // 如果备份路径不存在，则创建备份路径
                    if (!Directory.Exists(backupPath))
                    {
                        Directory.CreateDirectory(backupPath);
                    }

                    newFilePath = Path.Combine(backupPath, $"{fileName}{appendFormat}{extension}");
                }

                File.Move(filePath, newFilePath);
                return newFilePath;
            }
            else
            {
                //throw new Exception($"备份失败，原因是：文件 '{filePath}' 不存在。");
                return null;
            }
        }

        /// <summary>
        /// 移动文件从一个位置到另一个位置。
        /// </summary>
        /// <param name="sourceFilePath">源文件的路径。</param>
        /// <param name="destinationFilePath">目标文件的路径(要带名字)。</param>
        /// <param name="overwrite">如果文件已存在，是否覆盖。</param>
        public static void MoveFile(string sourceFilePath, string destinationFilePath, bool overwrite = true)
        {
            // 检查源文件是否存在
            if (!File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException("源文件不存在。", sourceFilePath);
            }
            
            if (sourceFilePath == destinationFilePath)
            {
                return;
            }
            // 检查目标文件是否存在
            if (File.Exists(destinationFilePath))
            {
                if (overwrite)
                {
                    // 如果允许覆盖，先删除目标文件，然后移动源文件到目标位置
                    File.Delete(destinationFilePath);
                    File.Move(sourceFilePath, destinationFilePath);
                    Console.WriteLine("文件成功移动并覆盖目标文件。");
                }               
            }
            else
            {
                // 目标文件不存在，直接移动源文件到目标位置
                File.Move(sourceFilePath, destinationFilePath);
                Console.WriteLine("文件成功移动到目标位置。");
            }
        }
        /// <summary>
        /// 将文件从源路径移动到目标路径
        /// </summary>
        /// <param name="sourceFilePath">源文件路径</param>
        /// <param name="destinationFilePath">目标文件路径</param>
        /// <param name="username">共享目录用户名</param>
        /// <param name="password">共享目录密码</param>
        public static void MoveFileForWindows(string sourceFilePath, string destinationFilePath, string username, string password)
        {
            // 检查源文件是否存在
            if (!File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException("源文件不存在。", sourceFilePath);
            }

            // 获取目标文件夹路径
            string destinationFolderPath = Path.GetDirectoryName(destinationFilePath);
            if (string.IsNullOrEmpty(destinationFolderPath))
            {
                throw new ArgumentException("目标文件路径无效。", nameof(destinationFilePath));
            }

            // 使用 net use 命令连接到共享目录
            string netUseCommand = $"net use \"{destinationFolderPath}\" \"{password}\" /user:\"{username}\"";
            ExecuteCommand(netUseCommand);

            try
            {
                // 移动文件
                File.Move(sourceFilePath, destinationFilePath);
            }
            finally
            {
                // 断开连接
                string netUseDisconnectCommand = $"net use \"{destinationFolderPath}\" /delete";
                ExecuteCommand(netUseDisconnectCommand);
            }
        }

        private static void ExecuteCommand(string command)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/C " + command)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processInfo))
            {
                process.WaitForExit();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                if (process.ExitCode != 0)
                {
                    throw new Exception($"命令执行失败: {error}");
                }
            }
        }

        /// <summary>
        /// 复制文件从一个位置到另一个位置。
        /// </summary>
        /// <param name="sourceFilePath">源文件的路径。</param>
        /// <param name="destinationFilePath">目标文件的路径(要带名字)。</param>
        /// <param name="overwrite">如果文件已存在，是否覆盖。</param>
        public static void CopyFile(string sourceFilePath, string destinationFilePath,bool overwrite=true)
        {
            // 检查源文件是否存在
            if (!File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException("源文件不存在。", sourceFilePath);
            }
            if (sourceFilePath == destinationFilePath) return;
            // 复制文件
            File.Copy(sourceFilePath, destinationFilePath, overwrite);
        }

        /// <summary>
        /// 将文件复制到远程共享目录
        /// </summary>
        /// <param name="sourceFilePath">源文件路径</param>
        /// <param name="destinationFilePath">目标文件路径</param>
        /// <param name="username">共享目录用户名</param>
        /// <param name="password">共享目录密码</param>
        public static void CopyFileForWindows(string sourceFilePath, string destinationFilePath, string username, string password)
        {
            // 检查源文件是否存在
            if (!File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException("源文件不存在。", sourceFilePath);
            }

            // 获取目标文件夹路径
            string destinationFolderPath = Path.GetDirectoryName(destinationFilePath);
            if (string.IsNullOrEmpty(destinationFolderPath))
            {
                throw new ArgumentException("目标文件路径无效。", nameof(destinationFilePath));
            }

            // 使用 net use 命令连接到共享目录
            string netUseCommand = $"net use \"{destinationFolderPath}\" \"{password}\" /user:\"{username}\"";
            ExecuteCommand(netUseCommand);

            try
            {
                // 复制文件
                File.Copy(sourceFilePath, destinationFilePath, overwrite: true);
            }
            finally
            {
                // 断开连接
                string netUseDisconnectCommand = $"net use \"{destinationFolderPath}\" /delete";
                ExecuteCommand(netUseDisconnectCommand);
            }
        }



        /// <summary>
        /// 将给定的字节数组保存为指定路径的文件。
        /// </summary>
        /// <param name="bytes">要保存为文件的字节数组。</param>
        /// <param name="saveFileFullPath">要保存文件的完整路径。</param>
        public static void SaveFile(this byte[] bytes, string saveFileFullPath)
        {
            File.WriteAllBytes(saveFileFullPath, bytes);
        }

        /// <summary>
        /// 将一个文件转换为byte[]
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>byte[]</returns>
        public static byte[] FileConvertToByteArray(string filePath)
        {
            if (File.Exists(filePath))
            {
                return File.ReadAllBytes(filePath);
            }
            throw new FileNotFoundException("源文件不存在。", filePath);
        }

        /// <summary>
        /// 文件删除
        /// </summary>
        /// <param name="fileFullPath"></param>
        public static void DeleteFile(string fileFullPath)
        {
            if (File.Exists(fileFullPath))
            {
                File.Delete(fileFullPath);
            }
        }


        /// <summary>
        /// 从指定的源目录创建zip文件。
        /// </summary>
        /// <param name="sourceDirectory">源目录的路径。</param>
        /// <param name="zipFilePath">目标zip文件的路径。</param>
        /// <param name="encoding">字符编码 【设置默认编码为UTF-8】</param>
        public static void CreateZipFile(string sourceDirectory, string zipFilePath, Encoding encoding)
        {
            using (var zip = SharpCompress.Archives.Zip.ZipArchive.Create())
            {
                foreach (var file in Directory.EnumerateFiles(sourceDirectory))
                {
                    zip.AddEntry(Path.GetFileName(file), file);
                }
                var options = new WriterOptions(CompressionType.Deflate)
                {
                    ArchiveEncoding = new ArchiveEncoding
                    {
                        Default = encoding // 设置默认编码为UTF-8
                    },
                    LeaveStreamOpen = false
                };
                zip.SaveTo(zipFilePath, options);
            }
        }

        /// <summary>
        /// 将集合中的文件路径压缩到目标文件夹
        /// </summary>
        /// <param name="filePaths">集合中的文件路径</param>
        /// <param name="zipFilePath">目标zip文件路径</param>
        public static void CreateZipFile(IEnumerable<string> filePaths, string zipFilePath)
        {
            using (var zip = ZipArchive.Create())
            {
                foreach (var file in filePaths)
                {
                    // 添加文件到压缩包
                    zip.AddEntry(Path.GetFileName(file), file);
                }

                // 指定使用UTF-8编码
                var options = new WriterOptions(CompressionType.Deflate)
                {
                    ArchiveEncoding = new ArchiveEncoding
                    {
                        Default = Encoding.UTF8 // 设置默认编码为UTF-8
                    },
                    LeaveStreamOpen = false
                };
                using (var stream = File.OpenWrite(zipFilePath))
                {
                    zip.SaveTo(stream, options);
                }
            }

        }
        /// <summary>
        /// 自动清理文件
        /// </summary>
        /// <param name="path">清理路径</param>
        /// <param name="filter">文件特征</param>
        /// <exception cref="Exception"></exception>
        public static void AutomaticCleaning(this string path, params string[] filter)
        {
            if (!Directory.Exists(path))
            {
                Console.WriteLine($"{path}目录不存在！");
                return;
            }
            if (filter == null || filter.Length == 0)
            {
                return;
            }


            // 删除符合过滤器条件的文件
            foreach (string file in Directory.GetFiles(path))
            {
                if (filter.Any(f => f == "*.*" || file.Contains(f)))
                {
                    File.Delete(file);
                }
            }
            foreach (string subfolder in Directory.GetDirectories(path))
            {
                try
                {
                    AutomaticCleaning(subfolder, filter);
                }
                catch (Exception ex)
                {
                    throw new Exception($"无法删除文件夹 '{subfolder}': {ex.Message}");
                }
            }

            try
            {
                if (Directory.GetFiles(path).Length == 0 && Directory.GetDirectories(path).Length == 0)
                {
                    Directory.Delete(path);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"无法删除目录 '{path}': {ex.Message}");
            }
        }
        /// <summary>
        /// 将zip文件解压缩到指定的解压目录。
        /// </summary>
        /// <param name="zipFilePath">要解压缩的zip文件的路径。</param>
        /// <param name="extractionDirectory">解压目录的路径。</param>
        public static void ExtractZipFile(string zipFilePath, string extractionDirectory)
        {
            using (var zip = SharpCompress.Archives.Zip.ZipArchive.Open(zipFilePath))
            {
                foreach (var entry in zip.Entries)
                {
                    entry.WriteToDirectory(extractionDirectory, new ExtractionOptions()
                    {
                        ExtractFullPath = true,
                        Overwrite = true
                    });
                }
            }

        }
    }
}
