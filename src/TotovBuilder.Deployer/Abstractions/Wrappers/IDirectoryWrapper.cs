namespace TotovBuilder.Deployer.Abstractions.Wrappers
{
    /// <summary>
    /// Provides the functionalities of a <see cref="Directory"/> wrapper.
    /// </summary>
    public interface IDirectoryWrapper
    {
        /// <summary>
        /// Creates all directories and subdirectories in the specified path unless they already exist.
        /// </summary>
        /// <param name="path">The directory to create.</param>
        void CreateDirectory(string path);

        /// <summary>
        /// Returns the names of subdirectories (including their paths) in the specified directory.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <returns>An array of the full names (including paths) of subdirectories in the specified path, or an empty array if no directories are found.</returns>
        string[] GetDirectories(string path);

        /// <summary>
        /// Returns the names of files (including their paths) in the specified directory.
        /// </summary>
        /// <param name="path">The relative or absolute path to the directory to search. This string is not case-sensitive.</param>
        /// <returns>An array of the full names (including paths) for the files in the specified directory, or an empty array if no files are found.</returns>
        string[] GetFiles(string path);
    }
}
