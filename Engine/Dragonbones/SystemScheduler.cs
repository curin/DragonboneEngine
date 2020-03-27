using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Dragonbones
{
    public class SystemScheduler : ISystemScheduler
    {
        public SystemScheduler(SystemType type)
        {
            _type = type;
        }
        
        SystemType _type;

        public ISystemSchedule Schedule(List<SystemInfo> Systems)
        {
            //Check to make sure arguments are valid
            if (Systems == null)
                throw new ArgumentNullException(nameof(Systems));

            //start systemschedule
            SystemSchedule schedule = new SystemSchedule();

            //Determine the runlengths, the Aging of a system and compile it into the priority data
            Setup(Systems, schedule);

            //Sort Schedule by priority descending, and runRecurrence descending (with 0 put at the end)
            schedule.Sort(Sort);

            return schedule;
        }

        /// <summary>
        /// Set average run length and age of the system to create priority list
        /// </summary>
        /// <param name="Systems">all the systems being scheduled</param>
        /// <param name="deltaTime">the time between this and last frame</param>
        private void Setup(List<SystemInfo> Systems, SystemSchedule schedule)
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

                sysInf.PriorityComposite = sysInf.Age * sysInf.Priority;

                schedule.Add(sysInf);
            }

            //once all systems have been processed we have all priority data set
        }

        public static int Sort(SystemInfo x, SystemInfo y)
        {
            int result = x.RunReccurenceInterval - y.RunReccurenceInterval;
            if (result == 0)
                return (x.PriorityComposite - y.PriorityComposite);
            return result;
        }
    }
}
