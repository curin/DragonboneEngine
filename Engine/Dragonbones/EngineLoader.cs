using System;
using System.Collections.Generic;
using System.Text;
using Dragonbones.Components;
using Dragonbones.Entities;
using Dragonbones.Systems;

namespace Dragonbones
{
    /// <summary>
    /// The class which is responsible for starting up the entity admin
    /// and adding systems and components prior to the admin starting
    /// </summary>
    public abstract class EngineLoader
    {
        /// <summary>
        /// The controlling Entity Admin which will run the engine
        /// </summary>
        protected IEntityAdmin Admin { get; }

        /// <summary>
        /// Constructs an engine loader with the given parameters using the default admin and default storages
        /// </summary>
        /// <param name="targetLogicFrameLength">the frame length in seconds of logic updates which the engine will attempt to keep</param>
        /// <param name="targetMaxRenderFrameLength">the frame length in seconds of render updates which the engine will try to stay below</param>
        /// <param name="componentTypePagePower">the size of component type pages as a power of 2</param>
        /// <param name="componentTypePageCount">the number of component type pages to start with, this can affect memory and performance</param>
        /// <param name="systemPagePower">the size of system pages as a power of 2</param>
        /// <param name="systemPageCount">the number of system pages to start with, this can affect memory and performance</param>
        /// <param name="entityPagePower">the size of entity pages as a power of 2</param>
        /// <param name="entityPageCount">the number of entity pages to start with, this can affect memory and performance</param>
        /// <param name="entityComponentPagePower">the size of entity-component link pages as a power of 2</param>
        /// <param name="entityComponentPageCount">the number of entity-component link pages to start with, this can affect memory and performance</param>
        protected EngineLoader(float targetLogicFrameLength = 1f / 60f, float targetMaxRenderFrameLength = 1f / 30f, 
            int componentTypePagePower = 8, int componentTypePageCount = 1, int systemPagePower = 8, int systemPageCount = 1,
            int entityPagePower = 8, int entityPageCount = 1, int entityComponentPagePower = 8, int entityComponentPageCount = 1)
        {
            IComponentTypeRegistry registry = new ComponentTypeRegistry(componentTypePagePower, componentTypePageCount);
            Admin = new EntityAdmin(targetLogicFrameLength, targetMaxRenderFrameLength, registry,
                new SystemRegistry(systemPagePower, systemPageCount), new EntityBuffer(registry, 47, 47, entityPagePower, entityComponentPagePower, entityPagePower, systemPagePower,
                entityPageCount, entityComponentPageCount, entityPageCount, systemPageCount));
        }

        /// <summary>
        /// Constructs an engine loader with the given parameters using the default admin
        /// </summary>
        /// <param name="targetLogicFrameLength">the frame length in seconds of logic updates which the engine will attempt to keep</param>
        /// <param name="targetMaxRenderFrameLength">the frame length in seconds of render updates which the engine will try to stay below</param>
        /// <param name="componentTypeRegistry">the component type registry to use to store component types</param>
        /// <param name="systemRegistry">the system registry to store systems</param>
        /// <param name="entityBuffer">the entity buffer to use to store entities</param>
        protected EngineLoader(IComponentTypeRegistry componentTypeRegistry, 
            ISystemRegistry systemRegistry, IEntityBuffer entityBuffer, float targetLogicFrameLength = 1f/60f, float targetMaxRenderFrameLength = 1f/30f)
        {
            Admin = new EntityAdmin(targetLogicFrameLength, targetMaxRenderFrameLength, componentTypeRegistry, systemRegistry, entityBuffer);
        }

        /// <summary>
        /// Construct an engine loader with the given admin
        /// </summary>
        /// <param name="admin">The admin to run this engine instance</param>
        protected EngineLoader(IEntityAdmin admin)
        {
            Admin = admin;
        }

        /// <summary>
        /// Load component types in to the type buffer and initial components if necessary
        /// This runs before systems are loaded
        /// </summary>
        protected virtual void LoadComponents() { }

        /// <summary>
        /// Load systems into the system registry and initalize necessary variables
        /// This runs after components are loaded, but before system initialize
        /// </summary>
        protected virtual void LoadSystems() { }

        /// <summary>
        /// Starts the Engine loading components and systems first
        /// </summary>
        public void Run()
        {
            LoadComponents();
            LoadSystems();
            Admin.Run();
        }
    }
}
