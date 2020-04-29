using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones.Entities
{
    /// <summary>
    /// A structure that represents a link between an Entity and a Component
    /// </summary>
    public struct EntityComponentLink : IEquatable<EntityComponentLink>
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="entity">the ID of the entity</param>
        /// <param name="component">the ID of the component instance</param>
        public EntityComponentLink(int entity, int component)
        {
            EntityID = entity;
            ComponentID = component;
        }

        /// <summary>
        /// The ID of the entity that this link involves
        /// </summary>
        public int EntityID { get; set; }
        /// <summary>
        /// The ID of the component that this link involves
        /// </summary>
        public int ComponentID { get; set; }

        ///<inheritdoc/>
        public bool Equals(EntityComponentLink other)
        {
            return other.EntityID == EntityID;
        }
    }
}
