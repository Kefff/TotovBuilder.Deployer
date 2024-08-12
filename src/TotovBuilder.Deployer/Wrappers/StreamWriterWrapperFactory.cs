using System.Diagnostics.CodeAnalysis;
using System.IO;
using TotovBuilder.Deployer.Abstractions.Wrappers;

namespace TotovBuilder.Deployer.Wrappers
{
    /// <summary>
    /// Represents an <see cref="IStreamWriterWrapperFactory"/> factory.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Wrapper to be able to create mocks of the StreamWriter class.")]
    public class StreamWriterWrapperFactory : IStreamWriterWrapperFactory
    {
        /// <inheritdoc/>
        public IStreamWriterWrapper Create(StreamWriter instance)
        {
            StreamWriterWrapper wrapper = new(instance);

            return wrapper;
        }
    }
}
