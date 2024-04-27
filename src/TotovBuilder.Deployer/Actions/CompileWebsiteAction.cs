using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TotovBuilder.Deployer.Abstractions.Actions;
using TotovBuilder.Deployer.Abstractions.Configuration;
using TotovBuilder.Deployer.Abstractions.Utils;
using TotovBuilder.Deployer.Abstractions.Wrappers;

namespace TotovBuilder.Deployer.Actions
{
    /// <summary>
    /// Represents an action to compile the Totov Builder website.
    /// </summary>
    public class CompileWebsiteAction : IDeploymentAction<CompileWebsiteAction>
    {
        /// <inheritdoc/>
        public string Caption
        {
            get
            {
                return Properties.Resources.CompileWebsiteAction;
            }
        }

        /// <summary>
        /// Application configuration.
        /// </summary>
        private readonly IApplicationConfiguration Configuration;

        /// <summary>
        /// Logger.
        /// </summary>
        private readonly IApplicationLogger<CompileWebsiteAction> Logger;

        /// <summary>
        /// Process wrapper factory.
        /// </summary>
        private readonly IProcessWrapperFactory ProcessWrapperFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompileWebsiteAction"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="configuration">Configuration</param>
        /// <param name="processWrapperFactory">Process wrapper factory.</param>
        public CompileWebsiteAction(IApplicationLogger<CompileWebsiteAction> logger, IApplicationConfiguration configuration, IProcessWrapperFactory processWrapperFactory)
        {
            Configuration = configuration;
            Logger = logger;
            ProcessWrapperFactory = processWrapperFactory;
        }

        /// <inheritdoc/>
        public Task ExecuteAction()
        {
            Logger.LogInformation(string.Format(Properties.Resources.CompilingWebsite, Configuration.DeployerConfiguration.WebsiteCompilationCommand, Configuration.DeployerConfiguration.WebsiteDirectoryPath));

            using (IProcessWrapper processWrapper = ProcessWrapperFactory.Create())
            {
                processWrapper.StartInfo.FileName = "cmd";
                processWrapper.StartInfo.RedirectStandardInput = true;
                processWrapper.StartInfo.RedirectStandardOutput = true;
                processWrapper.StartInfo.WorkingDirectory = Configuration.DeployerConfiguration.WebsiteDirectoryPath;
                processWrapper.Start();

                processWrapper.StandardInput!.WriteLine($"{Configuration.DeployerConfiguration.WebsiteCompilationCommand} & exit");

                string? output = null;

                do
                {
                    output = processWrapper.StandardOutput!.ReadLine();

                    if (output != null)
                    {
                        Console.WriteLine(output);
                    }
                }
                while (output != null);

                processWrapper.WaitForExit();
            }

            Logger.LogSuccess(Properties.Resources.WebsiteCompiled);

            return Task.CompletedTask;
        }
    }
}
