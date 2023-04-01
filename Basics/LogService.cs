using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using YukariToolBox.LightLog;

namespace SoraBot.Basics
{
    internal class LogService : ILogService
    {
        private readonly ILogger _logger;

        public LogService()
        {
            _logger = Ioc.Require<ILogger<LogService>>();
        }

        public void Debug(string source, string message) => _logger.LogDebug("{source} {message}", source, message);

        public void Debug<T>(string source, string message, T context) => _logger.LogDebug("{source} {type} {message}", source, typeof(T).Name, message);

        public void Error(string source, string message) => _logger.LogError("{source} {message}", source, message);

        public void Error(Exception exception, string source, string message) => _logger.LogError(exception, "{source} {message}", source, message);

        public void Error<T>(string source, string message, T context) => _logger.LogError("{source} {type} {message}", source, typeof(T).Name, message);

        public void Error<T>(Exception exception, string source, string message, T context) => _logger.LogError(exception, "{source} {type} {message}", source, typeof(T).Name, message);

        public void Fatal(Exception exception, string source, string message) => _logger.LogCritical(exception, "{source} {message}", source, message);

        public void Fatal<T>(Exception exception, string source, string message, T context) => _logger.LogCritical(exception, "{source} {type} {message}", source, typeof(T).Name, message);

        public void Info(string source, string message) => _logger.LogInformation("{source} {message}", source, message);

        public void Info<T>(string source, string message, T context) => _logger.LogInformation("{source} {type} {message}", source, typeof(T).Name, message);

        public void SetCultureInfo(CultureInfo cultureInfo)
        {
        }

        public void UnhandledExceptionLog(UnhandledExceptionEventArgs args)
        {
        }

        public void Verbose(string source, string message) => _logger.LogTrace("{source} {message}", source, message);

        public void Verbose<T>(string source, string message, T context) => _logger.LogTrace("{source} {type} {message}", source, typeof(T).Name, message);

        public void Warning(string source, string message) => _logger.LogWarning("{source} {message}", source, message);

        public void Warning<T>(string source, string message, T context) => _logger.LogWarning("{source} {type} {message}", source, typeof(T).Name, message);
    }
}
