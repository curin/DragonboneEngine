using System;
using System.Collections.Generic;
using System.Text;
using Dragonbones.Collections;
using Dragonbones.Native;

namespace ConsoleTest
{
    public static class BinarySearchTest
    {
        public static void Run()
        {
            PrecisionTimer timer = new PrecisionTimer();
            CompleteBinarySearchTree<int> test = new CompleteBinarySearchTree<int>(8, 1);

            timer.Start();
            for (int i = 0; i < 10000; i++)
                    test.Add(i, i);
            timer.Stop();

            Console.WriteLine(timer.ElapsedSeconds / 10000);
            Console.WriteLine(timer.ElapsedSeconds);
            Console.WriteLine(timer.ElapsedSeconds / (1.0 / 60.0) / 500);

            int a = 0;
            timer.Reset();
            timer.Start();
            foreach (int i in test)
                a = i;
            timer.Stop();
            Console.WriteLine(a);
            Console.WriteLine(timer.ElapsedSeconds / 10000);
            Console.WriteLine(timer.ElapsedSeconds);
            Console.WriteLine(timer.ElapsedSeconds / (1.0/60.0) * 100);
            Console.WriteLine(1.0/60.0);
            Console.ReadLine();
        }
    }
}
