using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dragonbones.Collections.Paged
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
        private PagedArray<TPrimary>[] _value1s = new PagedArray<TPrimary>[3];
        private PagedArray<TSecondary>[] _value2s = new PagedArray<TSecondary>[3];
        private Queue<int>[] _dirtyEntries = new Queue<int>[2];
        private PagedArray<bool>[] _dirtyMarks = new PagedArray<bool>[2];

        /// <summary>
        /// Constructs a double data buffer of the specified length.
        /// </summary>
        /// <param name="pagePower">the size of pages as a power of 2</param>
        /// <param name="pageCount">the initial number of pages, may affect early adds if new pages need to be added</param>
        public DoubleDataBuffer(int pagePower = 8, int pageCount = 1)
        {
            for (int i = 0; i < 3; i++)
            {
                _value1s[i] = new PagedArray<TPrimary>(pagePower, pageCount);
                _value2s[i] = new PagedArray<TSecondary>(pagePower, pageCount);
            }

            for (int i = 0; i < 2; i++)
            {
                _dirtyMarks[i] = new PagedArray<bool>(pagePower, pageCount);
                _dirtyEntries[i] = new Queue<int>();
            }
        }

        /// <summary>
        /// Constructs a double data buffer of the specified length.
        /// </summary>
        /// <param name="pagePower">the size of pages as a power of 2</param>
        /// <param name="pageCount">the initial number of pages, may affect early adds if new pages need to be added</param>
        /// <param name="initialPrimary">The initial value for the primary buffer</param>
        /// <param name="initialSecondary">the initial value for the secondary buffer</param>
        public DoubleDataBuffer(TPrimary initialPrimary, TSecondary initialSecondary, int pagePower = 8, int pageCount = 1)
        {
            for (int i = 0; i < 3; i++)
            {
                _value1s[i] = new PagedArray<TPrimary>(pagePower, pageCount);
                _value2s[i] = new PagedArray<TSecondary>(pagePower, pageCount);

                for (int j = 0; j < _value1s.Length; j++)
                {
                    _value1s[i][j] = initialPrimary;
                    _value2s[i][j] = initialSecondary;
                }
            }

            for (int i = 0; i < 2; i++)
            {
                _dirtyMarks[i] = new PagedArray<bool>(pagePower, pageCount);
                _dirtyEntries[i] = new Queue<int>();
            }
        }

        /// <summary>
        /// Constructs a double data buffer of the specified length.
        /// </summary>
        /// <param name="pagePower">the size of pages as a power of 2</param>
        /// <param name="pageCount">the initial number of pages, may affect early adds if new pages need to be added</param>
        /// <param name="initialPrimary">The initial value for the primary buffer</param>
        public DoubleDataBuffer(TPrimary initialPrimary, int pagePower = 8, int pageCount = 1)
        {
            for (int i = 0; i < 3; i++)
            {
                _value1s[i] = new PagedArray<TPrimary>(pagePower, pageCount);
                _value2s[i] = new PagedArray<TSecondary>(pagePower, pageCount);

                for (int j = 0; j < _value1s.Length; j++)
                {
                    _value1s[i].Set(j, initialPrimary);
                }
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
        public Tuple<TPrimary, TSecondary> this[BufferTransactionType type, int index]
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
        public Tuple<TPrimary, TSecondary> Get(BufferTransactionType type, int index)
        {
            int page = MathHelper.MathShiftRem(index, _value1s[(int)type].PageSizeMinusOne, _value1s[(int)type].PagePower, out int id);
            return new Tuple<TPrimary, TSecondary>( _value1s[(int)type].Get(page, id), _value2s[(int)type].Get(page, id));
        }

        /// <summary>
        /// Retrieve a primary value from the buffer
        /// </summary>
        /// <param name="type">the transaction type determines if you pull from the read buffer or the write buffer</param>
        /// <param name="index">the index to access</param>
        /// <returns>the primary value at the given index from the corresponding buffer</returns>
        public TPrimary GetPrimary(BufferTransactionType type, int index)
        {
            return _value1s[(int)type].Get(index);
        }

        /// <summary>
        /// Retrieve a secondary value from the buffer
        /// </summary>
        /// <param name="type">the transaction type determines if you pull from the read buffer or the write buffer</param>
        /// <param name="index">the index to access</param>
        /// <returns>the secondary value at the given index from the corresponding buffer</returns>
        public TSecondary GetSecondary(BufferTransactionType type, int index)
        {
            return _value2s[(int)type].Get(index);
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
        public void Set(BufferTransactionType type, int index, TPrimary primary, TSecondary secondary)
        {
            if (type == BufferTransactionType.ReadOnly)
                return;

            int page = MathHelper.MathShiftRem(index, _value1s[(int)type].PageSizeMinusOne, _value1s[(int)type].PagePower, out int id);

            _value1s[0].Set(page, id, primary);
            _value2s[0].Set(page, id, secondary);
            if (_dirtyMarks[0].Get(page, id)) return;
            _dirtyMarks[0].Set(page, id, true);
            _dirtyEntries[0].Enqueue(index);
        }

        /// <summary>
        /// Sets a value in the buffer at the given index
        /// </summary>
        /// <param name="type">the transaction type determines how the function may respond
        /// Readonly transactions are ignored by set
        /// Write only transactions write to the write buffer and will be transferred after the swap write and then swap read are run in that order</param>
        /// <param name="index">the index to set</param>
        /// <param name="primary">the primary value to set at the index</param>
        public void SetPrimary(BufferTransactionType type, int index, TPrimary primary)
        {
            if (type == BufferTransactionType.ReadOnly)
                return;

            int page = MathHelper.MathShiftRem(index, _value1s[(int)type].PageSizeMinusOne, _value1s[(int)type].PagePower, out int id);

            _value1s[0].Set(page, id, primary);
            if (_dirtyMarks[0].Get(page, id)) return;
            _dirtyMarks[0].Set(page, id, true);
            _dirtyEntries[0].Enqueue(index);
        }

        /// <summary>
        /// Sets a value in the buffer at the given index
        /// </summary>
        /// <param name="type">the transaction type determines how the function may respond
        /// Readonly transactions are ignored by set
        /// Write only transactions write to the write buffer and will be transferred after the swap write and then swap read are run in that order</param>
        /// <param name="index">the index to set</param>
        /// <param name="secondary">the secondary value to set at the index</param>
        public void SetSecondary(BufferTransactionType type, int index, TSecondary secondary)
        {
            if (type == BufferTransactionType.ReadOnly)
                return;

            int page = MathHelper.MathShiftRem(index, _value1s[(int)type].PageSizeMinusOne, _value1s[(int)type].PagePower, out int id);

            _value2s[0].Set(page, id, secondary);
            if (_dirtyMarks[0].Get(page, id)) return;
            _dirtyMarks[0].Set(page, id, true);
            _dirtyEntries[0].Enqueue(index);
        }

        /// <summary>
        /// Gets a 64-bit value that represents the number of entries in the buffer
        /// </summary>
        /// <returns>the buffer entries count</returns>
        public long GetLongLength()
        {
            return _value1s[0].LongLength;
        }

        /// <summary>
        /// Gets a 32-bit value that represents the number of entries in the buffer
        /// </summary>
        /// <returns>the buffer entries count</returns>
        public int GetLength()
        {
            return _value1s[0].Length;
        }

        /// <summary>
        /// Copies primary buffer contents to an array at the specified index
        /// </summary>
        /// <param name="type">the transaction type which determines whether the write buffer or read buffer is copied</param>
        /// <param name="array">the array to copy to</param>
        /// <param name="index">the index to start the copy at</param>
        public void CopyPrimaryTo(BufferTransactionType type, Array array, int index)
        {
            _value1s[(int)type].CopyTo(array, index);
        }

        /// <summary>
        /// Copies secondary buffer contents to an array at the specified index
        /// </summary>
        /// <param name="type">the transaction type which determines whether the write buffer or read buffer is copied</param>
        /// <param name="array">the array to copy to</param>
        /// <param name="index">the index to start the copy at</param>
        public void CopySecondaryTo(BufferTransactionType type, Array array, int index)
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
                    int page = MathHelper.MathShiftRem(val, _value1s[1].PageSizeMinusOne, _value1s[1].PagePower, out int id);
                    _value1s[2].Set(page, id, _value1s[1].Get(page, id));
                    _value2s[2].Set(page, id, _value2s[1].Get(page, id));
                    _dirtyMarks[1].Set(page, id, false);
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
            {
                _value1s[i].ShrinkTo(index);
                _value2s[i].ShrinkTo(index);
            }

            for (int i = 0; i < 2; i++)
            {
                _dirtyMarks[i].ShrinkTo(index);
                int val;
                for (int j = 0; j < _dirtyEntries.Length; j++)
                    if ((val = _dirtyEntries[i].Dequeue()) <= index)
                        _dirtyEntries[i].Enqueue(val);
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
                    int page = MathHelper.MathShiftRem(val, _value1s[1].PageSizeMinusOne, _value1s[1].PagePower, out int id);
                    _value1s[1].Set(page, id, _value1s[0].Get(page, id));
                    _value2s[1].Set(page, id, _value2s[0].Get(page, id));
                    _dirtyMarks[0].Set(page, id, false);

                    if (_dirtyMarks[1][page, id]) continue;

                    _dirtyMarks[1].Set(page, id, true);
                    _dirtyEntries[1].Enqueue(val);
                }
            }
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

            _value1s[(int)type].CopyData(startIndex, length, shiftTo);
            _value2s[(int)type].CopyData(startIndex, length, shiftTo);
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Disposes this object
        /// </summary>
        /// <param name="disposing">should the managed objects also be disposed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _dirtyMarks = null;
                    _value1s = null;
                    _dirtyEntries = null;
                    _value2s = null;
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
            //GC.SuppressFinalize(this);
        }
        #endregion
    }
}
