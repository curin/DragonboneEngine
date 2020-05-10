using Dragonbones.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dragonbones.Systems
{
    /// <summary>
    /// A system which runs for every entity that matches its required components
    /// </summary>
    public abstract class EntityDrivenSystem : System
    {
        /// <summary>
        /// A link to the entity buffer in the admin
        /// Here for quick access
        /// </summary>
        protected IEntityBuffer Entities { get; set; }
        ///<inheritdoc/>
        protected EntityDrivenSystem(SystemInfo info) : base(info) { }

        ///<inheritdoc/>
        public override void Initialize()
        {
            Entities = Info.Admin.Entities;
            Entities.RegisterSystem(this);
            base.Initialize();
        }

        ///<inheritdoc/>
        public override void Run(float deltaTime)
        {
            IEnumerator<int> enumer = Entities.GetEntities(this).GetEnumerator();

            while (enumer.MoveNext())
            {
                Run(enumer.Current, deltaTime);
            }

            base.Run(deltaTime);
        }

        /// <summary>
        /// Runs this system on the given entity
        /// </summary>
        /// <param name="entity">the ID of the entity</param>
        /// <param name="deltaTime">the time since the last logic update (consistent for logic systems variable for rneder systems)</param>
        public virtual void Run(int entity, float deltaTime) { }
    }
}
