using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones
{
    public interface ISystem
    {
        /// <summary>
        /// What is the name of this system?
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Is this system currently active?
        /// </summary>
        bool Active { get; set; }
        /// <summary>
        /// The priority of the system, used to determine which systems should be run less frequently in order to maintain framerate
        /// </summary>
        int Priority { get; }
        /// <summary>
        /// If Set to 0, will run based on priority system.
        /// Otherwise, it represents the number of frames finished between each run
        /// </summary>
        int RunReccurenceInterval { get; }
        /// <summary>
        /// Is this system currently running
        /// </summary>
        bool Running { get; set; }
    }
}
