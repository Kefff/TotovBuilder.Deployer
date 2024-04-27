using System;
using System.IO;

namespace TotovBuilder.Deployer.Abstractions.Wrappers
{
    /// <summary>
    /// Provides the functionalities of a <see cref="StreamWriter"/> wrapper.
    /// </summary>
    public interface IStreamWriterWrapper : IDisposable
    {
        /// <summary>
        /// Writes a string to the stream, followed by a line terminator.
        /// </summary>
        /// <param name="value">The string to write. If value is null, only the line terminator is written.</param>
        void WriteLine(string? value);
    }
}
