using System.Collections.Generic;
using LogProxy.Auth;
using LogProxy.Messages;
using LogProxy.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            services.AddHealthChecks().AddCheck<AirTableHealthCheck>("airTableHealthCheck");
            services
                .AddTransient<IMessageConverter<IEnumerable<AirTableResponse>, IEnumerable<EnrichedTitleAndText>>,
                    ToEnrichedTitlesAndTexts>();
            services.AddTransient<IMessageConverter<IEnumerable<TitleAndText>, AirTableRequest>, ToAirTableRequest>();
            services.AddSingleton<ILogProxyService, LogProxyService>();
            services.AddControllers();
            services.Configure<BasicAuthenticatorConfig>(Configuration.GetSection("BasicAuth"));
            services.AddTransient<IBasicAuthenticator, BasicAuthenticator>();
            services
                .AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("BasicAuthentication", options => { });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("BasicAuthentication",
                    new AuthorizationPolicyBuilder("BasicAuthentication").RequireAuthenticatedUser().Build());
            });
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

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}