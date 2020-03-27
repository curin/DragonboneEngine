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
        /// <param name="systemLaneID">The id of system lane to provide a system for</param>
        /// <returns>if thereare more systems to run</returns>
        ScheduleResult NextSystem(int systemLaneID, out SystemInfo systemBatch);
        /// <summary>
        /// Add the next system to the schedule
        /// </summary>
        /// <param name="systemBatch">the batch of systems</param>
        void Add( SystemInfo systemBatch);
        /// <summary>
        /// Sets a lane to finished
        /// </summary>
        /// <param name="systemLaneID">the id of the lane to finish</param>
        void FinishLane(int systemLaneID);
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
    }

    public enum ScheduleResult
    {
        /// <summary>
        /// A new system was supplied
        /// </summary>
        Supplied,
        /// <summary>
        /// The Schedule is finished
        /// </summary>
        Finished,
        /// <summary>
        /// No system supplied due to conflicts
        /// retry soon
        /// </summary>
        Conflict
    }
}
