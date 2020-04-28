using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones.Collections
{
    /// <summary>
    /// A buffer designed for multi-threaded reading and writing
    /// where one thread reads and the other thread writes
    /// The write thread may also need to read what it has written
    /// </summary>
    public interface IDataBuffer : IDisposable
    {
        /// <summary>
        /// Swaps the data buffer for reading
        /// </summary>
        void SwapReadBuffer();
        /// <summary>
        /// Swaps the data buffer for writing
        /// Should be done when finished writing
        /// </summary>
        void SwapWriteBuffer();
    }
}
