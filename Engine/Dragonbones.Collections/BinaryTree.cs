using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones.Collections
{
    public class BinaryTree<TValue>
    {
        PagedArray<Entry> _entries;
        int _top;
        int _topContinuous;
        int _topLayer;
        
        private int GetLayerIndex(int layer)
        {
            return (1 << layer) - 1;
        }

        private int GetLayer(int index)
        {
            return (index + 1) >> 1;
        }

        private int GetChild(int parent, int childType)
        {
            return (parent << 1) + childType;
        }
        
        private int GetParent(int child)
        {
            return (child - 1) >> 1;
        }

        private bool FindLeaf(int value, out int index)
        {
            Entry ent;
            index = 0;
            int next = 0;
            while (next <= _topLayer)
            {
                index = next;
                ent = _entries[index];

                if (value == ent.NumericValue)
                    return true;


                next = GetChild(index, value < ent.NumericValue ? 1 : 0);
            }

            ent = _entries[next];

            index = value == ent.NumericValue ? next : index;
            return value == ent.NumericValue;
        }

        struct Entry
        {
            public Entry(TValue value, ChildType type, int number, int previous, int next = -1)
            {
                Value = value;
                NumericValue = number;
                Previous = previous;
                Next = next;
                Type = type;
            }

            public ChildType Type;
            public TValue Value;
            public int NumericValue;
            public int Next;
            public int Previous;
        }

        enum ChildType
        {
            Left = 1,
            Right = 2
        }
    }

    
}
