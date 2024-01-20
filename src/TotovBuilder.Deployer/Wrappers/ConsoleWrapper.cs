using System;
using System.Diagnostics.CodeAnalysis;
using TotovBuilder.Deployer.Abstractions.Wrappers;

namespace TotovBuilder.Deployer.Wrappers
{
    /// <summary>
    /// Represents a <see cref="Console"/> wrapper.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Wrapper to be able to create mocks of the Console class.")]
    public class ConsoleWrapper : IConsoleWrapper
    {
        /// <inheritdoc/>
        public ConsoleColor BackgroundColor
        {
            get
            {
                return Console.BackgroundColor;
            }

            set
            {
                Console.BackgroundColor = value;
            }
        }

        /// <inheritdoc/>
        public ConsoleColor ForegroundColor
        {
            get
            {
                return Console.ForegroundColor;
            }

            set
            {
                Console.ForegroundColor = value;
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            Console.Clear();
        }

        /// <inheritdoc/>
        public void Write(string? value = null)
        {
            Console.Write(value);
        }

        /// <inheritdoc/>
        public void WriteLine(string? value)
        {
            Console.WriteLine(value = null);
        }
    }
}
