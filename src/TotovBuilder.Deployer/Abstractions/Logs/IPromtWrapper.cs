
using System.Collections.Generic;
using Sharprompt;

namespace TotovBuilder.Deployer.Abstractions.Logs
{
    /// <summary>
    /// Provides the functionalities of a <see cref="Prompt"/> wrapper.
    /// </summary>
    public interface IPromtWrapper
    {
        /// <summary>
        /// Ask for a value.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <typeparam name="T">Type of expected value.</typeparam>
        /// <returns>Value entered.</returns>
        T Input<T>(string message);

        /// <summary>
        /// Displays a selection prompt.
        /// </summary>
        /// <param name="message">Messages.</param>
        /// <param name="items">Options to select from.</param>
        /// <returns>Selected option.</returns>
        /// <typeparam name="T">Type of elements to select from.</typeparam>
        T Select<T>(string message, IEnumerable<T> items);
    }
}
