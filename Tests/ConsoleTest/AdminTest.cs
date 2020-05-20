using System;
using System.Collections.Generic;
using System.Text;
using Dragonbones;
using Dragonbones.Components;
using Dragonbones.Entities;
using Dragonbones.Native;
using Dragonbones.Systems;

namespace ConsoleTest
{
    public static class AdminTest
    {
        public static void Run()
        {
            IComponentTypeRegistry registry = new ComponentTypeRegistry();
            EntityAdmin admin = new EntityAdmin(1f/60f, 1f/30f, registry, new SystemRegistry(), new EntityBuffer(registry));
            admin.Run();
            Console.WriteLine("Done");
            Console.ReadLine();
            admin.Dispose();
        }
    }
}
