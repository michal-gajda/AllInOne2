namespace AllInOne.Web
{
    using System;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Logging;

    internal class Program
    {
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls("http://localhost:5000")
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Information);

                    logging.AddFilter("Hangfire", LogLevel.Warning);
                    logging.AddFilter("Microsoft.AspNetCore", LogLevel.Warning);
                })
                .UseContentRoot(System.IO.Directory.GetCurrentDirectory());

        private static void Main(string[] args)
        {
            Console.WriteLine("=== 🚀 PRAWDZIWY ASP.NET Core 2.1 + .NET Framework 4.8.1 ===");
            Console.WriteLine("✨ Pełny Hangfire Dashboard bez OWIN!");
            Console.WriteLine();

            try
            {
                var host = CreateWebHostBuilder(args).Build();

                Console.WriteLine("🎉 ASP.NET Core Host uruchomiony pomyślnie!");
                Console.WriteLine("🌐 URL: http://localhost:5000");
                Console.WriteLine("📊 PRAWDZIWY Dashboard: http://localhost:5000/hangfire");
                Console.WriteLine();
                Console.WriteLine("✨ Pełne funkcje Dashboard:");
                Console.WriteLine("  • 📋 Tabele zadań z filtrowaniem");
                Console.WriteLine("  • 📊 Wykresy i statystyki w czasie rzeczywistym");
                Console.WriteLine("  • 🔍 Szczegółowe informacje o zadaniach");
                Console.WriteLine("  • 🔄 Możliwość restart/delete zadań");
                Console.WriteLine("  • 📈 Historia wykonania zadań");
                Console.WriteLine("  • ⚙️  Konfiguracja serwerów");
                Console.WriteLine();
                Console.WriteLine("💡 Naciśnij Ctrl+C aby zatrzymać serwer");
                Console.WriteLine();

                host.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Błąd uruchamiania ASP.NET Core: {ex.Message}");
                Console.WriteLine();
                Console.WriteLine("🔧 Sprawdź czy:");
                Console.WriteLine("  • .NET Framework 4.8.1 jest zainstalowany");
                Console.WriteLine("  • SQL Server LocalDB jest dostępny");
                Console.WriteLine("  • Port 5000 nie jest zajęty");
                Console.WriteLine("  • Pakiety NuGet zostały przywrócone");
                Console.WriteLine();
                Console.WriteLine("📋 Szczegóły błędu:");
                Console.WriteLine(ex.ToString());

                Console.WriteLine();
                Console.WriteLine("Naciśnij Enter aby zakończyć...");
                Console.ReadLine();
            }
        }
    }
}
