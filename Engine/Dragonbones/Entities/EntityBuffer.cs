using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Dragonbones.Collections;
using Dragonbones.Systems;
using System.Threading;

namespace Dragonbones.Entities
{
    /// <summary>
    /// A Default implementation of <see cref="IEntityBuffer"/>
    /// </summary>
    public class EntityBuffer : IEntityBuffer
    {
        private NameBuffer _buffer;
        private List<int> _removeList = new List<int>();
        private bool _clearCall;
        private ReaderWriterLockSlim _logicLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Constructor for Entity buffer
        /// </summary>
        /// <param name="initialCapacity">the initial capacity for the buffer</param>
        /// <param name="hashSize">the size of the buffer's hashtable the larger the quicker name searches are but the more memory used</param>
        public EntityBuffer(int initialCapacity = 64, int hashSize = 47)
        {
            _buffer = new NameBuffer(initialCapacity, hashSize);
        }

        /// <inheritdoc />
        public List<int> RemovedEntities => _removeList;
        
        /// <summary>
        /// Swaps the data buffer for reading
        /// </summary>
        public void SwapReadBuffer()
        {
            _buffer.SwapReadBuffer();
        }

        /// <summary>
        /// Swaps the data buffer for writing
        /// Should be done when finished writing
        /// </summary>
        public void SwapWriteBuffer()
        {
            if (_clearCall)
            {
                _buffer.Clear(BufferTransactionType.WriteRead);
                _clearCall = false;
            }
            else
            {
                foreach (int remove in _removeList)
                    _buffer.RemoveAt(BufferTransactionType.WriteRead, remove);
                _removeList.Clear();
            }
            _buffer.SwapWriteBuffer();
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<Tuple<int, string>> GetEnumerator()
        {
            return _buffer.GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Get the ID associated with a specific entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// This determines where the information comes from</param>
        /// <param name="name">the name of the entity</param>
        /// <returns>the ID associated with the entity</returns>
        public int this[SystemType systemType, string name] => GetID(systemType, name);

        /// <summary>
        /// Access the name associated with an entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// This determines where the information comes from</param>
        /// <param name="id">the ID associated with the entity</param>
        /// <returns>the name of the entity</returns>
        public string this[SystemType systemType, int id]
        {
            get => GetName(systemType, id);
            set => Rename(systemType, id, value);
        }

        /// <summary>
        /// retrieve the number of entities contained within this buffer
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// This determines where the information comes from</param>
        /// <returns>the number of entities contained within this buffer</returns>
        public int Count(SystemType systemType)
        {
            if (systemType == SystemType.Render)
                return _buffer.Count((BufferTransactionType) systemType);
            _logicLock.EnterReadLock();
            int count = _buffer.Count((BufferTransactionType)systemType);
            _logicLock.ExitReadLock();
            return count;
        }

        /// <summary>
        /// Add a new entity to this buffer
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// Render systems cannot create new entities</param>
        /// <param name="name">the name of the new entity</param>
        /// <returns>the ID associated with the new entity</returns>
        public int Add(SystemType systemType, string name)
        {
            if (systemType == SystemType.Render)
                return -1;
            _logicLock.EnterWriteLock();
            int id = _buffer.Add((BufferTransactionType) systemType, name);
            _logicLock.ExitWriteLock();
            return id;
        }

        /// <summary>
        /// Get the ID associated with an entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// This determines where the information comes from</param>
        /// <param name="name">the name of the entity</param>
        /// <returns>the ID associated with the entity or -1 if not found</returns>
        public int GetID(SystemType systemType, string name)
        {
            if (systemType == SystemType.Render)
                return _buffer.GetIDFromName((BufferTransactionType)systemType, name);
            _logicLock.EnterReadLock();
            int id = _buffer.GetIDFromName((BufferTransactionType)systemType, name);
            _logicLock.ExitReadLock();
            return id;
        }

        /// <summary>
        /// Get the name of an entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// This determines where the information comes from</param>
        /// <param name="id">the ID associated with the entity</param>
        /// <returns>the name of the entity</returns>
        public string GetName(SystemType systemType, int id)
        {
            if (systemType == SystemType.Render)
                return _buffer.Get((BufferTransactionType)systemType, id);
            _logicLock.EnterReadLock();
            string name = _buffer.Get((BufferTransactionType)systemType, id);
            _logicLock.ExitReadLock();
            return name;
        }

        /// <summary>
        /// Does the buffer contain an entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// This determines where the information comes from</param>
        /// <param name="name">the name of the entity</param>
        /// <returns>Whether the buffer contains the entity</returns>
        public bool Contains(SystemType systemType, string name)
        {
            if (systemType == SystemType.Render)
                return _buffer.ContainsName((BufferTransactionType)systemType, name);
            _logicLock.EnterReadLock();
            bool ret = _buffer.ContainsName((BufferTransactionType)systemType, name);
            _logicLock.ExitReadLock();
            return ret;
        }

        /// <summary>
        /// Does the buffer contain an entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// This determines where the information comes from</param>
        /// <param name="id">the ID associated with the entity</param>
        /// <returns>Whether the buffer contains the entity</returns>
        public bool Contains(SystemType systemType, int id)
        {
            if (systemType == SystemType.Render)
                return _buffer.ContainsID((BufferTransactionType)systemType, id);
            _logicLock.EnterReadLock();
            bool ret = _buffer.ContainsID((BufferTransactionType)systemType, id);
            _logicLock.ExitReadLock();
            return ret;
        }

        /// <summary>
        /// Rename an entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// Render systems cannot rename entities</param>
        /// <param name="oldName">the current name of the entity</param>
        /// <param name="newName">the new name of the entity</param>
        public void Rename(SystemType systemType, string oldName, string newName)
        {
            if (systemType == SystemType.Render)
                return;
            _logicLock.EnterWriteLock();
            _buffer.Rename((BufferTransactionType)systemType, oldName, newName);
            _logicLock.ExitWriteLock();
        }

        /// <summary>
        /// Rename an entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// Render systems cannot rename entities</param>
        /// <param name="id">the ID associated with the entity</param>
        /// <param name="newName">the new name of the entity</param>
        public void Rename(SystemType systemType, int id, string newName)
        {
            if (systemType == SystemType.Render)
                return;
            _logicLock.EnterWriteLock();
            _buffer.Rename((BufferTransactionType)systemType, id, newName);
            _logicLock.ExitWriteLock();
        }

        /// <summary>
        /// Remove an entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// Render systems cannot remove entities</param>
        /// <param name="name">the name of the entity to remove</param>
        public void Remove(SystemType systemType, string name)
        {
            if (systemType == SystemType.Render)
                return;
            _logicLock.EnterReadLock();
            int id = _buffer.GetIDFromName((BufferTransactionType)systemType, name);
            _logicLock.ExitReadLock();
            _removeList.Add(id);
        }

        /// <summary>
        /// Remove an entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// Render systems cannot remove entities</param>
        /// <param name="id">the id associated with the entity</param>
        public void Remove(SystemType systemType, int id)
        {
            if (systemType == SystemType.Render)
                return;
            _removeList.Add(id);
        }

        /// <summary>
        /// Clears the entity buffer
        /// !!! This destroys all entities !!!
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// Render systems cannot clear the buffer</param>
        public void Clear(SystemType systemType)
        {
            if (systemType == SystemType.Render)
                return;
            _clearCall = true;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="managed">should managed resources be disposed</param>
        protected virtual void Dispose(bool managed)
        {
            if (!managed) return;
            _buffer?.Dispose();
            _logicLock?.Dispose();
            _removeList = null;
        }
    }
}
