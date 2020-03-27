using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones
{
    public class SystemSchedule : ISystemSchedule
    {
        int _count = 0;
        List<SystemInfo> _systemBatchs = new List<SystemInfo>();
        int _place = 0;
        public bool Finished => _place == _systemBatchs.Count;

        public int Count => _count;

        public int BatchCount => _systemBatchs.Count;

        public void Add(SystemInfo systemBatch)
        {
            _systemBatchs.Add(systemBatch);
        }

        public void Clear()
        {
            _place = 0;
            _count = 0;
            _systemBatchs.Clear();
        }

        public bool NextSystem(out SystemInfo systemBatch)
        {
            if (_place >= _systemBatchs.Count)
            {
                systemBatch = default;
                return false;
            }
            systemBatch = _systemBatchs[_place];
            _place++;
            return true;
        }

        public void Reset()
        {
            _place = 0;
        }

        public void Sort(Comparison<SystemInfo> comparer)
        {
            _systemBatchs.Sort(comparer);
        }
    }
}
