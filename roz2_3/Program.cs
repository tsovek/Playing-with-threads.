using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace roz2_3
{
    class Program
    {
        static void Main(string[] args)
        {
            MinMaxCalculate calc = new MinMaxCalculate(1000);
            MinMax mm = calc.GetMinMax();

            Console.WriteLine("Min value: " + mm.Min);
            Console.WriteLine("Max value: " + mm.Max);

            Console.ReadLine();
        }

        class MinMax
        {
            public int Min { get; private set; }
            public int Max { get; private set; }

            public MinMax(int min, int max)
            {
                Min = min;
                Max = max;
            }
        }

        class MinMaxCalculate
        {
            static ManualResetEvent resetEvent = new ManualResetEvent(false);

            Random r = new Random();
            List<int> _results = new List<int>();

            int[] Array { get; set; }

            public MinMaxCalculate(int count)
            {
                Array = new int[count];
                FillArray();
            }

            public MinMax GetMinMax()
            {
                Calculate();
                return CalculateResult(_results, new MinMax(0, _results.Count));
            }

            private void FillArray()
            {
                for (int i = 0; i < Array.Length; i++)
                {
                    Array[i] = r.Next() & DateTime.Now.Millisecond;
                }
            }

            private void SearchInRange(object obj)
            {
                object[] minMax = (object[])obj;
                MinMax range = new MinMax((int)minMax[0], (int)minMax[1]);
                MinMax resultFromThread = CalculateResult(Array, range);
                _results.Add(resultFromThread.Min);
                _results.Add(resultFromThread.Max);

                resetEvent.Set();
            }

            private MinMax CalculateResult(
                IEnumerable<int> _collection, 
                MinMax range)
            {
                var collection = _collection.ToArray();
                int min = 0;
                int max = 0;
                for (int i = range.Min; i < range.Max; i++)
                {
                    int current = collection[i];

                    if (i == range.Min)
                    {
                        min = current;
                        max = current;
                    }

                    if (current < min)
                    {
                        min = current;
                    }

                    if (current > max)
                    {
                        max = current;
                    }
                }
                return new MinMax(min, max);
            }

            private void Calculate()
            {
                Console.WriteLine("Calculation is staring...");
                int countCPU = Environment.ProcessorCount;
                WaitCallback w = SearchInRange;
                int range = Array.Length / countCPU;
                int max = 0;
                for (int i = 0; i < countCPU; i++)
                {
                    int min = i * range;
                    max += range;
                    Console.WriteLine("New thread is starting... ");
                    ThreadPool.QueueUserWorkItem(w, new object[] { min, max });
                    Console.WriteLine("Thread finished...");
                }
                Console.WriteLine("Calculation finished");
                resetEvent.WaitOne();
            }
        }
    }
}
