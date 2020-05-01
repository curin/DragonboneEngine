using System;
using System.Collections.Generic;
using System.Text;
using Dragonbones.Components;
using Dragonbones.Systems;
using Dragonbones.Entities;

namespace Dragonbones
{
    public interface IEntityAdmin
    {
        IComponentTypeRegistry Components { get; set; }
        ISystemRegistry Systems { get; set; }
        IEntityBuffer Entities { get; set; }
        ILinkBuffer Links { get; set; }
        void Run();
    }
}
