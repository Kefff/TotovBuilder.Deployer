using System.Threading.Tasks;

namespace TotovBuilder.Deployer.Abstractions
{
    /// <summary>
    /// Provides the functionalities of an Azure Blob data updloader.
    /// </summary>
    public interface IAzureBlobDataUploader
    {
        /// <summary>
        /// Upload all the configuration files contained in the configurations directory to their blob storage.
        /// </summary>
        Task Upload();
    }
}
