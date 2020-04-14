using System;
using System.Collections;
using System.Collections.Generic;

namespace Dragonbones.Collections
{
    public class NamedDataRegistry<TValue> : IEnumerable<TValue>
        where TValue : IEquatable<TValue>
    {
        Entry[] _entries;
        TValue[] _values;
        int _count;
        int _top;
        int _next;
        int _capacity;
        int _hashSize;
        int _start = -1;
        int _end = -1;
        int[] _starts;
        int[] _ends;
        Queue<int> _freeIDs = new Queue<int>();

        public NamedDataRegistry(int capacity = 64, int hashSize = 47)
        {
            _entries = new Entry[capacity];
            _values = new TValue[capacity];
            _starts = new int[hashSize];
            _ends = new int[hashSize];

            for (int i = 0; i < hashSize; i++)
                _starts[i] = _ends[i] = -1;
            _count = _top = _next = 0;
            _capacity = capacity;
            _hashSize = hashSize;
        }


        public TValue this[string name] => Get(name);

        public TValue this[int id] => Get(id);

        public int Count => _count;

        public int Add(string name, TValue value)
        {
            if (_freeIDs.Count > 0)
                _next = _freeIDs.Dequeue();
            if (_next == _top && _top == _capacity)
                Expand(_capacity << 1);
            int hash = GetHashIndex(name.GetHashCode());
            int end = _ends[hash];
            Entry ent = new Entry(_next, name, end, _end);
            _entries[_next] = ent;
            _values[_next] = value;
            if (end == -1)
            {
                _starts[hash] = _next;
                _ends[hash] = _next;

                if (_end == -1)
                    _start = _end = _next;
                else
                {
                    Entry temp = _entries[_end];
                    temp.NextEnumerator = _next;
                    _entries[_end] = temp;
                    _end = _next;
                }
            }
            else
            {
                Entry temp = _entries[end];
                temp.NextLink = _next;
                _entries[end] = temp;
                if (_end == -1)
                    _start = _end = _next;
                else
                {
                    temp = _entries[_end];
                    temp.NextEnumerator = _next;
                    _entries[_end] = temp;
                    _end = _next;
                }
            }
            if (_next == _top)
                _top++;
            _next = _top;
            _count++;
            return ent.ID;
        }

        public bool TryGet(string name, out TValue value)
        {
            int id = FindEntry(name, out Entry ent);
            if (id == -1)
            {
                value = default;
                return false;
            }

            value = _values[id];
            return true;
        }

        public bool TryGet(int id, out TValue value)
        {
            if (id < 0 || id > _capacity)
            {
                value = default;
                return false;
            }

            Entry ent = _entries[id];
            if (ent.ID == -1 || ent.ID == 0 && ent.NextEnumerator == 0)
            {
                value = default;
                return false;
            }

            value = _values[id];
            return true;
        }

        public TValue Get(string name)
        {
            int id = FindEntry(name, out Entry ent);
            return _values[id];
        }

        public TValue Get(int id)
        {
            return _values[id];  
        }

        public bool ContainsName(string name)
        {
            return FindEntry(name, out Entry ent) != -1;
        }

        public bool Contains(TValue value)
        {
            return FindEntry(value, out Entry ent) != -1;
        }

        public int GetID(string name)
        {
            return FindEntry(name, out Entry ent);
        }

        public int GetID(TValue value)
        {
            return FindEntry(value, out Entry ent);
        }

        public bool ContainsID(int id)
        {
            Entry ent = _entries[id];
            return ent.ID != -1;
        }

        public TValue PopAt(string name)
        {
            TValue val = default;
            if (FindEntry(name, out Entry ent) != -1)
            {
                val = _values[ent.ID];
                Remove(ref ent);
                 
            }
            return val;
        }

        public TValue PopAt(int id)
        {
            TValue val = _values[id];
            Entry ent = _entries[id];
            Remove(ref ent);
            return val;
        }

        public TValue Pop(TValue value)
        {
            TValue val = default;
            if (FindEntry(value, out Entry ent) != -1)
            {
                val = _values[ent.ID];
                Remove(ref ent);
            }
            return val;
        }

        public void RemoveAt(string name)
        {
            if (FindEntry(name, out Entry ent) != -1)
                Remove(ref ent);
        }

        public void RemoveAt(int id)
        {
            Entry ent = _entries[id];
            Remove(ref ent);
        }

        public void Remove(TValue value)
        {
            if (FindEntry(value, out Entry ent) != -1)
                Remove(ref ent);
        }

        void Remove(ref Entry ent)
        {
            Entry temp;
            if(ent.NextEnumerator != -1)
            {
                temp = _entries[ent.NextEnumerator];
                temp.PreviousEnumerator = ent.PreviousEnumerator;
                _entries[ent.NextEnumerator] = temp;
            }
            else
            {
                _end = ent.PreviousEnumerator;
            }

            if (ent.PreviousEnumerator != -1)
            {
                temp = _entries[ent.PreviousEnumerator];
                temp.NextEnumerator = ent.NextEnumerator;
                _entries[ent.PreviousEnumerator] = temp;
            }
            else
            {
                _start = ent.NextEnumerator;
            }

            int loc = GetHashIndex(ent.Name.GetHashCode());
            if (ent.NextLink != -1)
            {
                temp = _entries[ent.NextLink];
                temp.PreviousLink = ent.PreviousLink;
                _entries[ent.NextLink] = temp;
            }
            else
            {
                _starts[loc] = ent.NextLink;
            }

            if (ent.PreviousLink != -1)
            {
                temp = _entries[ent.PreviousLink];
                temp.NextLink = ent.NextLink;
                _entries[ent.PreviousLink] = temp;
            }
            else
            {
                _ends[loc] = ent.PreviousLink;
            }

            if (ent.ID == _top - 1)
                _top--;
            else
                _freeIDs.Enqueue(ent.ID);
            _count--;

            int id = ent.ID;
            ent.ID = -1;
            _entries[id] = ent;
            _values[id] = default;
        }

        int FindEntry(string name, out Entry entry)
        {
            int loc = GetHashIndex(name.GetHashCode());
            if (_starts[loc] == -1)
            {
                entry = default;
                return -1;
            }

            entry = _entries[_starts[loc]];

            while (entry.NextLink != -1 && entry.Name != name)
                entry = _entries[entry.NextLink];

            if (entry.Name != name)
                return -1;

            return entry.ID;
        }

        int FindEntry(TValue value, out Entry entry)
        {
            if (_start == -1)
            {
                entry = default;
                return -1;
            }
            entry = _entries[_start];

            while (_values[entry.ID].Equals(value) && entry.NextEnumerator != -1)
                entry = _entries[entry.NextEnumerator];

            if (_values[entry.ID].Equals(value))
                return entry.ID;
            return -1;
        }

        void Expand(int newSize)
        {
            Entry[] temp = new Entry[newSize];
            TValue[] tempVals = new TValue[newSize];

            for (int i = 0; i < _capacity; i++)
            {
                temp[i] = _entries[i];
                tempVals[i] = _values[i];
            }
            _entries = temp;
            _values = tempVals;
        }

        public void Clear()
        {
            _start = _end = -1;
            _top = _count = _next = 0;
            _freeIDs.Clear();
            for (int i = 0; i < _hashSize; i++)
                _starts[i] = _ends[i] = -1;
        }

        int GetHashIndex(int hashCode) { return ((hashCode % _hashSize) + _hashSize) % _hashSize; }

        public IEnumerator<TValue> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        struct Entry
        {
            public Entry(int id, string name, int prevLink, int prevEnum, int nextLink = -1, int nextEnum = -1)
            {
                ID = id;
                Name = name;
                PreviousLink = prevLink;
                PreviousEnumerator = prevEnum;
                NextLink = nextLink;
                NextEnumerator = nextEnum;
            }
            public int ID;
            public string Name;
            public int NextLink;
            public int PreviousLink;
            public int NextEnumerator;
            public int PreviousEnumerator;
        }

        class Enumerator : IEnumerator<TValue>
        {
            NamedDataRegistry<TValue> _reg;
            public Enumerator(NamedDataRegistry<TValue> reg)
            {
                _reg = reg;
                _next = -2;
            }
            int _current, _next;
            public TValue Current => _reg._values[_current];

            object IEnumerator.Current => _reg._values[_current];

            public void Dispose()
            {
                _reg = null;
            }

            public bool MoveNext()
            {
                if (_next == -1)
                    return false;
                Entry ent = default;
                if (_next == -2)
                {
                    
                    _current = _reg._start;
                    if (_current != -1)
                        ent = _reg._entries[_current];
                    _next = ent.NextEnumerator;
                    return true;
                }

                _current = _next;
                ent = _reg._entries[_current];
                _next = ent.NextEnumerator;
                return true;
            }

            public void Reset()
            {
                _next = -2;
            }
        }
    }
}
