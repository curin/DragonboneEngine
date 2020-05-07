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
            //CompleteBinarySearchTree<int> test = new CompleteBinarySearchTree<int>(10, 1);

            int count = 16384;

            //timer.Start();
            //for (int i = 0; i < count; i++)
            //        test.Add(i, i);
            //timer.Stop();

            

            double time = timer.ElapsedSeconds / count;

            int[] testArray = new int[count];
            PagedArray<int> testPaged = new PagedArray<int>(12, 1);
            testPaged[count - 1] = 1;
            int a = 0;
            IEnumerator<int> enumer = testPaged.GetEnumerator();
            timer.Reset();
            Console.ReadLine();
            timer.Start();
            while (enumer.MoveNext())
            //foreach (int i in testArray)
            {
                //timer.Stop();
                //Console.WriteLine(i);
                //timer.Start();
                //a = enumer.Current;
                //a = i;
            }
            timer.Stop();
            //Console.WriteLine(a);
            
            //Console.WriteLine(time);
            //Console.WriteLine(time * count);
            //Console.WriteLine(time / (1.0 / 60.0) * 100);
            Console.WriteLine(timer.ElapsedSeconds / count);
            Console.WriteLine(timer.ElapsedSeconds);
            Console.WriteLine(timer.ElapsedSeconds / (1.0/60.0) * 100);
            Console.WriteLine(1.0/60.0);
            Console.ReadLine();
        }
    }
}
