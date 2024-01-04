using System;
using System.Diagnostics.CodeAnalysis;
using TotovBuilder.Deployer.Abstractions;

namespace TotovBuilder.Deployer
{
    /// <summary>
    /// Represents a logger.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Only uses Console class")]
    public class ApplicationLogger : IApplicationLogger
    {
        /// <inheritdoc/>
        public void LogInformation(string message)
        {
            Console.WriteLine(message);
        }

        /// <inheritdoc/>
        public void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(Properties.Resources.Error);
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <inheritdoc/>
        public void LogSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
