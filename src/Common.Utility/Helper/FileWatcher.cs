using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Common.Utility.Helper
{
    public class FileWatcher
    {
        private int _errorCount = 0; // 记录错误次数
        private const int MaxErrorCount = 3; // 最大错误次数

        public void Start(string path, string filter, Action<object, FileSystemEventArgs> onProcess)
        {
            FileSystemWatcher watcher = new FileSystemWatcher
            {
                Path = path,
                Filter = filter,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size,
                IncludeSubdirectories = true,
                InternalBufferSize = 1024 * 16
            };

            watcher.Changed += new FileSystemEventHandler(onProcess);
            watcher.Created += new FileSystemEventHandler(onProcess);
            watcher.Deleted += new FileSystemEventHandler(onProcess);
            watcher.Error += (object sender, ErrorEventArgs e) =>
            {
                _errorCount++; // 增加错误计数

                Console.WriteLine($"发生错误: {e.GetException().Message}");

                if (_errorCount <= MaxErrorCount)
                {
                    try
                    {
                        watcher.EnableRaisingEvents = false;
                        watcher.Dispose(); // 释放旧的 watcher
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"重启监视器时发生异常: {ex.Message}");
                    }

                    // 重新启动监视器
                    Start(path, filter, onProcess);
                }
                else
                {
                    Console.WriteLine("达到最大错误次数，停止重启监视器。");
                    watcher.Dispose(); // 停止监视器
                }
            };

            watcher.EnableRaisingEvents = true;
        }




        public virtual void OnProcess(object source, FileSystemEventArgs e)
        {
            try
            {
                if (e.ChangeType == WatcherChangeTypes.Created)
                {
                    Console.WriteLine($"监视到新文件：{e.FullPath}");
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
