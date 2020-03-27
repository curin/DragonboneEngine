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
        /// Gives the system to run
        /// </summary>
        /// <param name="system">the info of the system to run</param>
        /// <returns>if thereare more systems to run</returns>
        bool NextSystem(out SystemInfo systemBatch);
        /// <summary>
        /// Add the next system to the schedule
        /// </summary>
        /// <param name="systemBatch">the batch of systems</param>
        /// <param name="batchSize">how many systems are in this batch</param>
        void Add(SystemInfo systemBatch);
        /// <summary>
        /// Sorts the schedule by the comparer function
        /// </summary>
        /// <param name="comparer"></param>
        void Sort(Comparison<SystemInfo> comparer);
        /// <summary>
        /// Resets Schedule to start
        /// </summary>
        void Reset();
        /// <summary>
        /// Clears the Schedule
        /// </summary>
        void Clear();
        /// <summary>
        /// The number of systems in this schedule
        /// </summary>
        int Count { get; }
        /// <summary>
        /// The number of system batches in this schedule
        /// </summary>
        int BatchCount { get; }
    }
}
