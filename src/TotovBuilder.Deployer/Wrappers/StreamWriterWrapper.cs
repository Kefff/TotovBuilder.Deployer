using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using TotovBuilder.Deployer.Abstractions.Wrappers;

namespace TotovBuilder.Deployer.Wrappers
{
    /// <summary>
    /// Represents a <see cref="StreamWriter"/> wrapper.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Wrapper to be able to create mocks of the StreamWriter class.")]
    public class StreamWriterWrapper : IStreamWriterWrapper
    {
        /// <summary>
        /// Instance.
        /// </summary>
        private StreamWriter Instance { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamWriterWrapper"/> class.
        /// </summary>
        /// <param name="instance">Instance.</param>
        public StreamWriterWrapper(StreamWriter instance)
        {
            Instance = instance;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Instance.Dispose();
            GC.SuppressFinalize(this);
        }

        public void WriteLine(string? value)
        {
            Instance.WriteLine(value);
        }
    }
}
