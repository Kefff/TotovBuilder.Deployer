using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using TotovBuilder.Deployer.Abstractions.Utils;
using TotovBuilder.Deployer.Abstractions.Wrappers;

namespace TotovBuilder.Deployer.Utils
{
    /// <summary>
    /// Represents a logger.
    /// </summary>
    public class ApplicationLogger<T> : IApplicationLogger<T>
    {
        /// <summary>
        /// Console wrapper.
        /// </summary>
        private readonly IConsoleWrapper ConsoleWrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationLogger"/> class.
        /// </summary>
        /// <param name="consoleWrapper"></param>
        public ApplicationLogger(IConsoleWrapper consoleWrapper)
        {
            ConsoleWrapper = consoleWrapper;
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage(Justification = "Should never be called.")]
        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        /// <inheritdoc/>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            string message = formatter(state, exception);

            switch (logLevel)
            {
                case LogLevel.Critical:
                case LogLevel.Error:
                    if (exception != null)
                    {
                        LogError(string.Join(Environment.NewLine, message, exception.ToString()));
                    }
                    else
                    {
                        LogError(message);
                    }

                    break;
                default:
                    LogMessage(message);
                    break;
            }
        }

        /// <inheritdoc/>
        public void LogError(string message)
        {
            ConsoleColor originalForegroundColor = ConsoleWrapper.ForegroundColor;

            ConsoleWrapper.ForegroundColor = ConsoleColor.Red;
            ConsoleWrapper.WriteLine(Properties.Resources.Error);
            ConsoleWrapper.WriteLine(message);

            ConsoleWrapper.ForegroundColor = originalForegroundColor;
            ConsoleWrapper.WriteLine();
        }

        /// <inheritdoc/>
        public void LogSuccess(string message)
        {
            ConsoleColor originalForegroundColor = ConsoleWrapper.ForegroundColor;

            ConsoleWrapper.ForegroundColor = ConsoleColor.Green;
            ConsoleWrapper.WriteLine(message);

            ConsoleWrapper.ForegroundColor = originalForegroundColor;
            ConsoleWrapper.WriteLine();
        }

        /// <summary>
        /// Logs a messages.
        /// </summary>
        /// <param name="message">Message.</param>
        private void LogMessage(string message)
        {
            ConsoleWrapper.WriteLine(message);
        }
    }
}
