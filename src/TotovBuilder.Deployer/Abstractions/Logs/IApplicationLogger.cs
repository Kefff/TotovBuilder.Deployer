using Microsoft.Extensions.Logging;

namespace TotovBuilder.Deployer.Abstractions.Logs
{
    /// <summary>
    /// Provides the functionalities of a logger.
    /// </summary>
    public interface IApplicationLogger<T> : ILogger<T>
    {
        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">Messages</param>
        void LogError(string message);

        /// <summary>
        /// Logs a success message.
        /// </summary>
        /// <param name="message">Messages</param>
        void LogSuccess(string message);
    }
}
