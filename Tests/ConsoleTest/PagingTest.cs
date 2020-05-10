using System;
using System.Collections.Generic;
using System.Text;
using Dragonbones.Collections.Paged;
using Dragonbones.Native;

namespace ConsoleTest
{
    public static class PagingTest
    {
        public static void Run()
        {
            PrecisionTimer timer = new PrecisionTimer();
            PagedArray<int> paged = new PagedArray<int>(2, 64);
            int[] normy = new int[2048];

            normy[0] = 1;
            normy[15] = 1;
            paged[0] = 1;
            paged[15] = 1;
            for (int i = 0; i < 1; i++)
            {
                timer.Reset();
                timer.Start();
                Array.Copy(normy, 0, normy, 34, 16);
                timer.Stop();
            }

            Console.WriteLine(timer.ElapsedSecondsF);

            paged[50] = 0;

            for (int i = 0; i < 1; i++)
            {
                timer.Reset();

                timer.Start();
                paged.CopyData(0, 16, 34);
                timer.Stop();
            }

            Console.WriteLine(timer.ElapsedSecondsF);

            Console.ReadLine();
        }
    }
}
