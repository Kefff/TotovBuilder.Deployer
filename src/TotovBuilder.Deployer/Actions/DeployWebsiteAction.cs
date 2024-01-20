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
        public DeployWebsiteAction(IApplicationLogger<DeployWebsiteAction> logger, IApplicationConfiguration configuration, IFileWrapper fileWrapper, IAzureBlobStorageManager azureBlobStorageManager)
        {
            AzureBlobStorageManager = azureBlobStorageManager;
            Configuration = configuration;
            FileWrapper = fileWrapper;
            Logger = logger;
        }

        /// <inheritdoc/>
        public async Task ExecuteAction()
        {
            Logger.LogInformation(Properties.Resources.DeployingWebsite);

            Dictionary<string, byte[]> data = new Dictionary<string, byte[]>();

            string websiteBuildDirectoryPath = Path.Combine(Configuration.DeployerConfiguration.WebsiteDirectoryPath, Configuration.DeployerConfiguration.WebsiteBuildDirectory);
            IEnumerable<string> filePaths = GetDirectoryFilePaths(websiteBuildDirectoryPath);

            foreach (string filePath in filePaths)
            {
                byte[] fileContent = FileWrapper.ReadAllBytes(filePath);
                string azureFilePath = filePath.Replace(websiteBuildDirectoryPath + Path.DirectorySeparatorChar, string.Empty);
                data.Add(azureFilePath, fileContent);
            }

            BlobHttpHeaders createHttpHeaders() => new BlobHttpHeaders
            {
                CacheControl = Configuration.AzureFunctionsConfiguration.WebsiteFileCacheControl
            };
            await AzureBlobStorageManager.UpdateContainer(
                Configuration.AzureFunctionsConfiguration.AzureBlobStorageWebsiteContainerName,
                data,
                createHttpHeaders,
                Configuration.DeployerConfiguration.WebsiteDeploymentFileNotToDeletePattern);

            Logger.LogSuccess(Properties.Resources.WebsiteDeployed);
        }

        /// <summary>
        /// Gets the path of the files in a directory.
        /// </summary>
        /// <param name="directoryPath">Directory path.</param>
        /// <returns>File paths.</returns>
        private static IEnumerable<string> GetDirectoryFilePaths(string directoryPath)
        {
            string[] files = Directory.GetFiles(directoryPath);
            string[] subDirectoryFiles = Directory.GetDirectories(directoryPath)
                .SelectMany(dp => GetDirectoryFilePaths(dp))
                .ToArray();

            return files.Concat(subDirectoryFiles);
        }
    }
}
