using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using TotovBuilder.Configurator.Abstractions;

namespace TotovBuilder.Configurator
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
        /// Configuration reader.
        /// </summary>
        private readonly IConfigurationReader ConfigurationReader;

        /// <summary>
        /// Initialization task.
        /// </summary>
        private readonly Task InitializationTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureBlobDataUploader"/> class.
        /// </summary>
        /// <param name="configurationReader">Configuration reader.</param>
        public AzureBlobDataUploader(IConfigurationReader configurationReader)
        {
            ConfigurationReader = configurationReader;

            InitializationTask = Initialize();
        }

        /// <inheritdoc/>
        public async Task Upload()
        {
            await InitializationTask;

            List<Task> uploadTasks = new();
            IEnumerable<string> blobNames = ConfigurationReader.AzureFunctionsConfiguration.GetBlobNames();

            foreach (string file in Directory.GetFiles(ConfigurationReader.ConfiguratorConfiguration.ConfigurationsDirectory).Where(f => blobNames.Any(bn => f.EndsWith(bn))))
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
            await ConfigurationReader.WaitForLoading();

            BlobServiceClient blobServiceClient = new(ConfigurationReader.AzureFunctionsConfiguration.AzureBlobStorageConnectionString);
            BlobContainerClient = blobServiceClient.GetBlobContainerClient(ConfigurationReader.AzureFunctionsConfiguration.AzureBlobStorageContainerName);
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

            using FileStream fileStream = new(file, FileMode.Open);
            await blobClient.UploadAsync(fileStream);

            Logger.LogSuccess(string.Format(Properties.Resources.FileUploaded, fileName));
        }
    }
}
