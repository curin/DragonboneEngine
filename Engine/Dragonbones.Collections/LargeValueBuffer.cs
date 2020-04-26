using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones.Collections
{
    /// <summary>
    /// A <see cref="IDataBuffer"/> designed to store large single values for multi-threaded applications
    /// </summary>
    /// <typeparam name="TValue">The value stored</typeparam>
    public class LargeValueBuffer<TValue> : IDataBuffer
    where TValue:struct
    {
        private readonly TValue[] _value = new TValue[3];
        private readonly object _lock = new object();
        private readonly bool[] _dirty = new bool[2];

        /// <summary>
        /// Default Constructor
        /// </summary>
        public LargeValueBuffer() { }

        /// <summary>
        /// Construct the Value buffer with a default Value
        /// </summary>
        /// <param name="initialValue">the value to start with</param>
        public LargeValueBuffer(TValue initialValue)
        {
            for (int i = 0; i < 3; i++)
                _value[i] = initialValue;
        }

        /// <summary>
        /// Accesses the value
        /// </summary>
        /// <param name="type">the transaction type which determines which part of the buffer is accessed</param>
        /// <returns>the value stored</returns>
        public TValue this[BufferTransactionType type]
        {
            get => _value[(int)type];
            set => Set(type, value);
        }

        /// <summary>
        /// Set the value stored in the buffer
        /// </summary>
        /// <param name="type">the transaction type, Set cannot be done in a readonly transaction</param>
        /// <param name="value">the value to set</param>
        public void Set(BufferTransactionType type, TValue value)
        {
            if (type == BufferTransactionType.ReadOnly)
                return;
            _value[(int)type] = value;
            _dirty[(int) type] = true;
        }

        /// <summary>
        /// Swaps the data buffer for reading
        /// </summary>
        public void SwapReadBuffer()
        {
            if (!_dirty[1]) return;

            lock (_lock)
            {
                _value[2] = _value[1];
                _dirty[1] = false;
            }
        }

        /// <summary>
        /// Swaps the data buffer for writing
        /// Should be done when finished writing
        /// </summary>
        public void SwapWriteBuffer()
        {
            if (!_dirty[0]) return;
            
            lock (_lock)
            {
                _value[1] = _value[0];
                _dirty[0] = false;
                _dirty[1] = true;
            }
        }
    }
}
