﻿using System.Diagnostics.CodeAnalysis;
using System.IO;
using TotovBuilder.Deployer.Abstractions.Wrappers;

namespace TotovBuilder.Deployer.Wrappers
{
    /// <summary>
    /// Represents a <see cref="Directory"/> wrapper.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Wrapper to be able to create mocks of the Directory class.")]
    public class DirectoryWrapper : IDirectoryWrapper
    {
        /// <inheritdoc/>
        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(path);
        }
        
        /// <inheritdoc/>
        public string[] GetDirectories(string path)
        {
            return Directory.GetDirectories(path);
        }

        /// <inheritdoc/>
        public string[] GetFiles(string path)
        {
            return Directory.GetFiles(path);
        }
    }
}
