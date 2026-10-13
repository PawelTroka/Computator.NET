using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Bootstrapping;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;
using Computator.NET.WebApi.Hosting;
using Computator.NET.WebApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;

namespace Computator.NET.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration) { GSLInitializer.Initialize(); Configuration = configuration; }
        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<CoreBootstrapper>();
            services.AddScoped<IFunctionsProvider, FunctionsProvider>();
            services.AddScoped<IScriptsProvider, ScriptsProvider>();
            services.AddSingleton<IAutocompleteProvider>(s => s.GetRequiredService<CoreBootstrapper>().Create<AutocompleteProvider>());
            services.AddScoped<IExpressionsEvaluator>(_ => new ExpressionsEvaluator());
            services.AddScoped<IScriptEvaluator>(_ => new ScriptEvaluator());
            services.AddSingleton<IModeDeterminer>(s => s.GetRequiredService<CoreBootstrapper>().Create<ModeDeterminer>());
            services.AddSingleton<IChartFactory>(s => RuntimeObjectFactory.CreateInstance<IChartFactory>("Charting"));
            services.AddCors();
            services.AddControllers(options => options.Filters.Add<ExecutionPolicyFilter>()).AddNewtonsoftJson();
            services.AddSwaggerGen(options => options.SwaggerDoc("v1", new OpenApiInfo { Title = "Computator.NET.Web API", Version = "v1" }));
            services.AddSwaggerGenNewtonsoftSupport();
            var connection = Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"] ?? Configuration["ApplicationInsights:ConnectionString"];
            if (!string.IsNullOrWhiteSpace(connection)) services.AddApplicationInsightsTelemetry(options => options.ConnectionString = connection);
        }
        public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            if (environment.IsDevelopment()) app.UseDeveloperExceptionPage();
            app.UseRouting();
            app.UseCors(policy => policy.WithOrigins("http://localhost:63785", "http://localhost:63784",
                "http://localhost:5173", "http://127.0.0.1:5173").WithMethods("GET", "POST").AllowAnyHeader());
            app.UseEndpoints(endpoints => endpoints.MapControllers());
            app.UseSwagger();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Computator.NET.Web API"));
        }
    }
}
