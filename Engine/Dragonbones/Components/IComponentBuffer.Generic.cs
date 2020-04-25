using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Dragonbones.Systems;

namespace Dragonbones.Components
{
    public interface IComponentBuffer<TComponent> : IComponentBuffer, IEnumerable<TComponent>
        where TComponent : struct, IEquatable<TComponent>
    {
        TComponent this[SystemType systemType, int id] { get; set; }
        TComponent this[SystemType systemType, string name] { get; set; }

        int Add(string name, TComponent value);

        bool TryGet(SystemType systemType, int id, out TComponent value);
        bool TryGet(SystemType systemType, string name, out TComponent value);

        TComponent Get(SystemType systemType, int id);
        TComponent Get(SystemType systemType, string name);

        void Set(SystemType systemType, int id, TComponent value);
        void Set(SystemType systemType, string name, TComponent value);

        void Remove(TComponent value);

        bool Contains(TComponent value);
        int GetID(TComponent value);
        string GetName(TComponent value);

        TComponent PopAt(string name);
        TComponent PopAt(int id);
        TComponent Pop(TComponent value);
    }
}
