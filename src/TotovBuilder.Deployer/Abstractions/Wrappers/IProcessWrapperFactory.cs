namespace TotovBuilder.Deployer.Abstractions.Wrappers
{
    /// <summary>
    /// Provides the functionnalities of a <see cref="IProcessWrapperFactory"/> factory.
    /// </summary>
    public interface IProcessWrapperFactory
    {
        /// <summary>
        /// Creates an instance of a <see cref="IProcessWrapper"/>.
        /// </summary>
        /// <returns>Instance.</returns>
        IProcessWrapper Create();
    }
}
