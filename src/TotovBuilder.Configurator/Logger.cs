using System;
using System.Diagnostics.CodeAnalysis;

namespace TotovBuilder.Configurator
{
    /// <summary>
    /// Represents a logger.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class Logger
    {
        /// <summary>
        /// Logs an information.
        /// </summary>
        /// <param name="message">Messages</param>
        public static void LogInformation(string message)
        {
            Console.WriteLine(message);
        }
        
        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">Messages</param>
        public static void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(Properties.Resources.Error);
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
        
        /// <summary>
        /// Logs a success message.
        /// </summary>
        /// <param name="message">Messages</param>
        public static void LogSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
