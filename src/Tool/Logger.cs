using Microsoft.Extensions.Logging;

namespace TcpWtf.NumberSequence.Tool
{
    internal enum Verbosity
    {
        Info,
        Warn,
        Err
    }

    internal sealed class Logger<T> : Logger, ILogger<T>
    {
        public Logger(Verbosity verbosity)
            : base(verbosity)
        { }
    }

    internal class Logger : ILogger
    {
#if NET10_0_OR_GREATER
        private static readonly Lock consoleLock = new();
#else
        private static readonly object consoleLock = new();
#endif
        private readonly LogLevel minimumLogLevel;

        public Logger(Verbosity verbosity)
        {
            this.minimumLogLevel = verbosity switch
            {
                Verbosity.Info => LogLevel.Information,
                Verbosity.Warn => LogLevel.Warning,
                Verbosity.Err => LogLevel.Error,
                _ => LogLevel.Trace,
            };
        }

        public IDisposable BeginScope<TState>(TState state) => throw new NotImplementedException();

        public bool IsEnabled(LogLevel logLevel) => logLevel >= this.minimumLogLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel < this.minimumLogLevel)
            {
                return;
            }

            // If an exception is passed in here, it is currently ignored by the LoggerExtensions default formatter
            // Line 509: https://github.com/aspnet/Logging/blob/dev/src/Microsoft.Extensions.Logging.Abstractions/LoggerExtensions.cs
            // Because people may expect it to be logged, go ahead and log it here as an error (but as a concatenation to the current message)
            string message = formatter(state, exception);
            if (exception != null)
            {
                message += Environment.NewLine + exception.ToString();
            }

            string prefix = logLevel switch
            {
                LogLevel.Trace => "TRAC",
                LogLevel.Debug => "DEBG",
                LogLevel.Information => "INFO",
                LogLevel.Warning => "WARN",
                LogLevel.Error => "FAIL",
                LogLevel.Critical => "CRIT",
                _ => logLevel.ToString().ToUpper()
            };

            lock (consoleLock)
            {
                TextWriter output = logLevel >= LogLevel.Warning ? Console.Error : Console.Out;
                output.Write('[');
                Console.ForegroundColor = logLevel switch
                {
                    LogLevel.Information => ConsoleColor.Green,
                    LogLevel.Warning => ConsoleColor.Yellow,
                    LogLevel.Error => ConsoleColor.Red,
                    LogLevel.Critical => ConsoleColor.Black,
                    _ => Console.ForegroundColor
                };
                Console.BackgroundColor = logLevel switch
                {
                    LogLevel.Critical => ConsoleColor.White,
                    _ => Console.BackgroundColor
                };
                output.Write(prefix);
                Console.ResetColor();
                output.WriteLine($"] {message}");
            }
        }
    }
}
