using System.IO;

namespace TotovBuilder.Deployer.Abstractions.Wrappers
{
    /// <summary>
    /// Provides the functionnalities of a <see cref="IStreamReaderWrapperFactory"/> factory.
    /// </summary>
    public interface IStreamReaderWrapperFactory
    {
        /// <summary>
        /// Creates an instance of an <see cref="IStreamReaderWrapper"/>.
        /// </summary>
        /// <param name="instance">Instance of a <see cref="StreamReader"/> to wrap.</param>
        /// <returns>Instance.</returns>
        IStreamReaderWrapper Create(StreamReader instance);

        /// <summary>
        /// Creates an instance of an <see cref="IStreamReaderWrapper"/>.
        /// </summary>
        /// <param name="path">The complete file path to be read.</param>
        /// <returns>Instance.</returns>
        IStreamReaderWrapper Create(string path);
    }
}
