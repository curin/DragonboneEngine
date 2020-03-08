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
            SchedulingTest test = new SchedulingTest();
            test.Run(100);
        }
    }
}
