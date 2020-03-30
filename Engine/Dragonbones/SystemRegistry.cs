using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = "This is not just a collection, but smarter so a registry")]
    public class SystemRegistry : ISystemRegistry
    {
        Entry[] entries;
        ISystem[]

        public ISystem this[string systemName] => throw new NotImplementedException();

        public ISystem this[int id] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int Count { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool Add(ISystem system)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(string systemName)
        {
            throw new NotImplementedException();
        }

        public bool Contains(int id)
        {
            throw new NotImplementedException();
        }

        public bool Contains(ISystem system)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<ISystem> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int getID(string systemName)
        {
            throw new NotImplementedException();
        }

        public int getID(ISystem system)
        {
            throw new NotImplementedException();
        }

        public string getName(int id)
        {
            throw new NotImplementedException();
        }

        public ISystem getSystem(string systemName)
        {
            throw new NotImplementedException();
        }

        public ISystem getSystem(int id)
        {
            throw new NotImplementedException();
        }

        public int GetTypeCount(SystemType type)
        {
            throw new NotImplementedException();
        }

        public void Remove(string systemName)
        {
            throw new NotImplementedException();
        }

        public void Remove(int id)
        {
            throw new NotImplementedException();
        }

        public void Remove(ISystem system)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        struct Entry
        {
            public int ID;
            public string Name;
            public int NextLink;
            public int PreviousLink;
            public int NextEnumerator;
            public int PreviousEnumerator;
        }
    }
}
