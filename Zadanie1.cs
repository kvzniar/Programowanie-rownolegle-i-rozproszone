using System;
using System.Threading;
using System.Threading.Tasks;

namespace Kopalnia
{
    class Program
    {
        static int coal = 2000;                                   // złoże
        static int warehouse = 0;                                 // magazyn
        static SemaphoreSlim deposit = new SemaphoreSlim(2, 2);   // max 2 górników przy złożu
        static SemaphoreSlim storage = new SemaphoreSlim(1, 1);   // max 1 pojazd w magazynie
        static object lockObject = new object();

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Task[] tasks = new Task[5];                           // 5 górników
            for (int i = 0; i < tasks.Length; i++)
            {
                int id = i + 1;
                tasks[i] = Task.Run(() => Miner(id));
            }
            Task.WaitAll(tasks);
            Console.WriteLine($"Koniec. Stan magazynu: {warehouse} jednostek węgla");
        }

        static void Miner(int id)
        {
            while (true)
            {
                // 1. Wydobycie (max 2 górników naraz)
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

                if (load == 0) break;                             // złoże puste -> koniec pracy
                Console.WriteLine($"Górnik {id} wydobył {load} jednostek węgla. Pozostało w złożu: {coal} jednostek.");

                // 2. Transport
                Console.WriteLine($"Górnik {id} transportuje węgiel do magazynu...");
                Thread.Sleep(10000);

                // 3. Rozładunek (max 1 pojazd naraz)
                storage.Wait();
                Console.WriteLine($"Górnik {id} rozładowuje węgiel...");
                for (int i = 0; i < load; i++)
                {
                    Thread.Sleep(10);
                    lock (lockObject) warehouse++;
                }
                storage.Release();
            }
            Console.WriteLine($"Górnik {id} zakończył pracę.");
        }
    }
}
