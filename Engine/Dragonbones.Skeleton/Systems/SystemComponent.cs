using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones.Systems
{
    /// <summary>
    /// A structure representing a type of component that this system uses
    /// </summary>
    public struct SystemComponent
    {
        /// <summary>
        /// Constructs a system component
        /// </summary>
        /// <param name="typeName">the name of the component type</param>
        /// <param name="required">whether it is required for entities run on this system to have an instance of this component linked to them
        /// (This is likely to be set to false for singleton components or times where you access a component if a value is set in another component)</param>
        public SystemComponent(string typeName, bool required = true)
        {
            TypeName = typeName;
            Required = required;
            TypeID = -1;
        }

        /// <summary>
        /// The name of the component type
        /// </summary>
        public string TypeName { get; }
        /// <summary>
        /// Does an entity need this component to be run through this system
        /// </summary>
        public bool Required { get; }
        /// <summary>
        /// The ID assigned to the component type by the system
        /// </summary>
        public int TypeID { get; private set; }
        /// <summary>
        /// A Method for the system to set the ComponentTypeID
        /// </summary>
        /// <param name="ID">The ID for the component type</param>
        public void SetID(int ID)
        {
            TypeID = ID;
        }

        ///<inheritdoc/>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        ///<inheritdoc/>
        public override int GetHashCode() => TypeName.GetHashCode();

        ///<inheritdoc/>
        public static bool operator ==(SystemComponent left, SystemComponent right)
        {
            return left.TypeID == right.TypeID;
        }

        ///<inheritdoc/>
        public static bool operator !=(SystemComponent left, SystemComponent right)
        {
            return left.TypeID != right.TypeID;
        }
    }
}
