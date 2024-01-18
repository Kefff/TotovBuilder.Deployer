using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TotovBuilder.Deployer.Logs;
using TotovBuilder.Shared.Abstractions.Utils;
using Xunit;

namespace TotovBuilder.Deployer.Test.Logs
{
    /// <summary>
    /// Represents tests on the <see cref="ApplicationLogger"/> class.
    /// </summary>
    public class ApplicationLoggerTest
    {
        [Fact]
        public void IsEnableb_ShouldReturnTrue()
        {
            // Arrange
            ApplicationLogger<object> applicationLogger = new ApplicationLogger<object>(new Mock<IConsoleWrapper>().Object);

            // Act / Assert
            applicationLogger.IsEnabled(LogLevel.Error).Should().BeTrue();
        }

        [Theory]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.None)]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Warning)]
        public void Log_ShouldLogMessageToConsole(LogLevel logLevel)
        {
            // Arrange
            Mock<IConsoleWrapper> consoleWrapperMock = new Mock<IConsoleWrapper>();

            ApplicationLogger<object> applicationLogger = new ApplicationLogger<object>(consoleWrapperMock.Object);

            // Act
            applicationLogger.Log(logLevel, "Starting file uploading");

            // Assert
            consoleWrapperMock.Verify(m => m.WriteLine("Starting file uploading"));
        }

        [Theory]
        [InlineData(LogLevel.Critical, true)]
        [InlineData(LogLevel.Error, false)]
        public void Log_WithError_ShouldLogErrorToConsoleInRed(LogLevel logLevel, bool useException)
        {
            // Arrange
            Mock<IConsoleWrapper> consoleWrapperMock = new Mock<IConsoleWrapper>();

            ApplicationLogger<object> applicationLogger = new ApplicationLogger<object>(consoleWrapperMock.Object);

            // Act
            applicationLogger.Log(logLevel, useException ? new Exception("Invalid file") : null, "Error while uploading");

            // Assert
            consoleWrapperMock.VerifySet(m => m.ForegroundColor = ConsoleColor.Red);
            consoleWrapperMock.Verify(m => m.WriteLine("Error :"));

            if (useException)
            {
                consoleWrapperMock.Verify(m => m.WriteLine(@"Error while uploading
System.Exception: Invalid file"));

            }
            else
            {
                consoleWrapperMock.Verify(m => m.WriteLine("Error while uploading"));
            }
        }

        [Fact]
        public void LogError_ShouldWriteToConsoleInRed()
        {
            // Arrange
            Mock<IConsoleWrapper> consoleWrapperMock = new Mock<IConsoleWrapper>();

            ApplicationLogger<object> applicationLogger = new ApplicationLogger<object>(consoleWrapperMock.Object);

            // Act
            applicationLogger.LogError("Error while uploading");

            // Assert
            consoleWrapperMock.VerifySet(m => m.ForegroundColor = ConsoleColor.Red);
            consoleWrapperMock.Verify(m => m.WriteLine("Error :"));
            consoleWrapperMock.Verify(m => m.WriteLine("Error while uploading"));
        }

        [Fact]
        public void LogSuccess_ShouldWriteToConsoleInGreen()
        {
            // Arrange
            Mock<IConsoleWrapper> consoleWrapperMock = new Mock<IConsoleWrapper>();

            ApplicationLogger<object> applicationLogger = new ApplicationLogger<object>(consoleWrapperMock.Object);

            // Act
            applicationLogger.LogSuccess("Blob updated");

            // Assert
            consoleWrapperMock.VerifySet(m => m.ForegroundColor = ConsoleColor.Green);
            consoleWrapperMock.Verify(m => m.WriteLine("Blob updated"));
        }
    }
}
