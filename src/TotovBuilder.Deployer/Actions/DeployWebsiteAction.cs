using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using TotovBuilder.Deployer.Abstractions;
using TotovBuilder.Deployer.Abstractions.Actions;
using TotovBuilder.Shared.Abstractions.Azure;
using TotovBuilder.Shared.Azure;

namespace TotovBuilder.Deployer.Actions
{
    /// <summary>
    /// Represents an action to deploy the website to Azure.
    /// </summary>
    public class DeployWebsiteAction : IDeploymentAction
    {
        /// <inheritdoc/>
        public string Caption
        {
            get
            {
                return "Deploy the website to Azure";
            }
        }

        /// <summary>
        /// Azure blobl storage manager.
        /// </summary>
        private readonly IAzureBlobStorageManager AzureBlobStorageManager;

        /// <summary>
        /// Application configuration.
        /// </summary>
        private readonly IApplicationConfiguration Configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeployRawDataAction"/> class.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="azureBlobStorageManager"></param>
        public DeployWebsiteAction(IApplicationConfiguration configuration, IAzureBlobStorageManager azureBlobStorageManager)
        {
            AzureBlobStorageManager = azureBlobStorageManager;
            Configuration = configuration;
        }

        /// <inheritdoc/>
        public async Task ExecuteAction()
        {
            Dictionary<string, byte[]> data = new Dictionary<string, byte[]>();

            string websiteBuildDirectoryPath = Path.Combine(Configuration.ConfiguratorConfiguration.WebsiteDirectoryPath, Configuration.ConfiguratorConfiguration.WebsiteBuildDirectory);
            IEnumerable<string> filePaths = GetDirectoryFilePaths(websiteBuildDirectoryPath);

            foreach (string filePath in filePaths)
            {
                byte[] fileContent = File.ReadAllBytes(filePath);
                string azureFilePath = filePath.Replace(websiteBuildDirectoryPath + Path.DirectorySeparatorChar, string.Empty);
                data.Add(azureFilePath, fileContent);
            }

            BlobHttpHeaders httpHeaders = new BlobHttpHeaders()
            {
                CacheControl = Configuration.AzureFunctionsConfiguration.WebsiteFileCacheControl
            };
            await AzureBlobStorageManager.UpdateContainer(Configuration.AzureFunctionsConfiguration.AzureBlobStorageWebsiteContainerName, data, httpHeaders, "data/.*");
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
