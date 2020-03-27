using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Dragonbones;

namespace ConsoleTest
{
    public class SchedulingTest
    {
        Random random = new Random();
        bool[] activeRand = new bool[10] { true, true, true, true, true, true, true, false, false, false };
        int[] runReccurenceRand = new int[20] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4, 5, 5 };
        int[] priorityRand = new int[20] { 0, 0, 0, 1, 1, 1, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 4, 4, 5, 6 };
        SystemType[] typeRand = new SystemType[10] { SystemType.Logic, SystemType.Logic, SystemType.Logic, SystemType.Logic, SystemType.Logic,
            SystemType.Logic, SystemType.Render, SystemType.Render, SystemType.Render, SystemType.Render};
        int nextID = 0;

        public void Run(int SystemCount)
        {
            Stopwatch watch = new Stopwatch();
            double longestTime = 0;
            SystemScheduler scheduler = new SystemScheduler(6, SystemType.Logic);
            ISystemSchedule schedule = null;

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

            char run = 'y';
            int runIndex = 1;
            double temp;

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
                    for (int a = 0; a < 100; a++)
                    {
                        watch.Start();
                        schedule = scheduler.Schedule(systems);
                        watch.Stop();
                    }

                    double place = 0;
                    int batchNum = 0;

                    while (schedule.NextSystem(out SystemInfo sysInf))
                    {
                        sysInf.Age = 0;
                        Console.WriteLine(sysInf.ToString());
                        if (!systemsRun.Contains(sysInf.ID))
                            systemsRun.Add(sysInf.ID);
                        runSystems.Add(sysInf.ID);
                    }

                    Console.WriteLine("Batches Run : " + batchNum.ToString() + "/" + schedule.BatchCount.ToString());
                    Console.WriteLine("Run = " + schedule.Count.ToString() + "/" + logic.ToString());
                    Console.WriteLine("RunToDate = " + systemsRun.Count.ToString() + "/" + logic.ToString());
                    Console.WriteLine("Time = " + totalRunTime.ToString() + "/" + (1 / 120.0).ToString());
                    temp = watch.ElapsedTicks / (double)Stopwatch.Frequency;
                    Console.WriteLine("ScheduleTime = " + (temp).ToString());

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
                Console.WriteLine("Longest Scheduling Time : " + longestTime.ToString());
                Console.WriteLine("Runs to Complete : " + runIndex.ToString());
                longestTime = 0;
                Console.WriteLine();
                Console.WriteLine();

                Console.WriteLine("Continue?(y/n)");
                run = Console.ReadKey().KeyChar;
                Console.WriteLine();


            }
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
                components[i] = random.Next(0, 100);
            inf.UpdateAverage(random.Next(1, 100) / (double)Stopwatch.Frequency);
            inf.SetComponentIDs(components);
            inf.SetID(nextID);
            nextID++;
            return inf;
        }

        SystemInfo SystemUpdate(SystemInfo inf)
        {
            //if (!activeRand[random.Next(9)])
            //    inf.Active = !inf.Active;
            inf.UpdateAverage((random.Next(95, 105) / 100.0) * inf.AverageRunTime);
            return inf;
        }

        string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_.-";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
