using System.Threading.Tasks;
using Moq;
using TotovBuilder.Deployer.Abstractions;
using Xunit;

namespace TotovBuilder.Deployer.Test
{
    /// <summary>
    /// Represents tests on the <see cref="Deployer"/> class.
    /// </summary>
    public class ConfiguratorTest
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Execute_ShouldExecute(bool upload)
        {
            // Arrange
            Mock<IConfigurationLoader> configurationReaderMock = new Mock<IConfigurationLoader>();
            Mock<ITarkovDataExtractor> tarkovDataExtractorMock = new Mock<ITarkovDataExtractor>();
            Mock<IAzureBlobDataUploader>? azureBlobDataUploaderMock = upload ? new Mock<IAzureBlobDataUploader>() : null;
            Deployer configurator = new Deployer(configurationReaderMock.Object, tarkovDataExtractorMock.Object, azureBlobDataUploaderMock?.Object);

            // Act
            await configurator.Run();

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
