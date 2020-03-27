using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Dragonbones
{
    public class SystemSchedule : ISystemSchedule
    {
        public SystemSchedule(int laneCount, int maxSize)
        {
            _laneCount = laneCount;
            _lanes = new SystemInfo[laneCount];
            _running = new bool[laneCount];

            _maxSize = maxSize;
            _systemCache = new SystemInfo[maxSize];
            _entries = new Entry[maxSize];
        }

        readonly SystemInfo[] _lanes;
        readonly bool[] _running;
        int _count = 0, _next = 0, _top = 0;
        int _start = -1, _end = -1;
        int _rrStart, _rrEnd;
        int _maxSize, _laneCount;
        SystemInfo[] _systemCache;
        Entry[] _entries;
        List<RREntry> _runRecurrences = new List<RREntry>();
        Queue<int> freeSpace = new Queue<int>();

        public void Clear()
        {
            _start = _end = -1;
            _count = _next = 0;
            _runRecurrences.Clear();
            freeSpace.Clear();
        }


        //
        //
        //
        //Need to FIX ADD AND SORT
        //
        //
        //

        public void Add(SystemInfo system)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));
            _systemCache[_next] = system;
            _count++;

            if (_start == -1)
            {
                _entries[_next] = new Entry(system.RunReccurenceInterval, _next);
                _start = _end = _next;
                _rrStart = _rrEnd = _runRecurrences.Count;
                _runRecurrences.Add(new RREntry(_runRecurrences.Count, system.RunReccurenceInterval, _next));
                _top++;
                _next = _top;
                return;
            }

            int entry = findPreviousEntry(system, out RREntry rr);
            if (entry == -1)
            {
                Entry start = _entries[_start];

                if (start.RunRecurrence != system.RunReccurenceInterval)
                {
                    if (rr.Prev == -1)
                        _rrStart = _runRecurrences.Count;
                    rr.Prev = _runRecurrences.Count;
                    _runRecurrences.Add(new RREntry(_runRecurrences.Count, system.RunReccurenceInterval, _next, rr.Index));
                }
                else
                {
                    rr.Start = _next;
                }
                _runRecurrences[rr.Index] = rr;

                _entries[_next] = new Entry(system.RunReccurenceInterval, _next, _start);
                _start = _next;

                start.PrevEntry = _next;
                _entries[start.CacheIndex] = start;
                if (freeSpace.Count == 0)
                {
                    if (_next == _top)
                        _top++;
                    _next = _top;
                }
                else
                    _next = freeSpace.Dequeue();
                return;
            }
            
            if (entry == -2)
            {
                Entry end = _entries[_end];

                if (end.RunRecurrence != system.RunReccurenceInterval)
                {
                    _runRecurrences.Add(new RREntry(_runRecurrences.Count, system.RunReccurenceInterval, _next, rr.Next, rr.Index));
                    if (rr.Next == -1)
                        _rrEnd = _runRecurrences.Count - 1;
                    rr.Next = _runRecurrences.Count - 1;
                    _runRecurrences[rr.Index] = rr;
                }

                _entries[_next] = new Entry(system.RunReccurenceInterval, _next, -1, _end);
                _end = _next;

                end.NextEntry = _next;
                _entries[end.CacheIndex] = end;
                if (freeSpace.Count == 0)
                {
                    if (_next == _top)
                        _top++;
                    _next = _top;
                }
                else
                    _next = freeSpace.Dequeue();
                return;
            }

            if (rr.RunRecurrence != system.RunReccurenceInterval)
            {
                if (rr.Prev != -1)
                {
                    RREntry preRR = _runRecurrences[rr.Prev];
                    preRR.Next = _runRecurrences.Count;
                    _runRecurrences[preRR.Index] = preRR;
                }
                else
                    _rrStart = _runRecurrences.Count;
                
                _runRecurrences.Add(new RREntry(_runRecurrences.Count, system.RunReccurenceInterval, _next, rr.Index, rr.Prev));
                rr.Prev = _runRecurrences.Count - 1;

                _runRecurrences[rr.Index] = rr;
            }

            Entry prev = _entries[entry];
            Entry next = _entries[prev.NextEntry];
            
            prev.NextEntry = _next;
            next.PrevEntry = _next;
            
            _entries[_next] = new Entry(system.RunReccurenceInterval, _next, next.CacheIndex, prev.CacheIndex);
            
            _entries[entry] = prev;
            _entries[next.CacheIndex] = next;
            if (freeSpace.Count == 0)
            {
                if (_next == _top)
                    _top++;
                _next = _top;
            }
            else
                _next = freeSpace.Dequeue();
            return;
        }

        Entry current, searcher;
        bool started;
        public ScheduleResult NextSystem(int systemLaneID, out SystemInfo systemBatch)
        {
            if (systemLaneID >= _laneCount)
                throw new ArgumentOutOfRangeException(nameof(systemLaneID));
            if (_running[systemLaneID])
                _running[systemLaneID] = false;

            systemBatch = null;
            searcher = current;

            if (!started)
            {
                if (_start == -1)
                {
                    return ScheduleResult.Finished;
                }
                current = _entries[_start];
                systemBatch = _systemCache[current.CacheIndex];
                started = true;
                return ScheduleResult.Supplied;
            }

            bool invalid;
            while (systemBatch == null)
            {
                if (searcher.NextEntry == -1)
                    if (searcher.CacheIndex == current.CacheIndex)
                        return ScheduleResult.Finished;
                    else
                        return ScheduleResult.Conflict;

                searcher = _entries[searcher.NextEntry];
                systemBatch = _systemCache[searcher.CacheIndex];

                invalid = !(systemBatch.Active || systemBatch.Run);
                for (int i = 0; i < _laneCount; i++)
                    if (_running[i])
                        if (systemBatch.GetComponentsUsedIDs().Any((val) => { return _lanes[i].GetComponentsUsedIDs().Contains(val); }))
                            invalid = true;

                if (invalid)
                {
                    systemBatch = null;
                }
            }

            if (searcher.CacheIndex == current.NextEntry)
                current = searcher;
            systemBatch.Run = true;
            systemBatch.Age = 0;
            _lanes[systemLaneID] = systemBatch;
            _running[systemLaneID] = true;
            return ScheduleResult.Supplied;
        }

        int findPreviousEntry(SystemInfo system, out RREntry rr)
        {
            if (system.RunReccurenceInterval == 0)
            {
                rr = _runRecurrences[_rrEnd];
                if (rr.RunRecurrence != 0)
                    return -2;
            }
            else
            {
                rr = _runRecurrences[_rrStart];
                while (true)
                {
                    if (rr.RunRecurrence > system.RunReccurenceInterval)
                    {
                        if (rr.Prev == -1)
                            return -1;
                        return _entries[rr.Start].PrevEntry;
                    }
                    else if (rr.RunRecurrence < system.RunReccurenceInterval)
                    {
                        if (rr.RunRecurrence == 0)
                        {
                            if (rr.Prev == -1)
                                return -1;
                            return _entries[rr.Start].PrevEntry;
                        }

                        if (rr.Next == -1)
                            return -2;
                        rr = _runRecurrences[rr.Next];
                    }
                    else
                    {
                        break;
                    }
                }
            }

            Entry current = _entries[rr.Start];
            SystemInfo tempSys = _systemCache[current.CacheIndex];
            while (true)
            {
                if (current.RunRecurrence != system.RunReccurenceInterval)
                {
                    break;
                }

                if (tempSys.PriorityComposite > system.PriorityComposite)
                {
                    if (current.NextEntry == -1)
                        return -2;
                    current = _entries[current.NextEntry];
                    tempSys = _systemCache[current.CacheIndex];
                }
                else if (tempSys.PriorityComposite <= system.PriorityComposite)
                {
                    break;
                }
            }

            return current.PrevEntry;
        }

        struct RREntry
        {
            public RREntry(int index, int RR, int start, int next = -1, int prev = -1)
            {
                Index = index;
                RunRecurrence = RR;
                Start = start;
                Next = next;
                Prev = prev;
            }

            public int Index;
            public int RunRecurrence;
            public int Start;
            public int Next;
            public int Prev;
        }

        struct Entry
        {
            public Entry(int runRecurrence, int cacheIndex, int next = -1, int prev = -1)
            {
                RunRecurrence = runRecurrence;
                CacheIndex = cacheIndex;
                PrevEntry = prev;
                NextEntry = next;
            }

            public int RunRecurrence;
            public int CacheIndex;
            public int NextEntry;
            public int PrevEntry;
        }

        readonly List<SystemInfo> _systemBatchs = new List<SystemInfo>();
        
        public bool Finished => current.NextEntry == -1 || _start == -1;

        public int Count => _count;

        public void Reset()
        {
            started = false;
        }

        public void FinishLane(int systemLaneID)
        {
            if (systemLaneID >= _laneCount)
                throw new ArgumentOutOfRangeException(nameof(systemLaneID));
            _running[systemLaneID] = false;
        }

        public void Sort()
        {
            RREntry start = _runRecurrences[_rrStart];
            Entry eStart = _entries[start.Start];
            RREntry temp = default;
            Entry eTemp = default;
            for (int i = 0; i < _runRecurrences.Count; i++)
            {                    
                Entry eEnd;
                if (i != _runRecurrences.Count - 1)
                {
                    temp = _runRecurrences[start.Next];
                    eTemp = _entries[temp.Start];
                    eEnd = _entries[eTemp.PrevEntry];
                }
                else
                {
                    eEnd = _entries[_end];
                }

                if (eStart.CacheIndex == eEnd.CacheIndex)
                {
                    start = temp;
                    eStart = eTemp;
                    continue;
                }

                MergeSort(eStart, eEnd);

                start = temp;
                eStart = eTemp;
            }
        }

        void MergeSort(Entry start, Entry end)
        {
            if (start.CacheIndex == end.CacheIndex)
                return;

            Entry mid = getMiddle(start, end);
            Entry next = _entries[mid.NextEntry];

            MergeSort(start, mid);
            MergeSort(next, end);

            Merge(start, mid, next, end);
        }

        Entry getMiddle(Entry start, Entry end)
        {
            while (start.CacheIndex != end.CacheIndex)
            {
                end = _entries[end.PrevEntry];
                
                if (start.CacheIndex == end.CacheIndex)
                    break;

                start = _entries[start.NextEntry];
            }

            return end;
        }

        void Merge(Entry start, Entry mid, Entry next, Entry end)
        {
            SystemInfo low, high;
            Entry temp;
            if (start.PrevEntry != -1)
                temp = _entries[start.PrevEntry];
            else
                temp = new Entry(0, -1);
            int nextEntry;
            while (true)
            {
                nextEntry = next.NextEntry;
                low = _systemCache[start.CacheIndex];
                high = _systemCache[next.CacheIndex];

                if (SystemInfo.Sort(high, low) < 0)
                {
                    temp.NextEntry = next.CacheIndex;
                    start.PrevEntry = next.CacheIndex;
                    mid.NextEntry = next.NextEntry;
                    next.NextEntry = start.CacheIndex;
                    next.PrevEntry = temp.CacheIndex;
                    if (temp.CacheIndex != -1)
                        _entries[temp.CacheIndex] = temp;
                    else
                        _start = next.CacheIndex;
                    _entries[next.CacheIndex] = next;
                    _entries[mid.CacheIndex] = mid;
                    _entries[start.CacheIndex] = start;

                    if (next.CacheIndex == end.CacheIndex)
                    {
                        if (nextEntry != -1)
                        {
                            temp = _entries[nextEntry];
                            temp.PrevEntry = mid.CacheIndex;
                            _entries[nextEntry] = temp;
                        }
                        else
                        {
                            _end = mid.CacheIndex;
                        }
                        break;
                    }
                    temp = next;
                    next = _entries[nextEntry];
                    next.PrevEntry = mid.PrevEntry;
                    _entries[nextEntry] = next;
                    continue;
                }

                if (start.CacheIndex == mid.CacheIndex)
                {
                    start.NextEntry = next.CacheIndex;
                    next.PrevEntry = start.CacheIndex;
                    _entries[next.CacheIndex] = next;
                    _entries[start.CacheIndex] = start;
                    break;
                }
                
                temp = start;
                start = _entries[start.NextEntry];
                

            }
        }
    }
}
