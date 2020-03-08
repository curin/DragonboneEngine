using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones
{
    /// <summary>
    /// The system's info
    /// </summary>
    public struct SystemInfo
    {
        /// <summary>
        /// Constructs the system info using priority
        /// </summary>
        /// <param name="name">the system's name</param>
        /// <param name="type">the type of the system</param>
        /// <param name="priority">the priority of the system</param>
        public SystemInfo(string name, SystemType type, long priority)
        {
            _name = name;
            _id = -1;
            Active = true;
            Priority = priority;
            Type = type;
            Running = false;
            RunReccurenceInterval = 0;
        }

        /// <summary>
        /// Constructs the system info using run recurrence
        /// </summary>
        /// <param name="name">the system's name</param>
        /// <param name="type">the type of the system</param>
        /// <param name="runReccurrenceInterval">the frame interval between runs</param>
        public SystemInfo(string name, SystemType type, int runReccurrenceInterval)
        {
            _name = name;
            _id = -1;
            Active = true;
            Priority = 0;
            Type = type;
            Running = false;
            RunReccurenceInterval = runReccurrenceInterval;
        }

        string _name;
        long _id;
        /// <summary>
        /// The type of system, which defines when it is run
        /// </summary>
        public SystemType Type { get; }
        /// <summary>
        /// What is the name of this system?
        /// </summary>
        public string Name => _name;
        /// <summary>
        /// The ID for the system
        /// </summary>
        public long ID => _id;
        /// <summary>
        /// Is this system currently active?
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// The priority of the system, used to determine which systems should be run less frequently in order to maintain framerate
        /// </summary>
        public long Priority { get; }
        /// <summary>
        /// If Set to 0, will run based on priority system.
        /// Otherwise, it represents the number of frames finished between each run
        /// </summary>
        public int RunReccurenceInterval { get; }
        /// <summary>
        /// Is this system currently running
        /// </summary>
        public bool Running { get; set; }
        /// <summary>
        /// What was the length for the last run of this system
        /// this is used in scheduling
        /// </summary>
        public double lastRunTime { get; set; }
        /// <summary>
        /// ID as set by System Registry
        /// </summary>
        /// <param name="id">the system's ID</param>
        public void SetID(long id)
        {
            _id = id;
        }
    }
}
