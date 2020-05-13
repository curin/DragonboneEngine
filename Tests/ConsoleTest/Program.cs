using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Dragonbones;
using Dragonbones.Entities;
using Dragonbones.Systems;
using Dragonbones.Native;

namespace ConsoleTest
{
    internal class Program
    {
        private static void Main()
        {
            COMTest.Test();
            //SetEquivalenceTest.Run();
            //BinarySearchTest.Run();
            //TreeWalkTest.Run();
            //ConditionalTest.Run();
            //PagingTest.Run();
            AdminTest.Run();
            ThreadingTest.Run();
            EntityTest.Run(100000, 10000);
            SyncTest.Run();
            CollectionTest.Run();
            Console.ReadLine();
            SchedulingTest SchedTest = new SchedulingTest();
            SchedTest.Run(SystemType.Logic, 1 / 60.0, 2, 10);
        }
    }
}
