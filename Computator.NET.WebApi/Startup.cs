using Computator.NET.Core.Bootstrapping;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Computator.NET.WebApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            GSLInitializer.Initialize();

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var coreBootstrapper = new CoreBootstrapper();
            services.AddTransient<IExpressionsEvaluator, ExpressionsEvaluator>(isp => coreBootstrapper.Create<ExpressionsEvaluator>());
            services.AddTransient<IModeDeterminer, ModeDeterminer>(isp => coreBootstrapper.Create<ModeDeterminer>());
            services.AddTransient<IChartFactory>(isp => RuntimeObjectFactory.CreateInstance<IChartFactory>("Charting"));

            // Add framework services.
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
        }
    }
}
