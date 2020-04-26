using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Dragonbones.Collections;
using Dragonbones.Systems;

namespace Dragonbones.Components
{
    /// <summary>
    /// A buffer to store all component types 
    /// </summary>
    public interface IComponentBuffer : IDataBuffer, IEnumerable
    {
        /// <summary>
        /// The user defined typeName
        /// be sure to make this as unique as possible
        /// (use project specific identifiers like {StudioName}.{ProjectName}.{ComponentName})
        /// </summary>
        string TypeName { get; }
        /// <summary>
        /// A ID assigned by the system to this Buffer
        /// Should be assigned by SetBufferID function
        /// </summary>
        int BufferID { get; }
        /// <summary>
        /// Sets the BufferID field
        /// Used by the system to set the buffer's ID
        /// !!! DO NOT USE THIS FUNCTION !!!
        /// </summary>
        /// <param name="id">The ID to set to this buffer</param>
        void SetBufferID(int id);
        /// <summary>
        /// Gets the count of entries in the buffer
        /// </summary>
        /// <param name="systemType">the type of system making the call which affects where in the buffer the data is retrieved</param>
        /// <returns>the count of entries in the buffer from the perspective of the system</returns>

        int Count(SystemType systemType);

        /// <summary>
        /// Attempts to shrink the buffer to a new capacity.
        /// If it cannot it will shrink as small as it can
        /// </summary>
        /// <param name="newCapacity">the capacity to attempt to shrink to</param>
        void Constrict(int newCapacity);
        /// <summary>
        /// Expands the buffer to the new capacity
        /// </summary>
        /// <param name="newCapacity">the capacity to expand to</param>
        void Expand(int newCapacity);

        /// <summary>
        /// Checks if there is a stored component associated with the given ID
        /// </summary>
        /// <param name="id">the ID to check</param>
        /// <param name="systemType">the type of system making the call</param>
        /// <returns>Whether the ID is associated with a stored component</returns>
        bool ContainsID(SystemType systemType, int id);
        /// <summary>
        /// Check if there is a stored component associated with the given name
        /// </summary>
        /// <param name="name">the name to check</param>
        /// <param name="systemType">the type of system making the call</param>
        /// <returns>Whether the name is associated with a stored component</returns>
        bool ContainsName(SystemType systemType, string name);
        /// <summary>
        /// Gets the ID associated with the given name
        /// </summary>
        /// <param name="systemType">the type of system making the call</param>
        /// <param name="name">the name to find the ID of</param>
        /// <returns>the ID associated with the given name or -1 if not found in the buffer</returns>
        int GetIDFromName(SystemType systemType, string name);
        /// <summary>
        /// Removes a component associated with the given ID
        /// </summary>
        /// <param name="systemType">the type of system making the call
        /// Remove calls are ignored from Render Systems</param>
        /// <param name="id">the ID associated with the component to remove</param>
        void RemoveAt(SystemType systemType, int id);
        /// <summary>
        /// Removes a component associated with the given name
        /// </summary>
        /// <param name="systemType">the type of system making the call
        /// Remove calls are ignored from Render Systems</param>
        /// <param name="name"></param>
        void RemoveAt(SystemType systemType, string name);
        /// <summary>
        /// Remove all data from the buffer
        /// </summary>
        /// <param name="systemType">the type of system making the call
        /// Clear calls are ignored from Render Systems</param>
        void Clear(SystemType systemType);

    }
}
