using Computator.NET.Core.Autocompletion;
using Computator.NET.Core.Bootstrapping;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Natives;
using Computator.NET.DataTypes;
using Computator.NET.DataTypes.Charts;
using Computator.NET.WebApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;

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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var coreBootstrapper = new CoreBootstrapper();
            services.AddSingleton<IFunctionsProvider, FunctionsProvider>();
            services.AddSingleton<IScriptsProvider, ScriptsProvider>();
            services.AddSingleton<IAutocompleteProvider, AutocompleteProvider>(isp => coreBootstrapper.Create<AutocompleteProvider>());
            services.AddSingleton<IExpressionsEvaluator, ExpressionsEvaluator>(isp => coreBootstrapper.Create<ExpressionsEvaluator>());
            services.AddSingleton<IScriptEvaluator, ScriptEvaluator>(isp => coreBootstrapper.Create<ScriptEvaluator>());
            services.AddSingleton<IModeDeterminer, ModeDeterminer>(isp => coreBootstrapper.Create<ModeDeterminer>());
            services.AddSingleton<IChartFactory>(isp => RuntimeObjectFactory.CreateInstance<IChartFactory>("Charting"));

            services.AddCors();

            // Add framework services.
            services.AddMvc();

            // Register the Swagger generator, defining one or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Computator.NET.Web API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder => builder
                .WithOrigins("http://localhost:63785")
                .WithOrigins("http://localhost:63784"));//we only allow WebClient to call our API, at least for now

            app.UseMvc();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Computator.NET.Web API");
            });
        }
    }
}
