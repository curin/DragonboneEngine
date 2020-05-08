using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones.Collections.Paged
{
    /// <summary>
    /// A Complete Binary Tree which is also a Binary Search Tree. This tree is fast for lookup, and decently fast for adding.
    /// This tree is slow for walking when the tree becomes large (>4,000 entries)
    /// It will walk in order from smallest to largest entry
    /// </summary>
    /// <typeparam name="TValue">the value stored in the tree</typeparam>
    public class BufferedBinarySearchTree<TValue> : IEnumerable<TValue>, IDataBuffer
    {
        DataBuffer<Entry> _entries;
        int _top;
        int _topContinuous;
        int _topLayer;
        int _topLayerIndex;
        ValueBuffer<int> _count;
        
        /// <summary>
        /// Constructs a CompleteBinarySearchTree
        /// </summary>
        /// <param name="pagePower">the size of the pages in the Array expressed as a power of 2</param>
        /// <param name="initialPageCount">the initial number of pages to be added</param>
        public BufferedBinarySearchTree(int pagePower, int initialPageCount)
        {
            _entries = new DataBuffer<Entry>(pagePower, initialPageCount);
            _entries[BufferTransactionType.WriteRead, 0] = new Entry(-1, default, int.MinValue);
            _entries.SwapWriteBuffer();
            _entries.SwapReadBuffer();
            _count = new ValueBuffer<int>();
            _top = -1;
            _topContinuous = -1;
        }

        /// <summary>
        /// The number of Items stored in this binary tree
        /// </summary>
        public int Count(BufferTransactionType type) { return _count[type]; }

        /// <summary>
        /// Get a value from the tree
        /// </summary>
        /// <param name="ID">the ID of the value to find</param>
        /// <returns>the value found or default</returns>
        public TValue Get(BufferTransactionType type, int ID)
        {
            if (Find(type, ID, out Entry entry))
                return entry.Value;
            return default;
        }

        /// <summary>
        /// Attempt to get a value from the tree
        /// </summary>
        /// <param name="ID">the ID of the value to find</param>
        /// <param name="value">the value found</param>
        /// <returns>whether the ID had a value stored in the tree</returns>
        public bool TryGet(BufferTransactionType type, int ID, out TValue value)
        {
            bool ret = Find(type, ID, out Entry entry);
            value = entry.Value;
            return ret;
        }

        /// <summary>
        /// Sets a value in the tree at the given ID
        /// </summary>
        /// <param name="ID">the ID of the value</param>
        /// <param name="value">the value to place at the ID</param>
        public void Set(BufferTransactionType type, int ID, TValue value)
        {
            if (type == BufferTransactionType.ReadOnly)
                return;
            if (!Find(type, ID, out Entry ent))
            {
                Add(type, ID, value);
                return;
            }

            ent.Value = value;
            _entries.Set(type, ent.Index, ent);
            return;
        }

        /// <summary>
        /// Add a value to the tree
        /// </summary>
        /// <param name="ID">the number value used in the sort</param>
        /// <param name="value">the value to store</param>
        public void Add(BufferTransactionType type, int ID, TValue value)
        {
            if (type == BufferTransactionType.ReadOnly)
                return;
            Entry ent = new Entry(-1, value, ID);
            if (FindLeaf(type, ref ent, out int index, out int parent))
            {
                ent.Index = index;
                _entries.Set(type, index, ent);
            }

            Add(type, index, ref ent);
        }

        /// <summary>
        /// Removes a value from the tree with the specified ID
        /// </summary>
        /// <param name="ID">The ID to remove</param>
        public void Remove(BufferTransactionType type, int ID)
        {
            if (type == BufferTransactionType.ReadOnly)
                return;
            if (!Find(type, ID, out Entry ent))
                return;
            while (true)
            {
                if (ent.Index >= _topLayerIndex)
                {
                    ID = ent.Index;
                    if (_top == ent.Index)
                    {
                        if (_topContinuous == _top)
                            _top--;
                        while (_entries.Get(type, ID - 1).Index == ID - 1)
                            ID--;
                        _top = ID;
                    }
                    if (_topContinuous >= ent.Index)
                        _topContinuous = ent.Index - 1;

                    ent.Index = -1;
                    _entries[type, ID] = ent;
                    _count[type]--;
                    return;
                }
                int count = 1 << _topLayer - 1;
                int walk = GetWalkIndex(ent.Index);
                int diff;
                Entry temp;
                for (diff = 1; diff < count; diff += 2)
                {
                    ID = GetIndex(walk - diff);
                    temp = _entries[type, ID];
                    if (temp.Index == ID)
                        break;

                    ID = GetIndex(walk + diff);
                    temp = _entries[type, ID];
                    if (temp.Index == ID)
                        break;
                }

                if (diff >= count)
                {
                    _topLayer--;
                    _topLayerIndex = GetLayerIndex(_topLayer);
                    continue;
                }

                int index = -1;
                if (diff > 0)
                {
                    while (index != ID)
                    {
                        walk++;
                        index = GetIndex(walk);

                        temp = _entries[type, index];
                        temp.Index = ent.Index;
                        _entries[type, index] = temp;
                        ent.Index = index;
                    }
                }
                else
                {
                    while (index != ID)
                    {
                        walk--;
                        index = GetIndex(walk);

                        temp = _entries[type, index];
                        temp.Index = ent.Index;
                        _entries[type, index] = temp;
                        ent.Index = index;
                    }
                }

                ID = ent.Index;
                if (_top == ent.Index)
                {
                    if (_topContinuous == _top)
                        _top--;
                    while (_entries.Get(type, ID - 1).Index == ID - 1)
                        ID--;
                    _top = ID;
                }
                if (_topContinuous >= ent.Index)
                    _topContinuous = ent.Index - 1;

                ent.Index = -1;
                _entries[type, ID] = ent;
                _count[type]--;
                return;
            }
        }

        /// <summary>
        /// Clears the tree
        /// </summary>
        public void Clear(BufferTransactionType type)
        {
            if (type == BufferTransactionType.ReadOnly)
                return;
            Entry ent = new Entry(-1, default, 0);
            for (int i = 0; i < _top; i++)
                _entries[type, i] = ent;
            _count[type] = _topLayer = _topLayerIndex = 0;
            _top = -1;
            _topContinuous = -1;
        }

        private void Add(BufferTransactionType type, int location, ref Entry add)
        {
            Entry ent = _entries.Get(type, location);
            if (ent.Index != location)
            {
                add.Index = location;
                _entries.Set(type, location, add);

                if (location > _top)
                    _top = location;
                if (location == _topContinuous + 1)
                {
                    while (_entries.Get(type, location + 1).Index == location + 1)
                        location++;
                    _topContinuous = location;
                    if ((location = GetLayerIndex(_topLayer + 1)) - 1 == _topContinuous)
                    {
                        _topLayer++;
                        _topLayerIndex = location;
                    }
                }
                _count[type]++;
                return;
            }

            int dist = _topLayerIndex;
            int free = 0;
            int max = GetLayerIndex(_topLayer + 1);
            Entry tempEnt;
            for (int i = _topContinuous + 1; i < max;i++)
            {
                int nDist = Math.Abs(i - location);
                if (nDist > dist)
                    break;

                tempEnt = _entries.Get(type, i);

                if (tempEnt.Index != i)
                {
                    dist = nDist;
                    free = i;
                }
            }

            if (free < location)
            {
                if (ent < add)
                {
                    add.Index = location;
                    _entries.Set(type, location, add);
                    add = ent;
                }

                int walkIndex = GetWalkIndex(location);
                int freeWalk = GetWalkIndex(free);
                while (freeWalk < walkIndex)
                {
                    walkIndex--;
                    location = GetIndex(walkIndex);

                    ent = _entries.Get(type, location);

                    add.Index = location;
                    _entries.Set(type, location, add);
                    add = ent;
                }
            }
            else if (free > location)
            {
                if (ent > add)
                {
                    _entries.Set(type, location, add);
                    add = ent;
                }

                int walkIndex = GetWalkIndex(location);
                int freeWalk = GetWalkIndex(free);
                while (freeWalk > walkIndex)
                {
                    walkIndex++;
                    location = GetIndex(walkIndex);

                    ent = _entries.Get(type, location);

                    _entries.Set(type, location, add);
                    add = ent;
                }
            }

            if (location > _top)
                _top = location;
            if (location == _topContinuous + 1)
            {
                while (_entries.Get(type, location + 1).Index == location + 1)
                    location++;
                _topContinuous = location;
                if ((location = GetLayerIndex(_topLayer + 1)) - 1 == _topContinuous)
                {
                    _topLayer++;
                    _topLayerIndex = location;
                }
            }
            _count[type]++;
        }

        private int GetLayerIndex(int layer)
        {
            return (1 << layer) - 1;
        }

        private int GetLayer(int index)
        {
            return (int)Math.Log(index + 1, 2);
        }

        private int GetChild(int parent, ChildType childType)
        {
            return (parent << 1) + (int)childType;
        }

        private int GetChild(int parent, int childType)
        {
            return (parent << 1) + childType;
        }

        private int GetParent(int child)
        {
            return (child - 1) >> 1;
        }

        private int Next(int index)
        {
            return GetIndex(GetWalkIndex(index) + 1);
        }

        private int Previous(int index)
        {
            return GetIndex(GetWalkIndex(index) - 1);
        }

        private int GetIndex(int walkIndex)
        {
            int next = MathHelper.BitScanForward(~walkIndex) + 1;
            return ((1 << (_topLayer + 1 - next)) - 1 + (walkIndex - ((1 << (next - 1)) - 1) >> (next)));
        }

        private int GetWalkIndex(int index)
        {
            int layer = GetLayer(index);
            return (1 << (_topLayer - layer)) - 1 + ((index - ((1 << (layer)) - 1)) * (2 << (_topLayer - layer)));
        }

        private bool FindLeaf(BufferTransactionType type, ref Entry value, out int index, out int parent)
        {
            Entry ent;
            parent = 0;
            index = 0;
            while (index < _topLayerIndex)
            {
                ent = _entries.Get(type, index);

                if (value == ent)
                    return true;
                parent = index;

                index = GetChild(index, value < ent ? 1 : 2);
            }

            ent = _entries.Get(type, index);

            return value == ent;
        }

        private bool Find(BufferTransactionType type, int value, out Entry ent)
        {
            ent = _entries.Get(type, 0);
            while (ent.Index < _topLayerIndex)
            {
                if (value == ent)
                    return true;

                ent = _entries.Get(type, GetChild(ent.Index, value < ent ? 1 : 2));
            }
            return value == ent;
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public void SwapReadBuffer()
        {
            _entries.SwapReadBuffer();
            _count.SwapReadBuffer();
        }

        public void SwapWriteBuffer()
        {
            _entries.SwapWriteBuffer();
            _count.SwapWriteBuffer();
        }

        class Enumerator : IEnumerator<TValue>
        {
            BufferedBinarySearchTree<TValue> _tree;
            int _walkIndex;
            int index;
            Entry ent;

            public Enumerator(BufferedBinarySearchTree<TValue> tree)
            {
                _tree = tree;
                _walkIndex = -1;
            }

            public TValue Current => ent.Value;

            object IEnumerator.Current => ent.Value;

            public void Dispose()
            {
                _tree = null;
            }

            public bool MoveNext()
            {
                do
                {
                    _walkIndex++;
                    index = _tree.GetIndex(_walkIndex);

                    if (index > _tree._top)
                        return false;

                    ent = _tree._entries.Get(BufferTransactionType.ReadOnly, index);
                } while (ent.Index != index);
                return true;
            }

            public void Reset()
            {
                _walkIndex = -1;
            }
        }

        struct Entry
        {
            public Entry(int index, TValue value, int number)
            {
                Index = index;
                Value = value;
                NumericValue = number;
            }

            public int Index;
            public TValue Value;
            public int NumericValue;

            public static bool operator <(Entry lh, Entry rh)
            {
                return lh.NumericValue < rh.NumericValue;
            }

            public static bool operator >(Entry lh, Entry rh)
            {
                return lh.NumericValue > rh.NumericValue;
            }

            public static bool operator ==(Entry lh, Entry rh)
            {
                return lh.NumericValue == rh.NumericValue;
            }

            public static bool operator !=(Entry lh, Entry rh)
            {
                return lh.NumericValue != rh.NumericValue;
            }

            public static bool operator <(int lh, Entry rh)
            {
                return lh < rh.NumericValue;
            }

            public static bool operator >(int lh, Entry rh)
            {
                return lh > rh.NumericValue;
            }

            public static bool operator ==(int lh, Entry rh)
            {
                return lh == rh.NumericValue;
            }

            public static bool operator !=(int lh, Entry rh)
            {
                return lh != rh.NumericValue;
            }

            public static bool operator <(Entry lh, int rh)
            {
                return lh.NumericValue < rh;
            }

            public static bool operator >(Entry lh, int rh)
            {
                return lh.NumericValue > rh;
            }

            public static bool operator ==(Entry lh, int rh)
            {
                return lh.NumericValue == rh;
            }

            public static bool operator !=(Entry lh, int rh)
            {
                return lh.NumericValue != rh;
            }
        }

        enum ChildType
        {
            Left = 1,
            Right = 2
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Dispose this object
        /// </summary>
        /// <param name="disposing">should we be disposing managed objects?</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _entries?.Dispose();
                    _count?.Dispose();
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
