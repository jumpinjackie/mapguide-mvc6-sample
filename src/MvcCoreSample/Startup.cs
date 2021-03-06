﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OSGeo.MapGuide;

namespace MvcCoreSample
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                builder.AddJsonFile("appsettings.windows.json");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                builder.AddJsonFile("appsettings.linux.json");
            }
            else
            {
                throw new NotSupportedException("MapGuide doesn't work on this platform");
            }

            // Set up configuration sources.
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

            string mgWebConfigPath = Configuration["MapGuide.WebConfigPath"];
            MapGuideApi.MgInitializeWebTier(mgWebConfigPath);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var options = new RewriteOptions();
            //This is to workaround a limitation in mapguide-react-layout where a custom root cannot be set for static assets 
            //So just redirect the bad asset requests to the expected place
            options.AddRedirect("Home/dist/stdassets/(.*)", "lib/viewer/stdassets/$1");
            app.UseRewriter(options);

            //NOTE: https disabled for localhost. If you wish to enable, uncomment the following lines below
            //
            // 1. app.UseHsts()
            // 2. app.UseHttpsRedirection()

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                //app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
