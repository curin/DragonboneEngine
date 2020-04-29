using System;
using System.Collections.Generic;
using System.Text;
using Dragonbones.Collections;
using Dragonbones.Systems;
using System.Threading;

namespace Dragonbones.Entities
{
    /// <summary>
    /// A default implementation of <see cref="ILinkBuffer"/>
    /// </summary>
    public class LinkBuffer : IDataBuffer
    {
        /// <summary>
        /// Constructs an entity component buffer
        /// </summary>
        /// <param name="componentTypeCount">the number of different component types</param>
        /// <param name="initialComponentSize">the default number of links allocated for a component type. More space is allocated when needed</param>
        public LinkBuffer(int componentTypeCount, int initialComponentSize)
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

        /// <summary>
        /// Retrieve the ID of the component linked to an entity
        /// </summary>
        /// <param name="systemType">the type of system making the call, this depends on where the data comes from</param>
        /// <param name="componentType">the ID of the component type</param>
        /// <param name="entity">the ID of the entity</param>
        /// <returns>the ID of the component of the type related to the entity or -1 if no such link exists</returns>
        public int GetComponent(SystemType systemType, int componentType, int entity)
        {
            BufferTransactionType type = (BufferTransactionType)systemType;
            if (type == BufferTransactionType.WriteRead)
                _lock.EnterReadLock();
            if (_entries.GetLength() <= componentType)
                return -1;
            Entry ent = _entries.Get(type, componentType);
            if (type == BufferTransactionType.WriteRead)
                _lock.ExitReadLock();
            if (FindIndex(type, ent, entity, out _, out EntityComponentLink link))
                return link.ComponentID;
            return -1;
        }

        /// <summary>
        /// Get all components related to an entity
        /// </summary>
        /// <param name="systemType">the type of system making the call, this depends on where the data comes from</param>
        /// <param name="entity">the ID of the entity</param>
        /// <returns>An array of all the componentTypeIDs with the componentIDs the first ID in the tuple is the type, the second is the instance ID</returns>
        public Tuple<int,int>[] GetComponents(SystemType systemType, int entity)
        {
            List<Tuple<int, int>> ret = new List<Tuple<int, int>>();
            BufferTransactionType type = (BufferTransactionType)systemType;
            if (type == BufferTransactionType.WriteRead)
                _lock.EnterReadLock();
            for (int i = 0; i < _entries.GetLength(); i++)
            {
                Entry ent = _entries.Get(type, i);
                if (FindIndex(type, ent, entity, out _, out EntityComponentLink link))
                    ret.Add(new Tuple<int, int>(i, link.ComponentID));
            }
            if (type == BufferTransactionType.WriteRead)
                _lock.ExitReadLock();
            return ret.ToArray();
        }

        /// <summary>
        /// Get all the entity component links for a specific component type
        /// </summary>
        /// <param name="systemType">the type of system making the call, this depends on where the data comes from</param>
        /// <param name="componentType">The ID of the type of component</param>
        /// <returns></returns>
        public EntityComponentLink[] GetLinks(SystemType systemType, int componentType)
        {
            BufferTransactionType type = (BufferTransactionType)systemType;
            if (_entries.GetLength() <= componentType)
                return Array.Empty<EntityComponentLink>();

            if (type == BufferTransactionType.WriteRead)
                _lock.EnterReadLock();
            Entry ent = _entries.Get(type, componentType);
            if (type == BufferTransactionType.WriteRead)
                _lock.ExitReadLock();
            if (ent.Links == null)
                return Array.Empty<EntityComponentLink>();
            if (type == BufferTransactionType.WriteRead)
                Monitor.Enter(ent.Lock);
            EntityComponentLink[] ret = new EntityComponentLink[ent.Top[type]];
            ent.Links.CopyTo(type, ret, 0, ent.Top[type]);
            if (type == BufferTransactionType.WriteRead)
                Monitor.Exit(ent.Lock);
            return ret;
        }

        /// <summary>
        /// Does an entity have a link with a component of a specific type
        /// </summary>
        /// <param name="systemType">the type of system making the call, this affects what data is retrieved</param>
        /// <param name="componentType">the ID of the component type</param>
        /// <param name="entity">the ID of the entity</param>
        /// <returns></returns>
        public bool ContainsLink(SystemType systemType, int componentType, int entity)
        {
            if (_entries.GetLength() <= componentType)
                return false;

            BufferTransactionType type = (BufferTransactionType)systemType;

            if (type == BufferTransactionType.WriteRead)
                _lock.EnterReadLock();
            Entry ent = _entries.Get(type, componentType);
            if (type == BufferTransactionType.WriteRead)
                _lock.ExitReadLock();

            if (FindIndex(type, ent, entity, out _))
                return true;
            return false;
        }

        /// <summary>
        /// Gets component links for all entities which have all component types
        /// </summary>
        /// <param name="systemType">the type of system making the call, this affects what data is retrieved</param>
        /// <param name="componentTypes">the list of component types to look for</param>
        /// <returns>A list of component link arrays, each array is in the same order as the component type array passed and represents a single entity</returns>
        public List<EntityComponentLink[]> GetLinks(SystemType systemType, int[] componentTypes)
        {
            if (componentTypes == null)
                throw new ArgumentNullException(nameof(componentTypes));

            BufferTransactionType type = (BufferTransactionType)systemType;
            List<EntityComponentLink[]> ret = new List<EntityComponentLink[]>();

            EntityComponentLink[] links;

            Entry[] entries = new Entry[componentTypes.Length];
            int smallest = -1;
            int length = int.MaxValue;

            if (type == BufferTransactionType.WriteRead)
                _lock.EnterReadLock();
            for (int i = 0; i < componentTypes.Length; i++)
            {
                entries[i] = _entries.Get(type, componentTypes[i]);
                if (type == BufferTransactionType.WriteRead)
                    Monitor.Enter(entries[i].Lock);
                if (entries[i].Top[type] < length)
                {
                    smallest = i;
                    length = entries[i].Top[type];
                }
            }
            if (type == BufferTransactionType.WriteRead)
                _lock.ExitReadLock();

            links = new EntityComponentLink[componentTypes.Length];
            int[] place = new int[componentTypes.Length];
            for (place[smallest] =0; place[smallest] < length; place[smallest]++)
            {
                links[smallest] = entries[smallest].Links.Get(type, place[smallest]);
                bool clear = false;
                bool finished = false;
                for (int i = 0; i < componentTypes.Length; i++)
                    if (i != smallest)
                    {
                        links[i] = entries[i].Links.Get(type, place[i]);
                        while (links[i].EntityID < links[i].ComponentID)
                        {
                            place[i]++;
                            if (place[i] == entries[i].Top[type])
                            {
                                finished = true;
                                break;
                            }
                            links[i] = entries[i].Links.Get(type, place[i]);
                        }

                        if (finished)
                            break;
                        if (links[i].EntityID > links[i].ComponentID)
                        {
                            clear = true;
                            break;
                        }
                    }
                if (finished)
                    break;
                if (!clear)
                {
                    ret.Add(links);
                    links = new EntityComponentLink[componentTypes.Length];
                }
            }

            if (type == BufferTransactionType.WriteRead)
                for (int i = 0; i < componentTypes.Length; i++)
                    Monitor.Exit(entries[i].Lock);
            return ret;
        }

        /// <summary>
        /// Removes a specific entity component link
        /// </summary>
        /// <param name="systemType">the type of system making the call, render systems cannot make the call</param>
        /// <param name="componentType">the ID of the component type involved in the link</param>
        /// <param name="entity">the ID of the entity involved in the link</param>
        public void RemoveLink(SystemType systemType, int componentType, int entity)
        {
            if (systemType == SystemType.Render)
                return;

            BufferTransactionType type = (BufferTransactionType)systemType;
            _lock.EnterReadLock();

            Entry ent = _entries.Get(type, componentType);
            if (FindIndex(type, ent, entity, out int index))
            {
                lock (ent.Lock)
                {
                    ent.Links.ShiftData(type, index + 1, ent.Top[type] - index - 1, index);
                    ent.Top[type]--;
                }
            }

            _lock.ExitReadLock();
        }

        /// <summary>
        /// Removes all links related to a specific component
        /// </summary>
        /// <param name="systemType">the type of system making the call, render systems cannot make the call</param>
        /// <param name="componentType">the ID of the component type involved in the link</param>
        /// <param name="componentID">the ID of the component instance to remove</param>
        public void RemoveComponent(SystemType systemType, int componentType, int componentID)
        {
            if (systemType == SystemType.Render)
                return;

            BufferTransactionType type = (BufferTransactionType)systemType;
            _lock.EnterReadLock();
            Entry ent = _entries.Get(type, componentType);

            lock (ent.Lock)
            {
                for (int i = 0; i < ent.Top[type]; i++)
                {
                    EntityComponentLink link = ent.Links.Get(type, i);
                    if (link.ComponentID == componentID)
                    {
                        ent.Links.ShiftData(type, i + 1, ent.Top[type] - i - 1, i);
                        ent.Top[type]--;
                        i--;
                    }
                }
            }

            _lock.ExitReadLock();
        }

        /// <summary>
        /// Removes all links related to an entity
        /// </summary>
        /// <param name="systemType">the type of system making the call, render systems cannot make the call</param>
        /// <param name="entity">the ID of the entity involved in the links</param>
        public void RemoveEntity(SystemType systemType, int entity)
        {
            if (systemType == SystemType.Render)
                return;

            BufferTransactionType type = (BufferTransactionType)systemType;
            _lock.EnterReadLock();

            for (int i = 0; i < _entries.GetLength(); i++)
            {
                Entry ent = _entries.Get(type, i);
                if (FindIndex(type, ent, entity, out int index))
                {
                    lock (ent.Lock)
                    {
                        ent.Links.ShiftData(type, index + 1, ent.Top[type] - index - 1, index);
                        ent.Top[type]--;
                    }
                }
            }

            _lock.ExitReadLock();
        }

        /// <summary>
        /// Clears all link data
        /// </summary>
        /// <param name="systemType">the type of system making the call, render systems cannot make the call</param>
        public void Clear(SystemType systemType)
        {
            if (systemType == SystemType.Render)
                return;

            BufferTransactionType type = (BufferTransactionType)systemType;
            _lock.EnterWriteLock();

            for (int i = 0; i < _entries.GetLength(); i++)
            {
                Entry ent = _entries.Get(type, i);
                ent.Top[type] = 0;
                _entries.Set(type, i, ent);
            }

            _lock.ExitWriteLock();
        }

        /// <summary>
        /// Clears all data for a specific component type
        /// </summary>
        /// <param name="systemType">the type of system making the call, render systems cannot make the call</param>
        /// <param name="componentType">the ID of the component type to clear</param>
        public void Clear(SystemType systemType, int componentType)
        {
            if (systemType == SystemType.Render)
                return;

            if (_entries.GetLength() <= componentType)
                return;

            BufferTransactionType type = (BufferTransactionType)systemType;
            _lock.EnterWriteLock();

            Entry ent = _entries.Get(type, componentType);
            ent.Top[type] = 0;
            _entries.Set(type, componentType, ent);

            _lock.ExitWriteLock();
        }

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
            if (ent.Links == null)
            {
                index = 0;
                return false;
            }
            if (type == BufferTransactionType.WriteRead)
                Monitor.Enter(ent.Lock);
            if (ent.Top[type] == 0)
            {
                index = 0;
                if (type == BufferTransactionType.WriteRead)
                    Monitor.Exit(ent.Lock);
                return false;
            }
            EntityComponentLink val = ent.Links.Get(type, 0);
            if (val.EntityID > entityID)
            {
                index = -1;
                if (type == BufferTransactionType.WriteRead)
                    Monitor.Exit(ent.Lock);
                return false;
            }
            if (val.EntityID == entityID)
            {
                index = 0;
                if (type == BufferTransactionType.WriteRead)
                    Monitor.Exit(ent.Lock);
                return true;
            }

            index = ent.Top[type] - 1;
            val = ent.Links.Get(type, index);
            if (val.EntityID < entityID)
            {
                index = ent.Top[type];
                if (type == BufferTransactionType.WriteRead)
                    Monitor.Exit(ent.Lock);
                return false;
            }
            if (val.EntityID == entityID)
            {
                if (type == BufferTransactionType.WriteRead)
                    Monitor.Exit(ent.Lock);
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
                    continue;
                }
                if (val.EntityID < entityID)
                {
                    if (size == 1)
                        break;
                    size >>= 1;
                    index += size;
                    continue;
                }
                if (type == BufferTransactionType.WriteRead)
                    Monitor.Exit(ent.Lock);
                return true;
            }
            if (type == BufferTransactionType.WriteRead)
                Monitor.Exit(ent.Lock);
            return false;
        }

        /// <summary>
        /// Find where an entity exists in an Entry or where it should go
        /// </summary>
        /// <param name="type">the type of buffer transaction, depends on where the data comes from</param>
        /// <param name="ent">the entry to search</param>
        /// <param name="entityID">the entity ID</param>
        /// <param name="index">the returned index of where the entity is or should go</param>
        /// <param name="link">the returned link data of the entity found</param>
        /// <returns>If the entity already has a link in the entry</returns>
        private bool FindIndex(BufferTransactionType type, Entry ent, int entityID, out int index, out EntityComponentLink link)
        {
            
            if (ent.Links == null)
            {
                link = default;
                index = 0;
                return false;
            }
            if (type == BufferTransactionType.WriteRead)
                Monitor.Enter(ent.Lock);
            if (ent.Top[type] == 0)
            {
                index = 0;
                link = default;
                if (type == BufferTransactionType.WriteRead)
                    Monitor.Exit(ent.Lock);
                return false;
            }
            link = ent.Links.Get(type, 0);
            if (link.EntityID > entityID)
            {
                index = -1;
                if (type == BufferTransactionType.WriteRead)
                    Monitor.Exit(ent.Lock);
                return false;
            }
            if (link.EntityID == entityID)
            {
                index = 0;
                if (type == BufferTransactionType.WriteRead)
                    Monitor.Exit(ent.Lock);
                return true;
            }

            index = ent.Top[type] - 1;
            link = ent.Links.Get(type, index);
            if (link.EntityID < entityID)
            {
                index = ent.Top[type];
                if (type == BufferTransactionType.WriteRead)
                    Monitor.Exit(ent.Lock);
                return false;
            }
            if (link.EntityID == entityID)
            {
                if (type == BufferTransactionType.WriteRead)
                    Monitor.Exit(ent.Lock);
                return true;
            }

            int size = ent.Top[type] >> 1;
            index = size;

            while (true)
            {
                link = ent.Links.Get(type, index);
                if (link.EntityID > entityID)
                {
                    if (size == 1)
                    {
                        index -= size;
                        continue;
                    }
                    size >>= 1;
                    index -= size;
                    continue;
                }
                if (link.EntityID < entityID)
                {
                    if (size == 1)
                        break;
                    size >>= 1;
                    index += size;
                    continue;
                }
                if (type == BufferTransactionType.WriteRead)
                    Monitor.Exit(ent.Lock);
                return true;
            }
            if (type == BufferTransactionType.WriteRead)
                Monitor.Exit(ent.Lock);
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
