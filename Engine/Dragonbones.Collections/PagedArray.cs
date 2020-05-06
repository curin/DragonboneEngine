using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Dragonbones;

namespace Dragonbones.Collections
{
    public class PagedArray<TValue> : IEnumerable<TValue>
    {
        private TValue[][] _pages;
        private int _pageSize;
        private int _pageShiftSize;
        private int _pageCount;

        /// <summary>
        /// Constructor for the PagedArray
        /// </summary>
        /// <param name="pagePower">the power of two for the size of pages (example: 8 = 256) </param>
        /// <param name="initialPageCount"></param>
        public PagedArray(int pagePower, int initialPageCount)
        {
            _pages = new TValue[initialPageCount][];
            _pageShiftSize = pagePower;
            _pageSize = 1;
            _pageSize <<= pagePower;

            _pages[0] = new TValue[_pageSize];
            _pageCount = 1;
        }

        public int Length => _pageCount << _pageShiftSize;

        public TValue this[int index]
        {
            get => Get(index);
            set => Set(index, value);
        }

        public TValue Get(int index)
        {
            int page = MathHelper.MathShiftRem(index, _pageSize, _pageShiftSize, out int id);
            if (page >= _pageCount)
                return default;
            return _pages[page][id];
        }

        public bool TryGet(int index, out TValue value)
        {
            int page = MathHelper.MathShiftRem(index, _pageSize, _pageShiftSize, out int id);
            if (page >= _pageCount)
            {
                value = default;
                return false;
            }
            value = _pages[page][id];
            return true;
        }

        public void Set(int index, TValue value)
        {
            int page = Math.DivRem(index, _pageSize, out int id);
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

        public void CopyData(int startIndex, int length, int shiftTo)
        {
            if (startIndex == shiftTo)
                return;

            int srcPage = MathHelper.MathShiftRem(startIndex, _pageSize, _pageShiftSize, out int srcIndex);
            int destPage = MathHelper.MathShiftRem(shiftTo, _pageSize, _pageShiftSize, out int destIndex);
            int destEndPage = MathHelper.MathShiftRem(shiftTo + length, _pageSize, _pageShiftSize, out int destEndIndex);

            if (srcPage == destEndPage)
            {
                Array.Copy(_pages[srcPage], srcIndex, _pages[srcPage], destIndex, length);
                return;
            }

            int srcEndPage = MathHelper.MathShiftRem(startIndex + length, _pageSize, _pageShiftSize, out int srcEndIndex);

            if (startIndex < shiftTo)
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
            else if (startIndex > shiftTo)
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

        public void CopyTo(Array array, int index)
        {
            for (int i = 0; i < _pageCount; i++)
            {
                Array.Copy(_pages[i], 0, array, index, _pageSize);
                index += _pageSize;
            }
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public class Enumerator : IEnumerator<TValue>
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

                _index++;
                if (_index > _array._pageSize)
                {
                    _page++;
                    if (_page > _array._pageCount)
                        return false;
                }
                return true;
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
