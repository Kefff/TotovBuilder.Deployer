using System.Threading.Tasks;
using Moq;
using TotovBuilder.Deployer.Abstractions;
using Xunit;

namespace TotovBuilder.Deployer.Test
{
    /// <summary>
    /// Represents tests on the <see cref="Configurator"/> class.
    /// </summary>
    public class ConfiguratorTest
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Execute_ShouldExecute(bool upload)
        {
            // Arrange
            Mock<IConfigurationReader> configurationReaderMock = new Mock<IConfigurationReader>();
            Mock<ITarkovDataExtractor> tarkovDataExtractorMock = new Mock<ITarkovDataExtractor>();
            Mock<IAzureBlobDataUploader>? azureBlobDataUploaderMock = upload ? new Mock<IAzureBlobDataUploader>() : null;
            Configurator configurator = new Configurator(configurationReaderMock.Object, tarkovDataExtractorMock.Object, azureBlobDataUploaderMock?.Object);

            // Act
            await configurator.Execute();

            // Assert
            configurationReaderMock.Verify(m => m.WaitForLoading());
            tarkovDataExtractorMock.Verify(m => m.Extract());

            if (upload)
            {
                azureBlobDataUploaderMock?.Verify(m => m.Upload());
            }
            else
            {
                azureBlobDataUploaderMock?.Verify(m => m.Upload(), Times.Never);
            }
        }
    }
}
