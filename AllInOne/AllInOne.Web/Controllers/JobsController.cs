namespace AllInOne.Web.Controllers
{
    using System;
    using Hangfire;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Kontroler do zarządzania zadaniami Hangfire
    /// </summary>
    [Route("api/[controller]"), ApiController]
    public sealed class JobsController : ControllerBase
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ILogger<JobsController> _logger;

        public JobsController(IBackgroundJobClient backgroundJobClient, ILogger<JobsController> logger)
        {
            this._backgroundJobClient = backgroundJobClient;
            this._logger = logger;
        }

        /// <summary>
        /// Dodaje zadanie z opóźnieniem
        /// </summary>
        /// <param name="delaySeconds">Liczba sekund opóźnienia</param>
        /// <returns>Informacje o dodanym zadaniu</returns>
        [HttpPost("delayed")]
        public IActionResult AddDelayedJob([FromQuery] int delaySeconds = 10)
        {
            var jobId = this._backgroundJobClient.Schedule(
                () => HangfireJobs.SimpleBackgroundJob(),
                TimeSpan.FromSeconds(delaySeconds));

            this._logger.LogInformation($"Dodano zadanie z opóźnieniem {delaySeconds}s: {jobId}");

            return this.Ok(new
            {
                success = true,
                jobId,
                delaySeconds,
                message = $"Dodano zadanie z opóźnieniem {delaySeconds} sekund",
            });
        }

        /// <summary>
        /// Dodaje zadanie zapisu do pliku
        /// </summary>
        /// <param name="content">Zawartość pliku do zapisania</param>
        /// <returns>Informacje o dodanym zadaniu</returns>
        [HttpPost("file")]
        public IActionResult AddFileJob([FromQuery] string content = "Generated from API")
        {
            var fileName = $"api_test_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            var jobId = this._backgroundJobClient.Enqueue(() => HangfireJobs.FileWritingJob(fileName, content));
            this._logger.LogInformation($"Dodano zadanie zapisu do pliku {fileName}: {jobId}");

            return this.Ok(new
            {
                success = true,
                jobId,
                fileName,
                content,
                message = $"Dodano zadanie zapisu do pliku {fileName}",
            });
        }

        /// <summary>
        /// Dodaje ciąg zadań (drugie zadanie uruchamia się po zakończeniu pierwszego)
        /// </summary>
        /// <returns>Informacje o dodanych zadaniach</returns>
        [HttpPost("chain")]
        public IActionResult AddJobChain()
        {
            var jobId1 = this._backgroundJobClient.Enqueue(() => HangfireJobs.FirstJobInChain());
            var jobId2 = this._backgroundJobClient.ContinueJobWith(jobId1, () => HangfireJobs.SecondJobInChain());

            this._logger.LogInformation($"Dodano ciąg zadań: {jobId1} -> {jobId2}");

            return this.Ok(new
            {
                success = true,
                firstJobId = jobId1,
                secondJobId = jobId2,
                message = "Dodano ciąg zadań (pierwsze -> drugie)",
            });
        }

        /// <summary>
        /// Dodaje zadanie z parametrami
        /// </summary>
        /// <param name="name">Nazwa użytkownika</param>
        /// <param name="count">Liczba iteracji</param>
        /// <returns>Informacje o dodanym zadaniu</returns>
        [HttpPost("parameters")]
        public IActionResult AddParameterizedJob([FromQuery] string name = "APIUser", [FromQuery] int count = 3)
        {
            var jobId = this._backgroundJobClient.Enqueue(() => HangfireJobs.JobWithParameters(name, count));
            this._logger.LogInformation($"Dodano zadanie z parametrami dla {name}: {jobId}");

            return this.Ok(new
            {
                success = true,
                jobId,
                parameters = new { name, count },
                message = $"Dodano zadanie dla {name} z {count} iteracjami",
            });
        }

        /// <summary>
        /// Dodaje proste zadanie w tle
        /// </summary>
        /// <returns>Informacje o dodanym zadaniu</returns>
        [HttpPost("simple")]
        public IActionResult AddSimpleJob()
        {
            var jobId = this._backgroundJobClient.Enqueue(() => HangfireJobs.SimpleBackgroundJob());
            this._logger.LogInformation($"Dodano proste zadanie: {jobId}");

            return this.Ok(new
            {
                success = true,
                jobId,
                message = "Dodano proste zadanie w tle",
                dashboardUrl = "/hangfire",
            });
        }

        /// <summary>
        /// Pobiera informacje o dostępnych endpoint-ach API
        /// </summary>
        /// <returns>Lista dostępnych endpoint-ów</returns>
        [HttpGet("info")]
        public IActionResult GetInfo()
        {
            return this.Ok(new
            {
                message = "Hangfire Jobs API - ASP.NET Core 2.1 + .NET Framework 4.8.1",
                endpoints = new[]
                {
                    "POST /api/jobs/simple - Dodaj proste zadanie",
                    "POST /api/jobs/delayed?delaySeconds=10 - Dodaj zadanie z opóźnieniem",
                    "POST /api/jobs/parameters?name=User&count=5 - Dodaj zadanie z parametrami",
                    "POST /api/jobs/file?content=Hello - Dodaj zadanie zapisu do pliku",
                    "POST /api/jobs/chain - Dodaj ciąg zadań",
                    "GET /api/jobs/info - Te informacje",
                },
                dashboardUrl = "/hangfire",
                swaggerUrl = "/swagger",
                version = "ASP.NET Core 2.1 + .NET Framework 4.8.1",
            });
        }
    }

    /// <summary>
    /// Kontroler główny - przekierowuje na stronę główną
    /// </summary>
    public class HomeController : Controller
    {
        /// <summary>
        /// Strona główna - przekierowuje na dashboard
        /// </summary>
        /// <returns>Przekierowanie na stronę główną</returns>
        public IActionResult Index()
        {
            return this.Redirect("/");
        }
    }
}
