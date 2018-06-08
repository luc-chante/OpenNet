using System;

namespace QuadriPlus.Extensions.Logging.File
{
    public class FileLoggerOptions
    {
        public bool IncludeScopes { get; set; }

        public string Path { get; set; }

        public string Prefix { get; set; }

        public FileLoggerBehaviour Behaviour { get; set; }

        public FileLoggerBackupMode BackupMode { get; set; }

        public string MaxSize { get; set; }

        public TimeSpan MaxAge { get; set; }
    }
}
