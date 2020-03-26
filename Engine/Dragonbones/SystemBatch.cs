using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Dragonbones
{
    public class SystemBatch : ISystemBatch
    {
        public SystemBatch(int size, int laneCount)
        {
            _laneCount = laneCount;
            _lanes = new int[_laneCount];
            _laneEnds = new int[_laneCount];
            _laneLength = new double[_laneCount];
            _data = new SystemInfo[size];
            _entries = new Entry[size];
        }

        SystemInfo[] _data;
        int[] _lanes;
        int[] _laneEnds;
        double[] _laneLength;
        Entry[] _entries;
        int _laneCount;
        int _count;
        int _top;
        int _dataTop;
        int[] _next;
        bool[] _done;
        int _longestLane;

        public int Count => _count;

        public double Length => _laneLength[_longestLane];

        public bool Finished => _done.All(val => val);

        public bool Full => _count == _entries.Length;

        public void AppendToLane(int laneID, SystemInfo sysInf, double avgLength)
        {
            if (sysInf == null)
                throw new ArgumentNullException(nameof(sysInf));
            if (laneID >= _laneCount)
                throw new ArgumentOutOfRangeException(nameof(laneID));

            int entry = getLastEntry(laneID);
            Entry ent = new Entry()
            {
                ArrayID = _top,
                SystemID = _dataTop,
                Lane = laneID,
                Previous = entry,
                Set = true
            };

            lock (_entries)
            {
                _laneEnds[laneID] = _top;
                _laneLength[laneID] += avgLength;
                if (_laneLength[laneID] > _laneLength[_longestLane])
                    _longestLane = laneID;
                _entries[_top] = ent;
                _data[_dataTop] = sysInf;

                
                if (entry == -1)
                {
                    _lanes[laneID] = _top;
                }
                else
                {
                    _entries[entry].Next = _top;
                }
                _top++;
                _dataTop++;
                _count++;
            }
        }

        public void Clear()
        {
            lock (_entries)
            {
                _count = 0;
                for (int i = 0; i < _laneCount; i++)
                    _laneLength[i] = _lanes[i] = _laneEnds[i] = 0;
            }
        }

        public bool IsLaneEmpty(int laneID)
        {
            return _lanes[laneID] != 0 ? false : _entries[0].Lane == laneID;
        }

        /// <summary>
        /// Expands the number of entries allowed
        /// </summary>
        /// <param name="newSpaces">the number of additional spaces</param>
        public void Expand(int newSpaces)
        {
            int newLength = _entries.Length + newSpaces;
            lock (_entries)
            {
                Entry[] temp = new Entry[newLength];
                SystemInfo[] tempData = new SystemInfo[newLength];

                for (int i = 0; i < _entries.Length; i++)
                {
                    temp[i] = _entries[i];
                    tempData[i] = _data[i];
                }

                _entries = temp;
                _data = tempData;
            }
        }

        public bool IsLaneFinished(int laneID)
        {
            return _done[laneID];
        }

        public double LaneLenth(int laneID)
        {
            return _laneLength[laneID];
        }

        private int getLastEntry(int laneID)
        {
            int entry = _lanes[laneID];
            if (entry == 0)
            {
                if (!_entries[0].Set || _entries[0].Lane != laneID)
                {
                    return -1;
                }
            }

            while (_entries[entry].Next != 0)
                entry = _entries[entry].Next;

            return entry;
        }

        public void MergeLanes(int lane1ID, int lane2ID)
        {
            lock (_entries)
            {
                _entries[_laneEnds[lane1ID]].Next = _lanes[lane2ID];
                _laneLength[lane1ID] += _laneLength[lane2ID];
                _laneLength[lane2ID] = _lanes[lane2ID] = _laneEnds[lane2ID] = 0;
            }
        }

        public bool NextSystem(int laneID, out SystemInfo sysInf)
        {
            if (_done[laneID])
            {
                sysInf = null;
                return false;
            }

            if (_next[laneID] == 0)
                _next[laneID] = _lanes[laneID];

            Entry ent = _entries[_next[laneID]];
            if (ent.Lane != laneID || !ent.Set)
            {
                sysInf = null;
                return false;
            }

            sysInf = _data[ent.ArrayID];
            _next[laneID] = ent.Next;

            if (ent.Next == 0 && _entries[0].Previous != _next[laneID])
                _done[laneID] = true;

            return true;
        }

        public void Reset()
        {
            for (int i = 0; i < _laneCount; i++)
            {
                _next[i] = 0;
                _done[i] = false;
            }
        }

        public void ResetLane(int laneID)
        {
            _next[laneID] = 0;
            _done[laneID] = false;
        }

        public void SwapLanes(int lane1ID, int lane2ID)
        {
            lock (_entries)
            {
                int temp = _lanes[lane2ID];
                _lanes[lane2ID] = _lanes[lane1ID];
                _lanes[lane1ID] = temp;
                temp = _laneEnds[lane2ID];
                _laneEnds[lane2ID] = _laneEnds[lane1ID];
                _laneEnds[lane1ID] = temp;
                double temp2 = _laneLength[lane2ID];
                _laneLength[lane2ID] = _laneLength[lane1ID];
                _laneLength[lane1ID] = temp2;
            }
        }

        struct Entry
        {
            public int SystemID;
            public int ArrayID;
            public int Lane;
            public int Next;
            public int Previous;
            public bool Set;
        }
    }
}
