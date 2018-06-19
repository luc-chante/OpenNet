using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QuadriPlus.Extensions.Logging.File.Internal;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace QuadriPlus.Extensions.Logging.File
{
    [ProviderAlias("File")]
    public class FileLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private static readonly Func<string, LogLevel, bool> trueFilter = (cat, level) => true;
        private static readonly Func<string, LogLevel, bool> falseFilter = (cat, level) => false;

        private static readonly char[] _units = new char[] { 'o', 'O', 'k', 'K', 'm', 'M', 'g', 'G', 't', 'T' };

        private readonly ConcurrentDictionary<string, FileLogger> _loggers = new ConcurrentDictionary<string, FileLogger>();

        private readonly Func<string, LogLevel, bool> _filter;
        private readonly IDisposable _optionsReloadToken;
        private bool _includeScopes;
        private string _path;
        private string _prefix;
        private FileLoggerBehaviour _behaviour;
        private FileLoggerBackupMode _backupMode;
        private long _maxSize;
        private TimeSpan _maxAge;
        private IExternalScopeProvider _scopeProvider;
        private FileLoggerProcessor _fileLogger;

        public FileLoggerProvider(IOptionsMonitor<FileLoggerOptions> options)
        {
            // Filter would be applied on LoggerFactory level
            _filter = trueFilter;
            _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
            ReloadLoggerOptions(options.CurrentValue, true);
        }

        public ILogger CreateLogger(string name) =>
            _loggers.GetOrAdd(name, CreateLoggerImplementation);

        public void SetScopeProvider(IExternalScopeProvider scopeProvider) =>
            _scopeProvider = scopeProvider;

        public void Dispose()
        {
            _optionsReloadToken?.Dispose();
        }

        private void ReloadLoggerOptions(FileLoggerOptions options) =>
            ReloadLoggerOptions(options, false);

        private void ReloadLoggerOptions(FileLoggerOptions options, bool startup)
        {
            if (string.IsNullOrEmpty(options.Path))
            {
                throw new ArgumentException($"Invalid value", nameof(options.Path));
            }
            if (options.Behaviour == FileLoggerBehaviour.Backup)
            {
                if ((options.BackupMode & FileLoggerBackupMode.Size) == FileLoggerBackupMode.Size && !TryParseSize(options.MaxSize, out _maxSize))
                {
                    throw new ArgumentException($"Invalid value", nameof(options.MaxSize));
                }
                if ((options.BackupMode & FileLoggerBackupMode.Age) == FileLoggerBackupMode.Age && options.MaxAge <= TimeSpan.Zero)
                {
                    throw new ArgumentException($"Invalid value", nameof(options.MaxAge));
                }
            }

            _includeScopes = options.IncludeScopes;
            _path = ResolvePath(options.Path);
            _prefix = options.Pattern;
            _behaviour = options.Behaviour;
            _backupMode = options.BackupMode;
            _maxAge = options.MaxAge;

            var scopeProvider = GetScopeProvider();
            var fileLogger = GetFileProcessor(reset: true, startup: startup);
            foreach (var logger in _loggers.Values)
            {
                logger.ScopeProvider = scopeProvider;
                logger.FileProcessor = fileLogger;
                logger.Pattern = _prefix;
            }
        }

        private FileLogger CreateLoggerImplementation(string name) =>
            new FileLogger(name, _prefix, GetFilter(name), GetScopeProvider(), GetFileProcessor());

        private Func<string, LogLevel, bool> GetFilter(string name) =>
            _filter ?? falseFilter;

        private IExternalScopeProvider GetScopeProvider()
        {
            if (_includeScopes && _scopeProvider == null)
            {
                _scopeProvider = new LoggerExternalScopeProvider();
            }
            return _includeScopes ? _scopeProvider : null;
        }

        private FileLoggerProcessor GetFileProcessor(bool reset = false, bool startup = false)
        {
            if (reset && _fileLogger != null)
            {
                _fileLogger.Dispose();
                _fileLogger = null;
            }

            if (_fileLogger == null)
            {
                _fileLogger = _behaviour != FileLoggerBehaviour.Backup ?
                    new FileLoggerProcessor(_path, startup && _behaviour == FileLoggerBehaviour.Override)
                    : new FileLoggerBackupProcessor(_path, _backupMode, _maxSize, _maxAge, startup);
            }
            return _fileLogger;
        }

        private string ResolvePath(string path) =>
            !path.StartsWith("~") ? path : Path.Combine(AppContext.BaseDirectory, path.Substring(2));

        private bool TryParseSize(string value, out long size)
        {
            try
            {
                char u = value[value.Length - 1];

                for (int c = 0; c < _units.Length; c++)
                {
                    if (u == _units[c])
                    {
                        long number = Convert.ToInt64(value.Substring(0, value.Length - 1));
                        int power = 10 * (c = c >> 1);

                        size = number << power;
                        return true;
                    }
                }

                size = Convert.ToInt64(value);
                return true;
            }
            catch { }

            size = 0;
            return false;
        }
    }
}
