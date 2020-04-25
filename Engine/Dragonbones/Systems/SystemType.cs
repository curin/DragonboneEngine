using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones.Systems
{
    /// <summary>
    /// Defines what a system is used for
    /// </summary>
    public enum SystemType
    {
        /// <summary>
        /// Render systems run as often as possible but logic systems take priority
        /// Should only be used for rendering
        /// Should not be used to update logic
        /// </summary>
        Render = 2,
        /// <summary>
        /// Logic systems run on a multiple of 1/60th of a second time step
        /// Should only be used for logic
        /// Should not be used to draw anything to screen
        /// </summary>
        Logic = 0
    }
}
