using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace QuadriPlus.Extensions.Logging.File.Internal
{
    public class FileLoggerProcessor
    {
        private const int _maxQueuedMessages = 1024;

        private readonly BlockingCollection<string> _messageQueue = new BlockingCollection<string>(_maxQueuedMessages);
        private readonly Thread _outputThread;
        private StreamWriter _stream;

        public FileLoggerProcessor(string path, bool truncateFile)
        {
            FullName = path;

            if (truncateFile)
            {
                TruncateFile();
            }

            // Start Console message queue processor
            _outputThread = new Thread(ProcessLogQueue)
            {
                IsBackground = true,
                Name = "File logger queue processing thread"
            };
            _outputThread.Start();
        }

        public string FullName { get; }

        protected virtual void WriteMessage(string message)
        {
            lock (this)
            {
                OpenStream();

                _stream.Write(message);
            }
        }

        public virtual void EnqueueMessage(string message)
        {
            if (_messageQueue.IsAddingCompleted)
            {
                // Adding is completed so just log the message
                Task.Factory.StartNew(msg => WriteMessage(msg as string), message);
            }
            else
            {
                try
                {
                    _messageQueue.Add(message);
                }
                catch (InvalidOperationException) { }
            }
        }

        protected void TruncateFile()
        {
            lock (this)
            {
                CloseStream();

                System.IO.File.WriteAllText(FullName, string.Empty);
                System.IO.File.SetCreationTime(FullName, DateTime.Now);
            }
        }

        protected void CloseStream()
        {
            if (_stream != null)
            {
                lock (this)
                {
                    if (_stream != null)
                    {
                        _stream.Dispose();
                        _stream = null;
                    }
                }
            }
        }

        protected void OpenStream()
        {
            if (_stream == null)
            {
                lock (this)
                {
                    if (_stream == null)
                    {
                        _stream = System.IO.File.AppendText(FullName);
                        _stream.AutoFlush = true;
                    }
                }
            }
        }

        private void ProcessLogQueue()
        {
            try
            {
                foreach (var message in _messageQueue.GetConsumingEnumerable())
                {
                    WriteMessage(message);
                }
            }
            catch
            {
                try
                {
                    _messageQueue.CompleteAdding();
                }
                catch { }
            }
        }

        public void Dispose()
        {
            _messageQueue.CompleteAdding();

            try
            {
                _stream.Dispose();
                _stream = null;
            }
            catch { }

            try
            {
                _outputThread.Join(1500); // with timeout in-case Console is locked by user input
            }
            catch (TaskCanceledException) { }
            catch (AggregateException ex) when (ex.InnerExceptions.Count == 1 && ex.InnerExceptions[0] is TaskCanceledException) { }
        }
    }
}
