using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones
{
    public class SystemSchedule : ISystemSchedule
    {
        public SystemSchedule()
        {
            Systems = new List<SystemInfo>();
        }

        public List<SystemInfo> Systems;
        int place;

        public bool Finished => place >= Systems.Count;

        public long Count => Systems.Count;

        public bool NextSystem(out SystemInfo system)
        {
            if (place < Systems.Count)
            {
                lock (Systems)
                {
                    system = Systems[place];
                    place++;
                }
                return true;
            }
            system = default;
            return false;
        }

        public void Reset()
        {
            place = 0;
        }
    }
}
