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
            RunTimeSchedulerTest test2 = new RunTimeSchedulerTest();
            test2.Run(10000);
            SchedulingTest test = new SchedulingTest();
            test.Run(10000);
        }
    }
}
