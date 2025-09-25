namespace AllInOne.Web
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Hangfire;
    using Hangfire.SqlServer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Swashbuckle.AspNetCore.Swagger;

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

            // Włączenie Swagger
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "AllInOne Hangfire API v1");
                c.RoutePrefix = "swagger";
                c.DocumentTitle = "🚀 AllInOne Hangfire API";
                c.DefaultModelsExpandDepth(2);
                c.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Model);
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                c.EnableDeepLinking();
                c.DisplayOperationId();
            });

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
                    await context.Response.WriteAsync(@"
<!DOCTYPE html>
<html>
<head>
    <title>AllInOne - Dashboard</title>
    <style>
        body { 
            font-family: Arial, sans-serif; 
            text-align: center; 
            padding: 50px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            min-height: 100vh;
            margin: 0;
            display: flex;
            align-items: center;
            justify-content: center;
        }
        .container { 
            max-width: 600px; 
            margin: 0 auto;
            background: rgba(255, 255, 255, 0.1);
            padding: 40px;
            border-radius: 20px;
            backdrop-filter: blur(10px);
        }
        .button { 
            display: inline-block; 
            background: #007acc; 
            color: white; 
            padding: 15px 25px; 
            text-decoration: none; 
            border-radius: 10px; 
            margin: 15px; 
            transition: all 0.3s ease;
            box-shadow: 0 4px 15px rgba(0,0,0,0.2);
        }
        .button:hover {
            background: #005a99;
            transform: translateY(-2px);
            box-shadow: 0 6px 20px rgba(0,0,0,0.3);
        }
        .swagger { background: #85ea2d; }
        .swagger:hover { background: #6ab91a; }
        .hangfire { background: #f39c12; }
        .hangfire:hover { background: #d68910; }
        h1 { margin-bottom: 10px; }
        p { margin-bottom: 30px; opacity: 0.9; }
    </style>
</head>
<body>
    <div class='container'>
        <h1>🚀 AllInOne Dashboard</h1>
        <p>Wybierz narzędzie, które chcesz uruchomić:</p>
        <a href='/swagger' class='button swagger'>📚 Swagger API Documentation</a>
        <a href='/hangfire' class='button hangfire'>📊 Hangfire Dashboard</a>
        <br>
        <small style='opacity: 0.7; margin-top: 20px; display: block;'>
            ASP.NET Core 2.1 + .NET Framework 4.8.1
        </small>
    </div>
</body>
</html>");
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
        <a href='/' class='button'>🏠 Strona główna</a>
        <a href='/swagger' class='button'>📚 Swagger API</a>
        <a href='/hangfire' class='button'>📊 Hangfire Dashboard</a>
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

            // Konfiguracja Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Title = "AllInOne Hangfire API",
                    Version = "v1",
                    Description = "API do zarządzania zadaniami Hangfire w ASP.NET Core 2.1 + .NET Framework 4.8.1",
                    Contact = new Contact
                    {
                        Name = "AllInOne API",
                        Url = "https://github.com/michal-gajda/AllInOne2"
                    }
                });

                // Włączenie komentarzy XML
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            });
        }
    }
}
