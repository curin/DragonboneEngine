using System;
using System.Collections.Generic;
using System.Text;
using Dragonbones.Collections;
using Dragonbones.Collections.Paged;
using Dragonbones.Systems;

namespace Dragonbones.Entities
{
    public class EntityBufferV2
    {
        NamedDataBuffer<BufferedBinarySearchTree<EntityLink>> _entities;

        int CreateEntity(string name);
        int GetID(string name);
        string GetName(int entity);

        void Remove(int entity);

        bool ContainsComponent(int entity, int componentType);
        int GetComponent(int entity, int componentType);

        void SetLink(int entity, int componentType, int componentID);
        void RemoveLink(int entity, int componentType);

        void RegisterSystem(ISystem system);
        PagedArray<int> GetEntities(int systemID);
        void RemoveSystem(int systemID);

        struct SystemLink
        {
            public int SystemID;
            public int Index;
            public int Next;
            public int Previous;
        }

        struct EntityList
        {
            public int Top;
            public DataBuffer<int> Entities;
            int[] ComponentTypes;
        }
    }

    public struct EntityLink
    {
        public int ComponentType;
        public int Component;
    }
}
