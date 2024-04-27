using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using TotovBuilder.Deployer.Abstractions.Wrappers;

namespace TotovBuilder.Deployer.Wrappers
{
    /// <summary>
    /// Represents a <see cref="StreamReader"/> wrapper.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Wrapper to be able to create mocks of the StreamReader class.")]
    public class StreamReaderWrapper : IStreamReaderWrapper
    {
        /// <summary>
        /// Instance.
        /// </summary>
        private StreamReader Instance { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamReaderWrapper"/> class.
        /// </summary>
        /// <param name="instance">Instance.</param>
        public StreamReaderWrapper(StreamReader instance)
        {
            Instance = instance;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Instance.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public string? ReadLine()
        {
            return Instance.ReadLine();
        }
    }
}
