using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones
{
    /// <summary>
    /// The system's info
    /// </summary>
    public class SystemInfo
    {
        /// <summary>
        /// Constructs the system info using priority
        /// </summary>
        /// <param name="name">the system's name</param>
        /// <param name="type">the type of the system</param>
        /// <param name="priority">the priority of the system</param>
        public SystemInfo(string name, SystemType type, int priority, bool active = true, params string[] componentsUsed)
        {
            _name = name;
            _id = -1;
            Active = active;
            Priority = priority;
            Type = type;
            Running = false;
            RunReccurenceInterval = 0;
            lastRunTime = 0;
            ComponentsUsed = componentsUsed;
            _componentIDs = new int[componentsUsed.Length];
            Age = 0;
        }

        /// <summary>
        /// Constructs the system info using run recurrence
        /// </summary>
        /// <param name="name">the system's name</param>
        /// <param name="type">the type of the system</param>
        /// <param name="runReccurrenceInterval">the frame interval between runs</param>
        public SystemInfo(string name, int runReccurrenceInterval, SystemType type, bool active = true, params string[] componentsUsed)
        {
            _name = name;
            _id = -1;
            Active = active;
            Priority = 0;
            Type = type;
            Running = false;
            RunReccurenceInterval = runReccurrenceInterval;
            lastRunTime = 0;
            ComponentsUsed = componentsUsed;
            _componentIDs = new int[componentsUsed.Length];
            Age = 0;
        }

        string _name;
        int _id;
        int[] _componentIDs;
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
        public int ID => _id;
        /// <summary>
        /// The ids of the component types used by this system
        /// </summary>
        public int[] ComponentsUsedIDs => _componentIDs;
        /// <summary>
        /// The types of components this system uses
        /// </summary>
        public string[] ComponentsUsed { get; }
        /// <summary>
        /// Is this system currently active?
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// The priority of the system, used to determine which systems should be run less frequently in order to maintain framerate
        /// Priority is grouped by RunRecurrence (0 being the lowest, 1 the highest, then everything after)
        /// </summary>
        public int Priority { get; }
        /// <summary>
        /// If Set to 0, will run based on priority system only.
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
        /// How many frames have passed since last execution
        /// </summary>
        public int Age { get; set; }
        /// <summary>
        /// ID as set by System Registry
        /// </summary>
        /// <param name="id">the system's ID</param>
        public void SetID(int id)
        {
            _id = id;
        }
        /// <summary>
        /// Method so system can set the component ids from the names
        /// </summary>
        /// <param name="ids"></param>
        public void GetComponentIDs(int[] ids)
        {
            _componentIDs = ids;
        }

        public override string ToString()
        {
            return ID.ToString() + "\t" + Name.ToString() + "\t" + "Type:" + Type.ToString() + "   Active:" + Active.ToString() + "   Priority:" + Priority.ToString() + "   RRInterval:" + RunReccurenceInterval.ToString() + "   lastRunTime:" + lastRunTime.ToString();
        }

    }
}
