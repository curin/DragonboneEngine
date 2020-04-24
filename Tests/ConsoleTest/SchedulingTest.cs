using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Dragonbones;
using Dragonbones.Native;

namespace ConsoleTest
{
    public class SchedulingTest
    {
        readonly Random random = new Random();
        readonly bool[] activeRand = new bool[10] { true, true, true, true, true, true, true, false, false, false };
        readonly int[] runReccurenceRand = new int[20] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 5, 5 };
        readonly int[] priorityRand = new int[20] { 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 5, 6 };
        readonly SystemType[] typeRand = new SystemType[10] { SystemType.Logic, SystemType.Logic, SystemType.Logic, SystemType.Logic, SystemType.Logic,
            SystemType.Logic, SystemType.Render, SystemType.Render, SystemType.Render, SystemType.Render};
        int nextID = 0;

        public void Run(SystemType type, double time, int laneCount, int SystemCount)
        {
            List<SystemInfo> systems = new List<SystemInfo>();
            for (int i = 0; i < SystemCount; i++)
            {
                systems.Add(RandomSystem());
                systems[i].Age = 1;
                systems[i].PriorityComposite = systems[i].Priority * systems[i].Age;
            }
            Run(systems.ToArray(), type, time, laneCount);
        }

        public void Run(SystemType type, double time, int laneCount, SystemInfo[] systems)
        {
            if (systems == null)
                return;

            Run(systems, type, time, laneCount);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Test not localized")]
        void Run(SystemInfo[] systems, SystemType type, double time, int laneCount)
        {
            int typeCount = 0;
            double averageTime = 0; 
            PrecisionTimer processTimer = new PrecisionTimer();
            PrecisionTimer unitTimer = new PrecisionTimer();
            Console.WriteLine("Test 1 - Adding All Systems at Once");

            for (int i = 0; i < systems.Length; i++)
                if (systems[i].Type == type)
                    typeCount++;

            SystemSchedule schedule = new SystemSchedule(SystemType.Logic, laneCount, typeCount);

            processTimer.Start();
            for (int i = 0; i < systems.Length; i++)
            {
                unitTimer.Start();
                SystemInfo sysInf = systems[i];
                if (!sysInf.Active || sysInf.Type != type)
                {
                    unitTimer.Stop();
                    averageTime = MathHelper.MovingAverage(averageTime, unitTimer.ElapsedSeconds, i + 1);
                    unitTimer.Reset();
                    continue;
                }
                schedule.Add(sysInf);
                unitTimer.Stop();
                processTimer.Stop();
                averageTime = MathHelper.MovingAverage(averageTime, unitTimer.ElapsedSeconds, i + 1);
                unitTimer.Reset();
                processTimer.Start();
            }
            processTimer.Stop();

            Console.WriteLine("Total Time to Add : " + processTimer.ElapsedSeconds);
            Console.WriteLine("Average Time to Add : " + averageTime);
            Console.WriteLine("Extrapolated Time to Add : " + (averageTime * systems.Length));
            Console.WriteLine("Extrapolated Time without nonTypes : " + (averageTime * typeCount));
            processTimer.Reset();

            Console.ReadLine();

            Console.WriteLine("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
            Console.WriteLine("Test 2 - Adding Each System One at a Time");

            averageTime = 0;
            schedule.Clear();
            processTimer.Start();
            for (int i = 0; i < systems.Length; i++)
            {
                unitTimer.Start();
                SystemInfo sysInf = systems[i];
                if (!sysInf.Active || sysInf.Type != type)
                {
                    unitTimer.Stop();
                    averageTime = MathHelper.MovingAverage(averageTime, unitTimer.ElapsedSeconds, i + 1);
                    unitTimer.Reset();
                    continue;
                }
                schedule.Add(sysInf);
                unitTimer.Stop();
                processTimer.Stop();
                averageTime = MathHelper.MovingAverage(averageTime, unitTimer.ElapsedSeconds, i + 1);
                unitTimer.Reset();

                while (schedule.NextSystem(0, out SystemInfo sysInfo) != ScheduleResult.Finished)
                {
                    sysInfo.Age = 1;
                }

                processTimer.Start();
            }
            processTimer.Stop();

            Console.WriteLine("Total Time to Add : " + processTimer.ElapsedSeconds);
            Console.WriteLine("Average Time to Add : " + averageTime);
            Console.WriteLine("Extrapolated Time to Add : " + (averageTime * systems.Length));
            Console.WriteLine("Extrapolated Time without nonTypes : " + (averageTime * typeCount));
            processTimer.Reset();

            Console.ReadLine();

            Console.WriteLine("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
            Console.WriteLine("Test 3 - Clear List and Readd");

            SystemSchedule copy = new SystemSchedule(schedule);
            foreach (SystemInfo sysinf in systems)
                SystemUpdate(sysinf);

            processTimer.Start();
            schedule.Clear();

            for (int i = 0; i < systems.Length; i++)
            {
                SystemInfo sysInf = systems[i];
                if (!sysInf.Active || sysInf.Type != type)
                {
                    continue;
                }
                schedule.Add(sysInf);
            }
            processTimer.Stop();

            while (schedule.NextSystem(0, out SystemInfo sysInfo) != ScheduleResult.Finished)
            {
                Console.WriteLine(sysInfo);
            }

            Console.WriteLine("Total Time to clear then readd : " + processTimer.ElapsedSeconds);

            Console.ReadLine();

            Console.WriteLine("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
            Console.WriteLine("Test 4 - Remove Individually and Add One at a time");

            processTimer.Start();

            copy.Reset();

            for (int i = 0; i < systems.Length; i++)
            {
                SystemInfo sysInf = systems[i];
                if (!sysInf.Active || sysInf.Type != type)
                {
                    continue;
                }
                copy.Remove(sysInf);
                copy.Add(sysInf);
            }
            processTimer.Stop();

            while (copy.NextSystem(0, out SystemInfo sysInfo) != ScheduleResult.Finished)
            {
                Console.WriteLine(sysInfo);
            }

            Console.WriteLine("Total Time to remove then readd : " + processTimer.ElapsedSeconds);

            Console.ReadLine();

            Console.WriteLine("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
            Console.WriteLine("Test 5 - System Run");

            schedule.Clear();

            for (int i = 0; i < systems.Length; i++)
            {
                SystemInfo sysInf = systems[i];
                if (!sysInf.Active || sysInf.Type != type)
                {
                    continue;
                }
                schedule.Add(sysInf);
            }

            double currentTime = 0;
            int finishCount = 0;
            double[] endTimes = new double[laneCount];
            bool[] finished = new bool[laneCount];
            averageTime = 0;

            processTimer.Start();
            while (!schedule.Finished && currentTime < time && finishCount < laneCount)
            {
                for (int i = 0; i < laneCount; i++)
                    if (endTimes[i] <= currentTime && !finished[i])
                    {
                        unitTimer.Start();
                        schedule.FinishLane(i);
                        ScheduleResult result = schedule.NextSystem(i, out SystemInfo sysInf);

                        if (result == ScheduleResult.Supplied)
                        {
                            sysInf.Age = 0;
                            if (sysInf.AverageRunTime + currentTime > time)
                            {
                                finished[i] = true;
                                finishCount++;
                                unitTimer.Stop();
                                continue;
                            }
                            endTimes[i] = currentTime + sysInf.AverageRunTime;
                        }
                        else if (result == ScheduleResult.Finished)
                        {
                            finished[i] = true;
                            finishCount++;
                        }
                        unitTimer.Stop();
                        processTimer.Stop();
                        averageTime = MathHelper.MovingAverage(averageTime, unitTimer.ElapsedSeconds, i + 1);
                        unitTimer.Reset();
                        processTimer.Start();
                    }

                processTimer.Stop();
                double nextTime = double.PositiveInfinity;
                for (int i = 0; i < laneCount; i++)
                    if (endTimes[i] > currentTime && endTimes[i] < nextTime)
                        nextTime = endTimes[i];
                if (nextTime != double.PositiveInfinity)
                    currentTime = nextTime;
                processTimer.Start();
            }
            processTimer.Stop();

            Console.WriteLine("Total Time to decide : " + processTimer.ElapsedSeconds);
            Console.WriteLine("Average Time to decide : " + averageTime);
            Console.WriteLine("Extrapolated Time to decide : " + (averageTime * typeCount));
            Console.WriteLine("Total Time Used : " + (currentTime) + "/" + time);
            processTimer.Reset();

            Console.ReadLine();
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
            int[] components = new int[random.Next(1, 3)];
            for (int i = 0; i < components.Length; i++)
                components[i] = random.Next(1, 3);
            inf.Update(random.Next(1, 100) / (double)Stopwatch.Frequency);
            inf.SetComponentIDs(components);
            inf.SetID(nextID);
            nextID++;
            return inf;
        }

        void SystemUpdate(SystemInfo inf)
        {
            //if (!activeRand[random.Next(9)])
            //    inf.Active = !inf.Active;
            inf.PriorityComposite = inf.Age * inf.Priority;
            inf.Update((random.Next(95, 105) / 100.0) * inf.AverageRunTime);
            if (inf.Active)
                inf.Age++;
        }

        string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_.-";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
