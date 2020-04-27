using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Dragonbones.Collections;
using Dragonbones.Systems;

namespace Dragonbones.Entities
{
    public class EntityBuffer : IEntityBuffer
    {
        

        public EntityBuffer(int initialCapacity = 64)
        {
            
        }

        
        /// <summary>
        /// Swaps the data buffer for reading
        /// </summary>
        public void SwapReadBuffer()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Swaps the data buffer for writing
        /// Should be done when finished writing
        /// </summary>
        public void SwapWriteBuffer()
        {
            throw new NotImplementedException();
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<Tuple<int, string>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
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
        public int this[SystemType systemType, string name] => throw new NotImplementedException();

        /// <summary>
        /// Access the name associated with an entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// This determines where the information comes from</param>
        /// <param name="id">the ID associated with the entity</param>
        /// <returns>the name of the entity</returns>
        public string this[SystemType systemType, int id]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// retrieve the number of entities contained within this buffer
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// This determines where the information comes from</param>
        /// <returns>the number of entities contained within this buffer</returns>
        public int Count(SystemType systemType)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        /// <summary>
        /// Remove an entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// Render systems cannot remove entities</param>
        /// <param name="name">the name of the entity to remove</param>
        public void Remove(SystemType systemType, string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Remove an entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// Render systems cannot remove entities</param>
        /// <param name="id">the id associated with the entity</param>
        public void Remove(SystemType systemType, int id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clears the entity buffer
        /// !!! This destroys all entities !!!
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// Render systems cannot clear the buffer</param>
        public void Clear(SystemType systemType)
        {
            throw new NotImplementedException();
        }
    }
}
