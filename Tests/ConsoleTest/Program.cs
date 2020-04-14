using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Dragonbones;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            CollectionTest.Run();
            Console.ReadLine();
            SchedulingTest SchedTest = new SchedulingTest();
            SchedTest.Run(SystemType.Logic, 1 / 60.0, 2, 10);
        }
    }
}
