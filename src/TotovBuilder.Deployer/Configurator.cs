using System.Threading.Tasks;
using TotovBuilder.Deployer.Abstractions;

namespace TotovBuilder.Deployer
{
    /// <summary>
    /// Represents a configurator.
    /// </summary>
    public class Configurator : IConfigurator
    {
        /// <summary>
        /// Azure blob data uploader.
        /// </summary>
        private readonly IAzureBlobDataUploader? AzureBlobDataUploader;

        /// <summary>
        /// Configuration reader.
        /// </summary>
        private readonly IConfigurationReader ConfigurationReader;

        /// <summary>
        /// Tarkov data extractor.
        /// </summary>
        private readonly ITarkovDataExtractor TarkovDataExtractor;

        /// <summary>
        /// Initializes an new instance of the <see cref="Configurator"/> class.
        /// </summary>
        /// <param name="configurationReader">Configuration reader.</param>
        /// <param name="tarkovDataExtractor">Tarkov data extractor.</param>
        /// <param name="azureBlobDataUploader">Azure blob data uploader.</param>
        public Configurator(IConfigurationReader configurationReader, ITarkovDataExtractor tarkovDataExtractor, IAzureBlobDataUploader? azureBlobDataUploader)
        {
            AzureBlobDataUploader = azureBlobDataUploader;
            ConfigurationReader = configurationReader;
            TarkovDataExtractor = tarkovDataExtractor;
        }

        /// <inheritdoc/>
        public async Task Execute()
        {
            await ConfigurationReader.WaitForLoading();
            await TarkovDataExtractor.Extract();

            if (AzureBlobDataUploader != null)
            {
                await AzureBlobDataUploader.Upload();
            }
        }
    }
}
