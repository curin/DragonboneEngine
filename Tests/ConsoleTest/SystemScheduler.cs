using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Dragonbones.Native;

namespace Dragonbones
{
    public static class SystemScheduler //: ISystemScheduler
    {

        public static ISystemSchedule Schedule(List<SystemInfo> Systems, int size, SystemType type, int laneCount)
        {
            PrecisionTimer watch = new PrecisionTimer();
            PrecisionTimer watch2 = new PrecisionTimer();
            watch2.Start();
            double average = 0;
            //Check to make sure arguments are valid
            if (Systems == null)
                throw new ArgumentNullException(nameof(Systems));

            //start systemschedule
            SystemSchedule schedule = new SystemSchedule(laneCount, size);

            //Determine the runlengths, the Aging of a system and compile it into the priority data
            //for each system
            for (int i = 0; i < Systems.Count;i++)
            {
                watch.Reset();
                watch.Start();
                SystemInfo sysInf = Systems[i];
                //if the system is of the wrong type or not active skip it
                if (sysInf.Type != type)
                    continue;

                //age the system
                sysInf.Age++;

                sysInf.PriorityComposite = sysInf.Age * sysInf.Priority;
                schedule.Add(sysInf);
                watch.Stop();
                average = MathHelper.MovingAverage(average, watch.ElapsedSeconds,i+1);
            }
            watch2.Stop();

            Console.WriteLine("Average Add Time : " + average);
            Console.WriteLine("Total run Time : " + watch2.ElapsedSeconds);
            //once all systems have been processed we have all priority data set

           
            return schedule;
        }
    }
}
