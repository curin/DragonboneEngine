using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace Dragonbones
{
    public class SystemScheduler : ISystemScheduler
    {
        public SystemScheduler(int runLengthSize, double Time, int systemLanes, SystemType type)
        {
            _runLengthSize = runLengthSize;
            _time = Time;
            _type = type;
            _systemLanes = systemLanes;
        }

        Dictionary<long, double[]> RunLengths = new Dictionary<long, double[]>();
        List<ScheduleData> priorityData = new List<ScheduleData>();
        List<BatchData> batchData = new List<BatchData>();
        
        long _Frame;
        int _runLengthSize, _systemLanes;
        double _time;
        SystemType _type;

        public ISystemSchedule Schedule(List<SystemInfo> Systems, float deltaTime)
        {
            //Check to make sure arguments are valid
            if (Systems == null)
                throw new ArgumentNullException(nameof(Systems));

            //start systemschedule
            SystemSchedule schedule = new SystemSchedule();

            //Determine the runlengths, the Aging of a system and compile it into the priority data
            Setup(Systems, deltaTime);

            //Sort the priority data by run recurrence then the new composite priority value(age and priority combined)
            priorityData.Sort(ScheduleData.Sort);

            //Create the system groupings that will becoming our system batches
            GetBatches();

            foreach(BatchData data in batchData)
            {
                //For each group of systems
                //order them by system length descending
                data.Schedule.Sort(ScheduleData.LengthSort);

                //Then create the batch and add it to our schedule
                schedule.Add(MakeSystemBatch(data, Systems));
            }

            //return the completed schedule
            return schedule;
        }

        /// <summary>
        /// Create a system batch from batch data
        /// </summary>
        /// <param name="data">the batch data</param>
        /// <param name="systems">the list of system info</param>
        /// <returns></returns>
        private SystemBatch MakeSystemBatch(BatchData data, List<SystemInfo> systems)
        {
            //Determine target length
            double target = data.Length / _systemLanes;
            //create batch
            SystemBatch batch = new SystemBatch(data.Schedule.Count, _systemLanes);
            bool stored;
            int zeroIndex = 0;

            foreach (ScheduleData dat in data.Schedule)
            {
                //if the system has no length add it to the next lane to get a zero length system
                if (dat.AverageRunLength == 0)
                {
                    batch.AppendToLane(zeroIndex, systems[dat.ArrayIndex], 0);
                    //move the next index for zero length systems
                    zeroIndex = (zeroIndex + 1) % _systemLanes;
                    //move to the next system
                    continue;
                }

                double shortestTime = double.PositiveInfinity;
                int shortestLane = 0;
                stored = false;

                //go through each lane
                for (int laneID = 0; laneID < _systemLanes; laneID++)
                {
                    //if the lane is empty or this system can be added and reach the target exactly
                    double tempTime;
                    if (batch.IsLaneEmpty(laneID) || (tempTime = batch.LaneLenth(laneID) + dat.AverageRunLength) == target)
                    {
                        //if we can add the system to the lane
                        batch.AppendToLane(laneID, systems[dat.ArrayIndex], dat.AverageRunLength);
                        stored = true;
                        continue;
                    }

                    
                    //if you can add the system and be less than the target
                    if (tempTime < target)
                    {
                        double timeAdd = dat.AverageRunLength + data.Schedule.Last().AverageRunLength;
                        //then can we see if the shortest system can be added and still be less than the target
                        if (timeAdd + batch.LaneLenth(laneID) <= target)
                        {
                            //if we can add the system to the lane
                            batch.AppendToLane(laneID, systems[dat.ArrayIndex], dat.AverageRunLength);
                            stored = true;
                            continue;
                        }
                    }

                    //if the system cannot be added see if this lane is the shortest lane we have seen so far
                    if (tempTime < shortestTime)
                    {
                        //if so save that
                        shortestTime = tempTime;
                        shortestLane = laneID;
                    }
                }

                //if we did not store the system in a lane yet
                if (!stored)
                {
                    //add it to the shortest encountered lane
                    batch.AppendToLane(shortestLane, systems[dat.ArrayIndex], dat.AverageRunLength);
                    continue;
                }
            }

            //Once we have have added each system return the newly created batch
            return batch;
        }

        /// <summary>
        /// Create system batches from an ordered priority list
        /// </summary>
        private void GetBatches()
        {
            //clear old batch data
            batchData.Clear();
            bool added;
            
            //for each system in ordered priority list
            foreach (ScheduleData dat in priorityData)
            {
                added = false;
                //foreach batch so far
                for (int i = 0; i < batchData.Count; i++)
                {
                    BatchData batch = batchData[i];
                    //do we contain any of the same components as is currently in our batch
                    foreach (int component in dat.ComponentsTypeIDs)
                        if (batch.ComponentsUsed.Contains(component))
                        {
                            //if so move to the next batch
                            continue;
                        }
                    //else add to batch and stop looking at batches
                    batch.Schedule.Add(dat);
                    batch.ComponentsUsed.AddRange(dat.ComponentsTypeIDs);
                    batch.Length += dat.AverageRunLength;
                    batchData[i] = batch;
                    added = true;
                    break;
                }

                //if we haven't added the system to any batches yet
                if (!added)
                {
                    //make a new batch and add this system
                    BatchData batch = new BatchData
                    {
                        ComponentsUsed = new List<int>(dat.ComponentsTypeIDs),
                        Schedule = new List<ScheduleData>
                        {
                            dat
                        },
                        Length = dat.AverageRunLength
                    };
                    batchData.Add(batch);
                }
            }

            //once we are done the batch data should now be setup
        }

        /// <summary>
        /// Set average run length and age of the system to create priority list
        /// </summary>
        /// <param name="Systems">all the systems being scheduled</param>
        /// <param name="deltaTime">the time between this and last frame</param>
        private void Setup(List<SystemInfo> Systems, float deltaTime)
        {
            //clear old priority data
            priorityData.Clear();
            //for each system
            for (int i = 0; i < Systems.Count; i++)
            {
                //start with an age of 1
                SystemInfo sysInf = Systems[i];
                //if the system is of the wrong type or not active skip it
                if (sysInf.Type != _type || !sysInf.Active)
                    continue;
                //if the system doesn't have a run length already stored add it
                if (!RunLengths.ContainsKey(sysInf.ID))
                    RunLengths.Add(sysInf.ID, Enumerable.Repeat(sysInf.lastRunTime, _runLengthSize).ToArray());
                //otherwise set the lastframe to the last runTime length
                else
                    RunLengths[sysInf.ID][_Frame % _runLengthSize] = sysInf.lastRunTime;

                //age the system
                sysInf.Age++;

                // if the system age is less than its runRecurrence skip it
                if (sysInf.Age < sysInf.RunReccurenceInterval)
                    continue;

                //Create a schedule data with the average runlength, priority composite with age, the runRecurrence, and the components the system use
                ScheduleData data = new ScheduleData(i, Enumerable.Average(RunLengths[sysInf.ID]),
                    (long)sysInf.Priority * sysInf.Age, sysInf.RunReccurenceInterval, sysInf.ComponentsUsedIDs);

                //add this to the priority data
                priorityData.Add(data);
            }

            //once all systems have been processed we have all priority data set
        }

        public struct BatchData
        {
            public List<ScheduleData> Schedule { get; set; }
            public List<int> ComponentsUsed { get; set; }
            public double Length;
        }

        public struct ScheduleData : IEquatable<ScheduleData>
        {
            public ScheduleData(int arrayIndex, double avgRunLength, long priority, int runRecurrence, params int[] componentsTypeIDs)
            {
                _averageRunLength = avgRunLength;
                _priorityValue = priority;
                _runRecurrence = runRecurrence;
                _arrayIndex = arrayIndex;
                _componentsTypeIDs = componentsTypeIDs;
            }

            private int _arrayIndex;
            private double _averageRunLength;
            private long _priorityValue;
            private int _runRecurrence;
            private int[] _componentsTypeIDs;


            public int ArrayIndex { get => _arrayIndex; set => _arrayIndex = value; }
            public double AverageRunLength { get => _averageRunLength; set => _averageRunLength = value; }
            public long PriorityValue { get => _priorityValue; set => _priorityValue = value; }
            public int[] ComponentsTypeIDs { get => _componentsTypeIDs; set => _componentsTypeIDs = value; }
            public int RunRecurrence { get => _runRecurrence; set => _runRecurrence = value; }

            public static int Sort(ScheduleData x, ScheduleData y)
            {
                //Are the priorities equal?
                return (x.RunRecurrence == y.RunRecurrence && x.PriorityValue == y.PriorityValue) ? 0 :
                    //Does X have priority by runrecurrence? else
                    (x.RunRecurrence < y.RunRecurrence && x.RunRecurrence > 0) ? -1 : 
                    //Does X have the same runRecurrence and priority by value? else y has priority
                    (x.PriorityValue == y.PriorityValue && x.PriorityValue > y.PriorityValue) ? -1 : 1;
            }

            public static int LengthSort(ScheduleData x, ScheduleData y)
            {
                return x.AverageRunLength > y.AverageRunLength ? -1 : x.AverageRunLength == y.AverageRunLength ? 0 : 1;
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public static bool operator ==(ScheduleData left, ScheduleData right)
            {
                return left.ArrayIndex == right.ArrayIndex;
            }

            public static bool operator !=(ScheduleData left, ScheduleData right)
            {
                return left.ArrayIndex != right.ArrayIndex;
            }

            public bool Equals(ScheduleData other)
            {
                return this.ArrayIndex == other.ArrayIndex;
            }
        }
    }
}
