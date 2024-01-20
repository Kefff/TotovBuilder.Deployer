using System.IO;

namespace TotovBuilder.Deployer.Abstractions.Wrappers
{
    /// <summary>
    /// Provides the functionnalities of a <see cref="IStreamWriterWrapperFactory"/> factory.
    /// </summary>
    public interface IStreamWriterWrapperFactory
    {
        /// <summary>
        /// Creates an instance of an <see cref="IStreamWriterWrapper"/>.
        /// </summary>
        /// <param name="instance">Instance of a <see cref="StreamWriter"/> to wrap.</param>
        /// <returns>Instance.</returns>
        IStreamWriterWrapper Create(StreamWriter instance);
    }
}
