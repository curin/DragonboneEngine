using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones.Entities
{
    /// <summary>
    /// A link representing a component which is connected to an entity
    /// </summary>
    public struct EntityLink
    {
        /// <summary>
        /// Constructs an Entity Link
        /// </summary>
        /// <param name="componentTypeID">the ID of the component type</param>
        /// <param name="componentID">the ID of the component</param>
        public EntityLink(int componentTypeID, int componentID)
        {
            ComponentType = componentTypeID;
            Component = componentID;
        }

        /// <summary>
        /// The ID of the Component Type
        /// </summary>
        public int ComponentType { get; }
        /// <summary>
        /// The ID of the Component
        /// </summary>
        public int Component { get; }

        ///<inheritdoc/>
        public override bool Equals(object obj) => base.Equals(obj);

        ///<inheritdoc/>
        public override int GetHashCode() => base.GetHashCode();

        ///<inheritdoc/>
        public static bool operator ==(EntityLink left, EntityLink right)
        {
            return left.ComponentType == right.ComponentType && left.Component == right.Component;
        }

        ///<inheritdoc/>
        public static bool operator !=(EntityLink left, EntityLink right)
        {
            return left.ComponentType != right.ComponentType || left.Component != right.Component;
        }
    }
}
