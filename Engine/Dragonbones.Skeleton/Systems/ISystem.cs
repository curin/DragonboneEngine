﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones.Systems
{
    /// <summary>
    /// The interface which all systems are derived from
    /// Systems should not contain any state data for the game itself only
    /// </summary>
    public interface ISystem : IEquatable<ISystem>, IDisposable
    {
        /// <summary>
        /// The system's header info
        /// </summary>
        SystemInfo Info { get; }
        /// <summary>
        /// Method for required setup period prior to system run
        /// </summary>
        void Initialize();
        /// <summary>
        /// The method run for this system
        /// </summary>
        /// <param name="deltaTime">time since last logic update (this should be consistent for logic systems and variable for render)</param>
        void Run(float deltaTime);
    }
}
