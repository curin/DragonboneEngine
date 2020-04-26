using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Dragonbones.Systems;

namespace Dragonbones.Components
{
    /// <summary>
    /// The generic form of <see cref="IComponentBuffer"/>
    /// This contains information specific to the component
    /// </summary>
    /// <typeparam name="TComponent">The component type being stored</typeparam>
    public interface IComponentBuffer<TComponent> : IComponentBuffer, IEnumerable<TComponent>
        where TComponent : struct, IEquatable<TComponent>
    {
        /// <summary>
        /// Accesses a component for a system
        /// Render systems cannot set values
        /// </summary>
        /// <param name="systemType">what type of system is accessing the buffer</param>
        /// <param name="id">the ID of the component to access</param>
        /// <returns>The component associated with the ID</returns>
        TComponent this[SystemType systemType, int id] { get; set; }
        /// <summary>
        /// Accesses a component for a system
        /// Render systems cannot set values
        /// </summary>
        /// <param name="systemType">what type of system is accessing the buffer</param>
        /// <param name="name">the name of the component to access</param>
        /// <returns>The component associated with the name</returns>
        TComponent this[SystemType systemType, string name] { get; set; }

        /// <summary>
        /// Adds a component to the buffer
        /// </summary>
        /// <param name="systemType">the type of system making the call, Render systems cannot add components</param>
        /// <param name="name">the name of the component I suggest something like {entityName}.{ComponentType}#</param>
        /// <param name="value">the value to store</param>
        /// <returns>the id associated with the value to store</returns>
        int Add(SystemType systemType, string name, TComponent value);

        /// <summary>
        /// Attempts to retrieve a component for a system
        /// </summary>
        /// <param name="systemType">the type of system making the call</param>
        /// <param name="id">the ID associated with the component</param>
        /// <param name="value">the component associated with the ID</param>
        /// <returns>Whether a component is associated with the given ID</returns>
        bool TryGet(SystemType systemType, int id, out TComponent value);
        /// <summary>
        /// Attempts to retrieve a component for a system
        /// </summary>
        /// <param name="systemType">the type of system making the call</param>
        /// <param name="name">the name associated with the component</param>
        /// <param name="value">the component associated with the name</param>
        /// <returns>Whether a component is associated with the given name</returns>
        bool TryGet(SystemType systemType, string name, out TComponent value);

        /// <summary>
        /// Retrieve a component for a system
        /// </summary>
        /// <param name="systemType">the type of system making the call</param>
        /// <param name="id">the ID associated with the component</param>
        /// <returns>the component associated with the ID</returns>
        TComponent Get(SystemType systemType, int id);
        /// <summary>
        /// Retrieve a component for a system
        /// </summary>
        /// <param name="systemType">the type of system making the call</param>
        /// <param name="name">the name associated with the component</param>
        /// <returns>the component associated with the name</returns>
        TComponent Get(SystemType systemType, string name);

        /// <summary>
        /// Set the value of a component from a system
        /// </summary>
        /// <param name="systemType">the type of system making the call
        /// Set calls are ignored from Render Systems</param>
        /// <param name="id">the ID associated with component</param>
        /// <param name="value">the value to set</param>
        void Set(SystemType systemType, int id, TComponent value);
        /// <summary>
        /// Set the value of a component from a system
        /// </summary>
        /// <param name="systemType">the type of system making the call
        /// Set calls are ignored from Render Systems</param>
        /// <param name="name">the name associated with component</param>
        /// <param name="value">the value to set</param>
        void Set(SystemType systemType, string name, TComponent value);

        /// <summary>
        /// Remove a component matching the given value
        /// </summary>
        /// <param name="systemType">the type of system making the call
        /// Remove calls are ignored from Render Systems</param>
        /// <param name="value">the value to remove</param>
        void Remove(SystemType systemType, TComponent value);

        /// <summary>
        /// Does this Buffer contain a component
        /// </summary>
        /// <param name="systemType">the type of system making the call</param>
        /// <param name="value">the component to check</param>
        /// <returns>Whether the component was found in the buffer</returns>
        bool Contains(SystemType systemType, TComponent value);
        /// <summary>
        /// Get the ID associated with a component
        /// </summary>
        /// <param name="systemType">the type of system making the call</param>
        /// <param name="value">the component</param>
        /// <returns>the ID associated with the component or -1 if not found in the buffer</returns>
        int GetID(SystemType systemType, TComponent value);
        /// <summary>
        /// Get the name associated with a component
        /// </summary>
        /// <param name="systemType">the type of system making the call</param>
        /// <param name="value">the component</param>
        /// <returns>the name associated with a component or "~NOT FOUND~" if not found in the buffer</returns>
        string GetName(SystemType systemType, TComponent value);

        /// <summary>
        /// Retrieve then removes a component from the buffer
        /// </summary>
        /// <param name="systemType">the type of system making the call
        /// Pop calls are ignored from Render Systems</param>
        /// <param name="name">the name associated with the component</param>
        /// <returns>the component associated with the name</returns>
        TComponent PopAt(SystemType systemType, string name);
        /// <summary>
        /// Retrieve then removes a component from the buffer
        /// </summary>
        /// <param name="systemType">the type of system making the call
        /// Pop calls are ignored from Render Systems</param>
        /// <param name="id">the ID associated with the component</param>
        /// <returns>the component associated with the ID</returns>
        TComponent PopAt(SystemType systemType, int id);
        /// <summary>
        /// Retrieve current value of a component then remove it from the buffer
        /// This only works if the <see cref="TComponent"/>.Equals(<see cref="TComponent"/>); is written so that the items don't need to be completely identical
        /// </summary>
        /// <param name="systemType">the type of system making the call
        /// Pop calls are ignored from Render Systems</param>
        /// <param name="value">the component to retrieve and remove</param>
        /// <returns>the updated copy of the component</returns>
        TComponent Pop(SystemType systemType, TComponent value);

        /// <summary>
        /// Attempts to retrieve then remove a component from the buffer
        /// </summary>
        /// <param name="systemType">the type of system making the call
        /// Pop calls are ignored from Render Systems</param>
        /// <param name="name">the name associated with the component</param>
        /// <param name="value">the current value retrieved</param>
        /// <returns>Whether the pop was successful</returns>
        bool TryPopAt(SystemType systemType, string name, out TComponent value);
        /// <summary>
        /// Attempts to retrieve then remove a component from the buffer
        /// </summary>
        /// <param name="systemType">the type of system making the call
        /// Pop calls are ignored from Render Systems</param>
        /// <param name="id">the ID associated with the component</param>
        /// <param name="value">the current value retrieved</param>
        /// <returns>Whether the pop was successful</returns>
        bool TryPopAt(SystemType systemType, int id, out TComponent value);
        /// <summary>
        /// Attempts to retrieve current value of a component then remove it from the buffer
        /// This only works if the <see cref="TComponent"/>.Equals(<see cref="TComponent"/>); is written so that the items don't need to be completely identical
        /// </summary>
        /// <param name="systemType">the type of system making the call
        /// Pop calls are ignored from Render Systems</param>
        /// <param name="value">the component to retrieve and remove</param>
        /// <param name="newValue">the current value retrieved</param>
        /// <returns>Whether the pop was successful</returns>
        bool TryPop(SystemType systemType, TComponent value, out TComponent newValue);
    }
}
