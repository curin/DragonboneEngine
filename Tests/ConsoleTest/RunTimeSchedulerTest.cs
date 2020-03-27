using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Dragonbones;

namespace ConsoleTest
{
    public class RunTimeSchedulerTest
    {
        readonly Random random = new Random();
        int nextID = 0;
        bool[] activeRand = new bool[10] { true, true, true, true, true, true, true, false, false, false };
        int[] runReccurenceRand = new int[20] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 5, 5 };
        int[] priorityRand = new int[20] { 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 5, 6 };
        SystemType[] typeRand = new SystemType[10] { SystemType.Logic, SystemType.Logic, SystemType.Logic, SystemType.Logic, SystemType.Logic,
            SystemType.Logic, SystemType.Render, SystemType.Render, SystemType.Render, SystemType.Render};

        List<ScheduleData> priorityData = new List<ScheduleData>();

        int _systemLanes = 6;
        SystemType _type = SystemType.Logic;

        public void Run(int SystemCount)
        {
            Stopwatch watch = new Stopwatch();
            Stopwatch watch2 = new Stopwatch();

            double longestTime = 0;

            int logic = 0;
            int largestRunRecurrence = 0;
            List<SystemInfo> systems = new List<SystemInfo>();
            for (int i = 0; i < SystemCount; i++)
            {
                systems.Add(RandomSystem());
                //Console.WriteLine(systems[i].ToString());
                if (systems[i].Type == SystemType.Logic)
                {
                    logic++;
                    if (systems[i].RunReccurenceInterval > largestRunRecurrence)
                        largestRunRecurrence = systems[i].RunReccurenceInterval;
                }
            }

            Console.WriteLine("Logic = " + logic.ToString() + "/" + systems.Count.ToString());
            List<int> systemsRun = new List<int>();
            List<int> runSystems = new List<int>();
            List<ScheduleData> setAside = new List<ScheduleData>();
            List<ScheduleData> Available = new List<ScheduleData>();
            LaneData[] Lanes = new LaneData[_systemLanes];
            Queue<int> emptyLanes = new Queue<int>();

            char run = 'y';
            int runIndex = 1;
            double temp;
            double totalTemp = 0;

            Console.WriteLine();
            Console.WriteLine();

            while (run == 'y')
            {
                while (systemsRun.Count < logic)
                {
                    runSystems.Clear();
                    double totalRunTime = 0;
                    Console.WriteLine("==================================================");
                    Console.WriteLine("                     Run " + runIndex.ToString());
                    Console.WriteLine("==================================================");
                    setAside.Clear();

                    for (int i = 0; i < 10000; i++)
                    {
                        watch.Reset();
                        watch.Start();
                        Schedule(systems);
                        watch.Stop();
                    }

                    int top = 0;
                    int firstFree = 0;
                    double time = 0;
                    bool running = true;
                    while (running)
                    {
                        watch2.Start();
                        Available.Clear();
                        List<ScheduleData> tempSchedule = Available;
                        Available = setAside;
                        setAside = tempSchedule;
                        for (int i = 0; i < _systemLanes; i++)
                            if (Lanes[i].EndTime <= time)
                                emptyLanes.Enqueue(i);
                        bool sa = false;
                        foreach (ScheduleData data in Available)
                        {
                            if (emptyLanes.Count == 0)
                                break;
                            sa = false;

                            for (int i = 0; i < _systemLanes; i++)
                                if (Lanes[i].EndTime > time)
                                    if (data.GetComponentsTypeIDs().Any((id) => { return Lanes[i].System.GetComponentsUsedIDs().Contains(id); }))
                                    {
                                        sa = true;
                                        setAside.Add(data);
                                        break;
                                    }
                            if (!sa)
                            {
                                Lanes[emptyLanes.Dequeue()] = new LaneData()
                                {
                                    System = systems[data.ArrayIndex],
                                    EndTime = time + data.AverageRunLength
                                };
                            }
                        }

                        while (emptyLanes.Count > 0 && top < priorityData.Count)
                        {
                            for (int i = 0; i < _systemLanes; i++)
                            {
                                if (Lanes[i].EndTime > time)
                                    if (priorityData[top].GetComponentsTypeIDs().Any((id) => { return Lanes[i].System.GetComponentsUsedIDs().Contains(id); }))
                                    {
                                        setAside.Add(priorityData[top]);
                                        sa = true;
                                        break;
                                    }
                            }
                            if (!sa)
                            {
                                Lanes[emptyLanes.Dequeue()] = new LaneData()
                                {
                                    System = systems[priorityData[top].ArrayIndex],
                                    EndTime = time + priorityData[top].AverageRunLength
                                };
                            }
                            top++;
                        }
                        watch2.Stop();
                        watch2.Reset();
                        temp = watch2.ElapsedTicks / (double)Stopwatch.Frequency;
                        Console.WriteLine("Decision Time: " + temp);
                        totalTemp += temp;

                        double shortestTime = double.PositiveInfinity;
                        for(int i = 0; i < _systemLanes; i++)
                        {
                            if (Lanes[i].EndTime < shortestTime && Lanes[i].EndTime > shortestTime)
                            {
                                shortestTime = Lanes[i].EndTime;
                            }
                        }

                        time = shortestTime;

                        if (setAside.Count == 0 && top == priorityData.Count)
                            running = false;
                    }

                    Console.WriteLine("Run = " + top + "/" + logic);
                    Console.WriteLine("RunToDate = " + systemsRun.Count + "/" + logic);
                    Console.WriteLine("Time = " + totalRunTime + "/" + (1 / 120.0));
                    temp = watch.ElapsedTicks / (double)Stopwatch.Frequency;
                    Console.WriteLine("ScheduleTime = " + (temp));
                    Console.WriteLine("Total Time: " + totalTemp);

                    Console.ReadLine();

                    if (temp > longestTime)
                        longestTime = temp;
                    watch.Reset();

                    Console.WriteLine();
                    Console.WriteLine();

                    Console.WriteLine("--------------------------------------------------");
                    Console.WriteLine("     Systems run previously not run this time");
                    Console.WriteLine("--------------------------------------------------");

                    foreach (int id in systemsRun)
                    {
                        if (!runSystems.Contains(id))
                            Console.WriteLine(systems[id].ToString());
                    }

                    Console.WriteLine();
                    Console.WriteLine();

                    Console.WriteLine("--------------------------------------------------");
                    Console.WriteLine("              Systems still to run");
                    Console.WriteLine("--------------------------------------------------");

                    foreach (SystemInfo inf in systems)
                    {
                        if (inf.Type == SystemType.Logic && !systemsRun.Contains(inf.ID))
                            Console.WriteLine(inf.ToString());
                    }

                    Console.WriteLine();
                    Console.WriteLine();

                    runIndex++;
                    for (int i = 0; i < systems.Count; i++)
                    {
                        systems[i] = SystemUpdate(systems[i]);
                    }
                }

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Run Order");
                foreach (int ID in systemsRun)
                {
                    Console.WriteLine(systems[ID].ToString());
                }
                systemsRun.Clear();
                Console.WriteLine();
                Console.WriteLine("Longest Scheduling Time : " + longestTime);
                Console.WriteLine("Runs to Complete : " + runIndex);
                longestTime = 0;
                Console.WriteLine();
                Console.WriteLine();

                Console.WriteLine("Continue?(y/n)");
                run = Console.ReadKey().KeyChar;
                Console.WriteLine();


            }
        }

        public void Schedule(List<SystemInfo> Systems)
        {
            //Clear Old Data
            priorityData.Clear();

            //Check to make sure arguments are valid
            if (Systems == null)
                throw new ArgumentNullException(nameof(Systems));

            //Determine the runlengths, the Aging of a system and compile it into the priority data
            Setup(Systems);

            //Sort the priority data by run recurrence then the new composite priority value(age and priority combined)
            priorityData.Sort(ScheduleData.Sort);
        }

        /// <summary>
        /// Set average run length and age of the system to create priority list
        /// </summary>
        /// <param name="Systems">all the systems being scheduled</param>
        /// <param name="deltaTime">the time between this and last frame</param>
        private void Setup(List<SystemInfo> Systems)
        {
            //for each system
            for (int i = 0; i < Systems.Count; i++)
            {
                SystemInfo sysInf = Systems[i];
                //if the system is of the wrong type or not active skip it
                if (sysInf.Type != _type || !sysInf.Active)
                    continue;

                //age the system
                sysInf.Age++;

                // if the system age is less than its runRecurrence skip it
                if (sysInf.Age < sysInf.RunReccurenceInterval - 1)
                    continue;

                //Create a schedule data with the average runlength, priority composite with age, the runRecurrence, and the components the system use
                ScheduleData data = new ScheduleData(i, sysInf.AverageRunTime,
                    sysInf.Priority * sysInf.Age, sysInf.RunReccurenceInterval == 0 ? int.MaxValue : sysInf.RunReccurenceInterval, sysInf.GetComponentsUsedIDs());

                //add this to the priority data
                priorityData.Add(data);
            }

            //once all systems have been processed we have all priority data set
        }


        SystemInfo RandomSystem()
        {
            SystemInfo inf;
            if (random.Next(2) == 0)
            {
                inf = new SystemInfo(RandomString(random.Next(8, 12)), typeRand[random.Next(9)], priorityRand[random.Next(19)], true);// activeRand[random.Next(9)]);
            }
            else
            {
                inf = new SystemInfo(RandomString(random.Next(8, 12)), runReccurenceRand[random.Next(19)], priorityRand[random.Next(19)], typeRand[random.Next(9)], true);// activeRand[random.Next(9)]);
            }
            int[] components = new int[random.Next(1, 20)];
            for (int i = 0; i < components.Length; i++)
                components[i] = random.Next(0, 100);
            inf.Update(random.Next(1, 100) / (double)Stopwatch.Frequency);
            inf.SetComponentIDs(components);
            inf.SetID(nextID);
            nextID++;
            return inf;
        }

        SystemInfo SystemUpdate(SystemInfo inf)
        {
            //if (!activeRand[random.Next(9)])
            //    inf.Active = !inf.Active;
            inf.Update((random.Next(95, 105) / 100.0) * inf.AverageRunTime);
            return inf;
        }

        string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_.-";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public struct LaneData
        {
            public SystemInfo System { get; set; }
            public double EndTime { get; set; }
        }

        public struct ScheduleData : IEquatable<ScheduleData>
        {
            public ScheduleData(int arrayIndex, double avgRunLength, int priority, int runRecurrence, params int[] componentsTypeIDs)
            {
                _averageRunLength = avgRunLength;
                _priorityValue = priority;
                _runRecurrence = runRecurrence;
                _arrayIndex = arrayIndex;
                _componentsTypeIDs = componentsTypeIDs;
            }

            private int _arrayIndex;
            private double _averageRunLength;
            private int _priorityValue;
            private int _runRecurrence;
            private int[] _componentsTypeIDs;


            public int ArrayIndex { get => _arrayIndex; set => _arrayIndex = value; }
            public double AverageRunLength { get => _averageRunLength; set => _averageRunLength = value; }
            public int PriorityValue { get => _priorityValue; set => _priorityValue = value; }

            public int[] GetComponentsTypeIDs()
            {
                return _componentsTypeIDs;
            }

            public void SetComponentsTypeIDs(int[] value)
            {
                _componentsTypeIDs = value;
            }

            public int RunRecurrence { get => _runRecurrence; set => _runRecurrence = value; }

            public static int Sort(ScheduleData x, ScheduleData y)
            {
                int result = x._runRecurrence - y._runRecurrence;
                if (result == 0)
                    return (x._priorityValue - y._priorityValue);
                return result;
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
                return ArrayIndex == other.ArrayIndex;
            }
        }
    }
}
