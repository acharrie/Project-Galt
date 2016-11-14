﻿using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Galt.Authentication;
using Galt.Services;

namespace Galt
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            // NetCoreSample copypaste
            services.AddOptions();

            string secretKey = Configuration[ "JwtBearer:SigningKey" ];
            SymmetricSecurityKey signingKey = new SymmetricSecurityKey( Encoding.ASCII.GetBytes( secretKey ) );

            services.Configure<TokenProviderOptions>( o =>
            {
                o.Audience = Configuration[ "JwtBearer:Audience" ];
                o.Issuer = Configuration[ "JwtBearer:Issuer" ];
                o.SigningCredentials = new SigningCredentials( signingKey, SecurityAlgorithms.HmacSha256 );
            } );

            services.AddMvc();
            services.AddSingleton<PasswordHasher>();
            services.AddSingleton<UserService>();
            services.AddSingleton<TokenService>();
            services.AddSingleton<GitHubService>();
            services.AddSingleton<GitHubClient>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseApplicationInsightsRequestTelemetry();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseApplicationInsightsExceptionTelemetry();

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
