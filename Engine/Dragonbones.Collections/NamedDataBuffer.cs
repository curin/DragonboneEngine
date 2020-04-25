using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones.Collections
{
    public class NamedDataBuffer<TValue>: IDataBuffer//,  IEnumerable<TValue>
    {
        private DoubleDataBuffer<TValue, Entry> _buffer;
        private class Entry
        {
            public int ID;
            public string Name;
            public int Next;
            public int Previous;
            public int NextWalk;
            public int PreviousWalk;
        }

        /// <summary>
        /// Swaps the data buffers for rendering
        /// </summary>
        public void SwapReadBuffer()
        {
            _buffer.SwapReadBuffer();
        }

        /// <summary>
        /// Swaps the data buffers on finishing of updating
        /// </summary>
        public void SwapWriteBuffer()
        {
            _buffer.SwapWriteBuffer();
        }
    }
}
