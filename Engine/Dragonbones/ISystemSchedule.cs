using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones
{
    public interface ISystemSchedule
    {
        /// <summary>
        /// Has the schedule been finished?
        /// </summary>
        bool Finished { get; }
        /// <summary>
        /// Gives the next system to run
        /// </summary>
        /// <param name="system">the info of the next system to run</param>
        /// <returns>if there is another system to run</returns>
        bool NextSystem(out SystemInfo system);
        /// <summary>
        /// Resets Schedule to start
        /// </summary>
        void Reset();
    }
}
