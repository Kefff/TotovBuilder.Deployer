using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TotovBuilder.Deployer.Abstractions;
using TotovBuilder.Deployer.Abstractions.Actions;

namespace TotovBuilder.Deployer.Actions
{
    /// <summary>
    /// Represents an action to compile the Totov Builder website.
    /// </summary>
    public class CompileWebsiteAction : IDeploymentAction
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
        /// Initializes a new instance of the <see cref="CompileWebsiteAction"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="configuration">Configuration</param>
        public CompileWebsiteAction(IApplicationLogger<CompileWebsiteAction> logger, IApplicationConfiguration configuration)
        {
            Configuration = configuration;
            Logger = logger;
        }

        /// <inheritdoc/>
        public Task ExecuteAction()
        {
            Logger.LogInformation(string.Format(Properties.Resources.CompilingWebsite, Configuration.DeployerConfiguration.WebsiteCompilationCommand, Configuration.DeployerConfiguration.WebsiteDirectoryPath));

            Process process = new Process();
            process.StartInfo.FileName = "cmd";
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.WorkingDirectory = Configuration.DeployerConfiguration.WebsiteDirectoryPath;
            process.Start();

            process.StandardInput.WriteLine($"{Configuration.DeployerConfiguration.WebsiteCompilationCommand} & exit");

            string? output = null;

            do
            {
                output = process.StandardOutput.ReadLine();

                if (output != null)
                {
                    Console.WriteLine(output);
                }
            }
            while (output != null);

            process.WaitForExit();

            Logger.LogSuccess(Properties.Resources.WebsiteCompiled);

            return Task.CompletedTask;
        }
    }
}
