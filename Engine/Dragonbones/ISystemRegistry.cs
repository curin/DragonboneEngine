using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "Not a collection it should be smarter, a registry")]
    public interface ISystemRegistry : IEnumerable<ISystem>
    {
        /// <summary>
        /// Retrieve a particular system
        /// </summary>
        /// <param name="systemName">the name of the system</param>
        /// <returns>the system</returns>
        ISystem this[string systemName] { get; }
        /// <summary>
        /// Retrieve a particular system
        /// </summary>
        /// <param name="id">the id of the system</param>
        /// <returns>the system</returns>
        ISystem this[int id] { get; }

        /// <summary>
        /// The number of systems registered
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Get the number of systems registered of a specific type
        /// </summary>
        /// <param name="type">the type of the systems</param>
        /// <returns>the number of systems of the given type</returns>
        int GetTypeCount(SystemType type);

        /// <summary>
        /// Get a system's id
        /// </summary>
        /// <param name="systemName">the name of the system</param>
        /// <returns>the id of the system</returns>
        int GetID(string systemName);
        /// <summary>
        /// Get a system's id
        /// </summary>
        /// <param name="system">the system</param>
        /// <returns>the id of the system</returns>
        int GetID(ISystem system);

        /// <summary>
        /// Get a system's name
        /// </summary>
        /// <param name="id">the id of the system</param>
        /// <returns>the name of the system</returns>
        string GetName(int id);

        /// <summary>
        /// Retrieve a particular system
        /// </summary>
        /// <param name="systemName">the name of the system</param>
        /// <returns>the system</returns>
        ISystem GetSystem(string systemName);
        /// <summary>
        /// Retrieve a particular system
        /// </summary>
        /// <param name="id">the id of the system</param>
        /// <returns>the system</returns>
        ISystem GetSystem(int id);

        /// <summary>
        /// Does the registry contain a system
        /// </summary>
        /// <param name="systemName">the name of the system</param>
        /// <returns>if the system is stored</returns>
        bool Contains(string systemName);
        /// <summary>
        /// is the registry using a system
        /// </summary>
        /// <param name="id">the id</param>
        /// <returns>if the id is used</returns>
        bool Contains(int id);
        /// <summary>
        /// Does the registry contain a system
        /// </summary>
        /// <param name="system">the system</param>
        /// <returns>if the system is stored</returns>
        bool Contains(ISystem system);

        /// <summary>
        /// Add a system to the registry
        /// </summary>
        /// <param name="system">the system to add</param>
        /// <returns>if the system was added successfully</returns>
        bool Add(ISystem system);

        /// <summary>
        /// Remove a system from the registry
        /// </summary>
        /// <param name="systemName">the name of the system</param>
        void Remove(string systemName);
        /// <summary>
        /// Remove a system from the registry
        /// </summary>
        /// <param name="id">the id of the system</param>
        void Remove(int id);
        /// <summary>
        /// Remove a system from the registry
        /// </summary>
        /// <param name="system">the system</param>
        void Remove(ISystem system);

        /// <summary>
        /// Clear the registry of all data
        /// </summary>
        void Clear();
    }
}
