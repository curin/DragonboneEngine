using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Dragonbones.Collections;
using Dragonbones.Systems;

namespace Dragonbones.Entities
{
    public interface IEntityBuffer : IDataBuffer, IEnumerable<Tuple<int, string>>
    {
        /// <summary>
        /// Get the ID associated with a specific entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// This determines where the information comes from</param>
        /// <param name="name">the name of the entity</param>
        /// <returns>the ID associated with the entity</returns>
        int this[SystemType systemType, string name] { get; }
        /// <summary>
        /// Access the name associated with an entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// This determines where the information comes from</param>
        /// <param name="id">the ID associated with the entity</param>
        /// <returns>the name of the entity</returns>
        string this[SystemType systemType, int id] { get; set; }
        /// <summary>
        /// retrieve the number of entities contained within this buffer
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// This determines where the information comes from</param>
        /// <returns>the number of entities contained within this buffer</returns>
        int Count(SystemType systemType);
        /// <summary>
        /// Add a new entity to this buffer
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// Render systems cannot create new entities</param>
        /// <param name="name">the name of the new entity</param>
        /// <returns>the ID associated with the new entity</returns>
        int Add(SystemType systemType, string name);
        /// <summary>
        /// Get the ID associated with an entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// This determines where the information comes from</param>
        /// <param name="name">the name of the entity</param>
        /// <returns>the ID associated with the entity or -1 if not found</returns>
        int GetID(SystemType systemType, string name);
        /// <summary>
        /// Get the name of an entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// This determines where the information comes from</param>
        /// <param name="id">the ID associated with the entity</param>
        /// <returns>the name of the entity</returns>
        string GetName(SystemType systemType, int id);
        /// <summary>
        /// Does the buffer contain an entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// This determines where the information comes from</param>
        /// <param name="name">the name of the entity</param>
        /// <returns>Whether the buffer contains the entity</returns>
        bool Contains(SystemType systemType, string name);
        /// <summary>
        /// Does the buffer contain an entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// This determines where the information comes from</param>
        /// <param name="id">the ID associated with the entity</param>
        /// <returns>Whether the buffer contains the entity</returns>
        bool Contains(SystemType systemType, int id);
        /// <summary>
        /// Rename an entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// Render systems cannot rename entities</param>
        /// <param name="oldName">the current name of the entity</param>
        /// <param name="newName">the new name of the entity</param>
        void Rename(SystemType systemType, string oldName, string newName);
        /// <summary>
        /// Rename an entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// Render systems cannot rename entities</param>
        /// <param name="id">the ID associated with the entity</param>
        /// <param name="newName">the new name of the entity</param>
        void Rename(SystemType systemType, int id, string newName);
        /// <summary>
        /// Remove an entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// Render systems cannot remove entities</param>
        /// <param name="name">the name of the entity to remove</param>
        void Remove(SystemType systemType, string name);
        /// <summary>
        /// Remove an entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// Render systems cannot remove entities</param>
        /// <param name="id">the id associated with the entity</param>
        void Remove(SystemType systemType, int id);
        /// <summary>
        /// Clears the entity buffer
        /// !!! This destroys all entities !!!
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// Render systems cannot clear the buffer</param>
        void Clear(SystemType systemType);
    }
}
