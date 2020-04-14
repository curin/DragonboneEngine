using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Dragonbones.Collections;

namespace Dragonbones
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "This is not just a collection, but smarter so a registry")]
    public class SystemRegistry : ISystemRegistry
    {
        NamedDataRegistry<ISystem> _systems;
        Dictionary<SystemType, int> _typeCounts = new Dictionary<SystemType, int>();

        public SystemRegistry(int MaxSystemCount, int hashSize = 47)
        {
            _systems = new NamedDataRegistry<ISystem>(MaxSystemCount, hashSize);
        }

        public ISystem this[string systemName] => _systems[systemName];

        public ISystem this[int id] => _systems[id];

        public int Count => _systems.Count;

        public bool Add(ISystem system)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));
            int id = _systems.Add(system.SystemInfo.Name, system);
            if (id == -1)
                return false;
            system.SystemInfo.SetID(id);
            if (!_typeCounts.ContainsKey(system.SystemInfo.Type))
                _typeCounts.Add(system.SystemInfo.Type, 0);
            _typeCounts[system.SystemInfo.Type]++;
            return true;
        }

        public void Clear()
        {
            _systems.Clear();
        }

        public bool Contains(string systemName)
        {
            return _systems.ContainsName(systemName);
        }

        public bool Contains(int id)
        {
            return _systems.ContainsID(id);
        }

        public bool Contains(ISystem system)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));
            return Contains(system.SystemInfo.Name);
        }

        public IEnumerator<ISystem> GetEnumerator()
        {
            return _systems.GetEnumerator();
        }

        public int GetID(string systemName)
        {
            return _systems.GetID(systemName);
        }

        public int GetID(ISystem system)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));
            return _systems.GetID(system.SystemInfo.Name);
        }

        public string GetName(int id)
        {
            return _systems[id].SystemInfo.Name;
        }

        public ISystem GetSystem(string systemName)
        {
            return _systems.Get(systemName);
        }

        public ISystem GetSystem(int id)
        {
            return _systems.Get(id);
        }

        public int GetTypeCount(SystemType type)
        {
            if (_typeCounts.ContainsKey(type))
                return _typeCounts[type];
            return 0;
        }

        public void Remove(string systemName)
        {
            int preCount = _systems.Count;
            ISystem system = _systems.PopAt(systemName);
            if (preCount < _systems.Count)
                _typeCounts[system.SystemInfo.Type]--;
            system.SystemInfo.SetID(-1);
        }

        public void Remove(int id)
        {
            int preCount = _systems.Count;
            ISystem system = _systems.PopAt(id);
            if (preCount < _systems.Count)
                _typeCounts[system.SystemInfo.Type]--;
            system.SystemInfo.SetID(-1);
        }

        public void Remove(ISystem system)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));
            int preCount = _systems.Count;
            _systems.PopAt(system.SystemInfo.Name);
            if (preCount < _systems.Count)
                _typeCounts[system.SystemInfo.Type]--;
            system.SystemInfo.SetID(-1);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _systems.GetEnumerator();
        }
    }
}
