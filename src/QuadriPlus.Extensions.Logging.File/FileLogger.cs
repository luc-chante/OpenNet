using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions.Internal;
using QuadriPlus.Extensions.Logging.File.Internal;
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace QuadriPlus.Extensions.Logging.File
{
    public class FileLogger : ILogger
    {
        private static readonly Regex PrefixRegex = new Regex(@"%(-?\d)?(date|level|lvl|name|message)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private FileLoggerProcessor _fileProcessor;
        private Func<string, LogLevel, bool> _filter;
        private string _pattern;

        [ThreadStatic]
        private static StringBuilder _logBuilder;

        public FileLogger(string name, string pattern, Func<string, LogLevel, bool> filter, IExternalScopeProvider scopeProvider, FileLoggerProcessor loggerProcessor)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Pattern = pattern;
            Filter = filter ?? ((category, logLevel) => true);
            ScopeProvider = scopeProvider;
            FileProcessor = loggerProcessor;
        }

        public FileLoggerProcessor FileProcessor
        {
            get => _fileProcessor;
            set => _fileProcessor = value ?? throw new ArgumentNullException(nameof(value));
        }

        public Func<string, LogLevel, bool> Filter
        {
            get => _filter;
            set => _filter = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Name { get; }

        internal string Pattern
        {
            get => _pattern;
            set => _pattern = CompilePattern(value ?? throw new ArgumentNullException(nameof(value)));
        }

        internal IExternalScopeProvider ScopeProvider { get; set; }

        public IDisposable BeginScope<TState>(TState state) =>
            ScopeProvider?.Push(state) ?? NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) =>
            logLevel != LogLevel.None && Filter(Name, logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {

            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);

            if (!string.IsNullOrEmpty(message) || exception != null)
            {
                WriteMessage(logLevel, Name, eventId.Id, message, exception);
            }
        }

        public virtual void WriteMessage(LogLevel logLevel, string logName, int eventId, string message, Exception exception)
        {
            var logBuilder = _logBuilder;
            _logBuilder = null;

            if (logBuilder == null)
            {
                logBuilder = new StringBuilder();
            }

            // scope information
            GetScopeInformation(logBuilder);

            if (!string.IsNullOrEmpty(message))
            {
                logBuilder.AppendLine(FormatMessage(DateTime.Now, logLevel, message));
            }

            for (var ex = exception; ex != null; ex = ex.InnerException)
            {
                logBuilder.AppendLine(FormatMessage(DateTime.Now, logLevel, ex));
                logBuilder.AppendLine(ex.StackTrace);
            }

            // Queue log message
            FileProcessor.EnqueueMessage(logBuilder.ToString());

            logBuilder.Clear();
            if (logBuilder.Capacity > 1024)
            {
                logBuilder.Capacity = 1024;
            }
            _logBuilder = logBuilder;
        }

        private void GetScopeInformation(StringBuilder stringBuilder)
        {
            var scopeProvider = ScopeProvider;
            if (scopeProvider != null)
            {
                var initialLength = stringBuilder.Length;

                scopeProvider.ForEachScope((scope, state) =>
                {
                    var (builder, length) = state;
                    var first = length == builder.Length;
                    builder.Append(first ? "=> " : " => ").Append(scope);
                }, (stringBuilder, initialLength));
            }
        }

        private string FormatMessage(DateTime date, LogLevel logLevel, string message)
        {
            var levelString = logLevel.ToString();
            var lvlString = GetLogLevelString(logLevel);
            var log = string.Format(Pattern, date, levelString, levelString.ToLower(), lvlString, lvlString.ToLower(), Name, message);

            return log;
        }

        private string FormatMessage(DateTime date, LogLevel logLevel, Exception exception) =>
            FormatMessage(date, logLevel, $"{exception.GetType().FullName}: {exception.Message}");

        private static string CompilePattern(string pattern)
        {
            int cursor = 0;
            StringBuilder sb = new StringBuilder(pattern.Length);

            foreach (Match match in PrefixRegex.Matches(pattern))
            {
                sb.Append(pattern.Substring(cursor, match.Index - cursor));
                switch (match.Groups[2].Captures[0].Value)
                {
                    case "date":
                        sb.Append("{0");
                        break;
                    case "Level":
                        sb.Append("{1");
                        break;
                    case "level":
                        sb.Append("{2");
                        break;
                    case "Lvl":
                        sb.Append("{3");
                        break;
                    case "lvl":
                        sb.Append("{4");
                        break;
                    case "name":
                        sb.Append("{5");
                        break;
                    case "message":
                        sb.Append("{6");
                        break;
                }
                if (match.Groups[1].Captures.Count != 0)
                {
                    sb.Append(',').Append(match.Groups[1].Captures[0].Value);
                }
                sb.Append('}');

                cursor = match.Index + match.Length;
            }
            sb.Append(pattern.Substring(cursor));

            return sb.ToString();
        }

        private static string GetLogLevelString(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return "Trce";
                case LogLevel.Debug:
                    return "Dbug";
                case LogLevel.Information:
                    return "Info";
                case LogLevel.Warning:
                    return "Warn";
                case LogLevel.Error:
                    return "Fail";
                case LogLevel.Critical:
                    return "Crit";
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }
    }
}
