using System;

namespace QuadriPlus.Extensions.Logging.File
{
    public class FileLoggerOptions
    {
        public bool IncludeScopes { get; set; } = false;

        public string Path { get; set; }

        public string Pattern { get; set; } = "%date [%lvl] %name - %message";

        public FileLoggerBehaviour Behaviour { get; set; } = FileLoggerBehaviour.Append;

        public FileLoggerBackupMode BackupMode { get; set; } = FileLoggerBackupMode.Default;

        public string MaxSize { get; set; }

        public TimeSpan MaxAge { get; set; }
    }
}
