using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones.Collections
{
    /// <summary>
    /// Similar in concept to <see cref="DataBuffer{TValue}"/> this is meant for multithreaded use.
    /// The main difference is that this keeps two lists of data updated in tandem,
    /// this implementation is meant to decrease overhead over keeping two data buffers as much of the work can be done together.
    /// </summary>
    /// <typeparam name="TPrimary">The primary data type stored here</typeparam>
    /// <typeparam name="TSecondary">The secondary data type stored here</typeparam>
    public class DoubleDataBuffer<TPrimary, TSecondary> : IDataBuffer, IEnumerable<TPrimary>
    {
        private TPrimary[][] _value1s = new TPrimary[3][];
        private TSecondary[][] _value2s = new TSecondary[3][];
        private Queue<int>[] _dirtyEntries = new Queue<int>[2];
        private bool[][] _dirtyMarks = new bool[2][];

        /// <summary>
        /// Access the data buffer information
        /// </summary>
        /// <param name="type">Determines how the information can be manipulated and where information is retrieved from</param>
        /// <param name="index">the index to retrieve data from</param>
        /// <returns>the value at the given index from the corresponding buffer</returns>
        private Tuple<TPrimary, TSecondary> this[BufferTransactionType type, int index]
        {
            get => Get(type, index);
            set => Set(type, index, value.Item1, value.Item2);
        }

        /// <summary>
        /// Retrieve a value from the buffer
        /// </summary>
        /// <param name="type">the transaction type determines if you pull from the read buffer or the write buffer</param>
        /// <param name="index">the index to access</param>
        /// <returns>the values at the given index from the corresponding buffer</returns>
        Tuple<TPrimary, TSecondary> Get(BufferTransactionType type, int index)
        {
            return new Tuple<TPrimary, TSecondary>( _value1s[(int)type][index] , _value2s[(int)type][index]);
        }

        /// <summary>
        /// Sets a value in the buffer at the given index
        /// </summary>
        /// <param name="type">the transaction type determines how the function may respond
        /// Readonly transactions are ignored by set
        /// Write only transactions write to the write buffer and will be transferred after the swap write and then swap read are run in that order</param>
        /// <param name="index">the index to set</param>
        /// <param name="primary">the primary value to set at the index</param>
        /// <param name="secondary">the secondary value to set at the index</param>
        void Set(BufferTransactionType type, int index, TPrimary primary, TSecondary secondary)
        {
            if (type == BufferTransactionType.ReadOnly)
                return;

            _value1s[0][index] = primary;
            _value2s[0][index] = secondary;
            if (_dirtyMarks[0][index]) return;
            _dirtyMarks[0][index] = true;
            _dirtyEntries[0].Enqueue(index);
        }

        /// <summary>
        /// Gets a 64-bit value that represents the number of entries in the buffer
        /// </summary>
        /// <returns>the buffer entries count</returns>
        long GetLongLength()
        {
            return _value1s[0].GetLongLength(0);
        }

        /// <summary>
        /// Gets a 32-bit value that represents the number of entries in the buffer
        /// </summary>
        /// <returns>the buffer entries count</returns>
        int GetLength()
        {
            return _value1s[0].Length;
        }

        /// <summary>
        /// Copies primary buffer contents to an array at the specified index
        /// </summary>
        /// <param name="type">the transaction type which determines whether the write buffer or read buffer is copied</param>
        /// <param name="array">the array to copy to</param>
        /// <param name="index">the index to start the copy at</param>
        void CopyPrimaryTo(BufferTransactionType type, Array array, int index)
        {
            _value1s[(int)type].CopyTo(array, index);
        }

        /// <summary>
        /// Copies secondary buffer contents to an array at the specified index
        /// </summary>
        /// <param name="type">the transaction type which determines whether the write buffer or read buffer is copied</param>
        /// <param name="array">the array to copy to</param>
        /// <param name="index">the index to start the copy at</param>
        void CopySecondaryTo(BufferTransactionType type, Array array, int index)
        {
            _value2s[(int)type].CopyTo(array, index);
        }

        /// <summary>
        /// Swaps the data buffers for rendering
        /// </summary>
        public void SwapReadBuffer()
        {
            lock (_value1s[1])
            {
                while (_dirtyEntries[1].Count > 0)
                {
                    int val = _dirtyEntries[1].Dequeue();
                    _value1s[2][val] = _value1s[1][val];
                    _value2s[2][val] = _value2s[1][val];
                    _dirtyMarks[1][val] = false;
                }
            }
        }

        /// <summary>
        /// Swaps the data buffers on finishing of updating
        /// </summary>
        public void SwapWriteBuffer()
        {
            lock (_value1s[1])
            {
                while (_dirtyEntries[0].Count > 0)
                {
                    int val = _dirtyEntries[0].Dequeue();
                    _value1s[1][val] = _value1s[0][val];
                    _value2s[1][val] = _value2s[0][val];
                    _dirtyMarks[0][val] = false;

                    if (_dirtyMarks[1][val]) continue;

                    _dirtyMarks[1][val] = true;
                    _dirtyEntries[1].Enqueue(val);
                }
            }
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<TPrimary> GetEnumerator()
        {
            return (IEnumerator<TPrimary>)_value1s[2].GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _value1s[2].GetEnumerator();
        }
    }
}
