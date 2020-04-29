using System;
using System.Collections.Generic;
using System.Text;
using Dragonbones.Collections;
using Dragonbones.Systems;
using System.Threading;

namespace Dragonbones.Entities
{
    /// <summary>
    /// A buffer containing the links between entities and components

    /// </summary>
    public class EntityComponentBuffer : IDataBuffer
    {
        public EntityComponentBuffer(int componentTypeCount, int initialComponentSize)
        {
            _entries = new DataBuffer<Entry>(componentTypeCount);
            _initialSize = initialComponentSize;
        }

        DataBuffer<Entry> _entries;
        ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        int _initialSize;

        ///<inheritdoc/>
        public void SwapReadBuffer()
        {
            for (int i = 0; i < _entries.GetLength(); i++)
            {
                Entry ent = _entries.Get(BufferTransactionType.ReadOnly, i);
                ent.Top?.SwapReadBuffer();
                ent.Links?.SwapReadBuffer();
            }
            _entries.SwapReadBuffer();
        }

        ///<inheritdoc/>
        public void SwapWriteBuffer()
        {
            for (int i = 0; i < _entries.GetLength(); i++)
            {
                Entry ent = _entries.Get(BufferTransactionType.WriteRead, i);
                ent.Top?.SwapWriteBuffer();
                ent.Links?.SwapWriteBuffer();
            }
            _entries.SwapWriteBuffer();
        }

        /// <summary>
        /// Adds a link between a component and an entity
        /// Only one link between an entity and a component type can exist
        /// </summary>
        /// <param name="systemType">the type of system making the call, render systems cannot add entity component links</param>
        /// <param name="componentType">The ID of the component type</param>
        /// <param name="entityID">the ID of the entity</param>
        /// <param name="componentID">the ID of the instance of the component</param>
        public void Add(SystemType systemType,int componentType, int entityID, int componentID)
        {
            if (systemType == SystemType.Render)
                return;
            BufferTransactionType type = (BufferTransactionType)systemType;
            _lock.EnterUpgradeableReadLock();
            if (componentType >= _entries.GetLength())
                _entries = new DataBuffer<Entry>(_entries, componentType + 1);


            Entry ent = _entries.Get(type, componentType);
            _lock.EnterWriteLock();
            if (ent.Links == null)
            {
                ent.Links = new DataBuffer<EntityComponentLink>(_initialSize);
                ent.Lock = new object();
                ent.Top = new ValueBuffer<int>();
            }
            _entries.Set(type, componentType, ent);
            _lock.ExitWriteLock();
            _lock.ExitUpgradeableReadLock();

            lock (ent.Lock)
            {
                if (ent.Links.GetLength() == ent.Top[type])
                    ent.Links = new DataBuffer<EntityComponentLink>(ent.Links, ent.Top[type] << 1);

                if (!FindIndex(type, ent, entityID, out int index))
                {
                    ent.Links.ShiftData(type, index, ent.Top[type] - index, index + 1);
                }
                ent.Links.Set(type, index, new EntityComponentLink(entityID, componentID));
            }
        }

        //
        //
        // TODO FUNCTIONS:
        // REMOVE LINK
        // ENTITY GROUP FIND (from array of component type IDs
        // Clear
        // Does Link Exist
        // Get Entity Components
        // Get Component (entity and component type)
        //
        //

        /// <summary>
        /// Find where an entity exists in an Entry or where it should go
        /// </summary>
        /// <param name="type">the type of buffer transaction, depends on where the data comes from</param>
        /// <param name="ent">the entry to search</param>
        /// <param name="entityID">the entity ID</param>
        /// <param name="index">the returned index of where the entity is or should go</param>
        /// <returns>If the entity already has a link in the entry</returns>
        private bool FindIndex(BufferTransactionType type, Entry ent, int entityID, out int index)
        {
            if (ent.Top[type] == 0)
            {
                index = 0;
                return false;
            }
            EntityComponentLink val = ent.Links.Get(type, 0);
            if (val.EntityID > entityID)
            {
                index = -1;
                return false;
            }
            if (val.EntityID == entityID)
            {
                index = 0;
                return true;
            }

            index = ent.Top[type] - 1;
            val = ent.Links.Get(type, index);
            if (val.EntityID < entityID)
            {
                index = ent.Top[type];
                return false;
            }
            if (val.EntityID == entityID)
            {
                return true;
            }

            int size = ent.Top[type] >> 1;
            index = size;

            while (true)
            {
                val = ent.Links.Get(type, index);
                if (val.EntityID > entityID)
                {
                    if (size == 1)
                    {
                        index -= size;
                        continue;
                    }
                    size >>= 1;
                    index -= size;
                }
                if (val.EntityID < entityID)
                {
                    if (size == 1)
                        break;
                    size >>= 1;
                    index += size;
                }
                return true;
            }
            return false;
        }

        private struct Entry
        {
            /// <summary>
            /// The top entry in the Links buffer
            /// </summary>
            public ValueBuffer<int> Top { get; set; }
            public object Lock { get; set; }
            /// <summary>
            /// The buffer of entity component links for this component type
            /// </summary>
            public DataBuffer<EntityComponentLink> Links { get; set; }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Dispose this object
        /// </summary>
        /// <param name="disposing">Are managed objects being disposed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_entries != null)
                    {
                        for (int i = 0; i < _entries.GetLength(); i++)
                        {
                            Entry ent = _entries[BufferTransactionType.WriteRead, i];
                            ent.Top?.Dispose();
                            ent.Links?.Dispose();
                        }
                        _entries?.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~EntityComponentBuffer()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

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
