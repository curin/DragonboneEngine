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
            EntityAdmin admin = new EntityAdmin(1f/60f,new ComponentTypeRegistry(64), new SystemRegistry(64), new EntityBuffer(), new LinkBuffer(64, 32));
            admin.Run();
            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
