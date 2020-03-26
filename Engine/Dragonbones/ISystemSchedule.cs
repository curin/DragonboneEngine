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
        /// Gives the next batch of systems to run
        /// </summary>
        /// <param name="system">the info of the next batch of systems to run</param>
        /// <returns>if thereare more systems to run</returns>
        bool NextBatch(out ISystemBatch systemBatch);
        /// <summary>
        /// Add the next batch of systems to the schedule
        /// </summary>
        /// <param name="systemBatch">the batch of systems</param>
        /// <param name="batchSize">how many systems are in this batch</param>
        void Add(ISystemBatch systemBatch);
        /// <summary>
        /// adds a range of system batches to the schedule
        /// </summary>
        /// <param name="batches">batches to add</param>
        void AddRange(IEnumerable<ISystemBatch> batches);
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
