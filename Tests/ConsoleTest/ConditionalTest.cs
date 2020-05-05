using System;
using System.Collections.Generic;
using System.Text;
using Dragonbones.Native;
using Dragonbones;

namespace ConsoleTest
{
    public static class ConditionalTest
    {

        public static void Run()
        {
            PrecisionTimer timer = new PrecisionTimer();
            Random rand = new Random();
            int a, b, c = 0;
            timer.Start();
            for (int i = 0; i < 100000;i++)
            {
                a = rand.Next();
                b = rand.Next();
                if (a > b)
                {
                    c = a;
                    b = c;
                }
                else
                {
                    c = b;
                    b = a;
                }
            }
            timer.Stop();

            Console.WriteLine(c);
            Console.WriteLine(timer.ElapsedSeconds);

            timer.Reset();
            timer.Start();
            for (int i = 0; i < 100000; i++)
            {
                a = rand.Next();
                b = rand.Next();
                c = a > b ? a : b;
                b = a > b ? c : a;
            }
            timer.Stop();

            Console.WriteLine(c);
            Console.WriteLine(timer.ElapsedSeconds);

            Console.ReadLine();
        }
    }
}
