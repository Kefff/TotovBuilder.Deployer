using System.Threading.Tasks;

namespace TotovBuilder.Deployer.Abstractions.Wrappers
{
    /// <summary>
    /// Provides the functionalities of a <see cref="File"/> wrapper.
    /// </summary>
    public interface IFileWrapper
    {
        /// <summary>
        /// Detemines whether the specified file exists.
        /// </summary>
        /// <param name="path">The file to check.</param>
        /// <returns><see langword="true"/> if the caller has the required permissions and <paramref name="path"/> contains the name of an existing file; otherwise, <see langword="false"/>. This method also returns false if <paramref name="path"/> is null, an invalid path, or a zero-length string. If the caller does not have sufficient permissions to read the specified file, no exception is thrown and the method returns <see langword="false"/> regardless of the existence of <paramref name="path"/>.</returns>
        bool Exists(string? path);

        /// <summary>
        /// Moves a specified file to a new location, providing the option to specify a new file name.
        /// </summary>
        /// <param name="sourceFileName">The name of the file to move. Can include a relative or absolute path.</param>
        /// <param name="destFileName">The new path and name for the file.</param>
        void Move(string sourceFileName, string destFileName);

        /// <summary>
        /// Opens a binary file, reads the contents of the file into a byte array, and then closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <returns>A byte array containing the contents of the file.</returns>
        byte[] ReadAllBytes(string path);

        /// <summary>
        /// Asynchronously opens a text file, read all the text in the file, and then closes the file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>A task that represents the asynchronous read operation, which wraps the string containing all text in the file.</returns>
        Task<string> ReadAllTextAsync(string path);

        /// <summary>
        /// Creates a new file, write the contents to the file, and then closes the file. If the target file already exists, it is truncated and overwritten.
        /// </summary>
        /// <param name="path">The file to write to.</param>
        /// <param name="contents">The string to write to the file.</param>
        void WriteAllText(string path, string? contents);
    }
}
