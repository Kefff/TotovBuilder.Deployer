namespace TotovBuilder.Configurator
{
    /// <summary>
    /// Represents the configuration of the Totov Builder configurator.
    /// </summary>
    public class ConfiguratorConfiguration
    {
        /// <summary>
        /// Name of the Azure blob containing the Azure Functions configuration.
        /// </summary>
        public string AzureFunctionsConfigurationBlobName { get; set; } = string.Empty;

        /// <summary>
        /// Directory containing all the TotovBuilder configuration files.
        /// </summary>
        public string ConfigurationsDirectory { get; set; } = string.Empty;

        /// <summary>
        /// Configurator configuration file name.
        /// </summary>
        public string ConfiguratorConfigurationFileName { get; set; } = string.Empty;

        /// <summary>
        /// String to search to dertemine where the items data ends in the Tarkov resources file when extracting items.
        /// </summary>
        public string ItemsExtractionEndSearchString { get; set; } = string.Empty;

        /// <summary>
        /// String to search to dertemine where the items data starts in the Tarkov resources file when extracting items.
        /// </summary>
        public string ItemsExtractionStartSearchString { get; set; } = string.Empty;

        /// <summary>
        /// Directory for archiving previous data extractions.
        /// </summary>
        public string PreviousExtractionsArchiveDirectory { get; set; } = string.Empty;

        /// <summary>
        /// Path of the Tarkov file data are extracted from.
        /// </summary>
        public string TarkovResourcesFilePath { get; set; } = string.Empty;
    }
}
