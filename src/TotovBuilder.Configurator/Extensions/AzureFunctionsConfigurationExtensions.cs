using System;
using System.Collections.Generic;
using System.Linq;
using TotovBuilder.Model.Configuration;

namespace TotovBuilder.Configurator.Extensions
{
    /// <summary>
    /// Represents an extention class for <see cref="AzureFunctionsConfiguration"/>.
    /// </summary>
    public static class AzureFunctionsConfigurationExtensions
    {
        /// <summary>
        /// Gets the names of the blobs to upload to Azure.
        /// </summary>
        /// <returns>Names of the blobs to upload.</returns>
        public static IEnumerable<string> GetBlobToUploadNames(this AzureFunctionsConfiguration azureFunctionsConfiguration)
        {
            Type azureFunctionsConfigurationType = typeof(AzureFunctionsConfiguration);
            IEnumerable<string> blobsToUpload = azureFunctionsConfigurationType.GetProperties()
                .Where(p => p.Name.StartsWith("Raw") && p.Name.EndsWith("BlobName"))
                .Select(p => p.GetValue(azureFunctionsConfiguration) as string ?? string.Empty);

            return blobsToUpload;
        }
    }
}
