using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Sharprompt;
using TotovBuilder.Deployer.Abstractions.Logs;

namespace TotovBuilder.Deployer.Logs
{
    /// <summary>
    /// Represents a <see cref="PromptWrapper"/> wrapper.
    /// </summary>
    [ExcludeFromCodeCoverage(Justification = "Wrapper to be able to create mocks of the Prompt class.")]
    public class PromptWrapper : IPromtWrapper
    {
        /// <inheritdoc/>
        public T Input<T>(string message)
        {
            return Prompt.Input<T>(message);
        }

        /// <inheritdoc/>
        public T Select<T>(string message, IEnumerable<T> items)
        {
            return Prompt.Select<T>(message, items);
        }
    }
}
