using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones.Systems
{
    /// <summary>
    /// A class that orders systems by priority for running and handles supplying systems one at a time for each lane of systems running.
    /// </summary>
    public interface ISystemSchedule : IDisposable
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
        ScheduleResult NextSystem(int systemLaneID, out SystemInfo system);
        /// <summary>
        /// Register the next system to the schedule
        /// </summary>
        /// <param name="systemBatch">the batch of systems</param>
        void Add( SystemInfo systemBatch);
        /// <summary>
        /// Adds all systems from the attached registry
        /// </summary>
        /// <param name="registry">the system registry to add</param>
        void AddFromRegistry(ISystemRegistry registry);
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
        /// <summary>
        /// The type of systems for this schedule
        /// </summary>
        SystemType Type { get; }
    }

    /// <summary>
    /// The result of attempting to schedule the another system
    /// </summary>
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
