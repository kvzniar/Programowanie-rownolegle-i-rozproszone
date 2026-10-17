using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kopalnia
{
    class Program
    {
        static int coal = 2000;
        static int warehouse = 0;
        static SemaphoreSlim deposit = new SemaphoreSlim(2, 2);
        static SemaphoreSlim storage = new SemaphoreSlim(1, 1);
        static object lockObject = new object();

        static string[] status;                    // NOWE: status każdego górnika
        static volatile bool finished = false;     // NOWE: sygnał końca dla wątku podglądu

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.Clear();
            Task[] tasks = new Task[5];
            status = new string[tasks.Length];

            Task display = Task.Run(() => Display());       // NOWE: osobny wątek podglądu
            for (int i = 0; i < tasks.Length; i++)
            {
                int id = i + 1;
                tasks[i] = Task.Run(() => Miner(id));
            }
            Task.WaitAll(tasks);
            finished = true;
            display.Wait();
        }

        static void Display()
        {
            while (!finished)
            {
                Draw();
                Thread.Sleep(100);
            }
            Draw();                                         // ostatnie odświeżenie po zakończeniu
        }

        static void Draw()
        {
            lock (lockObject)
            {
                Console.SetCursorPosition(0, 0);
                Console.WriteLine($"Stan złoża: {coal} jednostek węgla".PadRight(50));
                Console.WriteLine($"Stan magazynu: {warehouse} jednostek węgla".PadRight(50));
                foreach (string s in status)
                    Console.WriteLine((s ?? "").PadRight(50));
            }
        }

        static void Miner(int id)
        {
            while (true)
            {
                int load = 0;
                status[id - 1] = $"Górnik {id}: Czeka na złoże...";
                deposit.Wait();
                status[id - 1] = $"Górnik {id}: Wydobywa węgiel...";
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

                status[id - 1] = $"Górnik {id}: Transportuje do magazynu...";
                Thread.Sleep(10000);

                status[id - 1] = $"Górnik {id}: Czeka na magazyn...";
                storage.Wait();
                status[id - 1] = $"Górnik {id}: Rozładowuje węgiel...";
                for (int i = 0; i < load; i++)
                {
                    Thread.Sleep(10);
                    lock (lockObject) warehouse++;
                }
                storage.Release();
            }
            status[id - 1] = $"Górnik {id} zakończył pracę.";
        }
    }
}
