using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dragonbones.Native;

namespace ConsoleTest
{
    public static class SetEquivalenceTest
    {
        public static void Run()
        {
            PrecisionTimer timer = new PrecisionTimer();
            Random rand = new Random();
            int[] ID1 = new int[100000];
            int[] ID2 = new int[1000];

            Console.WriteLine("Set Equivalence Test");

            for (int i = 0; i < ID1.Length; i++)
            {
                ID1[i] = rand.Next();
                if (i < ID2.Length)
                    ID2[i] = ID1[i];
            }

            bool found = false;
            timer.Start();
            for (int i = 0; i < ID1.Length; i++)
            {
                found = false;
                for (int j = 0; j < ID2.Length; j++)
                    if (ID1[i] == ID2[j])
                    {
                        found = true;
                        break;
                    }

                if (!found)
                    break;
            }
            timer.Stop();

            Console.WriteLine(found);
            Console.WriteLine(timer.ElapsedSeconds);
            timer.Reset();

            timer.Start();
            found = ID1.All((i) => { return ID2.Contains(i); });
            timer.Stop();

            Console.WriteLine(found);
            Console.WriteLine(timer.ElapsedSeconds);
            timer.Reset();

            timer.Start();
            found = ID1.All((i) => { return ID2.Any((j) => { return i == j; }); });
            timer.Stop();

            Console.WriteLine(found);
            Console.WriteLine(timer.ElapsedSeconds);
            timer.Reset();

            timer.Start();
            //Fastest in worst case
            found = !ID1.Any((i) => { return !ID2.Contains(i); });
            timer.Stop();

            Console.WriteLine(found);
            Console.WriteLine(timer.ElapsedSeconds);
            timer.Reset();

            for (int i = 0; i < ID2.Length; i++)
            {
                ID2[i] = rand.Next();
            }

            timer.Start();
            //fastest in best case
            for (int i = 0; i < ID1.Length; i++)
            {
                found = false;
                for (int j = 0; j < ID2.Length; j++)
                    if (ID1[i] == ID2[j])
                    {
                        found = true;
                        break;
                    }

                if (!found)
                    break;
            }
            timer.Stop();

            Console.WriteLine(found);
            Console.WriteLine(timer.ElapsedSeconds);
            timer.Reset();

            timer.Start();
            found = ID1.All((i) => { return ID2.Contains(i); });
            timer.Stop();

            double temp = timer.ElapsedSeconds;
            Console.WriteLine(found);
            Console.WriteLine(timer.ElapsedSeconds);
            timer.Reset();

            timer.Start();
            found = ID1.All((i) => { return ID2.Any((j) => { return i == j; }); });
            timer.Stop();

            Console.WriteLine(found);
            Console.WriteLine(timer.ElapsedSeconds);
            timer.Reset();

            //
            //
            // Best in Worst Case and Close 2nd in Best Case
            //
            //
            timer.Start();
            found = !ID1.Any((i) => { return !ID2.Contains(i); });
            timer.Stop();

            Console.WriteLine(found);
            Console.WriteLine(timer.ElapsedSeconds);
            Console.WriteLine(timer.ElapsedSeconds / temp * 100);
            timer.Reset();

            Console.ReadLine();
        }
    }
}
