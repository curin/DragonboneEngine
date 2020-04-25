using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones.Collections
{
    /// <summary>
    /// This refers to the type of transaction being made to a buffer
    /// </summary>
    public enum BufferTransactionType
    {
        /// <summary>
        /// Readonly transactions read from the read buffer and are unable to write
        /// </summary>
        ReadOnly = 2,
        /// <summary>
        /// WriteRead transactions read and write to the write buffer
        /// </summary>
        WriteRead = 0
    }
}
