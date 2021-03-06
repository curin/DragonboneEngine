﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Dragonbones.Collections.Paged;

namespace Dragonbones.Systems
{
    /// <summary>
    /// A base version of the <see cref="SystemRegistry"/>
    ///
    /// This uses a <see cref="NamedDataRegistry{ISystem}"/> to store the systems
    /// </summary>
#pragma warning disable CA1710 // Identifiers should have correct suffix
    public class SystemRegistry : ISystemRegistry
#pragma warning restore CA1710 // Identifiers should have correct suffix
    {
        private readonly NamedDataRegistry<ISystem> _systems;
        private Dictionary<SystemType, int> _typeCounts = new Dictionary<SystemType, int>();
        private IEntityAdmin _admin;

        /// <summary>
        /// Constructs an instance of <see cref="SystemRegistry"/>
        /// </summary>
        /// <param name="systemPagePower">the size of the system pages as a power of 2</param>
        /// <param name="systemPageCount">the number of pages to start with, larger values decrease early performance hits but increase memory cost</param>
        /// <param name="hashSize">the hash size used by the internal hashtable. Higher makes for faster searching but increases memory usage</param>
        public SystemRegistry(int systemPagePower = 8, int systemPageCount = 1, int hashSize = 47)
        {
            _systems = new NamedDataRegistry<ISystem>(systemPagePower, systemPageCount, hashSize);
        }

        /// <inheritdoc />
        public ISystem this[string systemName] => _systems[systemName];

        /// <inheritdoc />
        public ISystem this[int id] => _systems[id];

        /// <inheritdoc />
        public int Count => _systems.Count;

        /// <inheritdoc />
        public bool Register(ISystem system)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));
            int id = _systems.Add(system.Info.Name, system);
            if (id == -1)
                return false;
            system.Info.SetID(id);
            system.Info.SetAdmin(_admin);
            if (!_typeCounts.ContainsKey(system.Info.Type))
                _typeCounts.Add(system.Info.Type, 0);
            _typeCounts[system.Info.Type]++;
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
            if (type == SystemType.Logic)
            {
                SafeSystemSchedule schedule = new SafeSystemSchedule(type, lanes);

                schedule.AddFromRegistry(this);

                return schedule;
            }
            else
            {
                SystemSchedule schedule = new SystemSchedule(type, lanes, (int)((GetTypeCount(type)) + 1));

                schedule.AddFromRegistry(this);

                return schedule;
            }
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
            return Contains(system.Info.Name);
        }

        /// <inheritdoc />
        public IEnumerator<ISystem> GetEnumerator()
        {
            return _systems.GetEnumerator();
        }

        /// <inheritdoc />
        public int GetID(string systemName)
        {
            return _systems.GetIDFromName(systemName);
        }

        /// <inheritdoc />
        public int GetID(ISystem system)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));
            return _systems.GetIDFromName(system.Info.Name);
        }

        /// <inheritdoc />
        public string GetName(int id)
        {
            return _systems[id].Info.Name;
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
                _typeCounts[system.Info.Type]--;
            system.Info.SetID(-1);
        }

        /// <inheritdoc />
        public void Remove(int id)
        {
            int preCount = _systems.Count;
            ISystem system = _systems.PopAt(id);
            if (preCount < _systems.Count)
                _typeCounts[system.Info.Type]--;
            system.Info.SetID(-1);
        }

        /// <inheritdoc />
        public void Remove(ISystem system)
        {
            if (system == null)
                throw new ArgumentNullException(nameof(system));
            int preCount = _systems.Count;
            _systems.PopAt(system.Info.Name);
            if (preCount < _systems.Count)
                _typeCounts[system.Info.Type]--;
            system.Info.SetID(-1);
        }

        /// <summary>
        /// Sets the controlling admin for this registry
        /// </summary>
        /// <param name="admin">The controlling admin used to set admin in systemInfo when a system is registered</param>
        public void SetAdmin(IEntityAdmin admin)
        {
            _admin = admin;
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _systems.GetEnumerator();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Dispose this object
        /// </summary>
        /// <param name="disposing">Are managed objects being disposed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _systems?.Dispose();
                    _typeCounts = null;
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
