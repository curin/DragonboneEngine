﻿using System;
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
            while (true)
            {
                SchedulingTest test = new SchedulingTest();
                test.Run(SystemType.Logic, 1 / 60.0, 2, 10);
            }
        }
    }
}
