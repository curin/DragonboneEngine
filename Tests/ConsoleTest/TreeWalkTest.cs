using System;
using System.Collections.Generic;
using System.Text;
using Dragonbones;

namespace ConsoleTest
{
    public static class TreeWalkTest
    {
        public static void Run()
        {
            for (int i  = 0; i < 7; i++)
            {
                int next = MathHelper.ZeroIndex(i) + 1;
                int rem = i - ((1 << (next - 1)) - 1) >> (next);
                int ind = ((1 << (3 - next)) - 1 + rem);
                int layer = (int)Math.Log(ind + 1, 2);
                int index = (1 << (2 - layer)) - 1 + ((ind - ((1 << (layer)) - 1)) * (2 << (2 - layer)));
                Console.WriteLine(i + "\t" + next + "\t" + rem + "\t" + ind + "\t" + index);
            }

            Console.ReadLine();
        }
    }
}
