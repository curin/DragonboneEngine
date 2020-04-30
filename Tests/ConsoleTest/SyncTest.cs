using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTest
{
    public static class SyncTest
    {
        private static int[][] Arrays;
        private static bool[][] DirtyMarkers = new bool[2][];
        private static Queue<int>[] DirtyLists = new Queue<int>[2];

        public static void Run()
        {
            Arrays = new int[3][];
            for (int i = 0; i < 3; i++)
                Arrays[i] = new int[5];

            for (int i = 0; i < 2; i++)
            {
                DirtyMarkers[i] = new bool[5];
                DirtyLists[i] = new Queue<int>();
            }

            Write(20);
            Read();
            Write(20);
            Read();
            Console.ReadLine();
        }

        public static void Write(int iterations)
        {
            int iter = 0;

            while (iter < iterations)
            {
                Arrays[0][iter % 5] = Arrays[0][iter % 5] + iter;
                iter++;

                if (DirtyMarkers[0][iter % 5]) continue;

                DirtyLists[0].Enqueue(iter % 5);
                DirtyMarkers[0][iter % 5] = true;
            }

            lock (Arrays)
            {
                while (DirtyLists[0].TryDequeue(out int val))
                {
                    Arrays[1][val] = Arrays[0][val];
                    DirtyMarkers[0][val] = false;

                    if (DirtyMarkers[1][val]) continue;

                    DirtyLists[1].Enqueue(val);
                    DirtyMarkers[1][val] = true;

                }
            }
        }

        public static void Read()
        {
            lock (Arrays)
            {
                while (DirtyLists[1].TryDequeue(out int val))
                {
                    Arrays[2][val] = Arrays[1][val];
                    DirtyMarkers[1][val] = false;
                }
            }

#pragma warning disable CA1303 // Do not pass literals as localized parameters
            Console.WriteLine("-------------------------");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
            for (int i = 0; i < 5; i++)
                Console.WriteLine(Arrays[2][i]);
        }
    }
}
