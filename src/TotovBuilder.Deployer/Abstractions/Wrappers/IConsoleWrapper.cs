using System;

namespace TotovBuilder.Deployer.Abstractions.Wrappers
{
    /// <summary>
    /// Provides the functionalities of a <see cref="Console"/> wrapper.
    /// </summary>
    public interface IConsoleWrapper
    {
        /// <summary>
        /// Gets or sets the background color of the console.
        /// </summary>
        ConsoleColor BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the foreground color of the console.
        /// </summary>
        ConsoleColor ForegroundColor { get; set; }

        /// <summary>
        /// Clears the console buffer and corresponding console window of display information.
        /// </summary>
        void Clear();

        /// <summary>
        /// Writes the specified string value to the standard output stream.
        /// </summary>
        /// <param name="value">The value to write.</param>
        void Write(string? value = null);

        /// <summary>
        /// Writes the specified string value, followed by the current line terminator, to the standard output stream.
        /// </summary>
        /// <param name="value">The value to write.</param>
        void WriteLine(string? value = null);
    }
}
