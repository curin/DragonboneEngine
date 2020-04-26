using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones.Collections
{
    /// <summary>
    /// A <see cref="IDataBuffer"/> designed to store small single values for multi-threaded applications
    /// </summary>
    /// <typeparam name="TValue">The value stored</typeparam>
    public class ValueBuffer<TValue> : IDataBuffer
    where TValue:struct
    {
        private readonly TValue[] _value = new TValue[3];
        private readonly Object _lock = new object();

        /// <summary>
        /// Default Constructor
        /// </summary>
        public ValueBuffer() { }

        /// <summary>
        /// Construct the Value buffer with a default Value
        /// </summary>
        /// <param name="initialValue">the value to start with</param>
        public ValueBuffer(TValue initialValue)
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
            get => _value[(int) type];
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
        }

        /// <summary>
        /// Swaps the data buffer for reading
        /// </summary>
        public void SwapReadBuffer()
        {
            lock (_lock)
            {
                _value[2] = _value[1];
            }
        }

        /// <summary>
        /// Swaps the data buffer for writing
        /// Should be done when finished writing
        /// </summary>
        public void SwapWriteBuffer()
        {
            lock (_lock)
            {
                _value[1] = _value[0];
            }
        }
    }
}
