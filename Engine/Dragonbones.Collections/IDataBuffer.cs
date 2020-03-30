using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones.Collections
{
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
