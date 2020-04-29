using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Dragonbones.Collections;
using Dragonbones.Systems;

namespace Dragonbones.Components
{
    /// <summary>
    /// A base implementation of <see cref="IComponentBuffer{TComponent}"/>
    /// </summary>
    /// <typeparam name="TComponent">the component type stored</typeparam>
#pragma warning disable CA1710 // Identifiers should have correct suffix
    public class ComponentBuffer<TComponent> : IComponentBuffer<TComponent>
#pragma warning restore CA1710 // Identifiers should have correct suffix
    where TComponent: struct, IEquatable<TComponent>
    {
        private readonly NamedDataBuffer<TComponent> _buffer;
        private string _typeName;
        private int _bufferID = -1;
        private bool _waitingClear;

        /// <summary>
        /// Constructs the component buffer
        /// </summary>
        /// <param name="initialCapacity">the initial size of the buffer</param>
        /// <param name="hashSize">the size of the buffer's hash table, the larger the faster the name lookup but the more memory used</param>
        /// <param name="typeName">The name of the component type stored here</param>
        public ComponentBuffer(string typeName, int initialCapacity, int hashSize = 47)
        {
            _buffer = new NamedDataBuffer<TComponent>(initialCapacity, hashSize);
            _bufferID = -1;
            _typeName = typeName;
        }

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
            if (_waitingClear)
            {
                _buffer.Clear(BufferTransactionType.WriteRead);
                _waitingClear = false;
            }
            _buffer.SwapWriteBuffer();
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<TComponent> GetEnumerator()
        {
            return _buffer.GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _buffer.GetEnumerator();
        }

        /// <summary>
        /// The user defined typeName
        /// be sure to make this as unique as possible
        /// (use project specific identifiers like {StudioName}.{ProjectName}.{ComponentName})
        /// </summary>
        public string TypeName => _typeName;

        /// <summary>
        /// A ID assigned by the system to this Buffer
        /// Should be assigned by SetBufferID function
        /// </summary>
        public int BufferID => _bufferID;

        /// <inheritdoc/>
        public bool WaitingClear => _waitingClear;

        /// <summary>
        /// Sets the BufferID field
        /// Used by the system to set the buffer's ID
        /// !!! DO NOT USE THIS FUNCTION !!!
        /// </summary>
        /// <param name="id">The ID to set to this buffer</param>
        public void SetBufferID(int id)
        {
            _bufferID = id;
        }

        /// <summary>
        /// Gets the count of entries in the buffer
        /// </summary>
        /// <param name="systemType">the type of system making the call which affects where in the buffer the data is retrieved</param>
        /// <returns>the count of entries in the buffer from the perspective of the system</returns>
        public int Count(SystemType systemType)
        {
            return _buffer.Count((BufferTransactionType) systemType);
        }

        /// <summary>
        /// Attempts to shrink the buffer to a new capacity.
        /// If it cannot it will shrink as small as it can
        /// </summary>
        /// <param name="newCapacity">the capacity to attempt to shrink to</param>
        public void Constrict(int newCapacity)
        {
            _buffer.Constrict(newCapacity);
        }

        /// <summary>
        /// Expands the buffer to the new capacity
        /// </summary>
        /// <param name="newCapacity">the capacity to expand to</param>
        public void Expand(int newCapacity)
        {
            _buffer.Expand(newCapacity);
        }

        /// <summary>
        /// Checks if there is a stored component associated with the given ID
        /// </summary>
        /// <param name="id">the ID to check</param>
        /// <param name="systemType">the type of system making the call</param>
        /// <returns>Whether the ID is associated with a stored component</returns>
        public bool ContainsID(SystemType systemType, int id)
        {
            return _buffer.ContainsID((BufferTransactionType) systemType, id);
        }

        /// <summary>
        /// Check if there is a stored component associated with the given name
        /// </summary>
        /// <param name="name">the name to check</param>
        /// <param name="systemType">the type of system making the call</param>
        /// <returns>Whether the name is associated with a stored component</returns>
        public bool ContainsName(SystemType systemType, string name)
        {
            return _buffer.ContainsName((BufferTransactionType) systemType, name);
        }

        /// <summary>
        /// Gets the ID associated with the given name
        /// </summary>
        /// <param name="systemType">the type of system making the call</param>
        /// <param name="name">the name to find the ID of</param>
        /// <returns>the ID associated with the given name or -1 if not found in the buffer</returns>
        public int GetIDFromName(SystemType systemType, string name)
        {
            return _buffer.GetIDFromName((BufferTransactionType) systemType, name);
        }

        /// <summary>
        /// Removes a component associated with the given ID
        /// </summary>
        /// <param name="systemType">the type of system making the call
        /// Remove calls are ignored from Render Systems</param>
        /// <param name="id">the ID associated with the component to remove</param>
        public void RemoveAt(SystemType systemType, int id)
        {
            _buffer.RemoveAt((BufferTransactionType)systemType, id);
        }

        /// <summary>
        /// Removes a component associated with the given name
        /// </summary>
        /// <param name="systemType">the type of system making the call
        /// Remove calls are ignored from Render Systems</param>
        /// <param name="name"></param>
        public void RemoveAt(SystemType systemType, string name)
        {
            _buffer.RemoveAt((BufferTransactionType)systemType, name);
        }

        /// <summary>
        /// Remove all data from the buffer
        /// </summary>
        /// <param name="systemType">the type of system making the call
        /// Clear calls are ignored from Render Systems</param>
        public void Clear(SystemType systemType)
        {
            _waitingClear = true;
        }

        /// <summary>
        /// Accesses a component for a system
        /// Render systems cannot set values
        /// </summary>
        /// <param name="systemType">what type of system is accessing the buffer</param>
        /// <param name="id">the ID of the component to access</param>
        /// <returns>The component associated with the ID</returns>
        public TComponent this[SystemType systemType, int id]
        {
            get => _buffer.Get((BufferTransactionType)systemType, id);
            set => _buffer.Set((BufferTransactionType)systemType, id, value);
        }

        /// <summary>
        /// Accesses a component for a system
        /// Render systems cannot set values
        /// </summary>
        /// <param name="systemType">what type of system is accessing the buffer</param>
        /// <param name="name">the name of the component to access</param>
        /// <returns>The component associated with the name</returns>
        public TComponent this[SystemType systemType, string name]
        {
            get => _buffer.Get((BufferTransactionType)systemType, name);
            set => _buffer.Set((BufferTransactionType)systemType, name, value);
        }

        /// <summary>
        /// Adds a component to the buffer
        /// </summary>
        /// <param name="systemType">the type of system making the call, Render systems cannot add components</param>
        /// <param name="name">the name of the component I suggest something like {entityName}.{ComponentType}#</param>
        /// <param name="value">the value to store</param>
        /// <returns>the id associated with the value to store</returns>
        public int Add(SystemType systemType, string name, TComponent value)
        {
            return _buffer.Add((BufferTransactionType) systemType, name, value);
        }

        /// <summary>
        /// Attempts to retrieve a component for a system
        /// </summary>
        /// <param name="systemType">the type of system making the call</param>
        /// <param name="id">the ID associated with the component</param>
        /// <param name="value">the component associated with the ID</param>
        /// <returns>Whether a component is associated with the given ID</returns>
        public bool TryGet(SystemType systemType, int id, out TComponent value)
        {
            return _buffer.TryGet((BufferTransactionType) systemType, id, out value);
        }

        /// <summary>
        /// Attempts to retrieve a component for a system
        /// </summary>
        /// <param name="systemType">the type of system making the call</param>
        /// <param name="name">the name associated with the component</param>
        /// <param name="value">the component associated with the name</param>
        /// <returns>Whether a component is associated with the given name</returns>
        public bool TryGet(SystemType systemType, string name, out TComponent value)
        {
            return _buffer.TryGet((BufferTransactionType) systemType, name, out value);
        }

        /// <summary>
        /// Retrieve a component for a system
        /// </summary>
        /// <param name="systemType">the type of system making the call</param>
        /// <param name="id">the ID associated with the component</param>
        /// <returns>the component associated with the ID</returns>
        public TComponent Get(SystemType systemType, int id)
        {
            return _buffer.Get((BufferTransactionType) systemType, id);
        }

        /// <summary>
        /// Retrieve a component for a system
        /// </summary>
        /// <param name="systemType">the type of system making the call</param>
        /// <param name="name">the name associated with the component</param>
        /// <returns>the component associated with the name</returns>
        public TComponent Get(SystemType systemType, string name)
        {
            return _buffer.Get((BufferTransactionType) systemType, name);
        }

        /// <summary>
        /// Set the value of a component from a system
        /// </summary>
        /// <param name="systemType">the type of system making the call
        /// Set calls are ignored from Render Systems</param>
        /// <param name="id">the ID associated with component</param>
        /// <param name="value">the value to set</param>
        public void Set(SystemType systemType, int id, TComponent value)
        {
            _buffer.Set((BufferTransactionType)systemType, id, value);
        }

        /// <summary>
        /// Set the value of a component from a system
        /// </summary>
        /// <param name="systemType">the type of system making the call
        /// Set calls are ignored from Render Systems</param>
        /// <param name="name">the name associated with component</param>
        /// <param name="value">the value to set</param>
        public void Set(SystemType systemType, string name, TComponent value)
        {
            _buffer.Set((BufferTransactionType)systemType, name, value);
        }

        /// <summary>
        /// Remove a component matching the given value
        /// </summary>
        /// <param name="systemType">the type of system making the call
        /// Remove calls are ignored from Render Systems</param>
        /// <param name="value">the value to remove</param>
        public void Remove(SystemType systemType, TComponent value)
        {
            _buffer.Remove((BufferTransactionType)systemType, value);
        }

        /// <summary>
        /// Does this Buffer contain a component
        /// </summary>
        /// <param name="systemType">the type of system making the call</param>
        /// <param name="value">the component to check</param>
        /// <returns>Whether the component was found in the buffer</returns>
        public bool Contains(SystemType systemType, TComponent value)
        {
            return _buffer.Contains((BufferTransactionType) systemType, value);
        }

        /// <summary>
        /// Get the ID associated with a component
        /// </summary>
        /// <param name="systemType">the type of system making the call</param>
        /// <param name="value">the component</param>
        /// <returns>the ID associated with the component or -1 if not found in the buffer</returns>
        public int GetID(SystemType systemType, TComponent value)
        {
            return _buffer.GetID((BufferTransactionType) systemType, value);
        }

        /// <summary>
        /// Get the name associated with a component
        /// </summary>
        /// <param name="systemType">the type of system making the call</param>
        /// <param name="value">the component</param>
        /// <returns>the name associated with a component or "~NOT FOUND~" if not found in the buffer</returns>
        public string GetName(SystemType systemType, TComponent value)
        {
            return _buffer.GetName((BufferTransactionType) systemType, value);
        }

        /// <summary>
        /// Retrieve then removes a component from the buffer
        /// </summary>
        /// <param name="systemType">the type of system making the call
        /// Pop calls are ignored from Render Systems</param>
        /// <param name="name">the name associated with the component</param>
        /// <returns>the component associated with the name</returns>
        public TComponent PopAt(SystemType systemType, string name)
        {
            return _buffer.PopAt((BufferTransactionType) systemType, name);
        }

        /// <summary>
        /// Retrieve then removes a component from the buffer
        /// </summary>
        /// <param name="systemType">the type of system making the call
        /// Pop calls are ignored from Render Systems</param>
        /// <param name="id">the ID associated with the component</param>
        /// <returns>the component associated with the ID</returns>
        public TComponent PopAt(SystemType systemType, int id)
        {
            return _buffer.PopAt((BufferTransactionType)systemType, id);
        }


#pragma warning disable CS1723 // XML comment has cref attribute that refers to a type parameter
        /// <summary>
        /// Retrieve current value of a component then remove it from the buffer
        /// This only works if the <see cref="TComponent"/>.Equals(<see cref="TComponent"/>); is written so that the items don't need to be completely identical
        /// </summary>
        /// <param name="systemType">the type of system making the call
        /// Pop calls are ignored from Render Systems</param>
        /// <param name="value">the component to retrieve and remove</param>
        /// <returns>the updated copy of the component</returns>
        public TComponent Pop(SystemType systemType, TComponent value)
#pragma warning restore CS1723 // XML comment has cref attribute that refers to a type parameter
        {
            return _buffer.Pop((BufferTransactionType)systemType, value);
        }

        /// <summary>
        /// Attempts to retrieve then remove a component from the buffer
        /// </summary>
        /// <param name="systemType">the type of system making the call
        /// Pop calls are ignored from Render Systems</param>
        /// <param name="name">the name associated with the component</param>
        /// <param name="value">the current value retrieved</param>
        /// <returns>Whether the pop was successful</returns>
        public bool TryPopAt(SystemType systemType, string name, out TComponent value)
        {
            return _buffer.TryPopAt((BufferTransactionType)systemType, name, out value);
        }

        /// <summary>
        /// Attempts to retrieve then remove a component from the buffer
        /// </summary>
        /// <param name="systemType">the type of system making the call
        /// Pop calls are ignored from Render Systems</param>
        /// <param name="id">the ID associated with the component</param>
        /// <param name="value">the current value retrieved</param>
        /// <returns>Whether the pop was successful</returns>
        public bool TryPopAt(SystemType systemType, int id, out TComponent value)
        {
            return _buffer.TryPopAt((BufferTransactionType)systemType, id, out value);
        }


#pragma warning disable CS1723 // XML comment has cref attribute that refers to a type parameter
        /// <summary>
        /// Attempts to retrieve current value of a component then remove it from the buffer
        /// This only works if the <see cref="TComponent"/>.Equals(<see cref="TComponent"/>); is written so that the items don't need to be completely identical
        /// </summary>
        /// <param name="systemType">the type of system making the call
        /// Pop calls are ignored from Render Systems</param>
        /// <param name="value">the component to retrieve and remove</param>
        /// <param name="newValue">the current value retrieved</param>
        /// <returns>Whether the pop was successful</returns>
        public bool TryPop(SystemType systemType, TComponent value, out TComponent newValue)
#pragma warning restore CS1723 // XML comment has cref attribute that refers to a type parameter
        {
            return _buffer.TryPop((BufferTransactionType)systemType, value, out newValue);
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
        /// <param name="managed">dispose managed objects?</param>
        protected virtual void Dispose(bool managed)
        {
            if (!managed) return;
            _buffer?.Dispose();
            _typeName = null;
        }

        ///<inheritdoc/>
        public bool Equals(IComponentBuffer other)
        {
            return other.TypeName == TypeName;
        }
    }
}
