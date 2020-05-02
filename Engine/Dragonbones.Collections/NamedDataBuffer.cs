using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Dragonbones.Collections
{
    /// <summary>
    /// A Data buffer which takes in a name for every element
    /// and gives back an index rather than needing an index to be assigned ahead of time
    /// Names must be unique between values if not the values will be overwritten at write.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class NamedDataBuffer<TValue>: IDataBuffer,  IEnumerable<TValue>
    {
        private DoubleDataBuffer<TValue, Entry> _buffer;
        private readonly DataBuffer<int> _hashStarts;
        private readonly ValueBuffer<int> _count = new ValueBuffer<int>();
        private readonly ValueBuffer<int> _start = new ValueBuffer<int>(-1);

        private int[] _hashEnds;
        private int _capacity;
        private readonly int _hashSize;
        private int _end = -1;
        private int _next;
        private int _top;
        private Queue<int> _freeIDs = new Queue<int>();

        /// <summary>
        /// Constructs a NamedDataBuffer with the initial capacity
        /// </summary>
        /// <param name="initialCapacity">the starting size of the buffer</param>
        /// <param name="hashSize">the size of the hashtable in the buffer</param>
        public NamedDataBuffer(int initialCapacity, int hashSize = 47)
        {
            _buffer = new DoubleDataBuffer<TValue, Entry>(initialCapacity);
            _hashStarts = new DataBuffer<int>(hashSize, -1);
            _hashEnds = new int[hashSize];
            _hashSize = hashSize;
            _capacity = initialCapacity;

            _buffer.Set(BufferTransactionType.WriteRead, 0, default, new Entry() {ID = -1});
            _buffer.SwapWriteBuffer();
            _buffer.SwapReadBuffer();
        }

        /// <summary>
        /// Accesses a specified value at a given ID for a given transaction type
        /// (Set operations do not work for ReadOnly transactions)
        /// </summary>
        /// <param name="type">the type of transaction</param>
        /// <param name="id">the ID associated with the value</param>
        /// <returns>the value associated with the ID</returns>
        public TValue this[BufferTransactionType type, int id]
        {
            get => Get(type, id);
            set => Set(type, id, value);
        }

        /// <summary>
        /// Accesses a specified value at a given name for a given transaction type
        /// (Set operations do not work for ReadOnly transactions)
        /// </summary>
        /// <param name="type">the type of transaction</param>
        /// <param name="name">the name associated with the value</param>
        /// <returns>the value associated with the name</returns>
        public TValue this[BufferTransactionType type, string name]
        {
            get => Get(type, name);
            set => Set(type, name, value);
        }

        /// <summary>
        /// Returns the count of the buffer for the transaction type
        /// </summary>
        /// <param name="type">the transaction type which determines which part of the buffer the information is from</param>
        /// <returns>the count of values stored in the buffer</returns>
        public int Count(BufferTransactionType type)
        {
            return _count[type];
        }

        /// <summary>
        /// Add a value to the buffer
        /// (only works in WriteRead transaction types)
        /// </summary>
        /// <param name="type">the transaction type</param>
        /// <param name="name">the name of the value</param>
        /// <param name="value">the value</param>
        /// <returns>the ID associated with the name</returns>
        public int Add(BufferTransactionType type, string name, TValue value)
        {
            if (type == BufferTransactionType.ReadOnly)
                return -1;
            int find = FindEntry(type, name, out Entry _);
            if (find != -1)
            {
                _buffer.SetPrimary(type, find, value);
                return find;
            }

            if (_freeIDs.Count > 0)
                _next = _freeIDs.Dequeue();
            if (_next == _top && _top == _capacity)
                Expand(_capacity << 1);

            int hash = GetHashIndex(name.GetHashCode());
            int end = _hashEnds[hash];
            Entry ent = new Entry(_next, name, end, _end);

            _buffer.Set(type, _next, value, ent);

            if (end == -1)
            {
                _hashStarts.Set(type, hash, _next);
                _hashEnds[hash] = _next;

                if (_end == -1)
                    _start[BufferTransactionType.WriteRead] = _end = _next;
                else
                {
                    Entry temp = _buffer.Get(type, _end).Item2;
                    temp.NextIterator = _next;
                    _buffer.SetSecondary(type, _end, temp);
                    _end = _next;
                }
            }
            else
            {
                Entry temp = _buffer.Get(type, end).Item2;
                temp.Next = _next;
                _buffer.SetSecondary(type, end, temp);
                if (_end == -1)
                    _start[BufferTransactionType.WriteRead] = _end = _next;
                else
                {
                    temp = _buffer.Get(type, _end).Item2;
                    temp.NextIterator = _next;
                    _buffer.SetSecondary(type, _end, temp);
                    _end = _next;
                }
            }
            if (_next == _top)
                _top++;
            _next = _top;
            _count[BufferTransactionType.WriteRead]++;
            return ent.ID;
        }

        /// <summary>
        /// Attempt to get a value by name
        /// </summary>
        /// <param name="type">The type of transaction this call represents, changes what buffer the data is pulled from</param>
        /// <param name="name">the name of the value</param>
        /// <param name="value">the returned value</param>
        /// <returns>whether the name was found</returns>
        public bool TryGet(BufferTransactionType type, string name, out TValue value)
        {
            int id = FindEntry(type, name, out Tuple<TValue, Entry> valueEntry);
            if (id == -1)
            {
                value = default;
                return false;
            }

            value = valueEntry.Item1;
            return true;
        }

        /// <summary>
        /// Attempt to get a value by ID
        /// </summary>
        /// <param name="type">The type of transaction this call represents, changes what buffer the data is pulled from</param>
        /// <param name="id">the ID of the value</param>
        /// <param name="value">the returned value</param>
        /// <returns>Whether the ID was found</returns>
        public bool TryGet(BufferTransactionType type, int id, out TValue value)
        {
            if (id < 0 || id > _capacity)
            {
                value = default;
                return false;
            }

            Tuple<TValue, Entry> valueEntry = _buffer[type, id];
            if (valueEntry.Item2.ID != id)
            {
                value = default;
                return false;
            }

            value = valueEntry.Item1;
            return true;
        }

        /// <summary>
        /// Retrieve a value by name
        /// </summary>
        /// <param name="type">the type of transaction being made, this affects what buffer the data comes from</param>
        /// <param name="name">the name of the value</param>
        /// <returns>the value associated with the name</returns>
        public TValue Get(BufferTransactionType type, string name)
        {
            FindEntry(type, name, out Tuple<TValue, Entry> valueEntry);
            return valueEntry.Item1;
        }

        /// <summary>
        /// Retrieve a value by ID
        /// </summary>
        /// <param name="type">the type of transaction being made, this affects what buffer the data comes from</param>
        /// <param name="id">the ID associated with the value</param>
        /// <returns>the value associated with the ID</returns>
        public TValue Get(BufferTransactionType type, int id)
        {
            return _buffer.Get(type, id).Item1;
        }

        /// <summary>
        /// Sets the value in the buffer associated with the given id
        /// (only works in WriteRead transaction types)
        /// </summary>
        /// <param name="type">The transaction type</param>
        /// <param name="id">the id associated with the value to replace</param>
        /// <param name="value">the new value to set</param>
        public void Set(BufferTransactionType type, int id, TValue value)
        {
            if (id < 0 || id > _capacity)
                throw new ArgumentOutOfRangeException(nameof(id));

            Entry entry = _buffer.GetSecondary(type, id);
            if (entry.ID != id)
                throw new ArgumentException("There is no value in the NamedDataBuffer associated with ID " + id);

            _buffer.SetPrimary(type, id, value);
        }

        /// <summary>
        /// Sets the value in the buffer associated with the given name
        /// (only works in WriteRead transaction types)
        /// If a value is not found, the value is added
        /// </summary>
        /// <param name="type">the transaction type</param>
        /// <param name="name">the name associated with the value to replace</param>
        /// <param name="value">the new value</param>
        public void Set(BufferTransactionType type, string name, TValue value)
        {
            if (FindEntry(type, name, out Tuple<TValue, Entry> valueEntry) == -1)
                Add(type, name, value);

            _buffer.SetPrimary(type, valueEntry.Item2.ID, value);
        }

        /// <summary>
        /// Does this buffer contain a value with the associated ID
        /// </summary>
        /// <param name="type">the transaction type which determines where the information is pulled from</param>
        /// <param name="id">the ID associated with the value</param>
        /// <returns>Whether the value associated with the ID is within the buffer</returns>
        public bool ContainsID(BufferTransactionType type, int id)
        {
            if (id < 0 || id > _capacity)
                return false;

            Entry ent = _buffer.GetSecondary(type, id);
            return ent.ID == id;
        }

        /// <summary>
        /// Does this buffer contain a value with the associated name
        /// </summary>
        /// <param name="type">the transaction type which determines where the information is pulled from</param>
        /// <param name="name">the name associated with the value</param>
        /// <returns>Whether the value associated with the name is within the buffer</returns>
        public bool ContainsName(BufferTransactionType type, string name)
        {
            return FindEntry(type, name, out Entry _) != -1;
        }

        /// <summary>
        /// Does this buffer contain a value
        /// </summary>
        /// <param name="type">the transaction type which determines where the information is pulled from</param>
        /// <param name="value">the value to check</param>
        /// <returns>Whether the value is within the buffer</returns>
        public bool Contains(BufferTransactionType type, TValue value)
        {
            return FindEntry(type, value, out _) != -1;
        }

        /// <summary>
        /// Retrieve the ID associated with the given name
        /// </summary>
        /// <param name="type">the transaction type which determines where the information is pulled from</param>
        /// <param name="name">the name associated with the ID</param>
        /// <returns>The ID associated the given name or -1 if not found</returns>
        public int GetIDFromName(BufferTransactionType type, string name)
        {
            return FindEntry(type, name, out Entry _);
        }

        /// <summary>
        /// Retrieve the ID associated with the given value
        /// </summary>
        /// <param name="type">the transaction type which determines where the information is pulled from</param>
        /// <param name="value">the value associated with the ID</param>
        /// <returns>The ID associated the given value or -1 if not found</returns>
        public int GetID(BufferTransactionType type, TValue value)
        {
            return FindEntry(type, value, out _);
        }

        /// <summary>
        /// Gets the name associated with the given ID
        /// </summary>
        /// <param name="type">the transaction type which determines where the information is pulled from</param>
        /// <param name="id">the id associated with the name</param>
        /// <returns>the name associated with the id</returns>
        public string GetNameFromID(BufferTransactionType type, int id)
        {
            if (id < 0 || id > _capacity)
                throw new ArgumentOutOfRangeException(nameof(id));

            Entry ent = _buffer.GetSecondary(type, id);
            if (ent.ID == id)
                return ent.Name;
            throw new ArgumentException("NamedDataBuffer does not contain a value associated with ID " + id);
        }

        /// <summary>
        /// Gets the name associated with a value
        /// </summary>
        /// <param name="type">the transaction type which determines where the information is pulled from</param>
        /// <param name="value">the value</param>
        /// <returns>the name associated with the value</returns>
        public string GetName(BufferTransactionType type, TValue value)
        {
            if (FindEntry(type, value, out Tuple<TValue, Entry> valueEntry) != -1)
                return valueEntry.Item2.Name;
            throw new ArgumentException("NamedDataBuffer does not contain the value " + value.ToString());
        }

        /// <summary>
        /// Remove a value associated with the given name from the buffer
        /// </summary>
        /// <param name="type">the transaction type, Readonly transactions cannot remove values</param>
        /// <param name="name">the name associated with the value to remove</param>
        public void RemoveAt(BufferTransactionType type, string name)
        {
            if (type == BufferTransactionType.ReadOnly) return;

            if (FindEntry(type, name, out Entry ent) != -1)
                Remove(type, ref ent);
        }

        /// <summary>
        /// Remove a value associated with the given ID from the buffer
        /// </summary>
        /// <param name="type">the transaction type, Readonly transactions cannot remove values</param>
        /// <param name="id">the ID associated with the value to remove</param>
        public void RemoveAt(BufferTransactionType type, int id)
        {
            if (type == BufferTransactionType.ReadOnly) return;

            Entry ent = _buffer.GetSecondary(type, id);
            if (ent.ID == id)
                Remove(type, ref ent);
        }

        /// <summary>
        /// Remove a value from the buffer
        /// </summary>
        /// <param name="type">the transaction type, Readonly transactions cannot remove values</param>
        /// <param name="value">the value to remove</param>
        public void Remove(BufferTransactionType type, TValue value)
        {
            if (type == BufferTransactionType.ReadOnly) return;
            if (FindEntry(type, value, out Tuple<TValue, Entry> valueEntry) == -1) return;
            Entry entry = valueEntry.Item2;
            Remove(type, ref entry);
        }

        /// <summary>
        /// Removes an entry from the buffer
        /// </summary>
        /// <param name="type">the transaction type, Readonly transactions cannot remove values</param>
        /// <param name="ent">the entry to remove</param>
        private void Remove(BufferTransactionType type, ref Entry ent)
        {
            if (type == BufferTransactionType.ReadOnly) return;

            Entry temp;
            if (ent.NextIterator != -1)
            {
                temp = _buffer.GetSecondary(type, ent.NextIterator);
                temp.PreviousIterator = ent.PreviousIterator;
                _buffer.SetSecondary(type, ent.NextIterator, temp);
            }
            else
            {
                _end = ent.PreviousIterator;
            }

            if (ent.PreviousIterator != -1)
            {
                temp = _buffer.GetSecondary(type, ent.PreviousIterator);
                temp.NextIterator = ent.NextIterator;
                _buffer.SetSecondary(type, ent.PreviousIterator, temp);
            }
            else
            {
                _start[BufferTransactionType.WriteRead] = ent.NextIterator;
            }

            int loc = GetHashIndex(ent.Name.GetHashCode());
            if (ent.Next != -1)
            {
                temp = _buffer.GetSecondary(type, ent.Next);
                temp.Previous = ent.Previous;
                _buffer.SetSecondary(type, ent.Next, temp);
            }
            else
            {
                _hashStarts.Set(type, loc, ent.Next);
            }

            if (ent.Previous != -1)
            {
                temp = _buffer.GetSecondary(type, ent.Previous);
                temp.Next = ent.Next;
                _buffer.SetSecondary(type, ent.Previous, temp);
            }
            else
            {
                _hashEnds[loc] = ent.Previous;
            }

            if (ent.ID == _top - 1)
                _top--;
            else
                _freeIDs.Enqueue(ent.ID);

            _count[BufferTransactionType.WriteRead]--;

            int id = ent.ID;
            ent.ID = -1;
            _buffer.SetSecondary(type, id, ent);
        }

        /// <summary>
        /// Retrieves a value then removes it from the buffer
        /// </summary>
        /// <param name="type">the transaction type, Readonly transactions cannot pop</param>
        /// <param name="id">the ID of the value to pop</param>
        /// <returns>the value removed from the buffer</returns>
        public TValue PopAt(BufferTransactionType type, int id)
        {
            if (type == BufferTransactionType.ReadOnly) return default;

            (TValue value, Entry ent) = _buffer.Get(type, id);
            if (ent.ID != id)
                throw new ArgumentException("The NamedDataBuffer does not contain a value associated with the id " + id);
            Remove(type, ref ent);
            return value;
        }

        /// <summary>
        /// Retrieves a value then removes it from the buffer
        /// </summary>
        /// <param name="type">the transaction type, Readonly transactions cannot pop</param>
        /// <param name="name">the name of the value to pop</param>
        /// <returns>the value removed from the buffer</returns>
        public TValue PopAt(BufferTransactionType type, string name)
        {
            if (type == BufferTransactionType.ReadOnly) return default;

            if (FindEntry(type, name, out Tuple<TValue, Entry> valueEntry) == -1)
                throw new ArgumentException("The NamedDataBuffer does not contain a value associated with the name " + name);
            Entry ent = valueEntry.Item2;
            Remove(type, ref ent);
            return valueEntry.Item1;
        }

        /// <summary>
        /// Retrieves a value then removes it from the buffer
        /// </summary>
        /// <param name="type">the transaction type, Readonly transactions cannot pop</param>
        /// <param name="value">the value to pop</param>
        /// <returns>the current value removed from the buffer</returns>
        public TValue Pop(BufferTransactionType type, TValue value)
        {
            if (type == BufferTransactionType.ReadOnly) return default;
            if (FindEntry(type, value, out Tuple<TValue, Entry> valueEntry) == -1) throw new ArgumentException("The NamedDataBuffer does not contain the value " + value.ToString());
            Entry entry = valueEntry.Item2;
            Remove(type, ref entry);
            return valueEntry.Item1;
        }

        /// <summary>
        /// Attempts to Retrieve a value then removes it from the buffer
        /// </summary>
        /// <param name="type">the transaction type, Readonly transactions cannot pop</param>
        /// <param name="id">the ID of the value to pop</param>
        /// <param name="value">the value removed</param>
        /// <returns>Whether the pop was successful</returns>
        public bool TryPopAt(BufferTransactionType type, int id, out TValue value)
        {
            if (type == BufferTransactionType.ReadOnly)
            {
                value = default;
                return false;
            }

            Entry ent;
            (value, ent) = _buffer.Get(type, id);
            if (ent.ID != id)
                return false;
            Remove(type, ref ent);
            return true;
        }

        /// <summary>
        /// Attempts to Retrieve a value then removes it from the buffer
        /// </summary>
        /// <param name="type">the transaction type, Readonly transactions cannot pop</param>
        /// <param name="name">the name of the value to pop</param>
        /// <param name="value">the value removed</param>
        /// <returns>Whether the pop was successful</returns>
        public bool TryPopAt(BufferTransactionType type, string name, out TValue value)
        {
            if (type == BufferTransactionType.ReadOnly)
            {
                value = default;
                return false;
            }

            if (FindEntry(type, name, out Tuple<TValue, Entry> valueEntry) == -1)
            {
                value = default;
                return false;
            }

            Entry ent = valueEntry.Item2;
            Remove(type, ref ent);
            value = valueEntry.Item1;
            return true;
        }

        /// <summary>
        /// Attempts to Retrieve a value then removes it from the buffer
        /// </summary>
        /// <param name="type">the transaction type, Readonly transactions cannot pop</param>
        /// <param name="value">the value to removed</param>
        /// <param name="newValue">the current value that was removed</param>
        /// <returns>Whether the pop was successful</returns>
        public bool TryPop(BufferTransactionType type, TValue value, out TValue newValue)
        {
            if (type == BufferTransactionType.ReadOnly)
            {
                newValue = default;
                return false;
            }

            if (FindEntry(type, value, out Tuple<TValue, Entry> valueEntry) == -1)
            {
                newValue = default;
                return false;
            }

            Entry entry = valueEntry.Item2;
            Remove(type, ref entry);
            newValue = valueEntry.Item1;
            return true;
        }

        /// <summary>
        /// Clears the Buffer
        /// </summary>
        /// <param name="type">the transaction type, Readonly transactions cannot clear</param>
        public void Clear(BufferTransactionType type)
        {
            if (type == BufferTransactionType.ReadOnly)
                return;
            _start[BufferTransactionType.WriteRead] = _end = -1;
             _count[BufferTransactionType.WriteRead] = _top = _next = 0;
            _freeIDs.Clear();
            for (int i = 0; i < _hashSize; i++)
            {
                _hashStarts.Set(type, i, -1);
                _hashEnds[i] = -1;
            }
        }

        /// <summary>
        /// Find an entry of the given name
        /// </summary>
        /// <param name="type">the transaction type, which determines where the data comes from</param>
        /// <param name="name">the name to find</param>
        /// <param name="valueEntry">the returned entry and value pair</param>
        /// <returns>the ID of the entry</returns>
        private int FindEntry(BufferTransactionType type, string name, out Tuple<TValue, Entry> valueEntry)
        {
            int loc = GetHashIndex(name.GetHashCode());
            if (_hashStarts.Get(type, loc) == -1)
            {
                valueEntry = new Tuple<TValue, Entry>(default, default);
                return -1;
            }

            valueEntry = _buffer.Get(type, _hashStarts.Get(type, loc));

            while (valueEntry.Item2.Next != -1 && valueEntry.Item2.Name != name)
                valueEntry = _buffer.Get(type, valueEntry.Item2.Next);

            if (valueEntry.Item2.Name != name)
                return -1;

            return valueEntry.Item2.ID;
        }

        /// <summary>
        /// Find an entry of the given name
        /// </summary>
        /// <param name="type">the transaction type, which determines where the data comes from</param>
        /// <param name="name">the name to find</param>
        /// <param name="entry">the returned entry</param>
        /// <returns>the ID of the entry</returns>
        private int FindEntry(BufferTransactionType type, string name, out Entry entry)
        {
            int loc = GetHashIndex(name.GetHashCode());
            if (_hashStarts.Get(type, loc) == -1)
            {
                entry = default;
                return -1;
            }

            entry = _buffer.GetSecondary(type, _hashStarts.Get(type, loc));

            while (entry.Next != -1 && entry.Name != name)
                entry = _buffer.GetSecondary(type, entry.Next);

            if (entry.Name != name)
                return -1;

            return entry.ID;
        }

        /// <summary>
        /// Find an entry of the given value
        /// </summary>
        /// <param name="type">the transaction type which determines where the data comes from</param>
        /// <param name="value">the value to find</param>
        /// <param name="valueEntry">the returned entry and value pair</param>
        /// <returns>the ID of the entry</returns>
        private int FindEntry(BufferTransactionType type, TValue value, out Tuple<TValue, Entry> valueEntry)
        {
            if (_start[type] == -1)
            {
                valueEntry = new Tuple<TValue, Entry>(default, default);
                return -1;
            }

            valueEntry = _buffer[type, _start[type]];

            while (valueEntry.Item1.Equals(value) && valueEntry.Item2.NextIterator != -1)
                valueEntry = _buffer[type, valueEntry.Item2.NextIterator];

            if (valueEntry.Item1.Equals(value))
                return valueEntry.Item2.ID;
            return -1;
        }

        /// <summary>
        /// Constricts the size of the buffer to as small as possible
        /// </summary>
        /// <param name="newCapacity">the target capacity, if the buffer will shrink as close to this as possible non-destructively.</param>
        public void Constrict(int newCapacity)
        {
            while (_freeIDs.Contains(_top - 1))
                _top--;

            for (int i = 0; i < _freeIDs.Count; i++)
            {
                int val = _freeIDs.Dequeue();
                if (val < _top)
                    _freeIDs.Enqueue(val);
            }

            if (newCapacity < _top)
                newCapacity = _top;

            _buffer = new DoubleDataBuffer<TValue, Entry>(_buffer, newCapacity);
            _capacity = newCapacity;
        }

        /// <summary>
        /// Expands the size of the buffer
        /// </summary>
        /// <param name="newCapacity">the new capacity of the buffer</param>
        public void Expand(int newCapacity)
        {
            _buffer = new DoubleDataBuffer<TValue, Entry>(_buffer, newCapacity);
            _capacity = newCapacity;
        }

        /// <summary>
        /// Get the hash index of a particular hashcode
        /// </summary>
        /// <param name="hashCode">the hash code</param>
        /// <returns>the index in the hashtable for the hashcode to be placed in</returns>
        private int GetHashIndex(int hashCode)
        {
            return ((hashCode % _hashSize) + _hashSize) % _hashSize;
        }

        /// <summary>
        /// Swaps the data buffers for rendering
        /// </summary>
        public void SwapReadBuffer()
        {
            _buffer.SwapReadBuffer();
            _hashStarts.SwapReadBuffer();
            _count.SwapReadBuffer();
            _start.SwapReadBuffer();
        }

        /// <summary>
        /// Swaps the data buffers on finishing of updating
        /// </summary>
        public void SwapWriteBuffer()
        {
            _buffer.SwapWriteBuffer();
            _hashStarts.SwapWriteBuffer();
            _count.SwapWriteBuffer();
            _start.SwapWriteBuffer();
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<TValue> GetEnumerator()
        {
            return new Enumerator(BufferTransactionType.ReadOnly, this);
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Rename the value in the buffer associated with the given id
        /// (only works in WriteRead transaction types)
        /// </summary>
        /// <param name="type">The transaction type</param>
        /// <param name="id">the id associated with the value to replace</param>
        /// <param name="newName">the new name to set</param>
        public void Rename(BufferTransactionType type, int id, string newName)
        {

            if (id < 0 || id > _capacity)
                throw new ArgumentOutOfRangeException(nameof(id));

            NamedDataBuffer<TValue>.Entry entry = _buffer.GetSecondary(type, id);
            if (entry.ID != id)
                throw new ArgumentException("There is no value in the NamedDataBuffer associated with ID " + id);

            if (entry.Name == newName)
                return;

            int originalHash = GetHashIndex(entry.Name.GetHashCode());
            NamedDataBuffer<TValue>.Entry tempEnt;
            if (entry.Previous != -1)
            {
                tempEnt = _buffer.GetSecondary(type, entry.Previous);
                tempEnt.Next = entry.Next;
                _buffer.SetSecondary(type, entry.Previous, tempEnt);
            }
            else
            {
                _hashStarts[type, originalHash] = entry.Next;
            }

            if (entry.Next != -1)
            {
                tempEnt = _buffer.GetSecondary(type, entry.Next);
                tempEnt.Previous = entry.Previous;
                _buffer.SetSecondary(type, entry.Next, tempEnt);
            }
            else
            {
                _hashEnds[originalHash] = entry.Previous;
            }

            int hash = GetHashIndex(newName.GetHashCode());
            int end = _hashEnds[hash];

            entry.Next = -1;
            entry.Previous = end;
            entry.Name = newName;

            _buffer.SetSecondary(type, id, entry);

            tempEnt = _buffer.GetSecondary(type, end);
            tempEnt.Next = id;
            _buffer.SetSecondary(type, end, tempEnt);
        }

        /// <summary>
        /// Rename the value in the buffer associated with the given name
        /// (only works in WriteRead transaction types)
        /// If a value is not found, the value is added
        /// </summary>
        /// <param name="type">the transaction type</param>
        /// <param name="name">the name associated with the value to replace</param>
        /// <param name="newName">the new name to set</param>
        public void Rename(BufferTransactionType type, string name, string newName)
        {
            if (name == newName)
                return;

            int id = FindEntry(type, name, out NamedDataBuffer<TValue>.Entry entry);
            if (id == -1)
                throw new ArgumentException("The NamedDataBuffer does not contain the name " + name);

            int originalHash = GetHashIndex(entry.Name.GetHashCode());
            NamedDataBuffer<TValue>.Entry tempEnt;
            if (entry.Previous != -1)
            {
                tempEnt = _buffer.GetSecondary(type, entry.Previous);
                tempEnt.Next = entry.Next;
                _buffer.SetSecondary(type, entry.Previous, tempEnt);
            }
            else
            {
                _hashStarts[type, originalHash] = entry.Next;
            }

            if (entry.Next != -1)
            {
                tempEnt = _buffer.GetSecondary(type, entry.Next);
                tempEnt.Previous = entry.Previous;
                _buffer.SetSecondary(type, entry.Next, tempEnt);
            }
            else
            {
                _hashEnds[originalHash] = entry.Previous;
            }

            int hash = GetHashIndex(newName.GetHashCode());
            int end = _hashEnds[hash];

            entry.Next = -1;
            entry.Previous = end;
            entry.Name = newName;

            _buffer.SetSecondary(type, id, entry);

            tempEnt = _buffer.GetSecondary(type, end);
            tempEnt.Next = id;
            _buffer.SetSecondary(type, end, tempEnt);
        }

        /// <summary>
        /// Represents an entry in the hashtable for the named data buffer
        /// </summary>
        private struct Entry
        {
            public Entry(int id, string name, int prevLink, int prevEnum, int nextLink = -1, int nextEnum = -1)
            {
                ID = id;
                Name = name;
                Previous = prevLink;
                NextIterator = prevEnum;
                Next = nextLink;
                PreviousIterator = nextEnum;
            }

            public int ID;
            public string Name;
            public int Next;
            public int Previous;
            public int NextIterator;
            public int PreviousIterator;
        }

        /// <summary>
        /// The enumerator for <see cref="NamedDataBuffer{TValue}"/>
        /// </summary>
        private class Enumerator : IEnumerator<TValue>
        {
            private NamedDataBuffer<TValue> _buff;
            private readonly BufferTransactionType _type;
            public Enumerator(BufferTransactionType type, NamedDataBuffer<TValue> buff)
            {
                _buff = buff;
                _next = -2;
                _type = type;
            }

            private int _current, _next;
            public TValue Current => _buff._buffer[_type, _current].Item1;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _buff = null;
            }

            public bool MoveNext()
            {
                if (_next == -1)
                    return false;
                Entry ent;
                if (_next == -2)
                {
                    _current = _buff._start[_type];
                    if (_current == -1)
                        return false;
                    ent = _buff._buffer[_type, _current].Item2;
                    _next = ent.NextIterator;
                    return true;
                }

                _current = _next;
                ent = _buff._buffer[_type, _current].Item2;
                _next = ent.NextIterator;
                return true;
            }

            public void Reset()
            {
                _next = -2;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Disposes of this object
        /// </summary>
        /// <param name="disposing">Should managed objects be disposed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _buffer?.Dispose();
                    _hashStarts?.Dispose();
                    _count?.Dispose();
                    _start?.Dispose();
                    _freeIDs = null;
                    _hashEnds = null;
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
