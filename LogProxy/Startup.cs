using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LogProxy.Messages;
using LogProxy.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LogProxy
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AirTableAccessConfig>(Configuration.GetSection("AirTableConfig"));
            services.AddHttpClient<IAirTableAccess, AirTableAccess>();
            services.AddTransient<IMessageConverter<IEnumerable<AirTableResponse>, IEnumerable<EnrichedTitleAndText>>,
                    ToEnrichedTitlesAndTexts>();
            services.AddTransient<IMessageConverter<IEnumerable<TitleAndText>, AirTableRequest>, ToAirTableRequest>();
            services.AddSingleton<ILogProxyService, LogProxyService>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            //app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}