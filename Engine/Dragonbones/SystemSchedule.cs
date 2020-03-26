using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones
{
    public class SystemSchedule : ISystemSchedule
    {
        int _count = 0;
        List<ISystemBatch> _systemBatchs = new List<ISystemBatch>();
        int _place = 0;
        public bool Finished => _place == _systemBatchs.Count;

        public int Count => _count;

        public int BatchCount => _systemBatchs.Count;

        public void Add(ISystemBatch systemBatch)
        {
            _systemBatchs.Add(systemBatch);
        }

        public void AddRange(IEnumerable<ISystemBatch> batches)
        {
            _systemBatchs.AddRange(batches);
        }

        public void Clear()
        {
            _place = 0;
            _count = 0;
            _systemBatchs.Clear();
        }

        public bool NextBatch(out ISystemBatch systemBatch)
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
    }
}
