using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones
{
    public interface ISystemScheduler
    {
        public ISystemSchedule Schedule(List<SystemInfo> Systems, float deltaTime);
    }
}
