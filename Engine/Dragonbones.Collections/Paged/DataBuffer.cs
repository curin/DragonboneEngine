using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Dragonbones.Collections.Paged
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
        private PagedArray<TValue>[] _values = new PagedArray<TValue>[3];
        private Queue<int>[] _dirtyEntries = new Queue<int>[2];
        private PagedArray<bool>[] _dirtyMarks = new PagedArray<bool>[2];

        /// <summary>
        /// Constructs a data buffer of the specified length.
        /// </summary>
        /// <param name="pagePower">the size of pages as a power of 2</param>
        /// <param name="pageCount">the initial number of pages, may affect early adds if new pages need to be added</param>
        public DataBuffer(int pagePower = 8, int pageCount = 1)
        {
            for (int i = 0; i < 3; i++)
                _values[i] = new PagedArray<TValue>(pagePower, pageCount);

            for (int i = 0; i < 2; i++)
            {
                _dirtyMarks[i] = new PagedArray<bool>(pagePower, pageCount);
                _dirtyEntries[i] = new Queue<int>();
            }
        }

        /// <summary>
        /// Constructs a data buffer of the specified length.
        /// </summary>
        /// <param name="pagePower">the size of pages as a power of 2</param>
        /// <param name="pageCount">the initial number of pages, may affect early adds if new pages need to be added</param>
        /// <param name="initialValue">the value to start each entry at</param>
        public DataBuffer(TValue initialValue, int pagePower = 8, int pageCount = 1)
        {
            for (int i = 0; i < 3; i++)
            {
                _values[i] = new PagedArray<TValue>(pagePower, pageCount);
                for (int j = 0; j < _values[i].Length; j++)
                    _values[i].Set(j, initialValue);
            }

            for (int i = 0; i < 2; i++)
            {
                _dirtyMarks[i] = new PagedArray<bool>(pagePower, pageCount);
                _dirtyEntries[i] = new Queue<int>();
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
            return _values[(int)type].Get(index);
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

            int page = MathHelper.MathShiftRem(index, _values[(int)type].PageSizeMinusOne, _values[(int)type].PagePower, out int id);

            _values[0].Set(page, id, value);
            if (_dirtyMarks[0].Get(page, id)) return;
            _dirtyMarks[0].Set(page, id, true);
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
        public void CopyData(BufferTransactionType type, int startIndex, int length, int shiftTo)
        {
            if (type == BufferTransactionType.ReadOnly)
                return;

            _values[(int)type].CopyData(startIndex, length, shiftTo);
        }

        /// <summary>
        /// Gets a 64-bit value that represents the number of entries in the buffer
        /// </summary>
        /// <returns>the buffer entries count</returns>
        public long GetLongLength()
        {
            return _values[0].LongLength;
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
            _values[(int)type].CopyTo(array, index, length);
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
                    int page = MathHelper.MathShiftRem(val, _values[1].PageSizeMinusOne, _values[1].PagePower, out int id);
                    _values[2].Set(page, id, _values[1][page, id]);
                    _dirtyMarks[1].Set(page, id, false);
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
                    int page = MathHelper.MathShiftRem(val, _values[1].PageSizeMinusOne, _values[1].PagePower, out int id);

                    _values[1].Set(page, id, _values[0].Get(page, id));
                    _dirtyMarks[0].Set(page, id, false);

                    if (_dirtyMarks[1].Get(page, id)) continue;

                    _dirtyMarks[1].Set(page, id, true);
                    _dirtyEntries[1].Enqueue(val);
                }
            }
        }

        /// <summary>
        /// Shrinks the buffer to the given index
        /// </summary>
        /// <param name="index">index to shrink to</param>
        public void ShrinkTo(int index)
        {
            for (int i = 0; i < 3; i++)
                _values[i].ShrinkTo(index);

            for (int i = 0; i < 2; i++)
            {
                _dirtyMarks[i].ShrinkTo(index);
                int val;
                for (int j = 0; j < _dirtyEntries.Length; j++)
                    if ((val = _dirtyEntries[i].Dequeue()) <= index)
                        _dirtyEntries[i].Enqueue(val);
            }
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<TValue> GetEnumerator()
        {
            return _values[2].GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values[2].GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <param name="type">the type of transaction which affects the data returned</param>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<TValue> GetEnumerator(BufferTransactionType type)
        {
            return _values[(int)type].GetEnumerator();
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
