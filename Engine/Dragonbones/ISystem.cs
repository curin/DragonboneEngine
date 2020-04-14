using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones
{
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
