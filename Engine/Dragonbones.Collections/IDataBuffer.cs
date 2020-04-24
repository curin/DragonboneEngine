using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones.Collections
{
    /// <summary>
    /// A buffer of data which is used by both Render Systems and Logic Systems
    /// </summary>
    public interface IDataBuffer
    {
        /// <summary>
        /// Swaps the data buffers for rendering
        /// </summary>
        void SwapPresentBuffers();
        /// <summary>
        /// Swaps the data buffers on finishing of updating
        /// </summary>
        void SwapUpdateBuffers();
    }
}
