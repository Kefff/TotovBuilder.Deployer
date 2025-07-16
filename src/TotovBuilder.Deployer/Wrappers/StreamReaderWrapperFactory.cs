using System.Diagnostics.CodeAnalysis;
using System.IO;
using TotovBuilder.Deployer.Abstractions.Wrappers;

namespace TotovBuilder.Deployer.Wrappers
{
    /// <summary>
    /// Represents an <see cref="IStreamReaderWrapperFactory"/> factory.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Wrapper to be able to create mocks of the StreamReader class.")]
    public class StreamReaderWrapperFactory : IStreamReaderWrapperFactory
    {
        /// <inheritdoc/>
        public IStreamReaderWrapper Create(StreamReader instance)
        {
            StreamReaderWrapper wrapper = new(instance);

            return wrapper;
        }

        /// <inheritdoc/>
        public IStreamReaderWrapper Create(string path)
        {
            StreamReader instance = new(path);
            StreamReaderWrapper wrapper = new(instance);

            return wrapper;
        }
    }
}
