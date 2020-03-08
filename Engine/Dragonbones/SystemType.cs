using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones
{
    public enum SystemType
    {
        /// <summary>
        /// Logic systems run on a multiple of 1/60th of a second time step
        /// </summary>
        Logic,
        /// <summary>
        /// Render systems run as often as possible but logic systems take priority
        /// </summary>
        Render
    }
}
