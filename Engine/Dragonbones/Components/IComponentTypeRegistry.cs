using System;
using System.Collections.Generic;
using System.Text;
using Dragonbones.Systems;

namespace Dragonbones.Components
{
    /// <summary>
    /// A Data Registry meant to store the different types of <see cref="IComponentBuffer"/>
    /// This is suited for quick lookup and not quick removal as it should happen rarely that a component type is removed.
    /// </summary>
#pragma warning disable CA1710 // Identifiers should have correct suffix
    public interface IComponentTypeRegistry : IEnumerable<IComponentBuffer>
#pragma warning restore CA1710 // Identifiers should have correct suffix
    {
        /// <summary>
        /// Registers a componentBuffer with this registry
        /// </summary>
        /// <param name="typeName">the name of the type of component stored in the buffer</param>
        /// <param name="buffer">the buffer to register</param>
        /// <returns>the ID associated with the componentbuffer</returns>
        int Register(string typeName, IComponentBuffer buffer);


        /// <summary>
        /// Get a component buffer from this registry
        /// </summary>
        /// <typeparam name="TComponent">The type of component stored in the buffer</typeparam>
        /// <param name="id">the ID associated with the buffer</param>
        /// <returns>The Component buffer associated with the given ID</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Get is not reserved, get is")]
        IComponentBuffer<TComponent> Get<TComponent>(int id)
            where TComponent : struct, IEquatable<TComponent>;
        /// <summary>
        /// Get a component buffer from this registry
        /// </summary>
        /// <typeparam name="TComponent">The type of component stored in the buffer</typeparam>
        /// <param name="typeName">the name to the type stored in the buffer</param>
        /// <returns>The Component buffer associated with the given name</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Get is not reserved, get is")]
        IComponentBuffer<TComponent> Get<TComponent>(string typeName)
            where TComponent : struct, IEquatable<TComponent>;

        /// <summary>
        /// Attempts to get a component buffer from this registry
        /// </summary>
        /// <typeparam name="TComponent">The type of component stored in the buffer</typeparam>
        /// <param name="id">the ID associated with the buffer</param>
        /// <param name="buffer">The Component buffer associated with the given ID</param>
        /// <returns>Whether the buffer was found in this registry</returns>
        bool TryGet<TComponent>(int id, out IComponentBuffer<TComponent> buffer)
            where TComponent : struct, IEquatable<TComponent>;
        /// <summary>
        /// Attempts to get a component buffer from this registry
        /// </summary>
        /// <typeparam name="TComponent">The type of component stored in the buffer</typeparam>
        /// <param name="typeName">the name to the type stored in the buffer</param>
        /// <param name="buffer">The Component buffer associated with the given name</param>
        /// <returns>Whether the buffer was found in this registry</returns>
        bool TryGet<TComponent>(string typeName, out IComponentBuffer<TComponent> buffer)
            where TComponent : struct, IEquatable<TComponent>;

        /// <summary>
        /// Get the ID of a buffer registered here
        /// </summary>
        /// <param name="typeName">the name of the type stored in the buffer</param>
        /// <returns>the ID of the buffer</returns>
        int GetID(string typeName);
        /// <summary>
        /// Get the name of a buffer registered here
        /// </summary>
        /// <param name="id">the ID of the buffer</param>
        /// <returns>the name of the type stored in the buffer</returns>
        string GetName(int id);
        /// <summary>
        /// Get the ID of a buffer registered here
        /// </summary>
        /// <param name="buffer">the buffer</param>
        /// <returns>the ID of the buffer</returns>
        int GetID(IComponentBuffer buffer);
        /// <summary>
        /// Get the name of a buffer registered here
        /// </summary>
        /// <param name="buffer">the buffer</param>
        /// <returns>the name of the type stored in the buffer</returns>
        string GetName(IComponentBuffer buffer);
        /// <summary>
        /// Removes a buffer from the registry
        /// </summary>
        /// <param name="id">the ID of the buffer</param>
        void Remove(int id);
        /// <summary>
        /// Remove a buffer from the registry
        /// </summary>
        /// <param name="typeName">the name of the type stored in the buffer</param>
        void Remove(string typeName);
        /// <summary>
        /// Removes a buffer from the registry
        /// </summary>
        /// <param name="buffer">the buffer to remove</param>
        void Remove(IComponentBuffer buffer);
        /// <summary>
        /// Clears all the registry data
        /// </summary>
        void Clear();
    }
}
