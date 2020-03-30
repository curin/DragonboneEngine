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
            _systemCache = new SystemInfo[_maxSize];
            _entries = new Entry[_maxSize];
        }

        public SystemSchedule(SystemSchedule copy)
        {
            if (copy == null)
                throw new ArgumentNullException(nameof(copy));
            _count = copy._count;
            _next = copy._next;
            _top = copy._top;
            _start = copy._start;
            _end = copy._end;
            _rrStart = copy._rrStart;
            _rrEnd = copy._rrEnd;
            _laneCount = copy._laneCount;
            _lanes = new SystemInfo[_laneCount];
            _running = new bool[_laneCount];

            for (int i = 0; i < _laneCount; i++)
            {
                _lanes[i] = copy._lanes[i];
                _running[i] = copy._running[i];
            }

            _maxSize = copy._maxSize;
            _systemCache = new SystemInfo[_maxSize];
            _entries = new Entry[_maxSize];

            for (int i = 0; i < _maxSize; i++)
            {
                _systemCache[i] = copy._systemCache[i];
                _entries[i] = copy._entries[i];
            }

            for (int i = 0; i < copy._runRecurrences.Count; i++)
                _runRecurrences.Add(copy._runRecurrences[i]);

            for (int i = 0; i < copy.freeSpace.Count; i++)
            {
                int space = copy.freeSpace.Dequeue();
                freeSpace.Enqueue(space);
                copy.freeSpace.Enqueue(space);
            }
        }

        readonly SystemInfo[] _lanes;
        readonly bool[] _running;
        int _count = 0, _next = 0, _top = 0;
        int _start = -1, _end = -1;
        int _rrStart, _rrEnd;
        private readonly int _maxSize;
        private readonly int _laneCount;
        readonly SystemInfo[] _systemCache;
        readonly Entry[] _entries;
        readonly List<RREntry> _runRecurrences = new List<RREntry>();
        readonly Queue<int> freeSpace = new Queue<int>();

        public void Clear()
        {
            _start = _end = -1;
            _count = _next = _top = 0;
            _rrStart = _rrEnd = 0;
            _runRecurrences.Clear();
            freeSpace.Clear();
            Reset();
        }

        public void Add(SystemInfo system)
        {
            if (freeSpace.Count > 0)
                _next = freeSpace.Dequeue();
            _systemCache[_next] = system ?? throw new ArgumentNullException(nameof(system));
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

            int entry = FindPreviousEntry(system, out RREntry rr);
            if (entry == -1)
            {
                Entry start = _entries[_start];

                if (start.RunRecurrence != system.RunReccurenceInterval)
                {
                    if (rr.Prev == -1)
                        _rrStart = _runRecurrences.Count;
                    else
                    {
                        RREntry rrprev = _runRecurrences[rr.Prev];
                        rrprev.Next = _runRecurrences.Count;
                        _runRecurrences[rr.Prev] = rrprev;
                    }
                    _runRecurrences.Add(new RREntry(_runRecurrences.Count, system.RunReccurenceInterval, _next, rr.Index, rr.Prev));
                    rr.Prev = _runRecurrences.Count - 1;
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

            Entry prev = _entries[entry];
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
            else if (rr.RunRecurrence != prev.RunRecurrence)
            {
                rr.Start = _next;
                _runRecurrences[rr.Index] = rr;
            }

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

        /// <summary>
        /// find the index of the previous entry
        /// </summary>
        /// <param name="system">system to find previous entry</param>
        /// <param name="rr">the runreccurence of the system if it exists
        /// if it does not exist then it is the one after it unless it is at the end then the previous run reccurence</param>
        /// <returns>the index of the previous entry or
        /// -1 if this entry should be placed at the beginning or
        /// -2 if it should be placed at the end</returns>
        int FindPreviousEntry(SystemInfo system, out RREntry rr)
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
                    break;
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

        Entry FindEntry(SystemInfo sysInf, out RREntry rr)
        {
            Entry ret;
            SystemInfo tempInf;
            if (sysInf.RunReccurenceInterval == 0)
            {
                rr = _runRecurrences[_rrEnd];
                if (rr.RunRecurrence != 0)
                    return new Entry(-1, -1);
                ret = _entries[rr.Start];
                tempInf = _systemCache[rr.Start];
            }
            else
            {
                rr = _runRecurrences[_rrStart];
                while (rr.RunRecurrence != sysInf.RunReccurenceInterval && rr.Next != -1)
                    rr = _runRecurrences[rr.Next];

                if (rr.RunRecurrence != sysInf.RunReccurenceInterval)
                    return new Entry(-1, -1);

                ret = _entries[rr.Start];
                tempInf = _systemCache[rr.Start];
            }

            if (tempInf == sysInf)
                return ret;

            while (tempInf != sysInf && ret.RunRecurrence == sysInf.RunReccurenceInterval && ret.NextEntry != -1)
            {
                ret = _entries[ret.NextEntry];
                tempInf = _systemCache[ret.CacheIndex];
            }

            if (ret.RunRecurrence != sysInf.RunReccurenceInterval)
                return new Entry(-1, -1);

            if (tempInf != sysInf)
                return new Entry(-1, -1);

            return ret;
        }

        public void Remove(SystemInfo sysInf)
        {
            if (sysInf == null)
                return;

            Entry ent = FindEntry(sysInf, out RREntry rr);
            Entry tempEnt;

            if (ent.CacheIndex == -1)
                return;

            if (rr.Start == ent.CacheIndex)
            {
                bool end = false;
                if (ent.NextEntry != -1)
                {
                    tempEnt = _entries[ent.NextEntry];
                    if (tempEnt.RunRecurrence != sysInf.RunReccurenceInterval)
                        end = true;
                }
                else
                    end = true;
                if (end)
                {
                    RREntry rrTemp;
                    if (rr.Prev != -1)
                    {
                        rrTemp = _runRecurrences[rr.Prev];
                        rrTemp.Next = rr.Next;
                        _runRecurrences[rr.Prev] = rrTemp;
                    }
                    if (rr.Next != -1)
                    {
                        rrTemp = _runRecurrences[rr.Next];
                        rrTemp.Prev = rr.Prev;
                        _runRecurrences[rr.Next] = rrTemp;
                    }
                    _runRecurrences.RemoveAt(rr.Index);
                    if (_rrStart == rr.Index)
                        _rrStart = rr.Next;
                    if (_rrEnd == rr.Index)
                        _rrEnd = rr.Prev;
                    for (int i = 0; i < _runRecurrences.Count; i++)
                    {
                        rrTemp = _runRecurrences[i];
                        if (rrTemp.Next > rr.Index)
                            rrTemp.Next--;
                        if (rrTemp.Prev > rr.Index)
                            rrTemp.Prev--;
                        if (rrTemp.Index != i)
                            rrTemp.Index = i;
                        _runRecurrences[i] = rrTemp;
                    }
                    if (_rrStart > rr.Index)
                        _rrStart--;
                    if (_rrEnd > rr.Index)
                        _rrEnd--;
                }
                else
                {
                    rr.Start = ent.NextEntry;
                    _runRecurrences[rr.Index] = rr;
                }
            }

            if (ent.PrevEntry != -1)
            {
                tempEnt = _entries[ent.PrevEntry];
                tempEnt.NextEntry = ent.NextEntry;
                _entries[ent.PrevEntry] = tempEnt;
            }
            else
            {
                _start = ent.NextEntry;
            }

            if (ent.NextEntry != -1)
            {
                tempEnt = _entries[ent.NextEntry];
                tempEnt.PrevEntry = ent.PrevEntry;
                _entries[ent.NextEntry] = tempEnt;
            }
            else
            {
                _end = ent.PrevEntry;
            }

            freeSpace.Enqueue(ent.CacheIndex);
            _count--;
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
        
        public bool Finished => (current.NextEntry == -1 && started) || _start == -1;

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
    }
}
