using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Dragonbones.Native;

namespace ConsoleTest
{
    public class ThreadingTest
    {
        public static Barrier barrier;
        public static long[] Primes;
        public static bool Running = true;
        public static void Run()
        {
            int threadCount = Environment.ProcessorCount;
            Thread[] threads = new Thread[threadCount];
            Primes = new long[threadCount];
            barrier = new Barrier(threadCount);

            threads[0] = Thread.CurrentThread;

            PrecisionTimer timer = new PrecisionTimer();
            for (int i = 1; i < threadCount; i++)
                threads[i] = new Thread(ThreadMethodBarrier);

            timer.Reset();
            timer.Start();
            for (int j = 1; j < threadCount; j++)
                threads[j].Start(j);
            for (int i = 0; i < 1000; i++)
            {
                Primes[0] = FindPrimeNumber(1000);
                barrier.SignalAndWait();
            }
            Running = false;
            timer.Stop();
            Console.WriteLine("Barrier Time: " + (timer.ElapsedSecondsF / 1000));
            Console.WriteLine("Primes Found");
            for (int i = 0; i < threadCount; i++)
                Console.WriteLine(Primes[i]);
            Console.ReadLine();
        }

        public static void ThreadMethodStart(object oi)
        {
            int i = (int)oi;
            Primes[i] = FindPrimeNumber(1000);
            barrier.SignalAndWait();
        }

        public static void ThreadMethodBarrier(object oi)
        {
            while (Running)
            {
                int i = (int)oi;
                Primes[i] = FindPrimeNumber(1000);
                barrier.SignalAndWait();
            }
        }

        public static long FindPrimeNumber(int n)
        {
            int count = 0;
            long a = 2;
            while (count < n)
            {
                long b = 2;
                int prime = 1;// to check if found a prime
                while (b * b <= a)
                {
                    if (a % b == 0)
                    {
                        prime = 0;
                        break;
                    }
                    b++;
                }
                if (prime > 0)
                {
                    count++;
                }
                a++;
            }
            return (--a);
        }
    }
}
