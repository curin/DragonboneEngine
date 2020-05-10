using Dragonbones.Systems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Dragonbones.Collections.Paged;

namespace Dragonbones.Components
{
    /// <summary>
    /// The base implementation for <see cref="IComponentTypeRegistry"/>
    /// </summary>
#pragma warning disable CA1710 // Identifiers should have correct suffix
    public class ComponentTypeRegistry : IComponentTypeRegistry
#pragma warning restore CA1710 // Identifiers should have correct suffix
    {
        private readonly NamedDataRegistry<IComponentBuffer> _registry;

        /// <summary>
        /// Constructor for <see cref="ComponentTypeRegistry"/>
        /// </summary>
        /// <param name="pagePower">the size of pages as a power of 2</param>
        /// <param name="pageCount">the number of initial pages, has an affect on early performance but a memory impact</param>
        /// <param name="hashSize">the size of the hash table, the larger the faster the name search, the more memory it will use</param>
        public ComponentTypeRegistry(int pagePower = 8, int pageCount = 1, int hashSize = 47)
        {
            _registry = new NamedDataRegistry<IComponentBuffer>(pagePower, pageCount, hashSize);
        }

        ///<inheritdoc />
        public void Clear()
        {
            _registry.Clear();
        }

        ///<inheritdoc/>
        public IComponentBuffer<TComponent> Get<TComponent>(int id) where TComponent : struct, IEquatable<TComponent>
        {
            return _registry.Get(id) as IComponentBuffer<TComponent>;
        }

        ///<inheritdoc/>
        public IComponentBuffer<TComponent> Get<TComponent>(string typeName) where TComponent : struct, IEquatable<TComponent>
        {
            return _registry.Get(typeName) as IComponentBuffer<TComponent>;
        }

        ///<inheritdoc/>
        public IEnumerator<IComponentBuffer> GetEnumerator()
        {
            return _registry.GetEnumerator();
        }

        ///<inheritdoc/>
        public int GetID(string typeName)
        {
            return _registry.GetIDFromName(typeName);
        }

        ///<inheritdoc/>
        public int GetID(IComponentBuffer buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            return _registry.GetIDFromName(buffer.TypeName);
        }

        ///<inheritdoc/>
        public string GetName(int id)
        {
            return _registry.GetNameFromID(id);
        }

        ///<inheritdoc/>
        public string GetName(IComponentBuffer buffer)
        {
            return _registry.GetName(buffer);
        }

        ///<inheritdoc/>
        public int Register(IComponentBuffer buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            return _registry.Add(buffer.TypeName, buffer);
        }

        ///<inheritdoc/>
        public void Remove(int id)
        {
            _registry.RemoveAt(id);
        }

        ///<inheritdoc/>
        public void Remove(string typeName)
        {
            _registry.RemoveAt(typeName);
        }

        ///<inheritdoc/>
        public void Remove(IComponentBuffer buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            _registry.RemoveAt(buffer.TypeName);
        }

        ///<inheritdoc/>
        public bool TryGet<TComponent>(int id, out IComponentBuffer<TComponent> buffer) where TComponent : struct, IEquatable<TComponent>
        {
            bool ret = _registry.TryGet(id, out IComponentBuffer temp);
            buffer = temp as IComponentBuffer<TComponent>;
            return ret;
        }

        ///<inheritdoc/>
        public bool TryGet<TComponent>(string typeName, out IComponentBuffer<TComponent> buffer) where TComponent : struct, IEquatable<TComponent>
        {
            bool ret = _registry.TryGet(typeName, out IComponentBuffer temp);
            buffer = temp as IComponentBuffer<TComponent>;
            return ret;
        }

        ///<inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Disposes of this object
        /// </summary>
        /// <param name="disposing">Should managed objects be disposed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _registry?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        ///<inheritdoc/>
        public void SwapReadBuffer()
        {
            foreach (IComponentBuffer buffer in this)
                buffer.SwapReadBuffer();
        }

        ///<inheritdoc/>
        public void SwapWriteBuffer()
        {
            foreach (IComponentBuffer buffer in this)
                buffer.SwapWriteBuffer();
        }
        #endregion
    }
}
