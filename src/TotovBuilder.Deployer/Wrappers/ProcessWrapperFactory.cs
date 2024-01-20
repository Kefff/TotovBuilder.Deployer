using System.Diagnostics.CodeAnalysis;
using TotovBuilder.Deployer.Abstractions.Wrappers;

namespace TotovBuilder.Deployer.Wrappers
{
    /// <summary>
    /// Represents an <see cref="IProcessWrapperFactory"/> factory.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Wrapper to be able to create mocks of the Process class.")]
    public class ProcessWrapperFactory : IProcessWrapperFactory
    {
        /// <summary>
        /// Stream reader wrapper factory.
        /// </summary>
        private readonly IStreamReaderWrapperFactory StreamReaderWrapperFactory;

        /// <summary>
        /// Stream writer wrapper factory.
        /// </summary>
        private readonly IStreamWriterWrapperFactory StreamWriterWrapperFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessWrapperFactory"/> class.
        /// </summary>
        /// <param name="streamReaderWrapperFactory">Stream reader wrapper factory.</param>
        /// <param name="streamWriterWrapperFactory">Stream writer wrapper factory.</param>
        public ProcessWrapperFactory(IStreamReaderWrapperFactory streamReaderWrapperFactory, IStreamWriterWrapperFactory streamWriterWrapperFactory)
        {
            StreamReaderWrapperFactory = streamReaderWrapperFactory;
            StreamWriterWrapperFactory = streamWriterWrapperFactory;
        }

        /// <inheritdoc/>
        public IProcessWrapper Create()
        {
            ProcessWrapper wrapper = new ProcessWrapper(StreamReaderWrapperFactory, StreamWriterWrapperFactory);

            return wrapper;
        }
    }
}
