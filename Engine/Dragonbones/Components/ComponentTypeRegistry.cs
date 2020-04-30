using Dragonbones.Systems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Dragonbones.Collections;

namespace Dragonbones.Components
{
    /// <summary>
    /// The base implementation for <see cref="IComponentTypeRegistry"/>
    /// </summary>
#pragma warning disable CA1710 // Identifiers should have correct suffix
    public class ComponentTypeRegistry : IComponentTypeRegistry
#pragma warning restore CA1710 // Identifiers should have correct suffix
    {
        private NamedDataRegistry<IComponentBuffer> _registry;

        /// <summary>
        /// Constructor for <see cref="ComponentTypeRegistry"/>
        /// </summary>
        /// <param name="componentTypeCount">the number of component types that will be registerered here</param>
        /// <param name="hashSize">the size of the hash table, the larger the faster the name search, the more memory it will use</param>
        public ComponentTypeRegistry(int componentTypeCount, int hashSize = 47)
        {
            _registry = new NamedDataRegistry<IComponentBuffer>(componentTypeCount, hashSize);
        }

        ///<inheritdoc />
        public void Clear()
        {
            _registry.Clear();
        }

        ///<inheritdoc/>
        public IComponentBuffer<TComponent> Get<TComponent>(int id) where TComponent : struct, IEquatable<TComponent>
        {
            return _registry.Get(id) as IComponentBuffer<TComponent>;
        }

        ///<inheritdoc/>
        public IComponentBuffer<TComponent> Get<TComponent>(string typeName) where TComponent : struct, IEquatable<TComponent>
        {
            return _registry.Get(typeName) as IComponentBuffer<TComponent>;
        }

        ///<inheritdoc/>
        public IEnumerator<IComponentBuffer> GetEnumerator()
        {
            return _registry.GetEnumerator();
        }

        ///<inheritdoc/>
        public int GetID(string typeName)
        {
            return _registry.GetIDFromName(typeName);
        }

        ///<inheritdoc/>
        public int GetID(IComponentBuffer buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            return _registry.GetIDFromName(buffer.TypeName);
        }

        ///<inheritdoc/>
        public string GetName(int id)
        {
            return _registry.GetNameFromID(id);
        }

        ///<inheritdoc/>
        public string GetName(IComponentBuffer buffer)
        {
            return _registry.GetName(buffer);
        }

        ///<inheritdoc/>
        public int Register(string typeName, IComponentBuffer buffer)
        {
            return _registry.Add(typeName, buffer);
        }

        ///<inheritdoc/>
        public void Remove(int id)
        {
            _registry.RemoveAt(id);
        }

        ///<inheritdoc/>
        public void Remove(string typeName)
        {
            _registry.RemoveAt(typeName);
        }

        ///<inheritdoc/>
        public void Remove(IComponentBuffer buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));
            _registry.RemoveAt(buffer.TypeName);
        }

        ///<inheritdoc/>
        public bool TryGet<TComponent>(int id, out IComponentBuffer<TComponent> buffer) where TComponent : struct, IEquatable<TComponent>
        {
            bool ret = _registry.TryGet(id, out IComponentBuffer temp);
            buffer = temp as IComponentBuffer<TComponent>;
            return ret;
        }

        ///<inheritdoc/>
        public bool TryGet<TComponent>(string typeName, out IComponentBuffer<TComponent> buffer) where TComponent : struct, IEquatable<TComponent>
        {
            bool ret = _registry.TryGet(typeName, out IComponentBuffer temp);
            buffer = temp as IComponentBuffer<TComponent>;
            return ret;
        }

        ///<inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
