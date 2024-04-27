using System;
using System.Diagnostics;

namespace TotovBuilder.Deployer.Abstractions.Wrappers
{
    /// <summary>
    /// Provides the functionalities of a <see cref="Process"/> wrapper.
    /// </summary>
    public interface IProcessWrapper : IDisposable
    {
        /// <summary>
        /// Gets or sets the properties to pass to the <see cref="Process.Start()"/> method of the <see cref="Process"/>.
        /// </summary>
        ProcessStartInfo StartInfo { get; set; }

        /// <summary>
        /// Gets a stream used to read the textual output of the application.
        /// Has no value until <see cref="Start"/> is called.
        /// </summary>
        IStreamReaderWrapper? StandardOutput { get; }

        /// <summary>
        /// Gets a stream used to write the input of the application.
        /// Has no value until <see cref="Start"/> is called.
        /// </summary>
        IStreamWriterWrapper? StandardInput { get; }

        /// <summary>
        /// Starts a process resource and associates it with a <see cref="Process"/> component.
        /// </summary>
        void Start();

        /// <summary>
        /// Sets the period of time to wait for the associated process to exit, and blocks the current thread of execution until the time has elapsed or the process has exited. To avoid blocking the current thread, use the Exited event.
        /// </summary>
        void WaitForExit();
    }
}
