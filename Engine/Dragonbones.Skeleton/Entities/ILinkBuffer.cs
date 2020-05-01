using System;
using System.Collections.Generic;
using System.Text;
using Dragonbones.Collections;
using Dragonbones.Systems;

namespace Dragonbones.Entities
{
    /// <summary>
    /// The <see cref="IDataBuffer"/> designed to hold the links between components and entities
    /// This should hold these in such a way that they only update once every frame for polling.
    /// </summary>
    public interface ILinkBuffer : IDataBuffer
    {
        /// <summary>
        /// Adds a link between a component and an entity
        /// Only one link between an entity and a component type can exist
        /// </summary>
        /// <param name="systemType">the type of system making the call, render systems cannot add entity component links</param>
        /// <param name="componentType">The ID of the component type</param>
        /// <param name="entityID">the ID of the entity</param>
        /// <param name="componentID">the ID of the instance of the component</param>
        void Add(SystemType systemType, int componentType, int entityID, int componentID);

        /// <summary>
        /// Retrieve the ID of the component linked to an entity
        /// </summary>
        /// <param name="systemType">the type of system making the call, this depends on where the data comes from</param>
        /// <param name="componentType">the ID of the component type</param>
        /// <param name="entity">the ID of the entity</param>
        /// <returns>the ID of the component of the type related to the entity or -1 if no such link exists</returns>
        int GetComponent(SystemType systemType, int componentType, int entity);

        /// <summary>
        /// Get all components related to an entity
        /// </summary>
        /// <param name="systemType">the type of system making the call, this depends on where the data comes from</param>
        /// <param name="entity">the ID of the entity</param>
        /// <returns>An array of all the componentTypeIDs with the componentIDs the first ID in the tuple is the type, the second is the instance ID</returns>
        Tuple<int, int>[] GetComponents(SystemType systemType, int entity);

        /// <summary>
        /// Get all the entity component links for a specific component type
        /// </summary>
        /// <param name="systemType">the type of system making the call, this depends on where the data comes from</param>
        /// <param name="componentType">The ID of the type of component</param>
        /// <returns></returns>
        EntityComponentLink[] GetLinks(SystemType systemType, int componentType);

        /// <summary>
        /// Does an entity have a link with a component of a specific type
        /// </summary>
        /// <param name="systemType">the type of system making the call, this affects what data is retrieved</param>
        /// <param name="componentType">the ID of the component type</param>
        /// <param name="entity">the ID of the entity</param>
        /// <returns></returns>
        bool ContainsLink(SystemType systemType, int componentType, int entity);

        /// <summary>
        /// Gets component links for all entities which have all component types
        /// </summary>
        /// <param name="systemType">the type of system making the call, this affects what data is retrieved</param>
        /// <param name="componentTypes">the list of component types to look for</param>
        /// <returns>A list of component link arrays, each array is in the same order as the component type array passed and represents a single entity</returns>
        List<EntityComponentLink[]> GetLinks(SystemType systemType, int[] componentTypes);

        /// <summary>
        /// Removes a specific entity component link
        /// </summary>
        /// <param name="systemType">the type of system making the call, render systems cannot make the call</param>
        /// <param name="componentType">the ID of the component type involved in the link</param>
        /// <param name="entity">the ID of the entity involved in the link</param>
        void RemoveLink(SystemType systemType, int componentType, int entity);

        /// <summary>
        /// Removes all links related to a specific component
        /// </summary>
        /// <param name="systemType">the type of system making the call, render systems cannot make the call</param>
        /// <param name="componentType">the ID of the component type involved in the link</param>
        /// <param name="componentID">the ID of the component instance to remove</param>
        void RemoveComponent(SystemType systemType, int componentType, int componentID);

        /// <summary>
        /// Removes all links related to an entity
        /// </summary>
        /// <param name="systemType">the type of system making the call, render systems cannot make the call</param>
        /// <param name="entity">the ID of the entity involved in the links</param>
        void RemoveEntity(SystemType systemType, int entity);

        /// <summary>
        /// Clears all link data
        /// </summary>
        /// <param name="systemType">the type of system making the call, render systems cannot make the call</param>
        void Clear(SystemType systemType);

        /// <summary>
        /// Clears all data for a specific component type
        /// </summary>
        /// <param name="systemType">the type of system making the call, render systems cannot make the call</param>
        /// <param name="componentType">the ID of the component type to clear</param>
        void Clear(SystemType systemType, int componentType);
    }
}
