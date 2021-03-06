﻿using System;
using System.Collections.Generic;
using System.Text;
using Dragonbones.Components;
using Dragonbones.Systems;
using Dragonbones.Entities;

namespace Dragonbones
{
    /// <summary>
    /// The central controller of the whole engine
    /// An Entity Admin is responsible for 
    /// - scheduling and running systems
    /// - providing access to other resources
    /// </summary>
    public interface IEntityAdmin : IDisposable
    {
        /// <summary>
        /// The number of computational lanes runnning for the logic systems
        /// </summary>
        int LogicLaneCount { get; }
        /// <summary>
        /// the number of computational lanes running for the render systems
        /// </summary>
        int RenderLaneCount { get; }
        /// <summary>
        /// the number of computational lanes total
        /// </summary>
        int TotalLaneCount { get; }
        /// <summary>
        /// The container for all component buffers
        /// attempt to find component types before adding them
        /// </summary>
        IComponentTypeRegistry Components { get; set; }
        /// <summary>
        /// The container for all systems
        /// Systems need only be registered once
        /// </summary>
        ISystemRegistry Systems { get; set; }
        /// <summary>
        /// The container for all entities
        /// Entities are simply a name given to an ID and should most often be referenced by their ID
        /// </summary>
        IEntityBuffer Entities { get; set; }
        /// <summary>
        /// Starts the engine running
        /// </summary>
        void Run();
        /// <summary>
        /// The schedule to be used for logic systems
        /// Schedule should be filled with systems when being set
        /// </summary>
        ISystemSchedule LogicSchedule { get; set; }
        /// <summary>
        /// The schedule to be used for render systems
        /// Schedule should be filled with systems when being set
        /// </summary>
        ISystemSchedule RenderSchedule { get; set; }
    }
}
