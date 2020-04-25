using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Dragonbones.Collections;

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
        /// Should be assigned by SetTypeID function
        /// </summary>
        int BufferID { get; }
        /// <summary>
        /// Sets the BufferID field
        /// Used by the system to set the buffer's ID
        /// !!! DO NOT USE THIS FUNCTION !!!
        /// </summary>
        /// <param name="id">The ID to set to this buffer</param>
        void SetTypeID(int id);
        /// <summary>
        /// The number of components stored for rendering
        /// </summary>
        int RenderCount { get; }
        /// <summary>
        /// The number of components stored in the Logic portion of the buffer
        /// </summary>
        int LogicCount { get; }
        
        /// <summary>
        /// Checks if there is a stored component associated with the given ID
        /// </summary>
        /// <param name="id">the ID to check</param>
        /// <returns>Whether the ID is associated with a stored component</returns>
        bool ContainsID(int id);
        /// <summary>
        /// Check if there is a stored component associated with the given name
        /// </summary>
        /// <param name="name">the name to check</param>
        /// <returns>Whether the name is associated with a stored component</returns>
        bool ContainsName(string name);
        /// <summary>
        /// Gets the ID associated with the given name
        /// </summary>
        /// <param name="name">the name to find the ID of</param>
        /// <returns>the ID associated with the given name or -1 if not found in the buffer</returns>
        int GetIDFromName(string name);
        /// <summary>
        /// Removes a component associated with the given ID
        /// </summary>
        /// <param name="id">the ID associated with the component to remove</param>
        void RemoveAt(int id);
        /// <summary>
        /// Removes a component associated with the given name
        /// </summary>
        /// <param name="name"></param>
        void RemoveAt(string name);
        /// <summary>
        /// Remove all data from the buffer
        /// </summary>
        void Clear();

    }
}
