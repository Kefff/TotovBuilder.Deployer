using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using TotovBuilder.Deployer.Abstractions.Wrappers;

namespace TotovBuilder.Deployer.Wrappers
{
    /// <summary>
    /// Represents a <see cref="ProcessWrapper"/> wrapper.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Wrapper to be able to create mocks of the Process class.")]
    public class ProcessWrapper : IProcessWrapper
    {
        /// <inheritdoc/>
        public ProcessStartInfo StartInfo
        {
            get
            {
                return Instance.StartInfo;
            }
            set
            {
                Instance.StartInfo = value;
            }
        }

        /// <inheritdoc/>
        public IStreamReaderWrapper? StandardOutput { get; private set; }

        /// <inheritdoc/>
        public IStreamWriterWrapper? StandardInput { get; private set; }

        /// <summary>
        /// Instance.
        /// </summary>
        private Process Instance { get; }

        /// <summary>
        /// Stream reader wrapper factory.
        /// </summary>
        private readonly IStreamReaderWrapperFactory StreamReaderWrapperFactory;

        /// <summary>
        /// Stream writer wrapper factory.
        /// </summary>
        private readonly IStreamWriterWrapperFactory StreamWriterWrapperFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessWrapper"/> class.
        /// </summary>
        /// <param name="streamReaderWrapperFactory">Stream reader wrapper.</param>
        /// <param name="streamWriterWrapperFactory">Stream writer wrapper.</param>
        public ProcessWrapper(IStreamReaderWrapperFactory streamReaderWrapperFactory, IStreamWriterWrapperFactory streamWriterWrapperFactory)
        {
            StreamReaderWrapperFactory = streamReaderWrapperFactory;
            StreamWriterWrapperFactory = streamWriterWrapperFactory;

            Instance = new Process();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            StandardInput?.Dispose();
            StandardOutput?.Dispose();
            Instance.Dispose();
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public void Start()
        {
            Instance.Start();

            if (StartInfo.RedirectStandardInput)
            {
                StandardInput = StreamWriterWrapperFactory.Create(Instance.StandardInput);
            }

            if (StartInfo.RedirectStandardOutput)
            {
                StandardOutput = StreamReaderWrapperFactory.Create(Instance.StandardOutput);
            }
        }

        /// <inheritdoc/>
        public void WaitForExit()
        {
            Instance.WaitForExit();
        }
    }
}
