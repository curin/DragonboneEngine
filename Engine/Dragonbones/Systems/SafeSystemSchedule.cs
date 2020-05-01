using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;

namespace Dragonbones.Systems
{
    /// <summary>
    /// A base implementation of <see cref="ISystemSchedule"/> for Logic Systems
    /// uses a priority list to sort ahead of time
    /// Checks for conflicts in components to avoid parallel access
    /// </summary>
    public class SafeSystemSchedule : ISystemSchedule
    {
        /// <summary>
        /// constructs an instance of <see cref="SafeSystemSchedule"/>
        /// </summary>
        /// <param name="laneCount">the number of concurrent systems that can be run</param>
        /// <param name="maxSize">the maximum number of systems to be stored</param>
        /// <param name="type">the type of systems for this schedule</param>
        public SafeSystemSchedule(SystemType type, int laneCount, int maxSize)
        {
            //initialize all fields
            _laneCount = laneCount;
            _lanes = new SystemInfo[laneCount];
            _running = new bool[laneCount];
            _type = type;

            _maxSize = maxSize;
            _systemCache = new SystemInfo[_maxSize];
            _entries = new Entry[_maxSize];
        }

        /// <summary>
        /// copies an instance of <see cref="SafeSystemSchedule"/>
        /// </summary>
        /// <param name="copy">the schedule to copy</param>
        public SafeSystemSchedule(SafeSystemSchedule copy)
        {
            if (copy == null)
                throw new ArgumentNullException(nameof(copy));
            //copy int values
            _count = copy._count;
            _next = copy._next;
            _top = copy._top;
            _start = copy._start;
            _end = copy._end;
            _rrStart = copy._rrStart;
            _rrEnd = copy._rrEnd;
            _laneCount = copy._laneCount;

            //initialize int arrays
            _lanes = new SystemInfo[_laneCount];
            _running = new bool[_laneCount];

            //copy values in lane arrays
            for (int i = 0; i < _laneCount; i++)
            {
                _lanes[i] = copy._lanes[i];
                _running[i] = copy._running[i];
            }

            //copy storage arrays
            _maxSize = copy._maxSize;
            _systemCache = new SystemInfo[_maxSize];
            _entries = new Entry[_maxSize];

            for (int i = 0; i < _maxSize; i++)
            {
                _systemCache[i] = copy._systemCache[i];
                _entries[i] = copy._entries[i];
            }

            //copy run recurrences
            for (int i = 0; i < copy._runRecurrences.Count; i++)
                _runRecurrences.Add(copy._runRecurrences[i]);

            //copy free space queue
            for (int i = 0; i < copy.freeSpace.Count; i++)
            {
                int space = copy.freeSpace.Dequeue();
                freeSpace.Enqueue(space);
                copy.freeSpace.Enqueue(space);
            }
        }

        private readonly SystemInfo[] _lanes;
        private readonly bool[] _running;
        private int _count = 0, _next = 0, _top = 0;
        private int _start = -1, _end = -1;
        private int _rrStart, _rrEnd;
        private readonly int _maxSize;
        private readonly int _laneCount;
        private readonly SystemInfo[] _systemCache;
        private readonly Entry[] _entries;
        private readonly List<RREntry> _runRecurrences = new List<RREntry>();
        private readonly Queue<int> freeSpace = new Queue<int>();
        private SystemType _type;

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public void Clear()
        {
            _start = _end = -1;
            _count = _next = _top = 0;
            _rrStart = _rrEnd = 0;
            _runRecurrences.Clear();
            freeSpace.Clear();
            Reset();
        }

        /// <summary>
        /// <inheritdoc />
        /// Only sorted on insertion
        /// </summary>
        public void Add(SystemInfo system)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));
            if (system.Type != _type)
                return;
            if (freeSpace.Count > 0)
                _next = freeSpace.Dequeue();
            _systemCache[_next] = system;
            _count++;

            if (_start == -1)
            {
                _entries[_next] = new Entry(system.RunRecurrenceInterval, _next);
                _start = _end = _next;
                _rrStart = _rrEnd = _runRecurrences.Count;
                _runRecurrences.Add(new RREntry(_runRecurrences.Count, system.RunRecurrenceInterval, _next));
                _top++;
                _next = _top;
                return;
            }

            int entry = FindPreviousEntry(system, out RREntry rr);
            if (entry == -1)
            {
                Entry start = _entries[_start];

                if (start.RunRecurrence != system.RunRecurrenceInterval)
                {
                    if (rr.Prev == -1)
                        _rrStart = _runRecurrences.Count;
                    else
                    {
                        RREntry rrprev = _runRecurrences[rr.Prev];
                        rrprev.Next = _runRecurrences.Count;
                        _runRecurrences[rr.Prev] = rrprev;
                    }
                    _runRecurrences.Add(new RREntry(_runRecurrences.Count, system.RunRecurrenceInterval, _next, rr.Index, rr.Prev));
                    rr.Prev = _runRecurrences.Count - 1;
                }
                else
                {
                    rr.Start = _next;
                }
                _runRecurrences[rr.Index] = rr;

                _entries[_next] = new Entry(system.RunRecurrenceInterval, _next, _start);
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

                if (end.RunRecurrence != system.RunRecurrenceInterval)
                {
                    _runRecurrences.Add(new RREntry(_runRecurrences.Count, system.RunRecurrenceInterval, _next, rr.Next, rr.Index));
                    if (rr.Next == -1)
                        _rrEnd = _runRecurrences.Count - 1;
                    rr.Next = _runRecurrences.Count - 1;
                    _runRecurrences[rr.Index] = rr;
                }

                _entries[_next] = new Entry(system.RunRecurrenceInterval, _next, -1, _end);
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
            if (rr.RunRecurrence != system.RunRecurrenceInterval)
            {
                if (rr.Prev != -1)
                {
                    RREntry preRR = _runRecurrences[rr.Prev];
                    preRR.Next = _runRecurrences.Count;
                    _runRecurrences[preRR.Index] = preRR;
                }
                else
                    _rrStart = _runRecurrences.Count;
                
                _runRecurrences.Add(new RREntry(_runRecurrences.Count, system.RunRecurrenceInterval, _next, rr.Index, rr.Prev));
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
            
            _entries[_next] = new Entry(system.RunRecurrenceInterval, _next, next.CacheIndex, prev.CacheIndex);
            
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

        private Entry _current, _searcher;
        private SemaphoreSlim _lock = new SemaphoreSlim(1,1);
        private bool _started;
        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public ScheduleResult NextSystem(int systemLaneID, out SystemInfo systemBatch)
        {
            
            if (systemLaneID >= _laneCount)
                throw new ArgumentOutOfRangeException(nameof(systemLaneID));
            if (_running[systemLaneID])
                _running[systemLaneID] = false;
            _lock.Wait();
            systemBatch = null;
            _searcher = _current;

            if (!_started)
            {
                if (_start == -1)
                {
                    _lock.Release();
                    return ScheduleResult.Finished;
                }
                _current = _entries[_start];
                systemBatch = _systemCache[_current.CacheIndex];
                _started = true;
                _lock.Release();
                return ScheduleResult.Supplied;
            }

            bool invalid;
            while (systemBatch == null)
            {
                if (_searcher.NextEntry == -1)
                {
                    _lock.Release();
                    return _searcher.CacheIndex == _current.CacheIndex ? ScheduleResult.Finished : ScheduleResult.Conflict;
                }

                _searcher = _entries[_searcher.NextEntry];
                systemBatch = _systemCache[_searcher.CacheIndex];

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

            if (_searcher.CacheIndex == _current.NextEntry)
                _current = _searcher;
            _lock.Release();
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
        /// <param name="rr">the runrecurrence of the system if it exists
        /// if it does not exist then it is the one after it unless it is at the end then the previous run recurrence</param>
        /// <returns>the index of the previous entry or
        /// -1 if this entry should be placed at the beginning or
        /// -2 if it should be placed at the end</returns>
        private int FindPreviousEntry(SystemInfo system, out RREntry rr)
        {
            if (system.RunRecurrenceInterval == 0)
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
                    if (rr.RunRecurrence > system.RunRecurrenceInterval)
                    {
                        if (rr.Prev == -1)
                            return -1;
                        return _entries[rr.Start].PrevEntry;
                    }
                    else if (rr.RunRecurrence < system.RunRecurrenceInterval)
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
                if (current.RunRecurrence != system.RunRecurrenceInterval)
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

        private Entry FindEntry(SystemInfo sysInf, out RREntry rr)
        {
            Entry ret;
            SystemInfo tempInf;
            if (sysInf.RunRecurrenceInterval == 0)
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
                while (rr.RunRecurrence != sysInf.RunRecurrenceInterval && rr.Next != -1)
                    rr = _runRecurrences[rr.Next];

                if (rr.RunRecurrence != sysInf.RunRecurrenceInterval)
                    return new Entry(-1, -1);

                ret = _entries[rr.Start];
                tempInf = _systemCache[rr.Start];
            }

            if (tempInf == sysInf)
                return ret;

            while (tempInf != sysInf && ret.RunRecurrence == sysInf.RunRecurrenceInterval && ret.NextEntry != -1)
            {
                ret = _entries[ret.NextEntry];
                tempInf = _systemCache[ret.CacheIndex];
            }

            if (ret.RunRecurrence != sysInf.RunRecurrenceInterval)
                return new Entry(-1, -1);

            if (tempInf != sysInf)
                return new Entry(-1, -1);

            return ret;
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
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
                    if (tempEnt.RunRecurrence != sysInf.RunRecurrenceInterval)
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

        /// <summary>
        /// The entry used to track location of beginning of the run recurrences for faster sorting
        /// </summary>
        private struct RREntry
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
            public readonly int RunRecurrence;
            public int Start;
            public int Next;
            public int Prev;
        }

        /// <summary>
        /// Entry struct for storing linked list used in schedule
        /// </summary>
        private struct Entry
        {
            public Entry(int runRecurrence, int cacheIndex, int next = -1, int prev = -1)
            {
                RunRecurrence = runRecurrence;
                CacheIndex = cacheIndex;
                PrevEntry = prev;
                NextEntry = next;
            }

            public readonly int RunRecurrence;
            public readonly int CacheIndex;
            public int NextEntry;
            public int PrevEntry;
        }
        
        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public bool Finished => (_current.NextEntry == -1 && _started) || _start == -1;

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// The type of systems for this schedule
        /// </summary>
        public SystemType Type => _type;

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public void Reset()
        {
            _started = false;
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public void FinishLane(int systemLaneID)
        {
            if (systemLaneID >= _laneCount)
                throw new ArgumentOutOfRangeException(nameof(systemLaneID));
            _running[systemLaneID] = false;
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        public void AddFromRegistry(ISystemRegistry registry)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));
            foreach (ISystem system in registry)
                Add(system.SystemInfo);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Disposes this object
        /// </summary>
        /// <param name="disposing">should managed objects be disposed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _lock.Dispose();
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        ///<inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
