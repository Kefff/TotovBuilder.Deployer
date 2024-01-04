using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TotovBuilder.Deployer.Abstractions;
using TotovBuilder.Deployer.Abstractions.Actions;
using TotovBuilder.Deployer.Extensions;
using TotovBuilder.Shared.Abstractions.Azure;

namespace TotovBuilder.Deployer.Actions
{
    /// <summary>
    /// Represents an action to deploy to Azure raw data used by Azure Functions to generated website data.
    /// </summary>
    public class DeployRawDataAction : IDeploymentAction
    {
        /// <inheritdoc/>
        public string Caption
        {
            get
            {
                return "Deploy raw data to Azure";
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
        public DeployRawDataAction(IApplicationConfiguration configuration, IAzureBlobStorageManager azureBlobStorageManager)
        {
            AzureBlobStorageManager = azureBlobStorageManager;
            Configuration = configuration;
        }

        /// <inheritdoc/>
        public Task ExecuteAction()
        {
            List<Task> deploymentTasks = new List<Task>();
            IEnumerable<string> blobNames = Configuration.AzureFunctionsConfiguration.GetBlobToUploadNames();

            foreach (string file in Directory.GetFiles(Configuration.ConfiguratorConfiguration.ConfigurationsDirectory).Where(f => blobNames.Any(bn => f.EndsWith(bn))))
            {
                string fileName = Path.GetFileName(file);
                string fileContent = File.ReadAllText(file);
                deploymentTasks.Add(AzureBlobStorageManager.UpdateBlob(Configuration.AzureFunctionsConfiguration.AzureBlobStorageRawDataContainerName, fileName, fileContent));
            }

            return Task.WhenAll(deploymentTasks.ToArray());
        }
    }
}
