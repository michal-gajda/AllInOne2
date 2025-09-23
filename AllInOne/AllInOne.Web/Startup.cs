namespace AllInOne.Web
{
    using System;
    using System.Threading.Tasks;
    using Hangfire;
    using Hangfire.SqlServer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IBackgroundJobClient backgroundJobs, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseHangfireDashboard("/hangfire",
                new DashboardOptions
                {
                    Authorization = new[] { new LocalhostDashboardAuthorizationFilter() },
                    DashboardTitle = "🚀 Hangfire Dashboard - ASP.NET Core 2.1 + .NET Framework 4.8.1",
                    IsReadOnlyFunc = context => false,
                    DisplayStorageConnectionString = false,
                });

            logger.LogInformation("✅ Prawdziwy Hangfire Dashboard skonfigurowany na /hangfire");

            Task.Run(async () =>
            {
                await Task.Delay(2000);

                try
                {
                    backgroundJobs.Enqueue(() => HangfireJobs.SimpleBackgroundJob());
                    RecurringJob.AddOrUpdate("system-check", () => HangfireJobs.RecurringTask(), Cron.Minutely);

                    backgroundJobs.Schedule(() => HangfireJobs.JobWithParameters("StartupUser", 2), TimeSpan.FromSeconds(30));
                    backgroundJobs.Schedule(() => HangfireJobs.FileWritingJob("startup_demo.txt", "Created by ASP.NET Core startup"), TimeSpan.FromMinutes(1));

                    logger.LogInformation("📋 Dodano przykładowe zadania Hangfire");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "❌ Błąd podczas dodawania przykładowych zadań");
                }
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    "default",
                    "{controller=Home}/{action=Index}/{id?}");
            });

            app.Run(async context =>
            {
                if (context.Request.Path == "/")
                {
                    context.Response.Redirect("/hangfire");

                    return;
                }

                context.Response.StatusCode = 404;

                await context.Response.WriteAsync(@"
<!DOCTYPE html>
<html>
<head>
    <title>404 - Strona nie znaleziona</title>
    <style>
        body { font-family: Arial, sans-serif; text-align: center; padding: 50px; }
        .container { max-width: 500px; margin: 0 auto; }
        .button { display: inline-block; background: #007acc; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; margin: 10px; }
    </style>
</head>
<body>
    <div class='container'>
        <h1>🔍 Strona nie znaleziona</h1>
        <p>Ta strona nie istnieje.</p>
        <a href='/hangfire' class='button'>📊 Przejdź do Dashboard</a>
    </div>
</body>
</html>");
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = this.Configuration.GetConnectionString("HangfireConnection")
                                   ?? @"Server=(localdb)\MSSQLLocalDB;Database=HangfireTest;Integrated Security=true;";

            services.AddHangfire(configuration =>
            {
                configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSqlServerStorage(connectionString,
                        new SqlServerStorageOptions
                        {
                            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                            QueuePollInterval = TimeSpan.Zero,
                            UseRecommendedIsolationLevel = true,
                            DisableGlobalLocks = true,
                            PrepareSchemaIfNecessary = true,
                        });
            });

            services.AddHangfireServer(options =>
            {
                options.Queues = new[] { "default", "critical" };
                options.WorkerCount = Math.Max(Environment.ProcessorCount, val2: 2);
            });

            services.AddMvc();
        }
    }
}
