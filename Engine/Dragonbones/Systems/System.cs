using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones.Systems
{
    /// <summary>
    /// a basic implementation of <see cref="ISystem"/>
    /// This fills out basic functions which are the same for all systems
    /// </summary>
    public abstract class System : ISystem
    {
        /// <summary>
        /// Constructs a system
        /// </summary>
        /// <param name="info">the info for the system</param>
        protected System(SystemInfo info)
        {
            Info = info;
        }

        ///<inheritdoc/>
        public SystemInfo Info { get; }

        ///<inheritdoc/>
        public bool Equals(ISystem other) => Info.Name == other.Info.Name;

        ///<inheritdoc/>
        public virtual void Initialize() { }

        ///<inheritdoc/>
        public virtual void Run(float deltaTime) { }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Dispose of managed objects here
        /// </summary>
        protected virtual void DisposeManaged() { }
        
        /// <summary>
        /// Dispose of Native objects here
        /// </summary>
        protected virtual void DisposeNative() { }

        /// <summary>
        /// Disposes of this object
        /// </summary>
        /// <param name="disposing">should we dispose of managed objects</param>
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DisposeManaged();
                }

                DisposeNative();

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
