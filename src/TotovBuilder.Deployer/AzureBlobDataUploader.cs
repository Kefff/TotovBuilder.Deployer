using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using TotovBuilder.Deployer.Abstractions;
using TotovBuilder.Deployer.Extensions;

namespace TotovBuilder.Deployer
{
    /// <summary>
    /// Represents an Azure Blob data updloader.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AzureBlobDataUploader : IAzureBlobDataUploader
    {
        /// <summary>
        /// Blob container client.
        /// </summary>
        private BlobContainerClient? BlobContainerClient;

        /// <summary>
        /// Configuration.
        /// </summary>
        private readonly IApplicationConfiguration Configuration;

        /// <summary>
        /// Initialization task.
        /// </summary>
        private readonly Task InitializationTask;

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly ILogger<AzureBlobDataUploader> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobDataUploader"/> class.
        /// </summary>
        /// <param name="configuration">Configuration reader.</param>
        public AzureBlobDataUploader(ILogger<AzureBlobDataUploader> logger, IApplicationConfiguration configuration)
        {
            Configuration = configuration;
            Logger = logger;

            InitializationTask = Initialize();
        }

        /// <inheritdoc/>
        public async Task Upload()
        {
            await InitializationTask;

            List<Task> uploadTasks = new List<Task>();
            IEnumerable<string> blobNames = Configuration.AzureFunctionsConfiguration.GetBlobToUploadNames();

            foreach (string file in Directory.GetFiles(Configuration.ConfiguratorConfiguration.ConfigurationsDirectory).Where(f => blobNames.Any(bn => f.EndsWith(bn))))
            {
                uploadTasks.Add(Upload(file));
            }

            Task.WaitAll(uploadTasks.ToArray());
        }

        /// <summary>
        /// Initializes the connection to the Azure Storage.
        /// </summary>
        private async Task Initialize()
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(Configuration.AzureFunctionsConfiguration.AzureBlobStorageConnectionString);
            BlobContainerClient = blobServiceClient.GetBlobContainerClient(Configuration.AzureFunctionsConfiguration.AzureBlobStorageRawDataContainerName);
            await BlobContainerClient.CreateIfNotExistsAsync();
        }

        /// <summary>
        /// Uploads a file to a blob storage.
        /// </summary>
        /// <param name="file">File to upload.</param>
        /// <returns></returns>
        private async Task Upload(string file)
        {
            string fileName = Path.GetFileName(file);
            Logger.LogInformation(string.Format(Properties.Resources.Uploading, fileName));

            BlobClient blobClient = BlobContainerClient!.GetBlobClient(fileName);
            blobClient.DeleteIfExists();

            using FileStream fileStream = new FileStream(file, FileMode.Open);
            await blobClient.UploadAsync(fileStream);

            Logger.LogInformation(string.Format(Properties.Resources.FileUploaded, fileName));
        }
    }
}
