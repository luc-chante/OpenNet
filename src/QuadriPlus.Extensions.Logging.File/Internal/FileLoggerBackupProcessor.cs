using System;
using System.IO;
using System.Threading;

namespace QuadriPlus.Extensions.Logging.File.Internal
{
    public class FileLoggerBackupProcessor : FileLoggerProcessor
    {
        private static Func<string, bool> BackupFileFactory(long maxSize, TimeSpan maxAge) =>
            (path =>
            {
                FileInfo file = new FileInfo(path);
                return (maxSize > 0 && file.Length > maxSize)
                    || (maxAge != Timeout.InfiniteTimeSpan && file.CreationTime.Add(maxAge) < DateTime.Now);
            });

        private readonly Func<string, bool> _backupFile;

        public FileLoggerBackupProcessor(string path, FileLoggerBackupMode mode, long maxSize, TimeSpan maxAge, bool startup)
            : base(path, false)
        {
            var testSize = (mode & FileLoggerBackupMode.Size) == FileLoggerBackupMode.Size;
            var testAge = (mode & FileLoggerBackupMode.Age) == FileLoggerBackupMode.Age;
            _backupFile = BackupFileFactory(testSize ? maxSize : long.MaxValue, testAge ? maxAge : Timeout.InfiniteTimeSpan);

            if (startup && (mode & FileLoggerBackupMode.Startup) == FileLoggerBackupMode.Startup)
            {
                GenerateBackupFile();
            }
        }

        protected override void WriteMessage(string message)
        {
            if (_backupFile(FullName))
            {
                GenerateBackupFile();
            }

            base.WriteMessage(message);
        }

        private void GenerateBackupFile()
        {
            lock (this)
            {
                CloseStream();

                var fileInfo = new FileInfo(FullName);
                if (fileInfo.Exists && fileInfo.Length > 0)
                {
                    var dir = Path.GetDirectoryName(FullName);
                    var name = Path.GetFileNameWithoutExtension(FullName);
                    var ext = Path.GetExtension(FullName);

                    int number = -1;
                    foreach (var file in Directory.EnumerateFiles(dir, $"{name}.*{ext}", SearchOption.TopDirectoryOnly))
                    {
                        var f = Path.GetFileName(file);
                        var n = f.Substring(name.Length + 1, f.Length - name.Length - ext.Length - 1);
                        var num = int.Parse(n);
                        if (number < num)
                        {
                            number = num;
                        }
                    }

                    System.IO.File.Copy(FullName, Path.Combine(dir, $"{name}.{number + 1}{ext}"));
                }

                TruncateFile();
            }
        }
    }
}
