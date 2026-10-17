using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Kopalnia
{
    class Program
    {
        static int coal;
        static int warehouse;
        static SemaphoreSlim deposit = new SemaphoreSlim(2, 2);
        static SemaphoreSlim storage = new SemaphoreSlim(1, 1);
        static object lockObject = new object();

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            double t1 = 0;
            for (int n = 1; n <= 6; n++)
            {
                double t = Simulate(n);
                if (n == 1) t1 = t;
                double speedup = t1 / t;            // przyśpieszenie S = T1 / Tn
                double efficiency = speedup / n;    // efektywność  E = S / n
                Console.WriteLine($"liczba górników: {n}, czas: {t:F2} s przyśpieszenie: {speedup:F2} efektywność: {efficiency:F2}");
            }
        }

        // Jedna pełna symulacja dla n górników, zwraca czas w sekundach
        static double Simulate(int n)
        {
            coal = 2000;                            // reset stanu przed każdą symulacją
            warehouse = 0;
            Stopwatch sw = Stopwatch.StartNew();

            Task[] tasks = new Task[n];
            for (int i = 0; i < tasks.Length; i++)
            {
                int id = i + 1;
                tasks[i] = Task.Run(() => Miner(id));
            }
            Task.WaitAll(tasks);

            sw.Stop();
            return sw.Elapsed.TotalSeconds;
        }

        // Ten sam górnik co w zadaniu 1, tylko bez logów
        static void Miner(int id)
        {
            while (true)
            {
                int load = 0;
                deposit.Wait();
                while (load < 200)
                {
                    lock (lockObject)
                    {
                        if (coal == 0) break;
                        coal--;
                    }
                    load++;
                    Thread.Sleep(10);
                }
                deposit.Release();

                if (load == 0) break;

                Thread.Sleep(10000);

                storage.Wait();
                for (int i = 0; i < load; i++)
                {
                    Thread.Sleep(10);
                    lock (lockObject) warehouse++;
                }
                storage.Release();
            }
        }
    }
}
