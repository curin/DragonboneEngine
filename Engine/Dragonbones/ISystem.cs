using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones
{
    /// <summary>
    /// The interface which all systems are derived from
    /// </summary>
    public interface ISystem : IEquatable<ISystem>
    {
        /// <summary>
        /// The system's header info
        /// </summary>
        SystemInfo SystemInfo { get; }
        /// <summary>
        /// The method run for this system
        /// </summary>
        /// <param name="deltaTime">time since last run</param>
        void Run(float deltaTime);
    }
}
