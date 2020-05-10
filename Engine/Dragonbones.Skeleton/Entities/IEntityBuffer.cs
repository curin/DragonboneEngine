using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Dragonbones.Collections;
using Dragonbones.Systems;

namespace Dragonbones.Entities
{
    /// <summary>
    /// The buffer designed to hold entity data
    /// This buffer also holds the ties between entities and their components
    /// and is responsible for giving this data to systems when they need it
    /// Some interactions like remove should be postponed until the end of a frame
    /// (when SwapWriteBuffer is called)
    /// </summary>
#pragma warning disable CA1710 // Identifiers should have correct suffix
    public interface IEntityBuffer : IDataBuffer, IEnumerable<Tuple<int, string>>
#pragma warning restore CA1710 // Identifiers should have correct suffix
    {
        /// <summary>
        /// Get the ID associated with a specific entity
        /// </summary>
        /// <param name="name">the name of the entity</param>
        /// <returns>the ID associated with the entity</returns>
        int this[string name] { get; }
        /// <summary>
        /// Access the name associated with an entity
        /// </summary>
        /// <param name="id">the ID associated with the entity</param>
        /// <returns>the name of the entity</returns>
        string this[int id] { get; }
        /// <summary>
        /// retrieve the number of entities contained within this buffer
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// This determines where the information comes from</param>
        /// <returns>the number of entities contained within this buffer</returns>
        int Count { get; }
        /// <summary>
        /// The entities flagged for removal
        /// </summary>
        List<int> RemovedEntities { get; }
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
        int GetID(string name);
        /// <summary>
        /// Get the name of an entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// This determines where the information comes from</param>
        /// <param name="id">the ID associated with the entity</param>
        /// <returns>the name of the entity</returns>
        string GetName(int id);
        /// <summary>
        /// Does the buffer contain an entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// This determines where the information comes from</param>
        /// <param name="name">the name of the entity</param>
        /// <returns>Whether the buffer contains the entity</returns>
        bool Contains(string name);
        /// <summary>
        /// Does the buffer contain an entity
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// This determines where the information comes from</param>
        /// <param name="id">the ID associated with the entity</param>
        /// <returns>Whether the buffer contains the entity</returns>
        bool Contains(int id);
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
        /// <param name="id">the id associated with the entity</param>
        void Remove(SystemType systemType, int id);
        /// <summary>
        /// Clears the entities
        /// !!! This destroys all entities !!!
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// Render systems cannot clear the buffer</param>
        void ClearEntities(SystemType systemType);
        /// <summary>
        /// Clears the systems stored
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// Render systems cannot clear the buffer</param>
        void ClearSystems(SystemType systemType);

        /// <summary>
        /// Clears all data stored
        /// </summary>
        /// <param name="systemType">The type of system accessing this buffer
        /// Render systems cannot clear the buffer</param>
        void Clear(SystemType systemType);

        /// <summary>
        /// Does an entity contain a specific component type
        /// </summary>
        /// <param name="entity">the id of the entity</param>
        /// <param name="componentType">the id of the component type to look for</param>
        /// <param name="type">the type of buffer to access, this affects the data being returned</param>
        /// <returns>Whether the entity contains the given component</returns>
        bool ContainsComponent(int entity, int componentType, BufferTransactionType type = BufferTransactionType.ReadOnly);

        /// <summary>
        /// retrieve the ID of a component of a given type linked to an entity
        /// </summary>
        /// <param name="entity">the id of the entity</param>
        /// <param name="componentType">the id of the component type to look for</param>
        /// <param name="type">the type of buffer to access, this affects the data being returned</param>
        /// <returns>the ID of the component or -1 if no link exists</returns>

        int GetComponent(int entity, int componentType, BufferTransactionType type = BufferTransactionType.ReadOnly);

        /// <summary>
        /// Creates or edits a link between an entity and a component (only a single instance of a component can be linked at a time)
        /// </summary>
        /// <param name="systemType">the type of system making the call, calls from render systems are ignored</param>
        /// <param name="entity">the id of the entity being linked</param>
        /// <param name="componentType">the id of the type of component being linked</param>
        /// <param name="componentID">the id of the component being linked</param>
        void SetLink(SystemType systemType, int entity, int componentType, int componentID);

        /// <summary>
        /// Remove a link between an entity and a component type
        /// </summary>
        /// <param name="systemType">the type of system making the call, calls from render systems are ignored</param>
        /// <param name="entity">the id of the entity</param>
        /// <param name="componentType">the id of the type of component being unlinked</param>

        void RemoveLink(SystemType systemType, int entity, int componentType);
        /// <summary>
        /// Retrieves a list of components linked to an entity
        /// </summary>
        /// <param name="entity">the ID of the entity</param>
        /// <param name="type">the type of buffer to access, this affects the data being returned</param>
        /// <returns>The list of components linked</returns>
        IEnumerable<EntityLink> GetComponents(int entity, BufferTransactionType type = BufferTransactionType.ReadOnly);


        /// <summary>
        /// Registers a system in this buffer (doing this ahead of time can speed up the first attempt to retrieve entities that match this system)
        /// </summary>
        /// <param name="system">the system to register</param>
        void RegisterSystem(ISystem system);
        /// <summary>
        /// Retrieve the entities that match a given system's component requirements
        /// </summary>
        /// <param name="system">the system to retireve against</param>
        /// <returns>the list of entity ids</returns>
        IEnumerable<int> GetEntities(ISystem system);

        /// <summary>
        /// Removes a system from this buffer (this should be done if a system is removed so if a new system gets it's ID it doesn't get a bad list of entities)
        /// </summary>
        /// <param name="systemID">the id of the system to be removed</param>
        void RemoveSystem(int systemID);
        
    }
}
