using System.Threading.Tasks;

namespace TotovBuilder.Deployer.Abstractions
{
    /// <summary>
    /// Provides the functionalities of a Tarkov data extractor.
    /// </summary>
    public interface ITarkovDataExtractor
    {
        /// <summary>
        /// Extracts Tarkov data and saves them in files in the configurations directory.
        /// </summary>
        Task Extract();
    }
}
