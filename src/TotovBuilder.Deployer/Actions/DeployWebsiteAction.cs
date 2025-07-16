using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using TotovBuilder.Deployer.Abstractions.Actions;
using TotovBuilder.Deployer.Abstractions.Configuration;
using TotovBuilder.Deployer.Abstractions.Utils;
using TotovBuilder.Deployer.Abstractions.Wrappers;
using TotovBuilder.Shared.Abstractions.Azure;

namespace TotovBuilder.Deployer.Actions
{
    /// <summary>
    /// Represents an action to deploy the website to Azure.
    /// </summary>
    public class DeployWebsiteAction : IDeploymentAction<DeployWebsiteAction>
    {
        /// <inheritdoc/>
        public string Caption
        {
            get
            {
                return Properties.Resources.DeployWebsiteAction;
            }
        }

        /// <summary>
        /// Azure blob storage manager.
        /// </summary>
        private readonly IAzureBlobStorageManager AzureBlobStorageManager;

        /// <summary>
        /// Application configuration.
        /// </summary>
        private readonly IApplicationConfiguration Configuration;

        /// <summary>
        /// Directory wrapper.
        /// </summary>
        private readonly IDirectoryWrapper DirectoryWrapper;

        /// <summary>
        /// File wrapper.
        /// </summary>
        private readonly IFileWrapper FileWrapper;

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly IApplicationLogger<DeployWebsiteAction> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeployRawDataAction"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="fileWrapper">File wrapper.</param>
        /// <param name="azureBlobStorageManager">Azure blob manager.</param>
        public DeployWebsiteAction(
            IApplicationLogger<DeployWebsiteAction> logger,
            IApplicationConfiguration configuration,
            IFileWrapper fileWrapper,
            IDirectoryWrapper directoryWrapper,
            IAzureBlobStorageManager azureBlobStorageManager)
        {
            AzureBlobStorageManager = azureBlobStorageManager;
            Configuration = configuration;
            DirectoryWrapper = directoryWrapper;
            FileWrapper = fileWrapper;
            Logger = logger;
        }

        /// <inheritdoc/>
        public async Task ExecuteAction()
        {
            Logger.LogInformation(Properties.Resources.DeployingWebsite);

            Dictionary<string, byte[]> data = [];

            string websiteBuildDirectoryPath = Path.Combine(Configuration.DeployerConfiguration.WebsiteDirectoryPath, Configuration.DeployerConfiguration.WebsiteBuildDirectory);
            IEnumerable<string> filePaths = GetDirectoryFilePaths(websiteBuildDirectoryPath);

            foreach (string filePath in filePaths)
            {
                byte[] fileContent = FileWrapper.ReadAllBytes(filePath);
                string azureFilePath = filePath.Replace(websiteBuildDirectoryPath + Path.DirectorySeparatorChar, string.Empty);
                data.Add(azureFilePath, fileContent);
            }

            await AzureBlobStorageManager.UpdateContainer(
                Configuration.AzureFunctionsConfiguration.AzureBlobStorageWebsiteContainerName,
                data,
                () => new BlobHttpHeaders
                {
                    CacheControl = Configuration.AzureFunctionsConfiguration.WebsiteFileCacheControl
                },
                Configuration.DeployerConfiguration.WebsiteDeploymentFileNotToDeletePattern);

            Logger.LogSuccess(Properties.Resources.WebsiteDeployed);
        }

        /// <summary>
        /// Gets the path of the files in a directory.
        /// </summary>
        /// <param name="directoryPath">Directory path.</param>
        /// <returns>File paths.</returns>
        private IEnumerable<string> GetDirectoryFilePaths(string directoryPath)
        {
            string[] files = DirectoryWrapper.GetFiles(directoryPath);
            string[] subDirectoryFiles = DirectoryWrapper.GetDirectories(directoryPath)
                .SelectMany(dp => GetDirectoryFilePaths(dp))
                .ToArray();

            return files.Concat(subDirectoryFiles);
        }
    }
}
