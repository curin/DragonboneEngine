using System;
using System.Collections.Generic;
using System.Text;
using Dragonbones.Collections;

namespace ConsoleTest
{
    public static class CollectionTest
    {
        public static void Run()
        {
            NamedDataRegistry<int> registry = new NamedDataRegistry<int>();
            registry.Add("Zero", 0);
            registry.Add("One", 1);
            registry.Add("Two", 2);
            registry.Add("Three", 3);
            registry.Add("Four", 4);
            registry.Add("Five", 5);

            foreach(int i in registry)
            {
                Console.WriteLine(i);
            }
        }
    }
}
