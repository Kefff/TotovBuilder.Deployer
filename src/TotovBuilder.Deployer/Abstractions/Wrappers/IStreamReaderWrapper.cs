using System;

namespace TotovBuilder.Deployer.Abstractions.Wrappers
{
    /// <summary>
    /// Provides the functionalities of a <see cref="StreamReader"/> wrapper.
    /// </summary>
    public interface IStreamReaderWrapper : IDisposable
    {
        /// <summary>
        /// Reads a line of characters from the current stream and returns the data as a string.
        /// </summary>
        /// <returns>The next line from the input stream, or null if the end of the input stream is reached.</returns>
        string? ReadLine();
    }
}
