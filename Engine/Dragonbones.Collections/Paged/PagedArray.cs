using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Dragonbones;

namespace Dragonbones.Collections.Paged
{
    /// <summary>
    /// A self growing structure designed to house large lists which can grow frequently
    /// and shrink without wasting as much space before the garbage collector runs
    /// </summary>
    /// <typeparam name="TValue">the type of value stored in this array</typeparam>
    public class PagedArray<TValue> : IEnumerable<TValue>
    {
        private TValue[][] _pages;
        private readonly int _pageSize;
        private readonly int _shiftVal;
        private readonly int _pageShiftSize;
        private int _pageCount;

        /// <summary>
        /// Constructor for the PagedArray
        /// </summary>
        /// <param name="pagePower">the power of two for the size of pages (example: 8 = 256) 
        /// The larger this number the closer it will function to a flat array, but the more blank space it can use</param>
        /// <param name="initialPageCount">how many pages should be created to start (only one will be initialized to begin)</param>
        public PagedArray(int pagePower, int initialPageCount)
        {
            _pages = new TValue[initialPageCount][];
            _pageShiftSize = pagePower;
            _pageSize = 1;
            _pageSize <<= pagePower;
            _shiftVal = _pageSize - 1;

            _pages[0] = new TValue[_pageSize];
            _pageCount = 1;
        }

        /// <summary>
        /// The size of the array 
        /// (is always a multiple of the page size)
        /// </summary>
        public int Length => _pageCount << _pageShiftSize;

        /// <summary>
        /// The page size as a power of 2
        /// </summary>
        public int PagePower => _pageShiftSize;

        /// <summary>
        /// The page size minus one
        /// this can be used for fast page index calculations
        /// </summary>
        public int PageSizeMinusOne => _shiftVal;

        /// <summary>
        /// The size of the array as a 64-bit value
        /// (is always a multiple of the page size)
        /// This is safer as the array may be longer than max int.
        /// </summary>
        public long LongLength => (long)_pageCount << _pageShiftSize;

        /// <summary>
        /// The number of initialized pages
        /// </summary>
        public int PageCount => _pageCount;

        /// <summary>
        /// The number of elements in a single page
        /// </summary>
        public int PageLength => _pageSize;

        /// <summary>
        /// Access a value at a given index
        /// </summary>
        /// <param name="index">the index of the value to access</param>
        /// <returns>the value at the index</returns>
        public TValue this[int index]
        {
            get => Get(index);
            set => Set(index, value);
        }
        
        /// <summary>
        /// Access a value at a given index
        /// </summary>
        /// <param name="index">the index of the value to access</param>
        /// <returns>the value at the index</returns>
        public TValue this[long index]
        {
            get => Get(index);
            set => Set(index, value);
        }

        /// <summary>
        /// Access a value at a given page and index
        /// </summary>
        /// <param name="page">the page to access</param>
        /// <param name="index">the index of the value</param>
        /// <returns>the value at the given index on the given page</returns>
        public TValue this[int page, int index]
        {
            get => Get(page, index);
            set => Set(page, index, value);
        }

        /// <summary>
        /// Get a value at a given index
        /// </summary>
        /// <param name="page">the page to access</param>
        /// <param name="index">the index of the value to retrieve</param>
        /// <returns>the value at that index</returns>
        public TValue Get(int page, int index)
        {
            if (page >= _pageCount)
                return default;
            return _pages[page][index];
        }

        /// <summary>
        /// Attempt to get a value at the given index
        /// </summary>
        /// <param name="page">the page to access</param>
        /// <param name="index">the index to retrieve</param>
        /// <param name="value">the value retrieved</param>
        /// <returns>if there exists a page containing this index</returns>
        public bool TryGet(int page, int index, out TValue value)
        {
            if (page >= _pageCount)
            {
                value = default;
                return false;
            }
            value = _pages[page][index];
            return true;
        }

        /// <summary>
        /// Get a value at a given index
        /// </summary>
        /// <param name="index">the index of the value to retrieve</param>
        /// <returns>the value at that index</returns>
        public TValue Get(int index)
        {
            int page = MathHelper.MathShiftRem(index, _shiftVal, _pageShiftSize, out int id);
            if (page >= _pageCount)
                return default;
            return _pages[page][id];
        }

        /// <summary>
        /// Attempt to get a value at the given index
        /// </summary>
        /// <param name="index">the index to retrieve</param>
        /// <param name="value">the value retrieved</param>
        /// <returns>if there exists a page containing this index</returns>
        public bool TryGet(int index, out TValue value)
        {
            int page = MathHelper.MathShiftRem(index, _shiftVal, _pageShiftSize, out int id);
            if (page >= _pageCount)
            {
                value = default;
                return false;
            }
            value = _pages[page][id];
            return true;
        }

        /// <summary>
        /// Get a value at a given index
        /// </summary>
        /// <param name="index">the index of the value to retrieve</param>
        /// <returns>the value at that index</returns>
        public TValue Get(long index)
        {
            int page = MathHelper.MathShiftRem(index, _shiftVal, _pageShiftSize, out int id);
            if (page >= _pageCount)
                return default;
            return _pages[page][id];
        }

        /// <summary>
        /// Attempt to get a value at the given index
        /// </summary>
        /// <param name="index">the index to retrieve</param>
        /// <param name="value">the value retrieved</param>
        /// <returns>if there exists a page containing this index</returns>
        public bool TryGet(long index, out TValue value)
        {
            int page = MathHelper.MathShiftRem(index, _shiftVal, _pageShiftSize, out int id);
            if (page >= _pageCount)
            {
                value = default;
                return false;
            }
            value = _pages[page][id];
            return true;
        }

        /// <summary>
        /// Set a particular value at a particular index
        /// </summary>
        /// <param name="index">the index to set</param>
        /// <param name="value">the value to set</param>
        public void Set(int index, TValue value)
        {
            int page = MathHelper.MathShiftRem(index, _shiftVal, _pageShiftSize, out int id);
            if (page >= _pageCount)
            {
                if (page >= _pages.Length)
                {
                    int pageCount = _pages.Length << 1;
                    while (pageCount < page)
                        pageCount <<= 1;
                    TValue[][] temp = new TValue[pageCount][];
                    Array.Copy(_pages, 0, temp, 0, _pageCount);
                    _pages = temp;
                }
                for (int i = _pageCount; i <= page; i++)
                    _pages[i] = new TValue[_pageSize];
                _pageCount = page + 1;
            }
            _pages[page][id] = value;
        }

        /// <summary>
        /// Set a particular value at a particular index
        /// </summary>
        /// <param name="page">the page to access</param>
        /// <param name="index">the index to set</param>
        /// <param name="value">the value to set</param>
        public void Set(int page, int index, TValue value)
        {
            if (page >= _pageCount)
            {
                if (page >= _pages.Length)
                {
                    int pageCount = _pages.Length << 1;
                    while (pageCount < page)
                        pageCount <<= 1;
                    TValue[][] temp = new TValue[pageCount][];
                    Array.Copy(_pages, 0, temp, 0, _pageCount);
                    _pages = temp;
                }
                for (int i = _pageCount; i <= page; i++)
                    _pages[i] = new TValue[_pageSize];
                _pageCount = page + 1;
            }
            _pages[page][index] = value;
        }

        /// <summary>
        /// Set a particular value at a particular index
        /// </summary>
        /// <param name="index">the index to set</param>
        /// <param name="value">the value to set</param>
        public void Set(long index, TValue value)
        {
            int page = MathHelper.MathShiftRem(index, _shiftVal, _pageShiftSize, out int id);
            if (page >= _pageCount)
            {
                if (page >= _pages.Length)
                {
                    int pageCount = _pages.Length << 1;
                    while (pageCount < page)
                        pageCount <<= 1;
                    TValue[][] temp = new TValue[pageCount][];
                    Array.Copy(_pages, 0, temp, 0, _pageCount);
                    _pages = temp;
                }
                for (int i = _pageCount; i <= page; i++)
                    _pages[i] = new TValue[_pageSize];
                _pageCount = page + 1;
            }
            _pages[page][id] = value;
        }

        /// <summary>
        /// Shrinks the array to the given index
        /// </summary>
        /// <param name="index"></param>
        public void ShrinkTo(int index)
        {
            int page = (index >> _pageShiftSize) + 1;
            if (_pageCount <= page)
                return;

            for (int i = page; i < _pageCount; i++)
                _pages[i] = null;
        }

        /// <summary>
        /// Shrinks the array to the given index
        /// </summary>
        /// <param name="index"></param>
        public void ShrinkTo(long index)
        {
            int page = (int)(index >> _pageShiftSize) + 1;
            if (_pageCount <= page)
                return;

            for (int i = page; i < _pageCount; i++)
                _pages[i] = null;
        }

        /// <summary>
        /// Copy data from one index to another
        /// </summary>
        /// <param name="source">the index the source of the copy starts at</param>
        /// <param name="length">the number of elements to copy</param>
        /// <param name="destination">the index the destination of the copy starts at</param>
        public void CopyData(int source, int length, int destination)
        {
            if (source == destination)
                return;

            int srcPage = MathHelper.MathShiftRem(source, _shiftVal, _pageShiftSize, out int srcIndex);
            int destPage = MathHelper.MathShiftRem(destination, _shiftVal, _pageShiftSize, out int destIndex);
            int destEndPage = MathHelper.MathShiftRem(destination + length, _shiftVal, _pageShiftSize, out int destEndIndex);

            if (srcPage == destEndPage)
            {
                Array.Copy(_pages[srcPage], srcIndex, _pages[srcPage], destIndex, length);
                return;
            }

            int srcEndPage = MathHelper.MathShiftRem(source + length, _pageSize - 1, _pageShiftSize, out int srcEndIndex);

            if (source < destination)
            {
                if (destEndIndex > srcEndIndex)
                {
                    int start1 = destEndIndex - srcEndIndex;
                    int count1 = srcEndIndex + 1;
                    
                    Array.Copy(_pages[srcEndPage], 0, _pages[destEndPage], start1, count1);
                    if (srcEndPage == srcPage)
                        return;

                    int count2 = start1 + 1;
                    int start2 = _pageSize - count2;

                    srcEndPage--;
                    Array.Copy(_pages[srcEndPage], start2, _pages[destEndPage], 0, count2);
                    destEndPage--;
                    count1 = _pageSize - start1;

                    while (srcEndPage != srcPage)
                    {
                        Array.Copy(_pages[srcEndPage], 0, _pages[destEndPage], start1, count1);
                        srcEndPage--;
                        Array.Copy(_pages[srcEndPage], start2, _pages[destEndPage], 0, count2);
                        destEndPage--;
                    }

                    count1 = (_pageSize - srcIndex) - (_pageSize - start1);
                    Array.Copy(_pages[srcPage], srcIndex, _pages[destPage], destIndex, count1);
                    return;
                }
                else
                {
                    int start1 = srcEndIndex - destEndIndex;
                    int count1 = srcEndIndex + 1;

                    Array.Copy(_pages[srcEndPage], start1, _pages[destEndPage], 0, count1);
                    if (destEndPage == destPage)
                        return;

                    int count2 = start1 + 1;
                    int start2 = _pageSize - count2;

                    destEndPage--;
                    Array.Copy(_pages[srcEndPage], 0, _pages[destEndPage], start2, count2);
                    srcEndPage--;
                    count1 = _pageSize - start1;

                    while(destEndPage != destPage)
                    {
                        Array.Copy(_pages[srcEndPage], start1, _pages[destEndPage], 0, count1);
                        destEndPage--;
                        Array.Copy(_pages[srcEndPage], 0, _pages[destEndPage], start2, count2);
                        srcEndPage--;
                    }

                    count1 = (_pageSize - destIndex) - (_pageSize - start1);
                    Array.Copy(_pages[srcPage], srcIndex, _pages[destPage], destIndex, count1);
                    return;
                }
            }
            else if (source > destination)
            {
                if (destIndex > srcIndex)
                {
                    int count1 = _pageCount - destIndex;

                    Array.Copy(_pages[srcPage], srcIndex, _pages[destPage], destIndex, count1);
                    if (srcEndPage == srcPage)
                        return;

                    int start2 = srcIndex + count1;
                    int count2 = _pageCount - start2;

                    destPage++;
                    Array.Copy(_pages[srcPage], start2, _pages[destPage], 0, count2);
                    srcPage++;
                    int start1 = count2;
                    count1 = _pageSize - start1;

                    while (srcEndPage != srcPage)
                    {
                        Array.Copy(_pages[srcPage], 0, _pages[destPage], start1, count1);
                        destPage++;
                        Array.Copy(_pages[srcPage], start2, _pages[destPage], 0, count2);
                        srcPage++;
                    }


                    Array.Copy(_pages[srcPage], 0, _pages[destPage], start1, count1);
                    destPage++;
                    count1 = destEndIndex - start2;
                    Array.Copy(_pages[srcPage], start2, _pages[destPage], 0, count1);
                    return;
                }
                else
                {
                    int count1 = _pageCount - srcIndex;

                    Array.Copy(_pages[srcPage], srcIndex, _pages[destPage], destIndex, count1);
                    if (srcEndPage == srcPage)
                        return;

                    int start2 = destIndex + count1;
                    int count2 = _pageCount - start2;

                    destPage++;
                    Array.Copy(_pages[srcPage], 0, _pages[destPage], start2, count2);
                    srcPage++;
                    int start1 = count2;
                    count1 = _pageSize - start1;

                    while (srcEndPage != srcPage)
                    {
                        Array.Copy(_pages[srcPage], start1, _pages[destPage], 0, count1);
                        destPage++;
                        Array.Copy(_pages[srcPage], 0, _pages[destPage], start2, count2);
                        srcPage++;
                    }


                    Array.Copy(_pages[srcPage], start1, _pages[destPage], 0, count1);
                    destPage++;
                    count1 = destEndIndex - start2;
                    Array.Copy(_pages[srcPage], 0, _pages[destPage], start1, count1);
                    return;
                }
            }
        }

        /// <summary>
        /// Copy data from one index to another
        /// </summary>
        /// <param name="source">the index the source of the copy starts at</param>
        /// <param name="length">the number of elements to copy</param>
        /// <param name="destination">the index the destination of the copy starts at</param>
        public void CopyData(long source, int length, long destination)
        {
            if (source == destination)
                return;

            int srcPage = MathHelper.MathShiftRem(source, _shiftVal, _pageShiftSize, out int srcIndex);
            int destPage = MathHelper.MathShiftRem(destination, _shiftVal, _pageShiftSize, out int destIndex);
            int destEndPage = MathHelper.MathShiftRem(destination + length, _shiftVal, _pageShiftSize, out int destEndIndex);

            if (srcPage == destEndPage)
            {
                Array.Copy(_pages[srcPage], srcIndex, _pages[srcPage], destIndex, length);
                return;
            }

            int srcEndPage = MathHelper.MathShiftRem(source + length, _pageSize - 1, _pageShiftSize, out int srcEndIndex);

            if (source < destination)
            {
                if (destEndIndex > srcEndIndex)
                {
                    int start1 = destEndIndex - srcEndIndex;
                    int count1 = srcEndIndex + 1;

                    Array.Copy(_pages[srcEndPage], 0, _pages[destEndPage], start1, count1);
                    if (srcEndPage == srcPage)
                        return;

                    int count2 = start1 + 1;
                    int start2 = _pageSize - count2;

                    srcEndPage--;
                    Array.Copy(_pages[srcEndPage], start2, _pages[destEndPage], 0, count2);
                    destEndPage--;
                    count1 = _pageSize - start1;

                    while (srcEndPage != srcPage)
                    {
                        Array.Copy(_pages[srcEndPage], 0, _pages[destEndPage], start1, count1);
                        srcEndPage--;
                        Array.Copy(_pages[srcEndPage], start2, _pages[destEndPage], 0, count2);
                        destEndPage--;
                    }

                    count1 = (_pageSize - srcIndex) - (_pageSize - start1);
                    Array.Copy(_pages[srcPage], srcIndex, _pages[destPage], destIndex, count1);
                    return;
                }
                else
                {
                    int start1 = srcEndIndex - destEndIndex;
                    int count1 = srcEndIndex + 1;

                    Array.Copy(_pages[srcEndPage], start1, _pages[destEndPage], 0, count1);
                    if (destEndPage == destPage)
                        return;

                    int count2 = start1 + 1;
                    int start2 = _pageSize - count2;

                    destEndPage--;
                    Array.Copy(_pages[srcEndPage], 0, _pages[destEndPage], start2, count2);
                    srcEndPage--;
                    count1 = _pageSize - start1;

                    while (destEndPage != destPage)
                    {
                        Array.Copy(_pages[srcEndPage], start1, _pages[destEndPage], 0, count1);
                        destEndPage--;
                        Array.Copy(_pages[srcEndPage], 0, _pages[destEndPage], start2, count2);
                        srcEndPage--;
                    }

                    count1 = (_pageSize - destIndex) - (_pageSize - start1);
                    Array.Copy(_pages[srcPage], srcIndex, _pages[destPage], destIndex, count1);
                    return;
                }
            }
            else if (source > destination)
            {
                if (destIndex > srcIndex)
                {
                    int count1 = _pageCount - destIndex;

                    Array.Copy(_pages[srcPage], srcIndex, _pages[destPage], destIndex, count1);
                    if (srcEndPage == srcPage)
                        return;

                    int start2 = srcIndex + count1;
                    int count2 = _pageCount - start2;

                    destPage++;
                    Array.Copy(_pages[srcPage], start2, _pages[destPage], 0, count2);
                    srcPage++;
                    int start1 = count2;
                    count1 = _pageSize - start1;

                    while (srcEndPage != srcPage)
                    {
                        Array.Copy(_pages[srcPage], 0, _pages[destPage], start1, count1);
                        destPage++;
                        Array.Copy(_pages[srcPage], start2, _pages[destPage], 0, count2);
                        srcPage++;
                    }


                    Array.Copy(_pages[srcPage], 0, _pages[destPage], start1, count1);
                    destPage++;
                    count1 = destEndIndex - start2;
                    Array.Copy(_pages[srcPage], start2, _pages[destPage], 0, count1);
                    return;
                }
                else
                {
                    int count1 = _pageCount - srcIndex;

                    Array.Copy(_pages[srcPage], srcIndex, _pages[destPage], destIndex, count1);
                    if (srcEndPage == srcPage)
                        return;

                    int start2 = destIndex + count1;
                    int count2 = _pageCount - start2;

                    destPage++;
                    Array.Copy(_pages[srcPage], 0, _pages[destPage], start2, count2);
                    srcPage++;
                    int start1 = count2;
                    count1 = _pageSize - start1;

                    while (srcEndPage != srcPage)
                    {
                        Array.Copy(_pages[srcPage], start1, _pages[destPage], 0, count1);
                        destPage++;
                        Array.Copy(_pages[srcPage], 0, _pages[destPage], start2, count2);
                        srcPage++;
                    }


                    Array.Copy(_pages[srcPage], start1, _pages[destPage], 0, count1);
                    destPage++;
                    count1 = destEndIndex - start2;
                    Array.Copy(_pages[srcPage], 0, _pages[destPage], start1, count1);
                    return;
                }
            }
        }

        /// <summary>
        /// Copies all the elements of the current one-dimensional array to the specified
        ///     one-dimensional array starting at the specified destination array index. The
        ///    index is specified as a 32-bit integer.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from
        ///     the current array.</param>
        /// <param name="index">A 32-bit integer that represents the index in array at which copying begins.</param>
        public void CopyTo(Array array, int index)
        {
            for (int i = 0; i < _pageCount; i++)
            {
                Array.Copy(_pages[i], 0, array, index, _pageSize);
                index += _pageSize;
            }
        }

        /// <summary>
        /// Copies all the elements of the current one-dimensional array to the specified
        ///     one-dimensional array starting at the specified destination array index. The
        ///    index is specified as a 32-bit integer.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied from
        ///     the current array.</param>
        /// <param name="index">A 32-bit integer that represents the index in array at which copying begins.</param>
        /// <param name="length">the number of elements to copy</param>
        public void CopyTo(Array array, int index, int length)
        {
            for (int i = 0; i < _pageCount; i++)
            {
                if (length > _pageSize)
                    Array.Copy(_pages[i], 0, array, index, _pageSize);
                else
                {
                    Array.Copy(_pages[i], 0, array, index, length);
                    break;
                }
                length -= _pageSize;
                index += _pageSize;
            }
        }

        ///<inheritdoc/>
        public IEnumerator<TValue> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        private class Enumerator : IEnumerator<TValue>
        {
            PagedArray<TValue> _array;
            int _index;
            int _page;
            bool _start;

            public Enumerator(PagedArray<TValue> array)
            {
                _array = array;
            }

            public TValue Current => _array._pages[_page][_index];

            object IEnumerator.Current => _array._pages[_page][_index];

            public bool MoveNext()
            {
                if (!_start)
                {
                    _start = true;
                    return true;
                }

                _index = (_index >= _array._pageSize) ? 0 : _index++;
                _page = (_index >= _array._pageSize) ? _page++ : _page;
                return _page >= _array._pageCount;
            }

            ///<inheritdoc/>
            public void Reset()
            {
                _index = 0;
                _page = 0;
                _start = false;
            }

            #region IDisposable Support
            private bool disposedValue = false; // To detect redundant calls

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        _array = null;
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
}
