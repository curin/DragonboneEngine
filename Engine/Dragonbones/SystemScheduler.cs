using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace Dragonbones
{
    public class SystemScheduler : ISystemScheduler
    {
        public SystemScheduler(int runLengthSize, double Time, SystemType type)
        {
            _runLengthSize = runLengthSize;
            _time = Time;
            _type = type;
        }

        Dictionary<long, double[]> RunLengths = new Dictionary<long, double[]>();
        Dictionary<long, float> Age = new Dictionary<long, float>();
        long _Frame;
        int _runLengthSize;
        double _time;
        SystemType _type;

        public ISystemSchedule Schedule(List<SystemInfo> Systems, float deltaTime)
        {
            List<ScheduleData> data = new List<ScheduleData>();
            SystemSchedule schedule = new SystemSchedule();
            for(int i = 0; i < Systems.Count; i++)
            {
                SystemInfo sysInf = Systems[i];
                if (sysInf.Type != _type || !sysInf.Active)
                    continue;
                if (!RunLengths.ContainsKey(sysInf.ID))
                    RunLengths.Add(sysInf.ID, Enumerable.Repeat(sysInf.lastRunTime, _runLengthSize).ToArray());
                else
                    RunLengths[sysInf.ID][_Frame % _runLengthSize] = sysInf.lastRunTime;
                if (!Age.ContainsKey(sysInf.ID))
                    Age.Add(sysInf.ID, deltaTime);
                else
                    Age[sysInf.ID] += deltaTime;

                double priority = FindPriorityValue(sysInf, _Frame);
                if (priority == -1)
                    continue;
                data.Add(new ScheduleData(sysInf.ID, i, Enumerable.Average(RunLengths[sysInf.ID]), priority));
            }

            data.Sort(ScheduleData.Sort);

            double time = 0;
            foreach (ScheduleData dat in data)
            {
                if (time + dat.AverageRunLength <= _time)
                {
                    time += dat.AverageRunLength;
                    Age[dat.SystemID] = 0;
                    schedule.Systems.Add(Systems[dat.ArrayIndex]);
                }
            }
            
            _Frame++;
            return schedule;
        }

        public double FindPriorityValue(SystemInfo sysInf, long frame)
        {
            if (sysInf.RunReccurenceInterval == 0)
            {
                return sysInf.Priority * (Age[sysInf.ID]);
            }
            else
            {
                return frame % sysInf.RunReccurenceInterval == 0 ? double.PositiveInfinity : -1;
            }
        }

        public struct ScheduleData
        {
            public ScheduleData(long id, int arrayIndex, double avgRunLength, double priority)
            {
                SystemID = id;
                AverageRunLength = avgRunLength;
                PriorityValue = priority;
                ArrayIndex = arrayIndex;
            }

            public long SystemID;
            public int ArrayIndex;
            public double AverageRunLength;
            public double PriorityValue;

            public static int Sort(ScheduleData x, ScheduleData y)
            {
                return x.PriorityValue > y.PriorityValue ? -1 : x.PriorityValue == y.PriorityValue ? 0 : 1;
            }
        }
    }
}
