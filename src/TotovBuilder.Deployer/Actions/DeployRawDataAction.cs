using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TotovBuilder.Deployer.Abstractions.Actions;
using TotovBuilder.Deployer.Abstractions.Configuration;
using TotovBuilder.Deployer.Abstractions.Utils;
using TotovBuilder.Deployer.Abstractions.Wrappers;
using TotovBuilder.Deployer.Extensions;
using TotovBuilder.Shared.Abstractions.Azure;

namespace TotovBuilder.Deployer.Actions
{
    /// <summary>
    /// Represents an action to deploy to Azure raw data used by Azure Functions to generated website data.
    /// </summary>
    public class DeployRawDataAction : IDeploymentAction<DeployRawDataAction>
    {
        /// <inheritdoc/>
        public string Caption
        {
            get
            {
                return Properties.Resources.DeployRawDataAction;
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
        private readonly IApplicationLogger<DeployRawDataAction> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeployRawDataAction"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="configuration">Configuration.</param>
        /// <param name="fileWrapper">File wrapper.</param>
        /// <param name="azureBlobStorageManager">Azure blob storage manager.</param>
        public DeployRawDataAction(
            IApplicationLogger<DeployRawDataAction> logger,
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
        public Task ExecuteAction()
        {
            Logger.LogInformation(Properties.Resources.DeployingRawData);

            List<Task> uploadTasks = new List<Task>();
            IEnumerable<string> blobNames = Configuration.AzureFunctionsConfiguration.GetBlobToUploadNames();

            foreach (string filePath in DirectoryWrapper.GetFiles(Configuration.DeployerConfiguration.ConfigurationsDirectory).Where(f => blobNames.Any(bn => f.EndsWith(bn))))
            {
                string fileName = Path.GetFileName(filePath);
                byte[] fileContent = FileWrapper.ReadAllBytes(filePath);
                uploadTasks.Add(AzureBlobStorageManager.UpdateBlob(Configuration.AzureFunctionsConfiguration.AzureBlobStorageRawDataContainerName, fileName, fileContent));
            }

            Task.WaitAll(uploadTasks.ToArray());

            Logger.LogSuccess(Properties.Resources.RawDataDeployed);

            return Task.CompletedTask;
        }
    }
}
