using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Dragonbones.Collections;
using Dragonbones.Collections.Paged;
using Dragonbones.Components;
using Dragonbones.Systems;

namespace Dragonbones.Entities
{
    /// <summary>
    /// A basic implementation of <see cref="IEntityBuffer"/>
    /// This is designed for consistent speed especially as the number of entities and components get large
    /// Larger entities(ones which have many components) will be slowed in this implementation
    /// </summary>
#pragma warning disable CA1710 // Identifiers should have correct suffix
    public class EntityBuffer : IEntityBuffer
#pragma warning restore CA1710 // Identifiers should have correct suffix
    {
        readonly NamedDataRegistry<BufferedBinarySearchTree<EntityLink>> _entities;
        PagedArray<SystemLink> _links;
        PagedArray<EntityList> _lists;
        readonly int[] _starts;
        readonly int[] _ends;
        int _topLink;
        int _topList;
        readonly int _hashVal;
        Queue<int> _freeEntriesList = new Queue<int>();
        Queue<int> _freeEntriesLink = new Queue<int>();
        readonly IComponentTypeRegistry _componentTypes;
        readonly ReaderWriterLockSlim _systemLock = new ReaderWriterLockSlim(), _entityLock = new ReaderWriterLockSlim();
        readonly int _listPower, _listPages, _entityPower, _entityPages;
        bool _waitingClear = false;
        readonly List<int> _removal = new List<int>();

        /// <summary>
        /// Constructs an entity buffer
        /// </summary>
        /// <param name="entityHash">the hash value for the entity hash table, larger means faster name searches but more memory</param>
        /// <param name="systemLinkHash">the hash value for system link hash table, larger means faster search but more memory</param>
        /// <param name="entityPagePower">the size of the entity pages in the form of a power of 2</param>
        /// <param name="entityPageCount">the initial number of entity pages, this has a small affect on early performance</param>
        /// <param name="entityComponentPagePower">the size of the entity component pages in the form of a power of 2</param>
        /// <param name="entityComponentPageCount">the initial number of entity pages, this has a small affect on early performance</param>
        /// <param name="entityListPagePower">the size of the entity list pages in the form of a power of 2</param>
        /// <param name="entityListPageCount">the initial number of entity pages, this has a small affect on early performance</param>
        /// <param name="systemLinkPagePower">the size of the systemlink pages in the form of a power of 2</param>
        /// <param name="systemLinkPageCount">the initial number of entity pages, this has a small affect on early performance</param>
        /// <param name="componentTypes">The registry for component types which is used to get type IDs if a system is registered before its type ids are filled out</param>
        public EntityBuffer(IComponentTypeRegistry componentTypes, int entityHash = 47, int systemLinkHash = 47, int entityPagePower = 8, 
            int entityComponentPagePower = 8, int entityListPagePower = 8, int systemLinkPagePower = 8, int entityPageCount = 1, 
            int entityComponentPageCount = 1, int entityListPageCount = 1, int systemLinkPageCount = 1)
        {
            _entities = new NamedDataRegistry<BufferedBinarySearchTree<EntityLink>>(entityPagePower, entityPageCount, entityHash);
            _links = new PagedArray<SystemLink>(systemLinkPagePower, systemLinkPageCount);
            _lists = new PagedArray<EntityList>(systemLinkPagePower, systemLinkPageCount);
            _hashVal = systemLinkHash;
            _starts = new int[_hashVal];
            _ends = new int[_hashVal];
            _listPower = entityListPagePower;
            _listPages = entityListPageCount;
            _entityPower = entityComponentPagePower;
            _entityPages = entityComponentPageCount;
            _componentTypes = componentTypes;
        }

        /// <inheritdoc/>
        public List<int> RemovedEntities => _removal;

        /// <inheritdoc/>
        public int Count => _entities.Count;

        /// <inheritdoc/>
        public string this[int id] { get => GetName(id); }

        /// <inheritdoc/>
        public int this[string name] => GetID(name);

        /// <inheritdoc/>
        public int Add(SystemType systemType, string name)
        {
            if (systemType == SystemType.Render)
                return _entities.GetIDFromName(name);
            BufferedBinarySearchTree<EntityLink> links = new BufferedBinarySearchTree<EntityLink>(-1, _entityPower, _entityPages);
            links.SetID( _entities.Add(name, links));
            return links.ID;
        }

        /// <inheritdoc/>
        public int GetID(string name)
        {
            return _entities.GetIDFromName(name);
        }

        /// <inheritdoc/>
        public string GetName(int entity)
        {
            return _entities.GetNameFromID(entity);
        }

        /// <inheritdoc/>
        public void Rename(SystemType systemType, int id, string newName)
        {
            if (systemType == SystemType.Render)
                return;
            _entities.Rename(id, newName);
        }

        /// <inheritdoc/>
        public void Remove(SystemType systemType, int entity)
        {
            if (systemType == SystemType.Render)
                return;

            _removal.Add(entity);
        }

        private void Remove(int entity)
        {
            if (!_entities.TryGet(entity, out BufferedBinarySearchTree<EntityLink> links))
                throw new ArgumentException("No entity with ID " + entity);

            BufferTransactionType type = BufferTransactionType.WriteRead;
            IEnumerator<EntityList> enumerator = _lists.GetEnumerator();
            EntityList list;
            int i = 0;

            while (enumerator.MoveNext())
            {
                if (i == _topLink)
                    break;
                i++;
                list = enumerator.Current;
                if (list.ReferenceCount == 0)
                    continue;

                if (!links.Any((link) => { return !list.ComponentTypes.Contains(link.ComponentType); }))
                {
                    IEnumerator<int> listWalk = list.Entities.GetEnumerator(type);
                    int j = 0;
                    while (listWalk.MoveNext())
                    {
                        if (j == list.Top)
                            break;
                        j++;

                        if (list.Entities.Get(type, j) == entity)
                        {
                            if (j == list.Top - 1)
                            {
                                list.Top--;
                                _lists.Set(list.Index, list);
                            }
                            else
                                list.FreeEntries.Enqueue(j);
                            list.Entities.Set(type, j, -1);
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public bool ContainsComponent(int entity, int componentType, BufferTransactionType type = BufferTransactionType.ReadOnly)
        {
            if (!_entities.TryGet(entity, out BufferedBinarySearchTree<EntityLink> links))
                throw new ArgumentException("No entity with ID " + entity);
            return links.Contains(type, componentType);
        }

        /// <inheritdoc/>
        public int GetComponent(int entity, int componentType, BufferTransactionType type = BufferTransactionType.ReadOnly)
        {
            if (!_entities.TryGet(entity, out BufferedBinarySearchTree<EntityLink> links))
                throw new ArgumentException("No entity with ID " + entity);

            if (links.TryGet(type, componentType, out EntityLink link))
                return -1;
            return link.Component;
        }

        /// <inheritdoc/>
        public IEnumerable<EntityLink> GetComponents(int entity, BufferTransactionType type = BufferTransactionType.ReadOnly)
        {
            if (!_entities.TryGet(entity, out BufferedBinarySearchTree<EntityLink> links))
                throw new ArgumentException("No entity with ID " + entity);

            return links;
        }

        /// <inheritdoc/>
        public void SetLink(SystemType systemType, int entity, int componentType, int componentID)
        {
            if (systemType == SystemType.Render)
                return;
            if (!_entities.TryGet(entity, out BufferedBinarySearchTree<EntityLink> links))
                throw new ArgumentException("No entity with ID " + entity);

            BufferTransactionType type = (BufferTransactionType)systemType;
            links.Add(type, componentType, new EntityLink(componentType, componentID));

            IEnumerator<EntityList> enumerator = _lists.GetEnumerator();
            EntityList list;
            int i = 0;

            while (enumerator.MoveNext())
            {
                if (i == _topLink)
                    break;
                i++;
                list = enumerator.Current;
                if (list.ReferenceCount == 0)
                    continue;

                if (list.ComponentTypes.Length <= links.Count(type))
                {
                    if (!list.ComponentTypes.Any((comp) => { return !links.Contains(type, comp); }))
                    {
                        int listNext = (list.FreeEntries.Count == 0) ? list.Top : list.FreeEntries.Dequeue();
                        list.Entities.Set(type, listNext, links.ID);
                        if (listNext == list.Top)
                        {
                            list.Top++;
                            _lists.Set(list.Index, list);
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void RemoveLink(SystemType systemType, int entity, int componentType)
        {
            if (systemType == SystemType.Render)
                return;
            if (!_entities.TryGet(entity, out BufferedBinarySearchTree<EntityLink> links))
                throw new ArgumentException("No entity with ID " + entity);

            BufferTransactionType type = (BufferTransactionType)systemType;
            links.Remove(type, componentType);

            IEnumerator<EntityList> enumerator = _lists.GetEnumerator();
            EntityList list;
            int i = 0;

            while (enumerator.MoveNext())
            {
                if (i == _topLink)
                    break;
                i++;
                list = enumerator.Current;
                if (list.ReferenceCount == 0)
                    continue;

                if (list.ComponentTypes.Contains(componentType))
                {
                    IEnumerator<int> listWalk = list.Entities.GetEnumerator(type);
                    int j = 0;
                    while (listWalk.MoveNext())
                    {
                        if (j == list.Top)
                            break;
                        j++;

                        if (list.Entities.Get(type, j) == entity)
                        {
                            if (j == list.Top - 1)
                            {
                                list.Top--;
                                _lists.Set(list.Index, list);
                            }
                            else
                                list.FreeEntries.Enqueue(j);
                            list.Entities.Set(type, j, -1);
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void RegisterSystem(ISystem system)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));

            AddSystem(system.Info, out _);
        }

        /// <inheritdoc/>
        public IEnumerable<int> GetEntities(ISystem system)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));

            AddSystem(system.Info, out SystemLink link);
            return _lists.Get(link.Index);
        }

        /// <inheritdoc/>
        public void RemoveSystem(int systemID)
        {
            _systemLock.EnterWriteLock();
            if (!FindSystem(systemID, out SystemLink link))
            {
                _systemLock.ExitWriteLock();
                return;
            }

            int hash = GetHashIndex(systemID);
            int index;
            SystemLink temp;
            if (link.Previous == -1)
            {
                index = _starts[hash];
                _starts[hash] = link.Next;
            }
            else
            {
                temp = _links.Get(link.Previous);
                index = temp.Next;
                temp.Next = link.Next;
                _links.Set(link.Previous, temp);
            }

            if (link.Next == -1)
            {
                _ends[hash] = link.Previous;
            }
            else
            {
                temp = _links.Get(link.Next);
                temp.Previous = link.Previous;
                _links.Set(link.Next, temp);
            }

            EntityList list = _lists.Get(link.Index);
            if (list.ReferenceCount == 1)
            {
                list.Entities.Dispose();
                if (link.Index == _topList - 1)
                    _topList--;
                else
                    _freeEntriesList.Enqueue(link.Index);
            }
            list.ReferenceCount--;
            _lists.Set(link.Index, list);

            if (index == _topLink - 1)
                _topLink--;
            else
                _freeEntriesList.Enqueue(index);
        }

        private void AddSystem(SystemInfo sysInf, out SystemLink link)
        {
            _systemLock.EnterWriteLock();
            if (FindSystem(sysInf.ID, out link))
            {
                _systemLock.ExitWriteLock();
                return;
            }
            SystemLink prev = link;
            
            List<int> temp = new List<int>();
            SystemComponent comp;
            for (int i = 0; i < sysInf.Components.Length; i++)
            {
                comp = sysInf.Components[i];
                if (comp.TypeID == -1)
                {
                    comp.SetID(_componentTypes.GetID(comp.TypeName));
                    sysInf.Components[i] = comp;
                }
                if (comp.Required)
                    temp.Add(comp.TypeID);
            }

            int[] components = temp.ToArray();
            int next;
            int hash = GetHashIndex(sysInf.ID);
            if (FindList(components, out EntityList list))
            {
                link = new SystemLink(sysInf.ID, list.Index, _ends[hash]);
                list.ReferenceCount++;
                _lists.Set(list.Index, list);
            }
            else
            {
                next = (_freeEntriesList.Count == 0) ? _topList : _freeEntriesList.Dequeue();
                link = new SystemLink(sysInf.ID, next, _ends[hash]);
                list = new EntityList(next, components, _listPower, _listPages);
                _entityLock.EnterReadLock();
                IEnumerator<BufferedBinarySearchTree<EntityLink>> enumer = _entities.GetEnumerator();
                while (enumer.MoveNext())
                {
                    if (!components.Any((component) => { return !enumer.Current.Contains(BufferTransactionType.WriteRead, component); }))
                    {
                        int listNext = (list.FreeEntries.Count == 0) ? list.Top : list.FreeEntries.Dequeue();
                        list.Entities.Set(BufferTransactionType.WriteRead, listNext, enumer.Current.ID);
                        if (listNext == list.Top)
                            list.Top++;
                    }
                }
                _entityLock.ExitReadLock();

                list.Entities.SwapWriteBuffer();
                list.Entities.SwapReadBuffer();

                _lists.Set(next, list);
                if (next == _topList)
                    _topList++;
            }

            next = (_freeEntriesLink.Count == 0) ? _topLink : _freeEntriesLink.Dequeue();
            if (_ends[hash] != -1)
            {
                prev.Next = next;
                _links.Set(_ends[hash], prev);
            }
            _ends[hash] = next;
            _links.Set(next, link);
            if (next == _topLink)
                _topLink++;
            _systemLock.ExitWriteLock();
        }

        private bool FindSystem(int systemID, out SystemLink link)
        {
            int index = _starts[GetHashIndex(systemID)];

            if (index == -1)
            {
                link = default;
                return false;
            }

            link = _links.Get(index);

            while (link.SystemID != systemID && link.Next != -1)
                link = _links.Get(link.Next);

            return link.SystemID == systemID;
        }

        private bool FindList(int[] components, out EntityList list)
        {
            IEnumerator<EntityList> enumerator = _lists.GetEnumerator();
            list = default;
            int i = 0;

            while (enumerator.MoveNext())
            {
                if (i == _topLink)
                    break;
                i++;
                list = enumerator.Current;
                if (list.ReferenceCount == 0)
                    continue;

                

                if (list.ComponentTypes.Length != components.Length)
                    continue;

                if (!list.ComponentTypes.Any((component) => { return !components.Contains(component); }))
                    return true;
            }

            return false;
        }

        private int GetHashIndex(int value) { return value % _hashVal; }

        /// <inheritdoc/>
        public bool Contains(string name)
        {
            return _entities.ContainsName(name);
        }

        /// <inheritdoc/>
        public bool Contains(int id)
        {
            return _entities.ContainsID(id);
        }

        /// <inheritdoc/>
        public void ClearEntities(SystemType systemType)
        {
            if (systemType == SystemType.Render)
                return;

            _waitingClear = true;

            IEnumerator<EntityList> enumerator = _lists.GetEnumerator();
            EntityList list;
            int i = 0;

            while (enumerator.MoveNext())
            {
                if (i == _topLink)
                    break;
                i++;
                list = enumerator.Current;
                if (list.ReferenceCount == 0)
                    continue;

                list.Top = 0;
                _lists.Set(list.Index, list);
            }
        }

        /// <inheritdoc/>
        public void ClearSystems(SystemType systemType)
        {
            if (systemType == SystemType.Render)
                return;

            _topList = 0;
            _topLink = 0;
            _freeEntriesLink.Clear();
            _freeEntriesList.Clear();
        }

        /// <inheritdoc/>
        public void Clear(SystemType systemType)
        {
            ClearSystems(systemType);
            ClearEntities(systemType);
        }

        /// <inheritdoc/>
        public void SwapReadBuffer()
        {
            IEnumerator<EntityList> enumerator = _lists.GetEnumerator();
            EntityList list;
            int i = 0;
            while (enumerator.MoveNext())
            {
                if (i == _topLink)
                    break;
                i++;
                list = enumerator.Current;
                if (list.ReferenceCount == 0)
                    continue;

                list.Entities.SwapReadBuffer();
            }
        }

        /// <inheritdoc/>
        public void SwapWriteBuffer()
        {
            if (_waitingClear)
                _entities.Clear();
            else if (_removal.Count > 0)
            {
                foreach (int entity in _removal)
                    Remove(entity);
                _removal.Clear();
            }
            

            IEnumerator<EntityList> enumerator = _lists.GetEnumerator();
            EntityList list;
            int i = 0;
            while (enumerator.MoveNext())
            {
                if (i == _topLink)
                    break;
                i++;
                list = enumerator.Current;
                if (list.ReferenceCount == 0)
                    continue;

                list.Entities.SwapWriteBuffer();
            }
        }

        /// <inheritdoc/>
        public IEnumerator<Tuple<int, string>> GetEnumerator()
        {
            return _entities.GetNameEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _entities.GetNameEnumerator();
        }

        struct SystemLink
        {
            public SystemLink(int systemID, int index, int previous, int next = -1)
            {
                SystemID = systemID;
                Index = index;
                Previous = previous;
                Next = next;
            }

            public int SystemID;
            public int Next;
            public int Index;
            public int Previous;
        }

        struct EntityList : IEnumerable<int>
        {
            public EntityList(int index, int[] componentTypes, int pagePower, int pageCount)
            {
                Index = index;
                ComponentTypes = componentTypes;
                Top = 0;
                ReferenceCount = 1;
                Entities = new DataBuffer<int>(pagePower, pageCount);
                FreeEntries = new Queue<int>();
            }

            public int Index;
            public int ReferenceCount;
            public int Top;
            public Queue<int> FreeEntries;
            public DataBuffer<int> Entities;
            public int[] ComponentTypes;

            public IEnumerator<int> GetEnumerator()
            {
                return new Enumerator(this);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new Enumerator(this);
            }

            class Enumerator : IEnumerator<int>
            {
                EntityList _list;
                IEnumerator<int> _enumer;
                int i;
                public Enumerator(EntityList list)
                {
                    _list = list;
                    _enumer = list.GetEnumerator();
                    i = 0;
                }
                public int Current => _enumer.Current;

                object IEnumerator.Current => _enumer.Current;

                public void Dispose()
                {
                    _enumer?.Dispose();
                }

                public bool MoveNext()
                {
                    do
                    {
                        i++;
                        _enumer.MoveNext();
                    } while (_enumer.Current == -1);

                    return _list.Top > i;
                }

                public void Reset()
                {
                    i = 0;
                    _enumer.Reset();
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Dispose of this object
        /// </summary>
        /// <param name="disposing">should managed objects be disposed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _entities?.Dispose();
                    _entityLock?.Dispose();
                    _systemLock?.Dispose();
                    _freeEntriesLink = null;
                    _freeEntriesList = null;
                    _links = null;
                    _lists = null;
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
