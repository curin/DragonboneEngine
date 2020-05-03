using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Dragonbones.Collections
{
    /// <summary>
    /// A Databuffer is a multithreaded structure to store data in an array format.
    /// The structure is designed to be written to on one thread and read from on another.
    /// The write thread can also read, but it will only read what it has recently written.
    ///
    /// When the write thread is done with a batch of writes it can swap the write buffer to make it read for the read buffer.
    /// When the read thread is ready to read it can swap the read buffer pulling the latest buffer flagged ready to read.
    /// </summary>
    /// <typeparam name="TValue">the type of value stored in the buffer</typeparam>
    public class DataBuffer<TValue> : IDataBuffer, IEnumerable<TValue>
    {
        private TValue[][] _values = new TValue[3][];
        private Queue<int>[] _dirtyEntries = new Queue<int>[2];
        private bool[][] _dirtyMarks = new bool[2][];

        /// <summary>
        /// Constructs a data buffer of the specified length.
        /// </summary>
        /// <param name="length">the length of the buffer</param>
        public DataBuffer(int length)
        {
            for (int i = 0; i < 3; i++)
                _values[i] = new TValue[length];

            for (int i = 0; i < 2; i++)
            {
                _dirtyMarks[i] = new bool[length];
                _dirtyEntries[i] = new Queue<int>();
            }
        }

        /// <summary>
        /// Constructs a data buffer of the specified length.
        /// </summary>
        /// <param name="length">the length of the buffer</param>
        /// <param name="initialValue">the value to start each entry at</param>
        public DataBuffer(int length, TValue initialValue)
        {
            for (int i = 0; i < 3; i++)
            {
                _values[i] = new TValue[length];
                for (int j = 0; j < length; j++)
                    _values[i][j] = initialValue;
            }

            for (int i = 0; i < 2; i++)
            {
                _dirtyMarks[i] = new bool[length];
                _dirtyEntries[i] = new Queue<int>();
            }
        }

        /// <summary>
        /// Perform a deep copy of a DataBuffer
        /// </summary>
        /// <param name="copy">buffer to copy</param>
        /// <param name="length">the length of the new buffer</param>
        public DataBuffer(DataBuffer<TValue> copy, int length = -1)
        {
            if (length == -1)
                length = copy.GetLength();
            for (int i = 0; i < 3; i++)
            {
                _values[i] = new TValue[length];
                copy._values[i].CopyTo(_values[i], 0);
            }

            for (int i = 0; i < 2; i++)
            {
                _dirtyMarks[i] = new bool[length];
                copy._dirtyMarks[i].CopyTo(_dirtyMarks[i], 0);

                _dirtyEntries[i] = new Queue<int>(copy._dirtyEntries[i]);
            }
        }

        /// <summary>
        /// Access the data buffer information
        /// </summary>
        /// <param name="type">Determines how the information can be manipulated and where information is retrieved from</param>
        /// <param name="index">the index to retrieve data from</param>
        /// <returns>the value at the given index from the corresponding buffer</returns>
        public TValue this[BufferTransactionType type, int index]
        {
            get => Get(type, index);
            set => Set(type, index, value);
        }

        /// <summary>
        /// Retrieve a value from the buffer
        /// </summary>
        /// <param name="type">the transaction type determines if you pull from the read buffer or the write buffer</param>
        /// <param name="index">the index to access</param>
        /// <returns>the value at the given index from the corresponding buffer</returns>
        public TValue Get(BufferTransactionType type, int index)
        {
            return _values[(int)type][index];
        }

        /// <summary>
        /// Sets a value in the buffer at the given index
        /// </summary>
        /// <param name="type">the transaction type determines how the function may respond
        /// Readonly transactions are ignored by set
        /// Write only transactions write to the write buffer and will be transferred after the swap write and then swap read are run in that order</param>
        /// <param name="index">the index to set</param>
        /// <param name="value">the value to set at the index</param>
        public void Set(BufferTransactionType type, int index, TValue value)
        {
            if (type == BufferTransactionType.ReadOnly)
                return;

            _values[0][index] = value;
            if (_dirtyMarks[0][index]) return;
            _dirtyMarks[0][index] = true;
            _dirtyEntries[0].Enqueue(index);
        }

        /// <summary>
        /// Shifts a section of data in the array
        /// This does not work for readonly transactions
        /// </summary>
        /// <param name="type">the type of transaction which affects what data is affected</param>
        /// <param name="startIndex">the index to start the shift from</param>
        /// <param name="length">the number of elements to move</param>
        /// <param name="shiftTo">the index to shift to</param>
        public void ShiftData(BufferTransactionType type, int startIndex, int length, int shiftTo)
        {
            if (type == BufferTransactionType.ReadOnly)
                return;
            TValue[] temp = new TValue[length];

            Array.Copy(_values[(int)type], startIndex, temp, 0, length);

            Array.Copy(temp, 0, _values[(int)type], shiftTo, length);
        }

        /// <summary>
        /// Gets a 64-bit value that represents the number of entries in the buffer
        /// </summary>
        /// <returns>the buffer entries count</returns>
        public long GetLongLength()
        {
            return _values[0].GetLongLength(0);
        }

        /// <summary>
        /// Gets a 32-bit value that represents the number of entries in the buffer
        /// </summary>
        /// <returns>the buffer entries count</returns>
        public int GetLength()
        {
            return _values[0].Length;
        }

        /// <summary>
        /// Copies buffer contents to an array at the specified index
        /// </summary>
        /// <param name="type">the transaction type which determines whether the write buffer or read buffer is copied</param>
        /// <param name="array">the array to copy to</param>
        /// <param name="index">the index to start the copy at</param>
        public void CopyTo(BufferTransactionType type, TValue[] array, int index)
        {
            _values[(int)type].CopyTo(array, index);
        }

        /// <summary>
        /// Copies buffer contents to an array at the specified index
        /// </summary>
        /// <param name="type">the transaction type which determines whether the write buffer or read buffer is copied</param>
        /// <param name="array">the array to copy to</param>
        /// <param name="index">the index to start the copy at</param>
        /// <param name="length">the number of entries to copy</param>
        public void CopyTo(BufferTransactionType type, TValue[] array, int index, int length)
        {
            Array.Copy(_values[(int)type], 0, array, index, length);
        }

        /// <summary>
        /// Swaps the data buffers for rendering
        /// </summary>
        public void SwapReadBuffer()
        {
            lock (_values[1])
            {
                while (_dirtyEntries[1].Count > 0)
                {
                    int val = _dirtyEntries[1].Dequeue();
                    _values[2][val] = _values[1][val];
                    _dirtyMarks[1][val] = false;
                }
            }
        }

        /// <summary>
        /// Swaps the data buffers on finishing of updating
        /// </summary>
        public void SwapWriteBuffer()
        {
            lock (_values[1])
            {
                while (_dirtyEntries[0].Count > 0)
                {
                    int val = _dirtyEntries[0].Dequeue();
                    _values[1][val] = _values[0][val];
                    _dirtyMarks[0][val] = false;

                    if (_dirtyMarks[1][val]) continue;

                    _dirtyMarks[1][val] = true;
                    _dirtyEntries[1].Enqueue(val);
                }
            }
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<TValue> GetEnumerator()
        {
            return (IEnumerator<TValue>)_values[2].GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values[2].GetEnumerator();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Disposes of this object
        /// </summary>
        /// <param name="disposing">should the managed objects be disposed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _values = null;
                    _dirtyMarks = null;
                    _dirtyEntries = null;
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
