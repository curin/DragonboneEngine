using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Dragonbones.Collections;

namespace Dragonbones.Systems
{
    /// <summary>
    /// A base version of the <see cref="SystemRegistry"/>
    ///
    /// This uses a <see cref="NamedDataRegistry{ISystem}"/> to store the systems
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "This is not just a collection, but smarter so a registry")]
    public class SystemRegistry : ISystemRegistry
    {
        private readonly NamedDataRegistry<ISystem> _systems;
        private readonly Dictionary<SystemType, int> _typeCounts = new Dictionary<SystemType, int>();

        /// <summary>
        /// Constructs an instance of <see cref="SystemRegistry"/>
        /// </summary>
        /// <param name="maxSystemCount">the maximum number of systems to every be added to the registry</param>
        /// <param name="hashSize">the hash size used by the internal hashtable. Higher makes for faster searching but increases memory usage</param>
        public SystemRegistry(int maxSystemCount, int hashSize = 47)
        {
            _systems = new NamedDataRegistry<ISystem>(maxSystemCount, hashSize);
        }

        /// <inheritdoc />
        public ISystem this[string systemName] => _systems[systemName];

        /// <inheritdoc />
        public ISystem this[int id] => _systems[id];

        /// <inheritdoc />
        public int Count => _systems.Count;

        /// <inheritdoc />
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

        /// <inheritdoc />
        public void Clear()
        {
            _systems.Clear();
        }

        ///<inheritdoc/>
        public ISystemSchedule CreateSchedule(SystemType type, int lanes)
        {
            SystemSchedule schedule = new SystemSchedule(type, lanes, (int)((GetTypeCount(type) * 1.1f) + 1));

            schedule.AddFromRegistry(this);

            return schedule;
        }

        ///<inheritdoc/>
        public void RecreateSchedule(ref ISystemSchedule schedule)
        {
            if (schedule == null)
                throw new ArgumentNullException(nameof(schedule));

            schedule.Clear();
            
            schedule.AddFromRegistry(this);
        }

        /// <inheritdoc />
        public bool Contains(string systemName)
        {
            return _systems.ContainsName(systemName);
        }

        /// <inheritdoc />
        public bool Contains(int id)
        {
            return _systems.ContainsID(id);
        }

        /// <inheritdoc />
        public bool Contains(ISystem system)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));
            return Contains(system.SystemInfo.Name);
        }

        /// <inheritdoc />
        public IEnumerator<ISystem> GetEnumerator()
        {
            return _systems.GetEnumerator();
        }

        /// <inheritdoc />
        public int GetID(string systemName)
        {
            return _systems.GetID(systemName);
        }

        /// <inheritdoc />
        public int GetID(ISystem system)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));
            return _systems.GetID(system.SystemInfo.Name);
        }

        /// <inheritdoc />
        public string GetName(int id)
        {
            return _systems[id].SystemInfo.Name;
        }

        /// <inheritdoc />
        public ISystem GetSystem(string systemName)
        {
            return _systems.Get(systemName);
        }

        /// <inheritdoc />
        public ISystem GetSystem(int id)
        {
            return _systems.Get(id);
        }

        /// <inheritdoc />
        public int GetTypeCount(SystemType type)
        {
            return _typeCounts.ContainsKey(type) ? _typeCounts[type] : 0;
        }

        /// <inheritdoc />
        public void Remove(string systemName)
        {
            int preCount = _systems.Count;
            ISystem system = _systems.PopAt(systemName);
            if (preCount < _systems.Count)
                _typeCounts[system.SystemInfo.Type]--;
            system.SystemInfo.SetID(-1);
        }

        /// <inheritdoc />
        public void Remove(int id)
        {
            int preCount = _systems.Count;
            ISystem system = _systems.PopAt(id);
            if (preCount < _systems.Count)
                _typeCounts[system.SystemInfo.Type]--;
            system.SystemInfo.SetID(-1);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _systems.GetEnumerator();
        }
    }
}
