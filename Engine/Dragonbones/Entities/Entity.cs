using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Dragonbones.Collections;
using Dragonbones.Systems;

namespace Dragonbones.Entities
{
    public struct Entity
    {
        public int ID { get; private set; }
        public string Name { get; set; }


        public Entity(string name, int componentCount)
        {
            Name = name;
            ID = -1;
        }

        /// <summary>
        /// Sets the Entity's ID
        /// Designed to be used by System only
        /// !!! DO NOT USE THIS FUNCTION !!!
        /// </summary>
        /// <param name="id">the id of the entity</param>
        public void SetID(int id)
        {
            ID = id;
        }
    }
}
