using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones.Collections
{
    public class CompleteBinarySearchTree<TValue> : IEnumerable<TValue>
    {
        PagedArray<Entry> _entries;
        int _top;
        int _topContinuous;
        int _topLayer;
        int _topLayerIndex;
        
        /// <summary>
        /// Constructs a CompleteBinarySearchTree
        /// </summary>
        /// <param name="pagePower">the size of the pages in the Array expressed as a power of 2</param>
        /// <param name="initialPageCount">the initial number of pages to be added</param>
        public CompleteBinarySearchTree(int pagePower, int initialPageCount)
        {
            _entries = new PagedArray<Entry>(pagePower, initialPageCount);
            _entries[0] = new Entry(-1, default, int.MinValue);
            _top = -1;
            _topContinuous = -1;
            _topLayer = 0;
            _topLayerIndex = 0;
        }

        /// <summary>
        /// Get a value from the tree
        /// </summary>
        /// <param name="ID">the ID of the value to find</param>
        /// <returns>the value found or default</returns>
        public TValue Get(int ID)
        {
            if (Find(ID, out Entry entry))
                return entry.Value;
            return default;
        }

        /// <summary>
        /// Attempt to get a value from the tree
        /// </summary>
        /// <param name="ID">the ID of the value to find</param>
        /// <param name="value">the value found</param>
        /// <returns>whether the ID had a value stored in the tree</returns>
        public bool TryGet(int ID, out TValue value)
        {
            bool ret = Find(ID, out Entry entry);
            value = entry.Value;
            return ret;
        }

        /// <summary>
        /// Sets a value in the tree at the given ID
        /// </summary>
        /// <param name="ID">the ID of the value</param>
        /// <param name="value">the value to place at the ID</param>
        public void Set(int ID, TValue value)
        {
            if (!Find(ID, out Entry ent))
            {
                Add(ID, value);
                return;
            }

            ent.Value = value;
            _entries[ent.Index] = ent;
            return;
        }

        /// <summary>
        /// Add a value to the tree
        /// </summary>
        /// <param name="ID">the number value used in the sort</param>
        /// <param name="value">the value to store</param>
        public void Add(int ID, TValue value)
        {
            Entry ent = new Entry(-1, value, ID);
            if (FindLeaf(ref ent, out int index, out int parent))
            {
                ent.Index = index;
                _entries[index] = ent;
            }

            Add(index, ref ent);
        }

        private void Add(int location, ref Entry add)
        {
            Entry ent = _entries[location];
            if (ent.Index != location)
            {
                add.Index = location;
                _entries[location] = add;

                if (location > _top)
                    _top = location;
                if (location == _topContinuous + 1)
                {
                    while (_entries[location + 1].Index == location + 1)
                        location++;
                    _topContinuous = location;
                    if ((location = GetLayerIndex(_topLayer + 1)) - 1 == _topContinuous)
                    {
                        _topLayer++;
                        _topLayerIndex = location;
                    }
                }
                return;
            }

            int dist = _topLayerIndex;
            int free = 0;
            int max = GetLayerIndex(_topLayer + 1);
            Entry tempEnt;
            for (int i = _topLayerIndex; i < max;i++)
            {
                int nDist = Math.Abs(i - location);
                if (nDist > dist)
                    break;
                tempEnt = _entries[i];
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
                    _entries[location] = add;
                    add = ent;
                }

                int walkIndex = GetWalkIndex(location);
                int freeWalk = GetWalkIndex(free);
                while (freeWalk < walkIndex)
                {
                    walkIndex--;
                    location = GetIndex(walkIndex);

                    ent = _entries[location];
                    add.Index = location;
                    _entries[location] = add;
                    add = ent;
                }
            }
            else if (free > location)
            {
                if (ent > add)
                {
                    _entries[location] = add;
                    add = ent;
                }

                int walkIndex = GetWalkIndex(location);
                int freeWalk = GetWalkIndex(free);
                while (freeWalk > walkIndex)
                {
                    walkIndex++;
                    location = GetIndex(walkIndex);

                    ent = _entries[location];
                    _entries[location] = add;
                    add = ent;
                }
            }

            if (location > _top)
                _top = location;
            if (location == _topContinuous + 1)
            {
                while (_entries[location + 1].Index == location + 1)
                    location++;
                _topContinuous = location;
                if ((location = GetLayerIndex(_topLayer + 1)) - 1 == _topContinuous)
                {
                    _topLayer++;
                    _topLayerIndex = location;
                }
            }
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
            int next = MathHelper.ZeroIndex(walkIndex) + 1;
            return ((1 << (_topLayer + 1 - next)) - 1 + (walkIndex - ((1 << (next - 1)) - 1) >> (next)));
        }

        private int GetWalkIndex(int index)
        {
            int layer = GetLayer(index);
            return (1 << (_topLayer - layer)) - 1 + ((index - ((1 << (layer)) - 1)) * (2 << (_topLayer - layer)));
        }

        private bool FindLeaf(ref Entry value, out int index, out int parent)
        {
            Entry ent;
            parent = 0;
            index = 0;
            while (index < _topLayerIndex)
            {
                ent = _entries[index];

                if (value == ent)
                    return true;
                parent = index;

                index = GetChild(index, value < ent ? 1 : 2);
            }

            ent = _entries[index];

            return value == ent;
        }

        private bool Find(int value, out Entry ent)
        {
            ent = _entries[0];
            while (ent.Index < _topLayerIndex)
            {
                if (value == ent)
                    return true;

                ent = _entries[GetChild(ent.Index, value < ent ? 1 : 2)];
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

        class Enumerator : IEnumerator<TValue>
        {
            CompleteBinarySearchTree<TValue> _tree;
            int _walkIndex;
            int index;

            public Enumerator(CompleteBinarySearchTree<TValue> tree)
            {
                _tree = tree;
                _walkIndex = -1;
            }

            public TValue Current => _tree._entries[index].Value;

            object IEnumerator.Current => _tree._entries[index].Value;

            public void Dispose()
            {
                _tree = null;
            }

            public bool MoveNext()
            {
                Entry ent;
                do
                {
                    _walkIndex++;
                    index = _tree.GetIndex(_walkIndex);

                    if (index > _tree._top)
                        return false;

                    ent = _tree._entries[index];
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
    }

    
}
