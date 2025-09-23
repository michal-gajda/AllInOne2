namespace AllInOne.Web
{
    using System;
    using System.IO;
    using System.Threading;
    using Hangfire;

    public class HangfireJobs
    {
        public static void FileWritingJob(string fileName, string content)
        {
            try
            {
                var filePath = Path.Combine(Environment.CurrentDirectory, fileName);
                Console.WriteLine($"[{DateTime.Now}] Zapisuję do pliku: {filePath}");

                File.WriteAllText(filePath, $"{content}\nCzas utworzenia: {DateTime.Now}");

                Console.WriteLine($"[{DateTime.Now}] Plik {fileName} został utworzony pomyślnie");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now}] Błąd podczas tworzenia pliku: {ex.Message}");

                throw;
            }
        }

        public static void FirstJobInChain()
        {
            Console.WriteLine($"[{DateTime.Now}] Wykonuję pierwsze zadanie w ciągu");
            Thread.Sleep(2000);
            Console.WriteLine($"[{DateTime.Now}] Pierwsze zadanie zakończone");
        }

        public static void JobWithParameters(string name, int count)
        {
            Console.WriteLine($"[{DateTime.Now}] Zadanie dla: {name}, liczba iteracji: {count}");

            for (var i = 1; i <= count; i++)
            {
                Console.WriteLine($"[{DateTime.Now}] {name} - Iteracja {i}/{count}");
                Thread.Sleep(1000);
            }

            Console.WriteLine($"[{DateTime.Now}] Zadanie dla {name} zakończone");
        }

        [AutomaticRetry(Attempts = 3)]
        public static void RecurringTask()
        {
            Console.WriteLine($"[{DateTime.Now}] Wykonuję zadanie cykliczne");

            var memoryBefore = GC.GetTotalMemory(false);
            GC.Collect();
            var memoryAfter = GC.GetTotalMemory(true);

            Console.WriteLine($"[{DateTime.Now}] Pamięć przed GC: {memoryBefore} bytes, po GC: {memoryAfter} bytes");
        }

        public static void SecondJobInChain()
        {
            Console.WriteLine($"[{DateTime.Now}] Wykonuję drugie zadanie w ciągu");
            Thread.Sleep(2000);
            Console.WriteLine($"[{DateTime.Now}] Drugie zadanie zakończone");
        }

        public static void SimpleBackgroundJob()
        {
            Console.WriteLine($"[{DateTime.Now}] Wykonuję proste zadanie w tle");
            Thread.Sleep(3000);
            Console.WriteLine($"[{DateTime.Now}] Proste zadanie zakończone");
        }

        [AutomaticRetry(Attempts = 2, DelaysInSeconds = new[] { 10, 30 })]
        public static void UnreliableJob(int failureRate = 50)
        {
            var random = new Random();

            if (random.Next(100) < failureRate)
            {
                Console.WriteLine($"[{DateTime.Now}] Zadanie nie powiodło się (symulacja)");

                throw new InvalidOperationException("Symulacja błędu");
            }

            Console.WriteLine($"[{DateTime.Now}] Zadanie wykonane pomyślnie!");
        }
    }
}
