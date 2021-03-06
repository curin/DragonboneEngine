﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones.Systems
{
    /// <summary>
    /// The system's info
    /// </summary>
    public class SystemInfo : IComparer<SystemInfo>, IEquatable<SystemInfo>
    {
        /// <summary>
        /// Constructs the system info using priority
        /// </summary>
        /// <param name="name">the system's name</param>
        /// <param name="type">the type of the system</param>
        /// <param name="priority">the priority of the system. Higher number is higher priority</param>
        /// <param name="active">is this system currently active</param>
        /// <param name="components">the components used by this system</param>
        public SystemInfo(string name, SystemType type, int priority, bool active = true, params SystemComponent[] components)
        {
            Name = name;
            ID = -1;
            Active = active;
            Priority = priority;
            Type = type;
            Running = false;
            RunRecurrenceInterval = 0;
            AverageRunTime = 0;
            RunCount = 0;
            Components = components;
            Age = 0;
        }

        /// <summary>
        /// Constructs the system info using run recurrence
        /// </summary>
        /// <param name="name">the system's name</param>
        /// <param name="type">the type of the system</param>
        /// <param name="runRecurrenceInterval">the frame interval between runs</param>
        /// <param name="active">is this system currently active</param>
        /// <param name="priority">the priority of the system. Higher number is higher priority</param>
        /// <param name="components">the components used by this system</param>
        public SystemInfo(string name, int runRecurrenceInterval, int priority, SystemType type, bool active = true, params SystemComponent[] components)
        {
            Name = name;
            ID = -1;
            Active = active;
            Priority = priority;
            Type = type;
            Running = false;
            RunRecurrenceInterval = runRecurrenceInterval;
            AverageRunTime = 0;
            RunCount = 0;
            Components = components;
            Age = 0;
        }


        /// <summary>
        /// The controlling <see cref="IEntityAdmin"/>
        /// </summary>
        public IEntityAdmin Admin { get; private set; }
        /// <summary>
        /// The type of system, which defines when it is run
        /// </summary>
        public SystemType Type { get; }
        /// <summary>
        /// What is the name of this system?
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// The ID for the system
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// The components used by this system
        /// Do not change this after creation
        /// This is available for the engine to set the ID values when registering
        /// </summary>
        public SystemComponent[] Components;

        /// <summary>
        /// Is this system currently active?
        /// </summary>
        public bool Active { get; set; }
        /// <summary>
        /// The priority of the system, used to determine which systems should be run less frequently in order to maintain frame rate
        /// Priority is grouped by RunRecurrence (0 being the lowest, 1 the highest, then everything after)
        /// </summary>
        public int Priority { get; }
        /// <summary>
        /// If Set to 0, will run based on priority system only.
        /// Otherwise, it represents the number of frames finished between each run
        /// </summary>
        public int RunRecurrenceInterval { get; }
        /// <summary>
        /// Is this system currently running
        /// </summary>
        public bool Running { get; set; }
        /// <summary>
        /// The average time it takes to run this system
        /// </summary>
        public double AverageRunTime { get; private set; }
        /// <summary>
        /// A variable to count the number of times a system runs
        /// </summary>
        public long RunCount { get; private set; }
        /// <summary>
        /// How many frames have passed since last execution
        /// Used in Scheduling
        /// </summary>
        public int Age { get; set; }
        /// <summary>
        /// A Space to store a priority composite variable
        /// </summary>
        public int PriorityComposite { get; set; }
        /// <summary>
        /// Whether this system was run this frame
        /// Used in scheduling
        /// </summary>
        public bool Run { get; set; }
        /// <summary>
        /// ID as set by System Registry
        /// </summary>
        /// <param name="id">the system's ID</param>
        public void SetID(int id)
        {
            ID = id;
        }

        /// <summary>
        /// Sets the Admin field so that this system has access to the admin
        /// </summary>
        /// <param name="admin">the admin controlling this system</param>
        public void SetAdmin(IEntityAdmin admin)
        {
            Admin = admin;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ID + "\t" + Name + "\t" + "Type:" + Type.ToString() + "   Active:" + Active + "   Priority Composite:" + PriorityComposite + "   RRInterval:" + RunRecurrenceInterval + "   Average Time:" + AverageRunTime;
        }


        /// <summary>
        /// Updates this system info
        /// it increases updates the average runtime, recomputes priority composite,
        /// and resets the run bool to false
        ///
        /// this is used to update info for scheduling
        /// </summary>
        /// <param name="newTime">the time it took for the system to run</param>
        public void Update(double newTime)
        {
            if (RunCount == long.MaxValue)
                RunCount = 1;
            RunCount++;
            AverageRunTime = MathHelper.MovingAverage(AverageRunTime, newTime, RunCount);
            PriorityComposite = Age * Priority;
            Run = false;
        }

        /// <inheritdoc />
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
        public int Compare(SystemInfo x, SystemInfo y)
        {
            int result = x.RunRecurrenceInterval - y.RunRecurrenceInterval;
            if (result == 0)
                return (y.PriorityComposite - x.PriorityComposite);
            return result;
        }

        /// <summary>
        /// Returns a comparison for two system infos
        /// 0 is equivalent,
        /// negative is higher priority
        /// positive is lower priority
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Performance")]
        public static int Sort(SystemInfo x, SystemInfo y)
        {
            int result = x.RunRecurrenceInterval - y.RunRecurrenceInterval;
            if (result == 0)
                return (y.PriorityComposite - x.PriorityComposite);
            return result;
        }

        /// <inheritdoc />
        public bool Equals(SystemInfo other)
        {
            if (other == null)
                return false;
            return Name == other.Name && ID == other.ID;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            return Equals(obj as SystemInfo);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
