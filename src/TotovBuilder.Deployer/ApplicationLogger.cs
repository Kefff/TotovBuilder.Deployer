using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using TotovBuilder.Deployer.Abstractions;

namespace TotovBuilder.Deployer
{
    /// <summary>
    /// Represents a logger.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Only uses Console class.")]
    public class ApplicationLogger<T> : IApplicationLogger<T>
    {
        /// <inheritdoc/>
        public IDisposable BeginScope<TState>(TState state)
        {
            // Should never be called.
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
                    LogError(string.Join(Environment.NewLine, message, exception?.ToString() ?? string.Empty));
                    break;
                default:
                    LogMessage(message);
                    break;
            }
        }

        /// <inheritdoc/>
        public void LogError(string message)
        {
            ConsoleColor originalForegroundColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(Properties.Resources.Error);
            Console.WriteLine(message);

            Console.ForegroundColor = originalForegroundColor;
            Console.WriteLine();
        }

        /// <inheritdoc/>
        public void LogSuccess(string message)
        {
            ConsoleColor originalForegroundColor = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{message}");

            Console.ForegroundColor = originalForegroundColor;
            Console.WriteLine();
        }

        /// <summary>
        /// Logs a messages.
        /// </summary>
        /// <param name="message">Message.</param>
        private static void LogMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
}
