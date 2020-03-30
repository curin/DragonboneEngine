using System;
using System.Collections;
using System.Collections.Generic;

namespace Dragonbones.Collections
{
    public class NamedDataRegistry<TValue>
    {
        Entry[] _entries;
        TValue[] _values;
        int _count;
        int _top;
        int _next;
        int _capacity;
        int _hashSize;
        int _start;
        int _end;
        int[] _starts;
        int[] _ends;
        Queue<int> _freeIDs;

        public int Add(string name, TValue value)
        {
            if (_freeIDs.Count > 0)
                _next = _freeIDs.Dequeue();
            if (_next == _top && _top == _capacity)
                Expand();
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

        void Expand()
        {
            Entry[] temp = new Entry[_capacity << 1];
            TValue[] tempVals = new TValue[_capacity << 1];

            for (int i = 0; i < _capacity; i++)
            {
                temp[i] = _entries[i];
                tempVals[i] = _values[i];
            }
            _entries = temp;
            _values = tempVals;
        }

        int GetHashIndex(int hashValue) { return hashValue % _hashSize; }

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
    }
}
