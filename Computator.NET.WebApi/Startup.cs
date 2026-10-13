using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Bootstrapping;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;
using Computator.NET.WebApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Computator.NET.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            GSLInitializer.Initialize();
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var coreBootstrapper = new CoreBootstrapper();
            services.AddSingleton<IFunctionsProvider, FunctionsProvider>();
            services.AddSingleton<IScriptsProvider, ScriptsProvider>();
            services.AddSingleton<IAutocompleteProvider>(isp => coreBootstrapper.Create<AutocompleteProvider>());
            services.AddSingleton<IExpressionsEvaluator>(isp => coreBootstrapper.Create<ExpressionsEvaluator>());
            services.AddSingleton<IScriptEvaluator>(isp => coreBootstrapper.Create<ScriptEvaluator>());
            services.AddSingleton<IModeDeterminer>(isp => coreBootstrapper.Create<ModeDeterminer>());
            services.AddSingleton<IChartFactory>(isp => RuntimeObjectFactory.CreateInstance<IChartFactory>("Charting"));
            services.AddCors();
            services.AddMvc();
            services.AddSwaggerGen(options => options.SwaggerDoc("v1",
                new OpenApiInfo { Title = "Computator.NET.Web API", Version = "v1" }));
            services.AddSwaggerGenNewtonsoftSupport();

            var connection = Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
                ?? Configuration["ApplicationInsights:ConnectionString"];
            var key = Configuration["ApplicationInsights:InstrumentationKey"];
            if (string.IsNullOrWhiteSpace(connection) && !string.IsNullOrWhiteSpace(key))
                connection = "InstrumentationKey=" + key;
            if (!string.IsNullOrWhiteSpace(connection))
                services.AddApplicationInsightsTelemetry(options => options.ConnectionString = connection);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            // CORS is NOT authentication or a script sandbox. Keep this API bound
            // to loopback; the local UI executes trusted scripts with full access.
            app.UseCors(policy => policy.WithOrigins(
                "http://localhost:63785", "http://localhost:63784",
                "http://localhost:5173", "http://127.0.0.1:5173")
                .WithMethods("GET").AllowAnyHeader());
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Computator.NET.Web API"));
        }
    }
}
