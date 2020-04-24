using System;
using System.Collections;
using System.Collections.Generic;

namespace Dragonbones.Collections
{
    /// <summary>
    /// A class which stores values by a string name, but also gives an ID for fast lookup
    /// </summary>
    /// <typeparam name="TValue">the values to store</typeparam>
    public class NamedDataRegistry<TValue> : IEnumerable<TValue>
        where TValue : IEquatable<TValue>
    {
        private Entry[] _entries;
        private TValue[] _values;
        private int _count;
        private int _top;
        private int _next;
        private readonly int _capacity;
        private readonly int _hashSize;
        private int _start = -1;
        private int _end = -1;
        private readonly int[] _starts;
        private readonly int[] _ends;
        private readonly Queue<int> _freeIDs = new Queue<int>();

        /// <summary>
        /// Constructs a <see cref="NamedDataRegistry{TValue}"/> of the specified size
        /// </summary>
        /// <param name="capacity">the number of values that can be stored</param>
        /// <param name="hashSize">the size of the hashtable used for finding values by name.
        /// The larger this is the faster name lookups but the more memory used</param>
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

        /// <summary>
        /// Access a value by name
        /// </summary>
        /// <param name="name">the name of the value</param>
        /// <returns>the value</returns>
        public TValue this[string name] => Get(name);

        /// <summary>
        /// Access a value by ID
        /// </summary>
        /// <param name="id">the ID of the value</param>
        /// <returns>the value</returns>
        public TValue this[int id] => Get(id);

        /// <summary>
        /// The number of objects stored in the registry
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Adds a value to the registry
        /// </summary>
        /// <param name="name">the name of the value</param>
        /// <param name="value">the value to store</param>
        /// <returns>the id of the stored value, -1 if it was unable to be stored</returns>
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

        /// <summary>
        /// Attempt to get a value by name
        /// </summary>
        /// <param name="name">the name of the value</param>
        /// <param name="value">the returned value</param>
        /// <returns>whether the name was found</returns>
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

        /// <summary>
        /// Attempt to get a value by ID
        /// </summary>
        /// <param name="id">the ID of the value</param>
        /// <param name="value">the returned value</param>
        /// <returns>Whether the ID was found</returns>
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

        /// <summary>
        /// Get a value by name
        /// </summary>
        /// <param name="name">the name of the value</param>
        /// <returns>the value</returns>
        public TValue Get(string name)
        {
            int id = FindEntry(name, out Entry ent);
            return _values[id];
        }

        /// <summary>
        /// Get a value by ID
        /// </summary>
        /// <param name="id">the ID of the value</param>
        /// <returns>the value</returns>
        public TValue Get(int id)
        {
            return _values[id];  
        }

        /// <summary>
        /// Does this registry contain a value with the given name
        /// </summary>
        /// <param name="name">the name</param>
        /// <returns>if the name was found</returns>
        public bool ContainsName(string name)
        {
            return FindEntry(name, out Entry ent) != -1;
        }

        /// <summary>
        /// Does this registry contain this value
        /// </summary>
        /// <param name="value">the value</param>
        /// <returns>if the value was found</returns>
        public bool Contains(TValue value)
        {
            return FindEntry(value, out Entry ent) != -1;
        }

        /// <summary>
        /// Get the ID associated with a name
        /// </summary>
        /// <param name="name">the name</param>
        /// <returns>the ID associated with the name or -1 if not found</returns>
        public int GetID(string name)
        {
            return FindEntry(name, out Entry ent);
        }

        /// <summary>
        /// Get the ID associated with a value
        /// </summary>
        /// <param name="value">the value</param>
        /// <returns>the ID associated with the value or -1 if not found</returns>
        public int GetID(TValue value)
        {
            return FindEntry(value, out Entry ent);
        }
        
        /// <summary>
        /// Does this registry contain a value with the given ID
        /// </summary>
        /// <param name="id">the ID</param>
        /// <returns>Whether a value is associated with the given ID</returns>
        public bool ContainsID(int id)
        {
            Entry ent = _entries[id];
            return ent.ID != -1;
        }

        /// <summary>
        /// Remove the value at the given name and return the value
        /// </summary>
        /// <param name="name">the name of the value to remove</param>
        /// <returns>the value of the given name</returns>
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

        /// <summary>
        /// Remove the value at the given ID and return the value
        /// </summary>
        /// <param name="id">the ID of the value to remove</param>
        /// <returns>the value associated with the given ID</returns>
        public TValue PopAt(int id)
        {
            TValue val = _values[id];
            Entry ent = _entries[id];
            Remove(ref ent);
            return val;
        }

        /// <summary>
        /// Find the given value and remove it from the registry
        /// </summary>
        /// <param name="value">value to remove</param>
        /// <returns>value found in the registry</returns>
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

        /// <summary>
        /// Remove a value with the given name
        /// </summary>
        /// <param name="name">name of the value to remove</param>
        public void RemoveAt(string name)
        {
            if (FindEntry(name, out Entry ent) != -1)
                Remove(ref ent);
        }

        /// <summary>
        /// Remove a value associated with the given ID
        /// </summary>
        /// <param name="id">the ID of the value to remove</param>
        public void RemoveAt(int id)
        {
            Entry ent = _entries[id];
            Remove(ref ent);
        }

        /// <summary>
        /// Remove the given value from the registry
        /// </summary>
        /// <param name="value">the value to remove</param>
        public void Remove(TValue value)
        {
            if (FindEntry(value, out Entry ent) != -1)
                Remove(ref ent);
        }

        /// <summary>
        /// Remove a given entry
        /// </summary>
        /// <param name="ent">the entry to remove</param>
        private void Remove(ref Entry ent)
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

        /// <summary>
        /// Find an entry with a given name
        /// </summary>
        /// <param name="name">the name of the entry</param>
        /// <param name="entry">the returned entry</param>
        /// <returns>the ID of the entry</returns>
        private int FindEntry(string name, out Entry entry)
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

        /// <summary>
        /// Find an entry of the given value
        /// </summary>
        /// <param name="value">the value to find</param>
        /// <param name="entry">the returned entry</param>
        /// <returns>the ID of the entry</returns>
        private int FindEntry(TValue value, out Entry entry)
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

        /// <summary>
        /// Expands the registry to the new size
        /// </summary>
        /// <param name="newSize">new size of the registry</param>
        private void Expand(int newSize)
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

        /// <summary>
        /// Clears the registry data
        /// </summary>
        public void Clear()
        {
            _start = _end = -1;
            _top = _count = _next = 0;
            _freeIDs.Clear();
            for (int i = 0; i < _hashSize; i++)
                _starts[i] = _ends[i] = -1;
        }

        /// <summary>
        /// Gets the hash index of a given hashcode
        /// </summary>
        /// <param name="hashCode">the hashcode of an entry</param>
        /// <returns>the hash index of the entry</returns>
        private int GetHashIndex(int hashCode) { return ((hashCode % _hashSize) + _hashSize) % _hashSize; }

        /// <inheritdoc/>
        public IEnumerator<TValue> GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        /// <summary>
        /// An individual entry which stores information about a stored value
        /// </summary>
        private struct Entry
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
            public readonly string Name;
            public int NextLink;
            public int PreviousLink;
            public int NextEnumerator;
            public int PreviousEnumerator;
        }

        /// <summary>
        /// The enumerator for <see cref="NamedDataRegistry{TValue}"/>
        /// </summary>
        private class Enumerator : IEnumerator<TValue>
        {
            private NamedDataRegistry<TValue> _reg;
            public Enumerator(NamedDataRegistry<TValue> reg)
            {
                _reg = reg;
                _next = -2;
            }

            private int _current, _next;
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
