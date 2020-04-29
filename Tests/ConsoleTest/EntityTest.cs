using System;
using System.Collections.Generic;
using System.Text;
using Dragonbones.Entities;
using Dragonbones.Native;
using Dragonbones.Systems;

namespace ConsoleTest
{
    public static class EntityTest
    {
        public static void Run(int entityCount, int componentCount)
        {
            PrecisionTimer timer = new PrecisionTimer();
            List<int>[] componentLists = new List<int>[componentCount];
            int[] requiredComponents = new int[componentCount];
            Random rand = new Random();
            
            for (int i = 0; i < componentCount; i++)
            {
                componentLists[i] = new List<int>(entityCount);
                requiredComponents[i] = i;
            }

            for (int i = 0; i < entityCount; i++)
            {
                for (int j = 0; j < componentCount; j++)
                    if (rand.Next(10) > 5)
                        componentLists[j].Add(i);
            }

            Console.WriteLine("Time to Create Entity : " + timer.ElapsedSeconds);

            timer.Reset();

            List<int[]> components = new List<int[]>();
            timer.Reset();

            timer.Start();
            int[] place = new int[requiredComponents.Length];
            bool done = false;

            for (place[0] = 0; place[0] < componentLists[0].Count;place[0]++)
            {
                int count = 1;
                int val = componentLists[0][place[0]];
                for (int i = 1; i < place.Length; i++)
                {
                    while (componentLists[i][place[i]] < val)
                    {
                        place[i]++;
                        if (place[i] == componentLists[i].Count)
                        {
                            done = true;
                            break;
                        }
                    }

                    if (done)
                        break;

                    if (componentLists[i][place[i]] > val)
                        break;
                    count++;
                }

                if (done)
                    break;

                if (count == place.Length)
                    components.Add(place);
            }

            timer.Stop();


            Console.WriteLine("Time for Component Worst Case: " + timer.ElapsedSeconds);
            Console.ReadLine();
        }
    }
}
